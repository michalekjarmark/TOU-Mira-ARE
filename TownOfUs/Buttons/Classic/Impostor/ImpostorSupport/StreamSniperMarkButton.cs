using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Stream Sniper's "Mark" button (AmonRa Edition, "marked player" mode): marks a nearby player for "Watch". A normal
///     player-targeting button, so the click counts as an interaction.
/// </summary>
public sealed class StreamSniperMarkButton : TownOfUsRoleButton<StreamSniperRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.StreamSniperMark", "Mark");
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<StreamSniperOptions>.Instance.MarkCooldown.Value + MapCooldown, 0.001f, 120f);

    public override LoadableAsset<Sprite> Sprite => AreAssets.StreamSniperMarkButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) &&
               OptionGroupSingleton<StreamSniperOptions>.Instance.TargetMode == StreamSniperTargetMode.Marked;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && Role != null && target != Role.Marked;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Mark: Target is null");
            return;
        }

        Role.Marked = Target;
        Info($"StreamSniper: marked {Target.Data.PlayerName}");
    }
}
