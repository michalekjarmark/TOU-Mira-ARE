using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Stream Sniper's "Watch" button (AmonRa Edition): a list of living players (or the marked player, option), then
///     the view window for the effect's time.
/// </summary>
public sealed class StreamSniperWatchButton : TownOfUsRoleButton<StreamSniperRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.StreamSniperWatch", "Watch");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<StreamSniperOptions>.Instance.WatchCooldown + MapCooldown, 5f, 120f);

    public override float EffectDuration => OptionGroupSingleton<StreamSniperOptions>.Instance.WatchDuration;
    public override int MaxUses => (int)OptionGroupSingleton<StreamSniperOptions>.Instance.MaxWatches;

    // TownOfUsButton defaults to false: without it the "0 = no limit" option would mean no watching at all.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override LoadableAsset<Sprite> Sprite => AreAssets.StreamSniperWatchButton;

    private static bool MarkedMode =>
        OptionGroupSingleton<StreamSniperOptions>.Instance.TargetMode == StreamSniperTargetMode.Marked;

    public override bool CanUse()
    {
        return base.CanUse() && Role != null && (!MarkedMode || Role.Marked is { } marked && !marked.HasDied());
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        if (MarkedMode)
        {
            base.ClickHandler();
            return;
        }

        var menu = CustomPlayerMenu.Create();
        menu.Begin(
            plr => !plr.HasDied() && !plr.AmOwner,
            plr =>
            {
                // The menu is still closing here (the player can't move yet), so no CanClick: it passed on opening.
                menu.ForceClose();
                if (plr != null && !plr.HasDied() && !EffectActive && !MeetingHud.Instance &&
                    !PlayerControl.LocalPlayer.HasDied())
                {
                    Watch(plr);
                }
            });
    }

    protected override void OnClick()
    {
        if (Role?.Marked != null)
        {
            StreamSniperSystem.Start(Role.Marked);
        }
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        StreamSniperSystem.Stop();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        // The window closed early (the player died or left): start the cooldown now.
        if (EffectActive && !StreamSniperSystem.Watching && Timer < EffectDuration - 0.25f)
        {
            ResetCooldownAndOrEffect();
        }
    }

    // List mode: what the normal click does, for the chosen player.
    private void Watch(PlayerControl target)
    {
        if (LimitedUses)
        {
            SetUses(UsesLeft - 1);
        }

        StreamSniperSystem.Start(target);
        EffectActive = true;
        Timer = EffectDuration;
    }
}
