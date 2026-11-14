using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class PossumOptions : AbstractRoleOptionGroup<PossumRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Possum", "Possum");

    [ModdedNumberOption("TouOptionPossumPlayDeadCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PlayDeadCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionPossumPlayDeadDuration", 5f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float PlayDeadDuration { get; set; } = 30f;

    [ModdedNumberOption("TouOptionPossumMaxUses", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxUses { get; set; } = 0f;

    [ModdedNumberOption("TouOptionPossumStunDuration", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float StunDuration { get; set; } = 3f;
}
