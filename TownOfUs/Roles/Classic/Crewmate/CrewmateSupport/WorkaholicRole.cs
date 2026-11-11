using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Are;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Workaholic (AmonRa Edition, Crewmate Support, idea of the owner's colleague): after finishing their own tasks,
///     takes over tasks of other crewmates, one at a time, to push the task bar. The whole mechanism is in
///     <see cref="WorkaholicSystem" />.
/// </summary>
public sealed class WorkaholicRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string IdPart => "Workaholic";
    public Color RoleColor => TownOfUsColors.Workaholic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Insight;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.WorkaholicIcon, "TouMira.Role.Crewmate.Workaholic", 1.45f),
        Icon = AreAssets.WorkaholicIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ToppatIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Take", "Take Task"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic", "Ability"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Take.WikiDescription"),
            AreAssets.WorkaholicButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        if (Player == null || !Player.AmOwner)
        {
            return sb;
        }

        var line = WorkaholicSystem.OwnTasksDone(Player)
            ? MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicHelpedCount")
                .Replace("<count>", $"{WorkaholicSystem.HelpedTasks}")
            : MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicOwnTasksFirst");
        sb.AppendLine(TownOfUsPlugin.Culture, $"<size=60%><color=#BFBFBF>{line}</color></size>");
        return sb;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        // No longer a Workaholic (death, role change): the held task goes back to its owner. Runs on every client.
        if (targetPlayer != null)
        {
            WorkaholicSystem.ReleaseAllHeldBy(targetPlayer.PlayerId);
        }
    }
}
