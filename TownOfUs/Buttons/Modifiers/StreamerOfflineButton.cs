using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;

namespace TownOfUs.Buttons.Modifiers;

/// <summary>
///     The Streamer's "Go offline" button (AmonRa Edition): hidden from the cameras for a few seconds. Uses per round
///     come back at every meeting.
/// </summary>
public sealed class StreamerOfflineButton : TownOfUsButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Modifier.StreamerOffline", "Go Offline");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Streamer;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override float EffectDuration => OptionGroupSingleton<StreamerOptions>.Instance.OfflineDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<StreamerOptions>.Instance.OfflineUses.Value;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => AreAssets.StreamerOfflineButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer &&
               PlayerControl.LocalPlayer.HasModifier<StreamerModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead &&
               OptionGroupSingleton<StreamerOptions>.Instance.OfflineButton;
    }

    public override bool CanUse()
    {
        return base.CanUse() && !PlayerControl.LocalPlayer.HasModifier<StreamerOfflineModifier>();
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcAddModifier<StreamerOfflineModifier>();
    }
}
