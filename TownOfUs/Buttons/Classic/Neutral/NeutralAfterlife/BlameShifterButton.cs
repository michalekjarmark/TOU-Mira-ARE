using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

/// <summary>
///     The Blame Shifter's button (AmonRa Edition), used as a ghost: "Blame" (a list of living players, not the
///     killer) while nobody is blamed, then "Possess" on the blamed player. Uses and cooldown count only possessions.
/// </summary>
public sealed class BlameShifterButton : TownOfUsRoleButton<BlameShifterRole>
{
    private bool? _shownBlame;

    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterBlame", "Blame");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.BlameShifter;
    public override bool UsableInDeath => true;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<BlameShifterOptions>.Instance.PossessCooldown + MapCooldown, 0.001f, 120f);

    public override float EffectDuration => OptionGroupSingleton<BlameShifterOptions>.Instance.PossessDuration;
    public override int MaxUses => (int)OptionGroupSingleton<BlameShifterOptions>.Instance.MaxPossessions;

    // TownOfUsButton defaults to false: without it the "0 = no limit" option would mean no possessions at all.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override LoadableAsset<Sprite> Sprite => AreAssets.BlameShifterBlameButton;

    private bool NeedsBlame => Role != null && (Role.Blamed == null || Role.Blamed.HasDied() ||
                                                Role.Blamed.Data == null || Role.Blamed.Data.Disconnected);

    // The Blame Shifter is always dead: TownOfUsButton hides buttons of dead players.
    public override void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role));
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        return NeedsBlame || BlameShifterSystem.CanPossess(PlayerControl.LocalPlayer, Role.Blamed);
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        if (!NeedsBlame)
        {
            base.ClickHandler();
            return;
        }

        // Choosing who to blame costs nothing.
        var local = PlayerControl.LocalPlayer;
        var killer = BlameShifterSystem.KillerOf(local);
        var menu = CustomPlayerMenu.Create();
        menu.Begin(
            plr => !plr.HasDied() && !plr.AmOwner && plr.PlayerId != killer,
            plr =>
            {
                menu.ForceClose();
                if (plr != null && !plr.HasDied() && !MeetingHud.Instance)
                {
                    BlameShifterRole.RpcBlameShifterBlame(local, plr);
                }
            });
    }

    protected override void OnClick()
    {
        if (Role?.Blamed != null)
        {
            BlameShifterRole.RpcBlameShifterPossess(PlayerControl.LocalPlayer, Role.Blamed);
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (Role == null)
        {
            return;
        }

        // "Blame" or "Possess": name and icon.
        if (!EffectActive && _shownBlame != NeedsBlame)
        {
            _shownBlame = NeedsBlame;
            if (NeedsBlame)
            {
                OverrideName(MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterBlame", "Blame"));
                OverrideSprite(AreAssets.BlameShifterBlameButton.LoadAsset());
            }
            else
            {
                OverrideName(MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterPossess", "Possess"));
                OverrideSprite(AreAssets.BlameShifterPossessButton.LoadAsset());
            }
        }

        // The possession ended early (the player died, a meeting): start the cooldown now.
        if (EffectActive && !BlameShifterSystem.IsPossessed(Role.Blamed) && Timer < EffectDuration - 0.25f)
        {
            ResetCooldownAndOrEffect();
        }
    }
}
