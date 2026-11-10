using MiraAPI.GameOptions;
using MiraAPI.MeetingAbilities;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Are;

/// <summary>
///     Auditor's public audit (AmonRa Edition): posts the latest audit report to everyone's chat and reveals the
///     Auditor's role. Limited per game by the PublicAudits option.
/// </summary>
public sealed class AuditorPublishButton : MeetingActionButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.AuditorPublish", "Public Audit");
    public override float Cooldown => 0.0001f;
    public override float InitialCooldown => 0.0001f;
    public override bool ZeroIsInfinite { get; set; }
    public override int MaxUses => (int)OptionGroupSingleton<AuditorOptions>.Instance.PublicAudits;
    public override LoadableAsset<Sprite> Sprite => AreAssets.AuditButton;
    public override bool HideUponWrapUp => true;
    public override bool DisableUponUse => true;
    public override Color TextOutlineColor => TownOfUsColors.Auditor;

    // Checked when the meeting buttons are created: a running audit becomes a report in OnMeetingStart.
    public override bool Enabled(RoleBehaviour? role)
    {
        return role is AuditorRole auditor && MaxUses > 0 && PlayerControl.LocalPlayer != null &&
               !PlayerControl.LocalPlayer.HasDied() && !PlayerControl.LocalPlayer.IsJailed() &&
               (auditor.LastReport != null || auditor.ActiveAudit != null);
    }

    public override bool CanUse()
    {
        return base.CanUse() && PlayerControl.LocalPlayer.Data.Role is AuditorRole { LastReport: not null } &&
               MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.Discussion
                   or MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is not AuditorRole { LastReport: { } report })
        {
            return;
        }

        AuditorRole.RpcAuditorPublishReport(PlayerControl.LocalPlayer, report.TargetId, report.TargetName,
            report.TasksCompleted, report.Irregularities, report.EncodeStations(), report.CutByMeeting,
            report.TargetDied);
    }
}

/// <summary>
///     Auditor's sanction (AmonRa Edition, off by default): once per game, the vote of the latest player whose audit
///     found irregularities doesn't count in this meeting.
/// </summary>
public sealed class AuditorSanctionButton : MeetingActionButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.AuditorSanction", "Freeze Vote");
    public override float Cooldown => 0.0001f;
    public override float InitialCooldown => 0.0001f;
    public override bool ZeroIsInfinite { get; set; }
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => AreAssets.AuditButton;
    public override bool HideUponWrapUp => true;
    public override bool DisableUponUse => true;
    public override Color TextOutlineColor => TownOfUsColors.Auditor;

    public override bool Enabled(RoleBehaviour? role)
    {
        return OptionGroupSingleton<AuditorOptions>.Instance.Sanction && role is AuditorRole auditor &&
               PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.HasDied() &&
               !PlayerControl.LocalPlayer.IsJailed() && !auditor.SanctionUsed && TargetAlive(auditor);
    }

    public override bool CanUse()
    {
        return base.CanUse() && PlayerControl.LocalPlayer.Data.Role is AuditorRole auditor && !auditor.SanctionUsed &&
               TargetAlive(auditor) &&
               MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.Discussion
                   or MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is AuditorRole auditor && TargetAlive(auditor))
        {
            AuditorRole.RpcAuditorSanction(PlayerControl.LocalPlayer, auditor.LastIrregularTargetId);
        }
    }

    private static bool TargetAlive(AuditorRole auditor)
    {
        return auditor.LastIrregularTargetId != byte.MaxValue &&
               GameData.Instance.GetPlayerById(auditor.LastIrregularTargetId) is { IsDead: false, Disconnected: false };
    }
}
