using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Rigger's "Rig" button (AmonRa Edition): targets the closest task console in use range that isn't rigged yet.
/// </summary>
public sealed class RiggerRigButton : TownOfUsRoleButton<RiggerRole, Console>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.RiggerRig", "Rig");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<RiggerOptions>.Instance.RigCooldown + MapCooldown, 5f, 120f);

    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => AreAssets.RiggerButton;

    public override Console? GetTarget()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || ShipStatus.Instance == null)
        {
            return null;
        }

        var position = local.GetTruePosition();
        Console? best = null;
        var bestDistance = float.MaxValue;
        var all = ShipStatus.Instance.AllConsoles;
        for (var i = 0; i < all.Length; i++)
        {
            var console = all[i];
            if (!RiggerSystem.IsTaskConsole(console) || RiggerSystem.IsRigged(i))
            {
                continue;
            }

            var distance = Vector2.Distance(position, console.transform.position);
            if (distance <= console.UsableDistance && distance < bestDistance)
            {
                best = console;
                bestDistance = distance;
            }
        }

        return best;
    }

    public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            Target.SetOutline(active, active);
        }
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
               RiggerSystem.ActiveRigsOf(PlayerControl.LocalPlayer.PlayerId) <
               (int)OptionGroupSingleton<RiggerOptions>.Instance.MaxActiveRigs;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        var index = RiggerSystem.ConsoleIndex(Target);
        if (index < 0)
        {
            Error("Rig: console not found in ShipStatus.AllConsoles");
            return;
        }

        RiggerRole.RpcRiggerRig(PlayerControl.LocalPlayer, index);
        RiggerSystem.Notify(MiraLocaleManager.Get("TownOfUsMira.Role.RiggerRigged")
            .Replace("<console>", RiggerSystem.ConsoleName(Target)));
    }
}
