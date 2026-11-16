using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Handyman (AmonRa Edition).
/// </summary>
public sealed class HandymanOptions : AbstractRoleOptionGroup<HandymanRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Handyman", "Handyman");

    [ModdedNumberOption("TouOptionHandymanUsesPerRound", 1f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float UsesPerRound { get; set; } = 1f;

    // Only matters with more than one use per round.
    [ModdedNumberOption("TouOptionHandymanCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float JobCooldown { get; set; } = 15f;

    [ModdedToggleOption("TouOptionHandymanSameJobTwice")]
    public bool SameJobTwice { get; set; } = true;

    [ModdedToggleOption("TouOptionHandymanServiceJob")]
    public bool ServiceJob { get; set; } = true;

    // Like the Engineer's fix delay.
    public ModdedNumberOption ServiceDelay { get; set; } =
        new("TouOptionHandymanServiceDelay", 0.5f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<HandymanOptions>.Instance.ServiceJob
        };

    [ModdedToggleOption("TouOptionHandymanPizzaJob")]
    public bool PizzaJob { get; set; } = true;

    public ModdedNumberOption PizzaDuration { get; set; } =
        new("TouOptionHandymanPizzaDuration", 8f, 2f, 30f, 1f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => OptionGroupSingleton<HandymanOptions>.Instance.PizzaJob
        };

    public ModdedNumberOption PizzaSpeed { get; set; } =
        new("TouOptionHandymanPizzaSpeed", 1.5f, 1.1f, 2.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")
        {
            Visible = () => OptionGroupSingleton<HandymanOptions>.Instance.PizzaJob
        };

    [ModdedToggleOption("TouOptionHandymanWarehouseJob")]
    public bool WarehouseJob { get; set; } = true;

    public ModdedNumberOption WarehouseDuration { get; set; } =
        new("TouOptionHandymanWarehouseDuration", 10f, 3f, 30f, 1f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => OptionGroupSingleton<HandymanOptions>.Instance.WarehouseJob
        };

    public ModdedToggleOption MoveWithAdmin { get; set; } =
        new("TouOptionHandymanMoveWithAdmin", false)
        {
            Visible = () => OptionGroupSingleton<HandymanOptions>.Instance.WarehouseJob
        };

    // Never drawn on maps without cameras (MIRA HQ).
    [ModdedToggleOption("TouOptionHandymanStreamJob")]
    public bool StreamJob { get; set; } = true;

    public ModdedNumberOption StreamDuration { get; set; } =
        new("TouOptionHandymanStreamDuration", 10f, 3f, 30f, 1f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => OptionGroupSingleton<HandymanOptions>.Instance.StreamJob
        };
}

/// <summary>The Handyman's job for a round (synced as nothing: only the Handyman's client knows it).</summary>
public enum HandymanJob
{
    None,
    Service,
    Pizza,
    Warehouse,
    Stream
}
