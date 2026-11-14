using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Lighthouse Keeper's "Place Lantern" button (AmonRa Edition): a lantern under the player's feet. Not in a vent,
///     on a ladder or a moving platform, and not on a spot the Sentry couldn't put a camera on (vents, doors, walls).
/// </summary>
public sealed class LighthouseKeeperPlaceButton : TownOfUsRoleButton<LighthouseKeeperRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.LighthouseKeeperPlace", "Place Lantern");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.LighthouseKeeper;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<LighthouseKeeperOptions>.Instance.LanternCooldown + MapCooldown, 0f, 120f);

    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => (int)OptionGroupSingleton<LighthouseKeeperOptions>.Instance.MaxLanterns;
    public override LoadableAsset<Sprite> Sprite => AreAssets.LighthouseKeeperButton;

    public override bool CanUse()
    {
        if (!base.CanUse())
        {
            return false;
        }

        var local = PlayerControl.LocalPlayer;
        if (local.inVent || local.walkingToVent || local.onLadder || local.inMovingPlat)
        {
            return false;
        }

        // The same spot check as the Sentry's camera (SentryPlaceCameraButton).
        var hits = Physics2D.OverlapBoxAll(local.transform.position, Vector2.one * 0.5f, 0);
        hits = hits.Where(c =>
            (c.name.Contains("Vent") || c.name.Contains("Door") || !c.isTrigger) &&
            c.gameObject.layer != 8 &&
            c.gameObject.layer != 5).ToArray();

        var noConflict = !PhysicsHelpers.AnythingBetween(local.Collider, local.Collider.bounds.center,
            local.transform.position, Constants.ShipAndAllObjectsMask, false);

        return hits.Length == 0 && noConflict && !ModCompatibility.GetPlayerElevator(local).Item1;
    }

    protected override void OnClick()
    {
        LighthouseSystem.PlaceLantern(PlayerControl.LocalPlayer);
    }
}
