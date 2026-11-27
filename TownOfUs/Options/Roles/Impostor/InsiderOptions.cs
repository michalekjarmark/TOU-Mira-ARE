using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

/// <summary>
///     Options of the Insider (AmonRa Edition).
/// </summary>
public sealed class InsiderOptions : AbstractRoleOptionGroup<InsiderRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Insider", "Insider");

    [ModdedNumberOption("TouOptionInsiderReductionPerTask", 0.5f, 5f, 0.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float ReductionPerTask { get; set; } = 1f;

    [ModdedNumberOption("TouOptionInsiderMinKillCooldown", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MinKillCooldown { get; set; } = 10f;

    // Off by default since 2026-11-27 (owner): the crew should win by tasks only when the crew's own tasks are done.
    // Renamed from TasksHelpCrew so that saved configs get the new default too (MiraAPI saves options under the C#
    // property name, see CAVEATS).
    [ModdedToggleOption("TouOptionInsiderTasksHelpCrew")]
    public bool TasksCountForCrew { get; set; } = false;
}
