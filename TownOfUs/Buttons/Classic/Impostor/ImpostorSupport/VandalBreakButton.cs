using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Vandal's "Break" button (AmonRa Edition): targets the closest task console in use range where someone has a finished
///     task to break (<see cref="VandalSystem" />).
/// </summary>
public sealed class VandalBreakButton : TownOfUsRoleButton<VandalRole, Console>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.VandalBreak", "Break");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<VandalOptions>.Instance.BreakCooldown + MapCooldown, 5f, 120f);

    public override int MaxUses => (int)OptionGroupSingleton<VandalOptions>.Instance.MaxBreaks;
    public override bool ZeroIsInfinite => true;
    public override LoadableAsset<Sprite> Sprite => AreAssets.VandalButton;

    public override Console? GetTarget()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || ShipStatus.Instance == null || local.inVent)
        {
            return null;
        }

        // Closest first: the expensive check (who finished what) only runs for the consoles in range.
        var position = local.GetTruePosition();
        var inRange = new List<(Console Console, float Distance)>();
        var all = ShipStatus.Instance.AllConsoles;
        for (var i = 0; i < all.Length; i++)
        {
            var console = all[i];
            if (!RiggerSystem.IsTaskConsole(console))
            {
                continue;
            }

            var distance = Vector2.Distance(position, console.transform.position);
            if (distance <= console.UsableDistance)
            {
                inRange.Add((console, distance));
            }
        }

        foreach (var (console, _) in inRange.OrderBy(x => x.Distance))
        {
            if (VandalSystem.HasBreakable(console, local))
            {
                return console;
            }
        }

        return null;
    }

    public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            Target.SetOutline(active, active);
        }
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        // Someone else's Vandal was faster (or the task got undone some other way): the use comes back.
        if (!VandalSystem.Break(PlayerControl.LocalPlayer, Target) && LimitedUses)
        {
            IncreaseUses();
        }
    }
}
