using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Informant's only button (AmonRa Edition), with two modes (a toggle like the Swooper's):
///     "Mark" (no mark yet): marks a player, costs no use, then the mark cooldown runs.
///     "Show" (a player is marked): shows the marked player's colour to someone else, costs one use, clears the mark,
///     then the show cooldown runs. A normal player-targeting button on purpose, so both clicks go through
///     MiraButtonClickEvent (Veteran on alert, shields...) like every other interaction.
/// </summary>
public sealed class InformantButton : TownOfUsRoleButton<InformantRole, PlayerControl>
{
    private bool _showMode;

    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.InformantMark", "Mark");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Informant;

    // Read by the click handler after OnClick, when the mode has already switched: Mark -> Show takes the mark
    // cooldown, Show -> Mark the show cooldown. After a meeting the cooldown of the current mode is used.
    public override float Cooldown
    {
        get
        {
            var options = OptionGroupSingleton<InformantOptions>.Instance;
            var cooldown = Role is { HasMark: true } ? options.MarkCooldown : options.ShowCooldown;
            return Math.Clamp(cooldown + MapCooldown, 5f, 120f);
        }
    }

    public override int MaxUses => (int)OptionGroupSingleton<InformantOptions>.Instance.MaxShows;
    public override bool UsableFirstRound => OptionGroupSingleton<InformantOptions>.Instance.FirstRoundUse;
    public override LoadableAsset<Sprite> Sprite => AreAssets.InformantMarkButton;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        _showMode = false;
    }

    public override PlayerControl? GetTarget()
    {
        // In the "Show" mode the marked player can't be the one who is told.
        var marked = Role is { HasMark: true } role ? role.MarkedPlayerId : InformantRole.NoMark;
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => x.PlayerId != marked);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Informant: Target is null");
            return;
        }

        if (Role.HasMark)
        {
            // The use was already taken by the click handler.
            Role.Show(Target);
        }
        else
        {
            // Marking is free: give back the use the click handler took.
            SetUses(UsesLeft + 1);
            Role.Mark(Target);
        }

        UpdateMode();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        UpdateMode();
    }

    private void UpdateMode()
    {
        if (Button == null || PlayerControl.LocalPlayer.Data.Role is not InformantRole role ||
            _showMode == role.HasMark)
        {
            return;
        }

        _showMode = role.HasMark;
        OverrideName(_showMode
            ? MiraLocaleManager.Get("TownOfUsMira.Role.InformantShow", "Show")
            : MiraLocaleManager.Get("TownOfUsMira.Role.InformantMark", "Mark"));
        OverrideSprite((_showMode ? AreAssets.InformantShowButton : AreAssets.InformantMarkButton).LoadAsset());
    }
}
