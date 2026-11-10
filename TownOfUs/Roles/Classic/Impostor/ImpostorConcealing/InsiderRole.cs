using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Are;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Insider (AmonRa Edition, Impostor Concealing): an impostor who can really do the tasks from their list. Everyone
///     sees the tasks being completed (visual tasks, Auditor reports...). Every completed task permanently lowers the
///     Insider's kill cooldown a little (like the Juggernaut does for kills), down to a minimum. By default the tasks
///     also count as crew progress: they are added to the completed tasks (not to the total), so helping yourself also
///     helps the crew towards a task win.
/// </summary>
public sealed class InsiderRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    ICrewVariant
{
    public string IdPart => "Insider";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;
    public DoomableType DoomHintType => DoomableType.Trickster;

    // Imitator: the crew role that is all about finishing tasks.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SnitchRole>());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.InsiderIcon, "TouMira.Role.Impostor.Insider", 1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.InsiderIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    public string RoleLongDescription =>
        MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription")
            .Replace("<reduction>", $"{OptionGroupSingleton<InsiderOptions>.Instance.ReductionPerTask}");

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public int CompletedTasks =>
        Player != null && Player.Data != null && Player.Data.Tasks != null
            ? Player.Data.Tasks.ToArray().Count(t => t != null && t.Complete)
            : 0;

    /// <summary>Kill cooldown after the task reductions (base cooldown includes Underdog and the map difference).</summary>
    public float KillCooldown
    {
        get
        {
            var opts = OptionGroupSingleton<InsiderOptions>.Instance;
            var full = Player.GetKillCooldown();
            return Mathf.Max(Mathf.Min(opts.MinKillCooldown, full), full - CompletedTasks * opts.ReductionPerTask);
        }
    }

    // The tasks are real: replace the vanilla "Fake Tasks:" header.
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        base.SpawnTaskHeader(playerControl);
        if (!playerControl.AmOwner)
        {
            return;
        }

        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text =
            $"{Palette.ImpostorRed.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.InsiderTaskHeader").Replace("<reduction>", $"{OptionGroupSingleton<InsiderOptions>.Instance.ReductionPerTask}")}</color>";
    }

    // Unlike other impostors, the Insider can use task consoles.
    public override bool CanUse(IUsable usable)
    {
        return GameManager.Instance.LogicUsables.CanUse(usable, Player);
    }

    /// <summary>Own task done (Insider's client): the current kill timer also drops right away.</summary>
    public void OnOwnTaskCompleted()
    {
        var reduction = OptionGroupSingleton<InsiderOptions>.Instance.ReductionPerTask;
        if (Player.killTimer > 0f)
        {
            Player.SetKillTimer(Mathf.Max(0f, Mathf.Min(Player.killTimer - reduction, KillCooldown)));
        }

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.InsiderTaskDone").Replace("<cooldown>", $"{KillCooldown:0.#}")}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: AreAssets.InsiderIcon);
        notif.AdjustNotification();
    }
}

/// <summary>
///     Adds the Insiders' completed tasks to the crew's completed tasks (not to the total) when the option is on. Runs
///     after the TOU recount (which always leaves impostors out), so the crew can never be blocked from a task win by
///     tasks the Insider didn't do.
/// </summary>
[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public static class InsiderTaskCountPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(GameData __instance)
    {
        if (__instance == null || !OptionGroupSingleton<InsiderOptions>.Instance.TasksHelpCrew)
        {
            return;
        }

        // Role when alive: a dead Insider becomes a ghost role, but the tasks they did stay done.
        var extra = 0;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Tasks == null ||
                player.GetRoleWhenAlive() is not InsiderRole)
            {
                continue;
            }

            extra += player.Data.Tasks.ToArray().Count(t => t != null && t.Complete);
        }

        if (extra > 0 && __instance.TotalTasks > 0)
        {
            __instance.CompletedTasks = Math.Min(__instance.TotalTasks, __instance.CompletedTasks + extra);
        }
    }
}
