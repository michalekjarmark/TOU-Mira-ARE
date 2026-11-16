using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Expert (AmonRa Edition).
/// </summary>
public sealed class ExpertOptions : AbstractRoleOptionGroup<ExpertRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Expert", "Expert");

    [ModdedNumberOption("TouOptionExpertExamineCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ExamineCooldown { get; set; } = 25f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionExpertMaxExaminations", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxExaminations { get; set; } = 3f;

    [ModdedNumberOption("TouOptionExpertWrongChance", 0f, 100f, 5f, MiraNumberSuffixes.Percent, "0")]
    public float WrongChance { get; set; } = 30f;

    // Added to the wrong result chance after every examination ("the surer of himself, the worse").
    [ModdedNumberOption("TouOptionExpertWrongChanceGrowth", 0f, 25f, 5f, MiraNumberSuffixes.Percent, "0")]
    public float WrongChanceGrowth { get; set; } = 0f;

    [ModdedEnumOption<ExpertFakeRoleSource>("TouOptionExpertFakeRoleSource",
        ["TouOptionExpertFakeEnumRolesInGame", "TouOptionExpertFakeEnumAnyRole", "TouOptionExpertFakeEnumOtherFaction"])]
    public ExpertFakeRoleSource FakeRoleSource { get; set; } = ExpertFakeRoleSource.RolesInGame;

    // Off: an examined player can't be examined again, the first result stays ("an expert doesn't change his mind").
    // On: examining them again rolls a new result, which replaces the old one.
    [ModdedToggleOption("TouOptionExpertCanReexamine")]
    public bool CanReexamine { get; set; } = false;
}

/// <summary>
///     Where a wrong result of the Expert comes from. It is never the player's real role.
/// </summary>
public enum ExpertFakeRoleSource
{
    /// <summary>A role that can spawn in this game (lobby settings), or plain Crewmate / Impostor.</summary>
    RolesInGame,

    /// <summary>Any role of the mod, also ones turned off in the lobby.</summary>
    AnyRole,

    /// <summary>A role of another faction than the player's real one (from this game if possible).</summary>
    OtherFaction
}
