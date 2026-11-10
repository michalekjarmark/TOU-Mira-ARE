using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

public sealed class AmonRaOptions : AbstractTouModifierOptionGroup<AmonRaModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.AmonRa", "AmonRa");
    public override uint GroupPriority => 30;
    public override Color GroupColor => TownOfUsColors.AmonRa;

    /// <summary>
    ///     Speed reached after accelerating in a straight line, relative to the player's normal speed.
    /// </summary>
    [ModdedNumberOption("TouOptionAmonRaTopSpeed", 1f, 2f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float TopSpeed { get; set; } = 1.3f;

    /// <summary>
    ///     Seconds needed to go from standing still to top speed.
    /// </summary>
    [ModdedNumberOption("TouOptionAmonRaAccelerationTime", 0.25f, 4f, 0.25f, MiraNumberSuffixes.Seconds, "0.00")]
    public float AccelerationTime { get; set; } = 1.25f;

    /// <summary>
    ///     Seconds needed to roll to a stop from normal speed after letting go of the controls.
    /// </summary>
    [ModdedNumberOption("TouOptionAmonRaBrakingTime", 0.25f, 4f, 0.25f, MiraNumberSuffixes.Seconds, "0.00")]
    public float BrakingTime { get; set; } = 1f;

    /// <summary>
    ///     How fast the wheelchair can turn at top speed, in degrees per second. When slow it turns much faster
    ///     (up to <see cref="SlowTurnSpeed"/>), so tight corners need slowing down: steering well keeps the speed.
    /// </summary>
    [ModdedNumberOption("TouOptionAmonRaTurnSpeed", 60f, 360f, 15f, MiraNumberSuffixes.None, "0")]
    public float TurnSpeed { get; set; } = 180f;

    [ModdedToggleOption("TouOptionAmonRaShowWheelchair")]
    public bool ShowWheelchair { get; set; } = true;

    [ModdedToggleOption("TouOptionAmonRaHideInCamouflage")]
    public bool HideInCamouflage { get; set; } = true;

    [ModdedToggleOption("TouOptionAmonRaRollingSound")]
    public bool RollingSound { get; set; } = true;

    /// <summary>
    ///     Whether nearby players hear someone else's wheelchair (only while they can see it).
    /// </summary>
    [ModdedToggleOption("TouOptionAmonRaOthersHearRolling")]
    public bool OthersHearRolling { get; set; } = false;

    /// <summary>
    ///     Turn rate when (almost) standing still, degrees per second.
    /// </summary>
    public const float SlowTurnSpeed = 720f;
}
