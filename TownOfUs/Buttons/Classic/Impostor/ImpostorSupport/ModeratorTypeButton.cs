using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

/// <summary>
///     Moderator's second button (AmonRa Edition), only with the "Moderator chooses the ban" option: switches to the next
///     enabled ban type, shown as the button's name. Not an ability use (no Aurial).
/// </summary>
public sealed class ModeratorTypeButton : TownOfUsRoleButton<ModeratorRole>, IAurialBypassButton
{
    private ModeratorBanType? _shown;

    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ModeratorType", "Ban Type");
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => AreAssets.ModeratorTypeButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && OptionGroupSingleton<ModeratorOptions>.Instance.ChoosesType;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (Role == null)
        {
            return;
        }

        // Shows the type the next ban gets (an option change in Freeplay may have switched the chosen one off).
        var enabled = ModeratorRole.EnabledTypes();
        if (enabled.Count > 0 && !enabled.Contains(Role.SelectedType))
        {
            Role.SelectedType = enabled[0];
        }

        if (_shown != Role.SelectedType)
        {
            _shown = Role.SelectedType;
            OverrideName(ModeratorSystem.BanShortName(Role.SelectedType));
        }
    }

    protected override void OnClick()
    {
        Role.CycleSelectedType();
    }
}
