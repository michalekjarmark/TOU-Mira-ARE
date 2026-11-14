using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Are;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Comfy (AmonRa Edition, Crewmate Support, the owner's idea, from "wygodny"): a task closed before it's finished
///     annoys them. The task gets locked for a while, a ready sulk makes them untouchable for a few seconds, and the
///     locked task can be offloaded to someone nearby. The whole mechanism is in <see cref="ComfySystem" />.
/// </summary>
public sealed class ComfyRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string IdPart => "Comfy";
    public Color RoleColor => TownOfUsColors.Comfy;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Protective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.ComfyIcon, "TouMira.Role.Crewmate.Comfy", 1.45f),
        Icon = AreAssets.ComfyIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ToppatIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sulk", "Sulk"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic", "Ability"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sulk.WikiDescription"),
            AreAssets.ComfyIcon),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Offload", "Offload Task"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic", "Ability"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Offload.WikiDescription"),
            AreAssets.ComfyButton)
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
        if (Player == null || !Player.AmOwner || Player.HasDied())
        {
            return sb;
        }

        var line = ComfySystem.SulkStatusText(Player);
        if (line.Length > 0)
        {
            sb.AppendLine(TownOfUsPlugin.Culture, $"<size=60%><color=#BFBFBF>{line}</color></size>");
        }

        return sb;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        // No longer a Comfy (death, role change): the offloaded tasks come back. Runs on every client.
        if (targetPlayer != null)
        {
            ComfySystem.ReleaseAllOf(targetPlayer.PlayerId);
        }
    }
}
