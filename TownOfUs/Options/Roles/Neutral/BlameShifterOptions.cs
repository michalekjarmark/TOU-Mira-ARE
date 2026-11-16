using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

/// <summary>
///     Options of the Blame Shifter (AmonRa Edition).
/// </summary>
public sealed class BlameShifterOptions : AbstractRoleOptionGroup<BlameShifterRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifter", "Blame Shifter");

    [ModdedNumberOption("TouOptionBlameShifterPossessCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PossessCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionBlameShifterPossessDuration", 2f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float PossessDuration { get; set; } = 4f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionBlameShifterMaxPossessions", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxPossessions { get; set; } = 3f;

    // How close the ghost has to be to the blamed player to possess them.
    [ModdedNumberOption("TouOptionBlameShifterPossessRange", 1f, 6f, 0.5f, MiraNumberSuffixes.None, "0.0")]
    public float PossessRange { get; set; } = 3f;

    // On: the possessed player sees a flash and a message, so they can defend themselves.
    [ModdedToggleOption("TouOptionBlameShifterVictimNotified")]
    public bool VictimNotified { get; set; } = true;

    [ModdedEnumOption<BlameShifterWinMode>("TouOptionBlameShifterWinMode",
        ["TouOptionBlameShifterWinModeEnumEjected", "TouOptionBlameShifterWinModeEnumVotes"])]
    public BlameShifterWinMode WinMode { get; set; } = BlameShifterWinMode.Ejected;

    public ModdedNumberOption VotesNeeded { get; set; } =
        new("TouOptionBlameShifterVotesNeeded", 8f, 3f, 20f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<BlameShifterOptions>.Instance.WinMode == BlameShifterWinMode.Votes
        };

    // Off: the Blame Shifter wins together with whoever wins the game.
    [ModdedToggleOption("TouOptionBlameShifterWinEndsGame")]
    public bool WinEndsGame { get; set; } = false;
}

/// <summary>What the blamed player has to suffer for the Blame Shifter to win.</summary>
public enum BlameShifterWinMode
{
    // Get ejected in a vote.
    Ejected,

    // Collect enough votes over all meetings.
    Votes
}
