using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Moderator's "Ban" button (AmonRa Edition). A normal player-targeting button, so the click counts as an interaction
///     (Veteran on alert, Warden, Sixth Sense...).
/// </summary>
public sealed class ModeratorBanButton : TownOfUsRoleButton<ModeratorRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ModeratorBan", "Ban");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<ModeratorOptions>.Instance.BanCooldown + MapCooldown, 5f, 120f);

    public override int MaxUses => (int)OptionGroupSingleton<ModeratorOptions>.Instance.MaxBans;

    // TownOfUsButton defaults to false: without it the "0 = no limit" option would mean no bans at all.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override LoadableAsset<Sprite> Sprite => AreAssets.ModeratorButton;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override bool CanUse()
    {
        var perRound = (int)OptionGroupSingleton<ModeratorOptions>.Instance.BansPerRound;
        return base.CanUse() && (perRound <= 0 || Role.BansThisRound < perRound) &&
               ModeratorRole.EnabledTypes().Count > 0;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Ban: Target is null");
            return;
        }

        var mask = Role.NextBanMask();
        if (mask == 0)
        {
            return;
        }

        // One click is one ban, also when it blocks everything (limits per game and per round).
        var opts = OptionGroupSingleton<ModeratorOptions>.Instance;
        var duration = opts.BanLength is ModeratorBanLength.Timed ? opts.BanDuration.Value : 0f;
        Role.BansThisRound++;
        ModeratorRole.RpcModeratorBan(PlayerControl.LocalPlayer, Target, mask, duration);
    }
}
