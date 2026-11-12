using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Alliance;

/// <summary>
///     ARE: who can be a pair of <see cref="RivalModifier"/>s. Crewmates only = 0% killer chance and no neutrals.
/// </summary>
public sealed class RivalsOptions : AbstractTouModifierOptionGroup<RivalModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Rivals", "Rivals");
    public override uint GroupPriority => 13;
    public override Color GroupColor => TownOfUsColors.Rival;

    [ModdedNumberOption("TouOptionRivalsKillerProbability", 0, 100, 10f, MiraNumberSuffixes.Percent)]
    public float KillerPercent { get; set; } = 20;

    [ModdedToggleOption("TouOptionRivalsBothKillers")]
    public bool BothKillers { get; set; } = false;

    [ModdedToggleOption("TouOptionRivalsNeutralRoles")]
    public bool NeutralRivals { get; set; } = true;
}
