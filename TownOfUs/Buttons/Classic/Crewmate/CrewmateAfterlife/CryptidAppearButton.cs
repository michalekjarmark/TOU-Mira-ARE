using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     The Cryptid's "Appear" button (AmonRa Edition), used as a ghost: the countdown is the sighting's time.
/// </summary>
public sealed class CryptidAppearButton : TownOfUsRoleButton<CryptidRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.CryptidAppear", "Appear");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Cryptid;
    public override bool UsableInDeath => true;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<CryptidOptions>.Instance.SightingCooldown + MapCooldown, 0.001f, 120f);

    public override float EffectDuration => OptionGroupSingleton<CryptidOptions>.Instance.SightingDuration;
    public override int MaxUses => (int)OptionGroupSingleton<CryptidOptions>.Instance.MaxSightings;

    // TownOfUsButton defaults to false: without it the "0 = no limit" option would mean no sightings at all.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override LoadableAsset<Sprite> Sprite => AreAssets.CryptidAppearButton;

    // The Cryptid is always dead: TownOfUsButton hides buttons of dead players.
    public override void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role));
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role is { ScaredOff: false } && !CryptidSystem.IsShowing(PlayerControl.LocalPlayer) &&
               !CryptidSystem.SomeoneTooClose(PlayerControl.LocalPlayer);
    }

    protected override void OnClick()
    {
        CryptidRole.RpcCryptidAppear(PlayerControl.LocalPlayer);
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        var local = PlayerControl.LocalPlayer;
        if (local != null && CryptidSystem.IsShowing(local))
        {
            CryptidRole.RpcCryptidVanish(local, (int)CryptidSystem.EndReason.Time);
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        // Someone came close before the time ran out: start the cooldown now.
        // MiraAPI starts the effect after OnClick, so give it a moment first.
        if (EffectActive && !CryptidSystem.IsShowing(PlayerControl.LocalPlayer) && Timer < EffectDuration - 0.25f)
        {
            ResetCooldownAndOrEffect();
        }
    }
}
