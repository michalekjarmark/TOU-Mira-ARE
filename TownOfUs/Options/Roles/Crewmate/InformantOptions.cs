using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Informant (AmonRa Edition). The colour switches work like the Seer's (non-Salem) ones and have the
///     same defaults, but are a separate copy, so the Seer's settings don't change the Informant.
/// </summary>
public sealed class InformantOptions : AbstractRoleOptionGroup<InformantRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Informant", "Donosiciel");

    [ModdedNumberOption("TouOptionInformantMarkCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MarkCooldown { get; set; } = 15f;

    [ModdedNumberOption("TouOptionInformantShowCooldown", 10f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float ShowCooldown { get; set; } = 60f;

    [ModdedNumberOption("TouOptionInformantUses", 1f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxShows { get; set; } = 3f;

    [ModdedToggleOption("TouOptionInformantFirstRoundUse")]
    public bool FirstRoundUse { get; set; } = true;

    [ModdedToggleOption("TouOptionInformantCrewmateKillingRed")]
    public bool ShowCrewmateKillingAsRed { get; set; } = false;

    [ModdedToggleOption("TouOptionInformantNeutralBenignRed")]
    public bool ShowNeutralBenignAsRed { get; set; } = false;

    [ModdedToggleOption("TouOptionInformantNeutralEvilRed")]
    public bool ShowNeutralEvilAsRed { get; set; } = false;

    [ModdedToggleOption("TouOptionInformantNeutralKillingRed")]
    public bool ShowNeutralKillingAsRed { get; set; } = true;

    [ModdedToggleOption("TouOptionInformantNeutralOutlierRed")]
    public bool ShowNeutralOutlierAsRed { get; set; } = false;

    [ModdedToggleOption("TouOptionInformantTraitorSwapsColors")]
    public bool SwapTraitorColors { get; set; } = true;
}
