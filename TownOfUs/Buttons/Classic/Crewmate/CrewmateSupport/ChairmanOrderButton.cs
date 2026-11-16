using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Chairman's "Order" button (AmonRa Edition). A normal player-targeting button, so the click counts as an
///     interaction. One order at a time; the countdown on the button is the time the player has.
/// </summary>
public sealed class ChairmanOrderButton : TownOfUsRoleButton<ChairmanRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ChairmanOrder", "Order");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Chairman;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<ChairmanOptions>.Instance.OrderCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration => OptionGroupSingleton<ChairmanOptions>.Instance.OrderTime;

    public override int MaxUses => (int)OptionGroupSingleton<ChairmanOptions>.Instance.MaxOrders;

    // TownOfUsButton defaults to false: without it the "0 = no limit" option would mean no orders at all.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override LoadableAsset<Sprite> Sprite => AreAssets.ChairmanOrderButton;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override bool CanUse()
    {
        return base.CanUse() && !ChairmanSystem.OrderActive;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        // The order ended before the countdown (task done, no tasks left, the player died): start the cooldown now.
        // MiraAPI starts the effect after OnClick, so give it a moment first.
        if (EffectActive && !ChairmanSystem.OrderActive && Timer < EffectDuration - 0.25f)
        {
            EffectActive = false;
            Timer = Cooldown;
        }
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Order: Target is null");
            return;
        }

        ChairmanSystem.StartOrder(Role, Target);
    }
}
