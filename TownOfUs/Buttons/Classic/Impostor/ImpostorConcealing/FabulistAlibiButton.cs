using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Fabulist's "Alibi" button (AmonRa Edition): a copy at the Fabulist's place; the countdown is the copy's time.
/// </summary>
public sealed class FabulistAlibiButton : TownOfUsRoleButton<FabulistRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.FabulistAlibi", "Alibi");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<FabulistOptions>.Instance.AlibiCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration => OptionGroupSingleton<FabulistOptions>.Instance.AlibiDuration;
    public override int MaxUses => (int)OptionGroupSingleton<FabulistOptions>.Instance.MaxAlibis;

    // TownOfUsButton defaults to false: without it the "0 = no limit" option would mean no alibis at all.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override LoadableAsset<Sprite> Sprite => AreAssets.FabulistAlibiButton;

    public override bool CanUse()
    {
        var local = PlayerControl.LocalPlayer;
        return base.CanUse() && !local.inVent && !FabulistSystem.HasAlibi(local) &&
               (!OptionGroupSingleton<FabulistOptions>.Instance.OnlyAtConsole || FabulistSystem.NearTaskConsole(local));
    }

    protected override void OnClick()
    {
        var local = PlayerControl.LocalPlayer;
        FabulistRole.RpcFabulistAlibi(local, local.transform.position);
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        var local = PlayerControl.LocalPlayer;
        if (local != null && FabulistSystem.HasAlibi(local))
        {
            FabulistRole.RpcFabulistAlibiEnd(local, false);
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        // The story fell apart before the time ran out: start the cooldown now.
        // MiraAPI starts the effect after OnClick, so give it a moment first.
        if (EffectActive && !FabulistSystem.HasAlibi(PlayerControl.LocalPlayer) && Timer < EffectDuration - 0.25f)
        {
            ResetCooldownAndOrEffect();
        }
    }
}
