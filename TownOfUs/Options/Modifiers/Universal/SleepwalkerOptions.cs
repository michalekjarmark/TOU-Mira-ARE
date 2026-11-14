using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

/// <summary>
///     ARE: options of the <see cref="SleepwalkerModifier"/>.
/// </summary>
public sealed class SleepwalkerOptions : AbstractTouModifierOptionGroup<SleepwalkerModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Sleepwalker", "Sleepwalker");
    public override uint GroupPriority => 37;
    public override Color GroupColor => TownOfUsColors.Sleepwalker;

    [ModdedNumberOption("TouOptionSleepwalkerIdleTime", 5f, 60f, 1f, MiraNumberSuffixes.Seconds)]
    public float IdleTime { get; set; } = 20f;

    [ModdedNumberOption("TouOptionSleepwalkerWalkSpeed", 0.25f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float WalkSpeed { get; set; } = 0.6f;

    // 0 = until the player moves.
    [ModdedNumberOption("TouOptionSleepwalkerWalkDuration", 0f, 30f, 1f, MiraNumberSuffixes.Seconds, "0", true)]
    public float WalkDuration { get; set; } = 0f;
}
