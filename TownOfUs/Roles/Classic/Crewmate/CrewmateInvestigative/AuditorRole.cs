using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Are;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Auditor (AmonRa Edition, Crewmate Investigative): secretly audits a player for a while and gets a report of
///     their task behaviour (tasks done, standing at task stations without progress, "irregularities" = vents, kills,
///     role abilities). Can publish a report to everyone in a meeting (revealing themselves) and, optionally, freeze the
///     vote of a player whose audit found irregularities.
/// </summary>
public sealed class AuditorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string IdPart => "Auditor";
    public Color RoleColor => TownOfUsColors.Auditor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;
    public DoomableType DoomHintType => DoomableType.Insight;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.AuditorIcon, "TouMira.Role.Crewmate.Auditor", 1.45f),
        Icon = AreAssets.AuditorIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.SuspenseIntro
    };

    /// <summary>Audit running right now (Auditor's client only).</summary>
    [HideFromIl2Cpp]
    public AuditSession? ActiveAudit { get; private set; }

    /// <summary>Finished reports not shown in a meeting yet (Auditor's client only).</summary>
    [HideFromIl2Cpp]
    public List<AuditReport> PendingReports { get; } = [];

    /// <summary>Latest finished report, the one a public audit publishes (Auditor's client only).</summary>
    [HideFromIl2Cpp]
    public AuditReport? LastReport { get; private set; }

    /// <summary>Audits given so far (initial ones + gained from tasks), capped by the MaxAudits option.</summary>
    public int AuditsGranted { get; set; }

    public int TasksTowardNextAudit { get; set; }

    public bool SanctionUsed { get; set; }

    /// <summary>Latest player whose audit found irregularities (Auditor's client), 255 = none. Sanction target.</summary>
    public byte LastIrregularTargetId { get; set; } = byte.MaxValue;

    /// <summary>Player whose vote doesn't count in the current meeting (all clients), 255 = nobody.</summary>
    public byte SanctionedThisMeeting { get; set; } = byte.MaxValue;

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Audit", "Audit"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Audit.WikiDescription"),
            AreAssets.AuditButton),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Publish", "Public Audit"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Meeting", "Meeting"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Publish.WikiDescription"),
            AreAssets.AuditButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        AuditsGranted = (int)OptionGroupSingleton<AuditorOptions>.Instance.InitialAudits;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ActiveAudit = null;
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        // A dead Auditor's audit just stops, no report.
        ActiveAudit = null;
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (!Player.AmOwner)
        {
            return;
        }

        if (ActiveAudit != null)
        {
            FinishAudit(cutByMeeting: true, targetDied: false);
        }

        foreach (var report in PendingReports)
        {
            var title =
                $"{TownOfUsColors.Auditor.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportTitle").Replace("<player>", report.TargetName)}</color>";
            MiscUtils.AddFakeChat(Player.Data, title, report.Format(), false, true);
        }

        PendingReports.Clear();
    }

    /// <summary>
    ///     Starts an audit (Auditor's client, after the button click passed the Veteran / interaction checks).
    /// </summary>
    public void StartAudit(PlayerControl target)
    {
        var opts = OptionGroupSingleton<AuditorOptions>.Instance;
        ActiveAudit = new AuditSession(target, opts.AuditDuration, opts.IdleStationTime);
        Coroutines.Start(CoRunAudit(ActiveAudit));

        if (opts.NotifyTarget)
        {
            RpcAuditorNotifyTarget(Player, target);
        }

        Notify(MiraLocaleManager.Get("TownOfUsMira.Role.AuditorAuditStarted").Replace("<player>", target.Data.PlayerName));
    }

    [HideFromIl2Cpp]
    private IEnumerator CoRunAudit(AuditSession session)
    {
        while (ActiveAudit == session)
        {
            if (MeetingHud.Instance || ExileController.Instance)
            {
                // OnMeetingStart finishes it.
                yield return null;
                continue;
            }

            if (session.Target == null || session.Target.Data == null || session.Target.Data.Disconnected ||
                session.Target.HasDied())
            {
                FinishAudit(cutByMeeting: false, targetDied: true);
                yield break;
            }

            session.Tick();
            if (session.TimeIsUp)
            {
                FinishAudit(cutByMeeting: false, targetDied: false);
                yield break;
            }

            yield return null;
        }
    }

    private void FinishAudit(bool cutByMeeting, bool targetDied)
    {
        var session = ActiveAudit;
        if (session == null)
        {
            return;
        }

        ActiveAudit = null;
        var report = session.Finish(cutByMeeting, targetDied);
        LastReport = report;
        PendingReports.Add(report);
        if (report.Verdict is AuditVerdict.Irregular)
        {
            LastIrregularTargetId = report.TargetId;
        }

        Message($"Auditor: audit of {report.TargetName} -> {report.Verdict}, tasks {report.TasksCompleted}, idle stations {report.IdleStations.Count}, irregularities {report.Irregularities}");

        if (!cutByMeeting)
        {
            var verdict = report.Verdict;
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.AuditorAuditFinished")
                .Replace("<player>", report.TargetName)
                .Replace("<verdict>",
                    $"{AuditReport.VerdictColor(verdict).ToTextColor()}{AuditReport.VerdictText(verdict)}</color>"));
        }
    }

    /// <summary>
    ///     Own task finished (Auditor's client): every TasksPerAudit tasks give one more audit, up to MaxAudits.
    /// </summary>
    public void OnOwnTaskCompleted()
    {
        var opts = OptionGroupSingleton<AuditorOptions>.Instance;
        var perAudit = (int)opts.TasksPerAudit;
        if (perAudit <= 0 || AuditsGranted >= (int)opts.MaxAudits)
        {
            return;
        }

        TasksTowardNextAudit++;
        if (TasksTowardNextAudit < perAudit)
        {
            return;
        }

        TasksTowardNextAudit = 0;
        AuditsGranted++;
        var button = CustomButtonSingleton<AuditorAuditButton>.Instance;
        button.SetUses(button.UsesLeft + 1);
        Notify(MiraLocaleManager.Get("TownOfUsMira.Role.AuditorAuditGained"));
    }

    private static void Notify(string text)
    {
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: AreAssets.AuditorIcon);
        notif.AdjustNotification();
    }

    [MethodRpc((uint)TownOfUsRpc.AuditorNotifyTarget)]
    public static void RpcAuditorNotifyTarget(PlayerControl auditor, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(auditor);
            return;
        }

        if (auditor.Data.Role is not AuditorRole)
        {
            Error("RpcAuditorNotifyTarget - Invalid auditor");
            return;
        }

        if (target.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Auditor, alpha: 0.4f));
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.AuditorYouAreAudited"));
        }
    }

    /// <summary>
    ///     Public audit: everyone sees the report in the chat (formatted in their own language) and the Auditor's role.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.AuditorPublishReport)]
    public static void RpcAuditorPublishReport(PlayerControl auditor, byte targetId, string targetName, int tasks,
        int irregularities, string stations, bool cutByMeeting, bool targetDied)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(auditor);
            return;
        }

        if (auditor.Data.Role is not AuditorRole role)
        {
            Error("RpcAuditorPublishReport - Invalid auditor");
            return;
        }

        var report = new AuditReport
        {
            TargetId = targetId,
            TargetName = targetName,
            TasksCompleted = tasks,
            Irregularities = irregularities,
            IdleStations = AuditReport.DecodeStations(stations),
            CutByMeeting = cutByMeeting,
            TargetDied = targetDied
        };

        if (!auditor.HasModifier<AuditorRevealModifier>())
        {
            auditor.AddModifier<AuditorRevealModifier>(role);
        }

        var title =
            $"{TownOfUsColors.Auditor.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.AuditorPublicReportTitle").Replace("<player>", targetName)}</color>";
        MiscUtils.AddFakeChat(auditor.Data, title, report.Format(), true, true);
    }

    /// <summary>
    ///     Sanction: the target's vote doesn't count in this meeting (removed by the host in ProcessVotesEvent).
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.AuditorSanction)]
    public static void RpcAuditorSanction(PlayerControl auditor, byte targetId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(auditor);
            return;
        }

        if (auditor.Data.Role is not AuditorRole role || MeetingHud.Instance == null)
        {
            Error("RpcAuditorSanction - Invalid auditor or no meeting");
            return;
        }

        role.SanctionUsed = true;
        role.SanctionedThisMeeting = targetId;

        var name = GameData.Instance.GetPlayerById(targetId)?.PlayerName ?? "?";
        var notif = Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.AuditorSanctionNotif").Replace("<player>", name)}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: AreAssets.AuditorIcon);
        notif.AdjustNotification();
    }
}
