using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Fabulist (AmonRa Edition, Impostor Concealing): leaves a copy of themselves at a task console as an alibi, seen by
///     everyone (cameras too) for a while. The copy disappears when another player comes close, at a meeting or when the
///     Fabulist dies. Logic: <see cref="FabulistSystem" />.
/// </summary>
public sealed class FabulistRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    ICrewVariant, IAmonRaContent
{
    public string IdPart => "Fabulist";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;
    public DoomableType DoomHintType => DoomableType.Trickster;

    // Imitator: the crew role that is all about tasks.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<WorkaholicRole>());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.FabulistIcon, "TouMira.Role.Impostor.Fabulist", 1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.FabulistIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Alibi", "Alibi"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Alibi.WikiDescription"),
            AreAssets.FabulistAlibiButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    /// <summary>A copy of <paramref name="fabulist" /> at <paramref name="position" />, on every client.</summary>
    [MethodRpc((uint)TownOfUsRpc.FabulistAlibi)]
    public static void RpcFabulistAlibi(PlayerControl fabulist, Vector2 position)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(fabulist);
            return;
        }

        if (fabulist.Data.Role is not FabulistRole)
        {
            Error("RpcFabulistAlibi - Invalid source");
            return;
        }

        FabulistSystem.Create(fabulist, position);
    }

    /// <summary>The copy is gone (time is up, or someone came close: <paramref name="broken" />).</summary>
    [MethodRpc((uint)TownOfUsRpc.FabulistAlibiEnd)]
    public static void RpcFabulistAlibiEnd(PlayerControl fabulist, bool broken)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(fabulist);
            return;
        }

        FabulistSystem.Remove(fabulist.PlayerId, broken);
    }
}
