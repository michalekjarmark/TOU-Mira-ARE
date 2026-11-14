using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     ARE: the Possum's "Play Dead" button. A second click ("Stand Up", after a short moment) or the time limit
///     gets the Possum up; a report of the fake body does too (<see cref="PossumSystem.HandleStandUp" />). Works like
///     the Swooper's button, but stays usable while the Possum lies (their <see cref="DisabledModifier" /> blocks
///     everything else).
/// </summary>
public sealed class PossumPlayDeadButton : TownOfUsRoleButton<PossumRole>
{
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.PossumPlayDead", "Play Dead");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<PossumOptions>.Instance.PlayDeadCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration => OptionGroupSingleton<PossumOptions>.Instance.PlayDeadDuration;
    public override int MaxUses => (int)OptionGroupSingleton<PossumOptions>.Instance.MaxUses;
    public override LoadableAsset<Sprite> Sprite => AreAssets.PossumButton;
    public override bool ZeroIsInfinite { get; set; } = true;

    // The colour of the cooldown number before it turns green while lying down.
    private Color? _timerColor;

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
    }

    // While lying down "Stand Up" works at any time, so it mustn't look like a cooldown (owner's feedback): no shading,
    // and the number (time left until the Possum gets up on their own) is green, like a duration.
    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        base.FixedUpdateHandler(playerControl);
        if (Button == null)
        {
            return;
        }

        _timerColor ??= Button.cooldownTimerText.color;
        if (EffectActive)
        {
            Button.isCoolingDown = false;
            Button.SetCooldownFill(0f);
            Button.cooldownTimerText.color = Palette.AcceptedGreen;
        }
        else
        {
            Button.cooldownTimerText.color = _timerColor.Value;
        }
    }

    public override bool CanUse()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || local.Data.IsDead || HudManager.Instance.Chat.IsOpenOrOpening ||
            MeetingHud.Instance || TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (EffectActive)
        {
            return PossumSystem.IsPlayingDead(local) && Timer <= EffectDuration - 1f;
        }

        if (local.HasModifier<DisabledModifier>(x => !x.CanUseAbilities) || !local.CanMove || local.inVent ||
            local.walkingToVent || local.onLadder || local.inMovingPlat)
        {
            return false;
        }

        return Timer <= 0 && (!LimitedUses || UsesLeft > 0);
    }

    protected override void OnClick()
    {
        if (EffectActive)
        {
            OnEffectEnd();
            return;
        }

        PossumRole.RpcPossumPlayDead(PlayerControl.LocalPlayer);
        UsesLeft--;
        if (LimitedUses)
        {
            Button?.SetUsesRemaining(UsesLeft);
        }
    }

    public override void OnEffectEnd()
    {
        if (PossumSystem.IsPlayingDead(PlayerControl.LocalPlayer))
        {
            PossumRole.RpcPossumStandUp(PlayerControl.LocalPlayer);
        }
    }
}
