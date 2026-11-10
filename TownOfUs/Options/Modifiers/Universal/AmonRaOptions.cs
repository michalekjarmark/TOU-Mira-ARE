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
    ///     How quickly sideways sliding is cancelled when turning. Lower = more drifting.
    /// </summary>
    [ModdedNumberOption("TouOptionAmonRaTurnGrip", 0.25f, 3f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float TurnGrip { get; set; } = 1f;

    [ModdedToggleOption("TouOptionAmonRaShowWheelchair")]
    public bool ShowWheelchair { get; set; } = true;

    [ModdedToggleOption("TouOptionAmonRaHideInCamouflage")]
    public bool HideInCamouflage { get; set; } = true;
}
