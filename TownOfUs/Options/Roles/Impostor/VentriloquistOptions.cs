using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class VentriloquistOptions : AbstractRoleOptionGroup<VentriloquistRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Ventriloquist", "Ventriloquist");

    [ModdedNumberOption("TouOptionVentriloquistEavesdropCooldown", 1f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float EavesdropCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionVentriloquistMaxEavesdrops", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxEavesdrops { get; set; } = 0f;

    [ModdedNumberOption("TouOptionVentriloquistMessagesPerMeeting", 1f, 5f)]
    public float MessagesPerMeeting { get; set; } = 1f;
}
