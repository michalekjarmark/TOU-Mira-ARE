using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Stream Sniper (AmonRa Edition, Impostor Support): watches another player's "stream" for a few seconds, a small
///     window with the view around them (like the Parasite's). The player is chosen from a list, or (option) is the one
///     marked earlier next to the Stream Sniper. A Streamer being watched gets a viewer. Logic:
///     <see cref="StreamSniperSystem" />.
/// </summary>
public sealed class StreamSniperRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable,
    IDoomable, ICrewVariant, IAmonRaContent
{
    public string IdPart => "StreamSniper";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public DoomableType DoomHintType => DoomableType.Perception;

    // Imitator: the crew role that watches a player.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<LookoutRole>());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.StreamSniperIcon, "TouMira.Role.Impostor.StreamSniper",
            1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.StreamSniperIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    /// <summary>The player marked with "Mark" (Stream Sniper's client, "marked player" mode).</summary>
    [HideFromIl2Cpp]
    public PlayerControl? Marked { get; set; }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Watch", "Watch"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Watch.WikiDescription"),
            AreAssets.StreamSniperWatchButton),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Mark", "Mark"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Mark.WikiDescription"),
            AreAssets.StreamSniperMarkButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Marked = null;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (targetPlayer.AmOwner)
        {
            StreamSniperSystem.Stop();
        }
    }

    // The view follows the watched player after they moved this frame.
    public void LateUpdate()
    {
        if (Player != null && Player.AmOwner)
        {
            StreamSniperSystem.LateUpdate();
        }
    }

    /// <summary>The Stream Sniper started watching <paramref name="target" /> ("the player knows" option).</summary>
    [MethodRpc((uint)TownOfUsRpc.StreamSniperWatch)]
    public static void RpcStreamSniperWatch(PlayerControl sniper, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(sniper);
            return;
        }

        if (sniper.Data.Role is not StreamSniperRole)
        {
            Error("RpcStreamSniperWatch - Invalid source");
            return;
        }

        StreamSniperSystem.HandleWatched(target);
    }
}
