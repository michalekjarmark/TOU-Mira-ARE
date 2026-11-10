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
    public float PointsToWin { get; set; } = 3f;

    [ModdedToggleOption("TouOptionGamblerWrongBetLosesPoint")]
    public bool WrongBetLosesPoint { get; set; } = true;

    [ModdedToggleOption("TouOptionGamblerCanBetOnNoEjection")]
    public bool CanBetOnNoEjection { get; set; } = true;

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
