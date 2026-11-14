using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Possum (AmonRa Edition, Impostor Killing): plays dead. The Possum disappears and their own body lies in their
///     place. Reporting that body doesn't start a meeting: the Possum gets up and the reporter is stunned for a moment,
///     which gives the Possum a chance to kill them (with a ready kill). Logic: <see cref="PossumSystem" />.
/// </summary>
public sealed class PossumRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string IdPart => "Possum";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;
    public DoomableType DoomHintType => DoomableType.Trickster;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.PossumIcon, "TouMira.Role.Impostor.Possum", 1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.PossumIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}PlayDead", "Play Dead"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}PlayDead.WikiDescription"),
            AreAssets.PossumButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [MethodRpc((uint)TownOfUsRpc.PossumPlayDead)]
    public static void RpcPossumPlayDead(PlayerControl possum)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(possum);
            return;
        }

        PossumSystem.HandlePlayDead(possum);
    }

    [MethodRpc((uint)TownOfUsRpc.PossumStandUp)]
    public static void RpcPossumStandUp(PlayerControl possum)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(possum);
            return;
        }

        PossumSystem.HandleStandUp(possum, null);
    }

    /// <summary>
    ///     Sent by the reporter's client instead of the report: the Possum gets up and the reporter is stunned.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.PossumReported)]
    public static void RpcPossumReported(PlayerControl reporter, PlayerControl possum)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(reporter);
            return;
        }

        PossumSystem.HandleStandUp(possum, reporter);
    }
}
