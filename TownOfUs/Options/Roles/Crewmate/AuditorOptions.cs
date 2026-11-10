using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Auditor (AmonRa Edition).
/// </summary>
public sealed class AuditorOptions : AbstractRoleOptionGroup<AuditorRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Auditor", "Auditor");

    [ModdedNumberOption("TouOptionAuditorAuditDuration", 10f, 60f, 5f, MiraNumberSuffixes.Seconds, "0")]
    public float AuditDuration { get; set; } = 25f;

    [ModdedNumberOption("TouOptionAuditorAuditCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AuditCooldown { get; set; } = 30f;

    [ModdedNumberOption("TouOptionAuditorInitialAudits", 1f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float InitialAudits { get; set; } = 1f;

    // 0 = audits are never gained from tasks.
    [ModdedNumberOption("TouOptionAuditorTasksPerAudit", 0f, 10f, 1f, MiraNumberSuffixes.None, "0")]
    public float TasksPerAudit { get; set; } = 3f;

    [ModdedNumberOption("TouOptionAuditorMaxAudits", 1f, 10f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxAudits { get; set; } = 3f;

    // How long the target has to stay at a task station without finishing a task to show up in the report.
    [ModdedNumberOption("TouOptionAuditorIdleStationTime", 1f, 6f, 0.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float IdleStationTime { get; set; } = 2f;

    [ModdedToggleOption("TouOptionAuditorNotifyTarget")]
    public bool NotifyTarget { get; set; } = false;

    // 0 = no public audit.
    [ModdedNumberOption("TouOptionAuditorPublicAudits", 0f, 3f, 1f, MiraNumberSuffixes.None, "0")]
    public float PublicAudits { get; set; } = 1f;

    [ModdedToggleOption("TouOptionAuditorSanction")]
    public bool Sanction { get; set; } = false;
}
