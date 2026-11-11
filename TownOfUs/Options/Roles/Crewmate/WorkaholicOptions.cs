using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Workaholic (AmonRa Edition).
/// </summary>
public sealed class WorkaholicOptions : AbstractRoleOptionGroup<WorkaholicRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Workaholic", "Workaholic");

    // Counted from finishing a taken task.
    [ModdedNumberOption("TouOptionWorkaholicTakeCooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TakeCooldown { get; set; } = 10f;

    // 0 = no limit (owner's decision: no limit by default).
    [ModdedNumberOption("TouOptionWorkaholicMaxTakenTasks", 0f, 20f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxTakenTasks { get; set; } = 0f;

    // Owner's decision: the crewmate always learns that someone helps them, by default without the name.
    [ModdedToggleOption("TouOptionWorkaholicShowHelperName")]
    public bool ShowHelperName { get; set; } = false;
}
