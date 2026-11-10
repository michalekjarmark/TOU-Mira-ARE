using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

/// <summary>
///     Options of the Gambler (AmonRa Edition).
/// </summary>
public sealed class GamblerOptions : AbstractRoleOptionGroup<GamblerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Gambler", "Gambler");

    [ModdedNumberOption("TouOptionGamblerPointsToWin", 1f, 6f, 1f, MiraNumberSuffixes.None, "0")]
    public float PointsToWin { get; set; } = 4f;

    [ModdedEnumOption<GamblerBetWindow>("TouOptionGamblerBetWindow",
        ["TouOptionGamblerBetWindowEnumTimer", "TouOptionGamblerBetWindowEnumWholeMeeting"])]
    public GamblerBetWindow BetWindow { get; set; } = GamblerBetWindow.Timer;

    public ModdedNumberOption BetTime { get; set; } =
        new("TouOptionGamblerBetTime", 20f, 5f, 120f, 5f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => OptionGroupSingleton<GamblerOptions>.Instance.BetWindow is not GamblerBetWindow.WholeMeeting
        };

    [ModdedToggleOption("TouOptionGamblerWrongBetLosesPoint")]
    public bool WrongBetLosesPoint { get; set; } = true;

    // Early meetings usually end with a skip, so "no one is ejected" would be a free point. 0 = can't bet on it.
    [ModdedNumberOption("TouOptionGamblerNoEjectionWinsLimit", 0f, 6f, 1f, MiraNumberSuffixes.None, "0")]
    public float NoEjectionWinsLimit { get; set; } = 1f;

    [ModdedToggleOption("TouOptionGamblerWinEndsGame")]
    public bool WinEndsGame { get; set; } = true;

    [ModdedToggleOption("TouOptionGamblerCanButton")]
    public bool CanButton { get; set; } = true;

    [ModdedEnumOption<GamblerWarning>("TouOptionGamblerWarnOthers",
        ["TouOptionGamblerWarnEnumOff", "TouOptionGamblerWarnEnumAlert", "TouOptionGamblerWarnEnumReveal"])]
    public GamblerWarning WarnOthers { get; set; } = GamblerWarning.Alert;

    public ModdedNumberOption WarnAtPointsLeft { get; set; } =
        new("TouOptionGamblerWarnAtPointsLeft", 1f, 1f, 3f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<GamblerOptions>.Instance.WarnOthers is not GamblerWarning.Off
        };
}

/// <summary>
///     How long the Gambler can bet in a meeting. Without a limit they could wait until everyone agreed on a vote.
/// </summary>
public enum GamblerBetWindow
{
    /// <summary>The first BetTime seconds of the voting (the vote popups only open in the voting phase).</summary>
    Timer,

    /// <summary>The whole voting, until the votes are revealed (no limit).</summary>
    WholeMeeting
}

/// <summary>
///     What the other players learn when the Gambler is close to winning.
/// </summary>
public enum GamblerWarning
{
    /// <summary>Nothing, the win can come as a surprise.</summary>
    Off,

    /// <summary>Screen flash and a message that a Gambler is close to winning (without the name).</summary>
    Alert,

    /// <summary>Like Alert, plus the Gambler's role is shown on their name to everyone.</summary>
    Reveal
}
