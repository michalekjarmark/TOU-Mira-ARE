using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

/// <summary>
///     ARE: options of the <see cref="ColorblindModifier"/>.
/// </summary>
public sealed class ColorblindOptions : AbstractTouModifierOptionGroup<ColorblindModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Colorblind", "Colorblind");
    public override uint GroupPriority => 36;
    public override Color GroupColor => TownOfUsColors.Colorblind;

    [ModdedNumberOption("TouOptionColorblindRange", 0.5f, 5f, 0.25f, MiraNumberSuffixes.None, "0.00")]
    public float Range { get; set; } = 1.75f;

    [ModdedToggleOption("TouOptionColorblindHideCosmetics")]
    public bool HideCosmetics { get; set; } = true;
}
