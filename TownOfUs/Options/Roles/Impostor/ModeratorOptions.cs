using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

/// <summary>
///     Options of the Moderator (AmonRa Edition).
/// </summary>
public sealed class ModeratorOptions : AbstractRoleOptionGroup<ModeratorRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Moderator", "Moderator");

    [ModdedNumberOption("TouOptionModeratorBanCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BanCooldown { get; set; } = 30f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionModeratorMaxBans", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxBans { get; set; } = 0f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionModeratorBansPerRound", 0f, 5f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float BansPerRound { get; set; } = 1f;

    [ModdedEnumOption<ModeratorBanLength>("TouOptionModeratorBanLength",
        ["TouOptionModeratorBanLengthEnumUntilMeeting", "TouOptionModeratorBanLengthEnumTimed"])]
    public ModeratorBanLength BanLength { get; set; } = ModeratorBanLength.UntilMeeting;

    public ModdedNumberOption BanDuration { get; set; } =
        new("TouOptionModeratorBanDuration", 45f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => OptionGroupSingleton<ModeratorOptions>.Instance.BanLength is ModeratorBanLength.Timed
        };

    // On: one ban blocks every enabled type at once (owner's request, 2026-11-26).
    [ModdedToggleOption("TouOptionModeratorBansEverything")]
    public bool BansEverything { get; set; } = false;

    // Off: the ban type is drawn at random from the enabled ones ("bans for anything").
    public ModdedToggleOption ChoosesType { get; } = new("TouOptionModeratorChoosesType", false)
    {
        Visible = () => !OptionGroupSingleton<ModeratorOptions>.Instance.BansEverything
    };

    // Off: no message, the banned player finds out when something doesn't work.
    [ModdedToggleOption("TouOptionModeratorTargetNotified")]
    public bool TargetNotified { get; set; } = true;

    [ModdedToggleOption("TouOptionModeratorBanReports")]
    public bool BanReports { get; set; } = true;

    [ModdedToggleOption("TouOptionModeratorBanMeetingButton")]
    public bool BanMeetingButton { get; set; } = true;

    [ModdedToggleOption("TouOptionModeratorBanTasks")]
    public bool BanTasks { get; set; } = true;

    [ModdedToggleOption("TouOptionModeratorBanVents")]
    public bool BanVents { get; set; } = true;

    [ModdedToggleOption("TouOptionModeratorBanEquipment")]
    public bool BanEquipment { get; set; } = true;

    [ModdedToggleOption("TouOptionModeratorBanAbilities")]
    public bool BanAbilities { get; set; } = true;

    [ModdedToggleOption("TouOptionModeratorBanVote")]
    public bool BanVote { get; set; } = true;

    public bool IsEnabled(ModeratorBanType type)
    {
        return type switch
        {
            ModeratorBanType.Reports => BanReports,
            ModeratorBanType.MeetingButton => BanMeetingButton,
            ModeratorBanType.Tasks => BanTasks,
            ModeratorBanType.Vents => BanVents,
            ModeratorBanType.Equipment => BanEquipment,
            ModeratorBanType.Abilities => BanAbilities,
            ModeratorBanType.Vote => BanVote,
            _ => false
        };
    }
}

/// <summary>How long a Moderator's ban lasts (a vote ban always lasts until the end of the next meeting).</summary>
public enum ModeratorBanLength
{
    UntilMeeting,
    Timed
}

/// <summary>What a Moderator's ban blocks. Synced as an int, so only append.</summary>
public enum ModeratorBanType
{
    /// <summary>Reporting bodies (report button and a click on a body).</summary>
    Reports,

    /// <summary>The emergency meeting button (also the Button Barry's).</summary>
    MeetingButton,

    /// <summary>Task consoles (sabotage repairs stay allowed).</summary>
    Tasks,

    /// <summary>Getting into a vent (getting out is allowed).</summary>
    Vents,

    /// <summary>Cameras, admin, vitals and door logs, also the portable ones (Operative, Scientist, Spy, Sentry).</summary>
    Equipment,

    /// <summary>All ability buttons, kill and sabotage included (Town of Us' "disabled" state).</summary>
    Abilities,

    /// <summary>The vote in the next meeting doesn't count.</summary>
    Vote
}
