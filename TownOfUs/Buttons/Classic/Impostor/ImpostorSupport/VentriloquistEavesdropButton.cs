using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Ventriloquist (AmonRa Edition): eavesdrop on the closest non-impostor to get their voice for the next meeting. A new
///     eavesdrop replaces the old voice. The click goes through MiraButtonClickEvent like every targeted ability.
/// </summary>
public sealed class VentriloquistEavesdropButton : TownOfUsRoleButton<VentriloquistRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistEavesdrop", "Eavesdrop");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<VentriloquistOptions>.Instance.EavesdropCooldown + MapCooldown, 1f, 120f);

    public override int MaxUses => (int)OptionGroupSingleton<VentriloquistOptions>.Instance.MaxEavesdrops;
    public override bool ZeroIsInfinite => true;
    public override LoadableAsset<Sprite> Sprite => AreAssets.VentriloquistButton;

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        VentriloquistRole.RpcVentriloquistEavesdrop(PlayerControl.LocalPlayer, Target);
    }

    public override PlayerControl? GetTarget()
    {
        var voice = (PlayerControl.LocalPlayer.Data.Role as VentriloquistRole)?.VoiceId ?? VentriloquistRole.NoVoice;
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, false,
            player => player.PlayerId != voice);
    }
}
