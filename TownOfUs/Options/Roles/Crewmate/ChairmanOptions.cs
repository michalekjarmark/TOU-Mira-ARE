using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Chairman (AmonRa Edition).
/// </summary>
public sealed class ChairmanOptions : AbstractRoleOptionGroup<ChairmanRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Chairman", "Chairman");

    [ModdedNumberOption("TouOptionChairmanOrderCooldown", 10f, 90f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float OrderCooldown { get; set; } = 30f;

    // How long the player has to finish a task.
    [ModdedNumberOption("TouOptionChairmanOrderTime", 10f, 90f, 5f, MiraNumberSuffixes.Seconds, "0")]
    public float OrderTime { get; set; } = 30f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionChairmanMaxOrders", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxOrders { get; set; } = 0f;

    // A done order finishes one of the Chairman's own tasks (taking credit for the work).
    [ModdedToggleOption("TouOptionChairmanFreeTask")]
    public bool FreeTask { get; set; } = true;

    [ModdedToggleOption("TouOptionChairmanTargetSeesName")]
    public bool TargetSeesName { get; set; } = false;

    // Done order: the player's vote counts twice in the next meeting.
    [ModdedToggleOption("TouOptionChairmanBonus")]
    public bool Bonus { get; set; } = true;

    // Order not done in time: the player's vote doesn't count in the next meeting.
    [ModdedToggleOption("TouOptionChairmanReprimand")]
    public bool Reprimand { get; set; } = true;

    // On: everyone gets a message at each promotion (and so knows who the Chairman is).
    [ModdedToggleOption("TouOptionChairmanPromotionsPublic")]
    public bool PromotionsPublic { get; set; } = false;

    [ModdedToggleOption("TouOptionChairmanDirector")]
    public bool Director { get; set; } = true;

    public ModdedNumberOption DirectorAt { get; set; } =
        new("TouOptionChairmanDirectorAt", 2f, 1f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<ChairmanOptions>.Instance.Director
        };

    [ModdedToggleOption("TouOptionChairmanPresident")]
    public bool President { get; set; } = true;

    public ModdedNumberOption PresidentAt { get; set; } =
        new("TouOptionChairmanPresidentAt", 4f, 1f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<ChairmanOptions>.Instance.President
        };

    [ModdedToggleOption("TouOptionChairmanKing")]
    public bool King { get; set; } = true;

    public ModdedNumberOption KingAt { get; set; } =
        new("TouOptionChairmanKingAt", 6f, 1f, 10f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<ChairmanOptions>.Instance.King
        };
}

/// <summary>Result of an order, synced as an int (only append).</summary>
public enum ChairmanOrderResult
{
    Done,
    Slacked,
    NoTasks
}

/// <summary>The Chairman's titles. Each one keeps the perks of the lower ones.</summary>
public enum ChairmanRank
{
    /// <summary>Start: orders only.</summary>
    Manager,

    /// <summary>Calls a meeting from anywhere, once per game.</summary>
    Director,

    /// <summary>Sees who voted for whom.</summary>
    President,

    /// <summary>Their vote counts twice.</summary>
    King
}
