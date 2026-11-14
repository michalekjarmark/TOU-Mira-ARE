using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Comfy (AmonRa Edition). Every number and rule is an option (owner's request: the group tunes the
///     balance in game).
/// </summary>
public sealed class ComfyOptions : AbstractRoleOptionGroup<ComfyRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Comfy", "Comfy");

    // How long a task closed before finishing stays locked for the Comfy. 0 = no lock (and nothing to offload).
    [ModdedNumberOption("TouOptionComfyLockDuration", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float LockDuration { get; set; } = 20f;

    // 0 = no sulk at all.
    [ModdedNumberOption("TouOptionComfySulkDuration", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float SulkDuration { get; set; } = 5f;

    // Counted from the end of a sulk.
    [ModdedNumberOption("TouOptionComfySulkCooldown", 0f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SulkCooldown { get; set; } = 30f;

    [ModdedToggleOption("TouOptionComfySulkBlocksKills")]
    public bool SulkBlocksKills { get; set; } = true;

    [ModdedToggleOption("TouOptionComfySulkBlocksAbilities")]
    public bool SulkBlocksAbilities { get; set; } = true;

    [ModdedToggleOption("TouOptionComfySulkBlocksTasks")]
    public bool SulkBlocksTasks { get; set; } = true;

    [ModdedToggleOption("TouOptionComfySulkBlocksReports")]
    public bool SulkBlocksReports { get; set; } = true;

    [ModdedToggleOption("TouOptionComfySulkBlocksMeetingButton")]
    public bool SulkBlocksMeetingButton { get; set; } = false;

    // Others see the "!" after the Comfy's name while they sulk (the Comfy always sees it, the dead with The Dead Know).
    [ModdedToggleOption("TouOptionComfySulkVisible")]
    public bool SulkVisible { get; set; } = false;

    // The attacker always sees a flash when the sulk stops them (like the Cleric's barrier); this is the Comfy's flash.
    [ModdedToggleOption("TouOptionComfyAttackNotif")]
    public bool AttackNotif { get; set; } = true;

    [ModdedNumberOption("TouOptionComfyOffloadCooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float OffloadCooldown { get; set; } = 10f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionComfyMaxOffloads", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxOffloads { get; set; } = 2f;

    // Owner's decision: an offloaded task that isn't done comes back at the start of the next meeting, so an impostor
    // can't hold the crew's task win. Off = it stays with the receiver.
    [ModdedToggleOption("TouOptionComfyTaskReturns")]
    public bool TaskReturns { get; set; } = true;

    // Like the Workaholic: the receiver always learns that someone gave them a task, by default without the name.
    [ModdedToggleOption("TouOptionComfyReceiverSeesName")]
    public bool ReceiverSeesName { get; set; } = false;
}
