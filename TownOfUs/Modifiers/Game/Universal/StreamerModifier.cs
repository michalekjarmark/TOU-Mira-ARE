using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Interfaces;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

/// <summary>
///     ARE universal modifier: the cameras are this player's stream. Whenever someone watches a camera feed with them
///     in it (the map's cameras, the Sentry's, the Operative's or the Handyman's portable ones, a Stream Sniper's view;
///     an open admin map as an option), they see "LIVE: N viewers", without names. "Going offline" hides them from the
///     cameras (and the admin map, with that option) for a few seconds. Logic: <see cref="StreamerSystem"/>.
/// </summary>
public sealed class StreamerModifier : UniversalGameModifier, IWikiDiscoverable, IButtonModifier, IAmonRaContent
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Streamer,
        TmpSpriteUtils.CreateSpriteAsset(AreAssets.StreamerIcon.LoadAsset(), "TouMira.Modifier.Universal.Streamer",
            1.45f));

    public override string IdPart => "Streamer";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}", "Streamer");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.StreamerIcon;
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}Offline", "Go Offline"),
            MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}Offline.WikiDescription"),
            AreAssets.StreamerOfflineButton)
    ];

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.StreamerChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.StreamerAmount;
    }

    // The "Go offline" button takes the modifier button slot, like the Operative's or the Button Barry's.
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) &&
               !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }

    /// <summary>
    ///     <paramref name="watcher" /> started or stopped seeing <paramref name="streamer" /> on a camera feed (sent by the
    ///     watcher's client, used by the Streamer's client).
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.StreamerView)]
    public static void RpcStreamerView(PlayerControl watcher, PlayerControl streamer, bool watching)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(watcher);
            return;
        }

        StreamerSystem.HandleView(watcher, streamer, watching);
    }
}
