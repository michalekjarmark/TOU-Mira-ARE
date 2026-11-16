using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Cryptid (AmonRa Edition).
/// </summary>
public sealed class CryptidOptions : AbstractRoleOptionGroup<CryptidRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Cryptid", "Cryptid");

    [ModdedNumberOption("TouOptionCryptidSightingCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SightingCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionCryptidSightingDuration", 2f, 15f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float SightingDuration { get; set; } = 6f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionCryptidMaxSightings", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxSightings { get; set; } = 3f;

    // A living player closer than this: the figure is gone.
    [ModdedNumberOption("TouOptionCryptidVanishDistance", 0.5f, 4f, 0.25f, MiraNumberSuffixes.None, "0.00")]
    public float VanishDistance { get; set; } = 1.5f;

    // On: an impostor who comes that close scares the Cryptid off for the rest of the game.
    [ModdedToggleOption("TouOptionCryptidImpostorScaresOff")]
    public bool ImpostorScaresOff { get; set; } = true;
}
