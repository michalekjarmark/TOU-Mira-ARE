using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

/// <summary>
///     Options of the Fabulist (AmonRa Edition).
/// </summary>
public sealed class FabulistOptions : AbstractRoleOptionGroup<FabulistRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Fabulist", "Fabulist");

    [ModdedNumberOption("TouOptionFabulistAlibiCooldown", 10f, 90f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AlibiCooldown { get; set; } = 30f;

    [ModdedNumberOption("TouOptionFabulistAlibiDuration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AlibiDuration { get; set; } = 20f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionFabulistMaxAlibis", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxAlibis { get; set; } = 3f;

    // Another living player closer than this to the copy: the story falls apart.
    [ModdedNumberOption("TouOptionFabulistBreakDistance", 0.25f, 3f, 0.25f, MiraNumberSuffixes.None, "0.00")]
    public float BreakDistance { get; set; } = 1f;

    // On: only next to a task console, so the copy looks like it's doing a task.
    [ModdedToggleOption("TouOptionFabulistOnlyAtConsole")]
    public bool OnlyAtConsole { get; set; } = true;

    // On: the admin map counts the copy too (and the real Fabulist where they are).
    [ModdedToggleOption("TouOptionFabulistCopyOnAdmin")]
    public bool CopyOnAdmin { get; set; } = false;
}
