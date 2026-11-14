using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

/// <summary>
///     ARE: options of the <see cref="BleederModifier"/>.
/// </summary>
public sealed class BleederOptions : AbstractTouModifierOptionGroup<BleederModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Bleeder", "Bleeder");
    public override uint GroupPriority => 22;
    public override Color GroupColor => TownOfUsColors.Bleeder;

    [ModdedNumberOption("TouOptionBleederTrailDuration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TrailDuration { get; set; } = 15f;

    [ModdedNumberOption("TouOptionBleederPrintDuration", 2f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float PrintDuration { get; set; } = 10f;

    [ModdedToggleOption("TouOptionBleederVentEndsTrail")]
    public bool VentEndsTrail { get; set; } = true;

    [ModdedToggleOption("TouOptionBleederNotifyKiller")]
    public bool NotifyKiller { get; set; } = true;
}
