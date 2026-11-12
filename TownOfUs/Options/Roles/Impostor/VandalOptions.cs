using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class VandalOptions : AbstractRoleOptionGroup<VandalRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Vandal", "Vandal");

    [ModdedNumberOption("TouOptionVandalBreakCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BreakCooldown { get; set; } = 30f;

    [ModdedNumberOption("TouOptionVandalMaxBreaks", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxBreaks { get; set; } = 0f;

    /// <summary>Whose task one use breaks when several players finished it at that console.</summary>
    public ModdedEnumOption<VandalTargets> Targets { get; } =
        new("TouOptionVandalTargets", VandalTargets.OneRandom,
            ["TouOptionVandalTargetsOneRandom", "TouOptionVandalTargetsEveryone"]);

    /// <summary>What the owner of the broken task is told.</summary>
    public ModdedEnumOption<VandalOwnerNotice> OwnerNotice { get; } =
        new("TouOptionVandalOwnerNotice", VandalOwnerNotice.WithTaskName,
            ["TouOptionVandalOwnerNoticeWithTaskName", "TouOptionVandalOwnerNoticeWithoutTaskName",
                "TouOptionVandalOwnerNoticeNone"]);
}

public enum VandalTargets
{
    /// <summary>One random player who finished the task.</summary>
    OneRandom,

    /// <summary>Everyone who finished the task.</summary>
    Everyone
}

public enum VandalOwnerNotice
{
    /// <summary>"Someone broke your task: Admin: Swipe Card".</summary>
    WithTaskName,

    /// <summary>"Someone broke one of your tasks!" (which one shows in the task list).</summary>
    WithoutTaskName,

    /// <summary>Nothing: only the task list and the task bar show it.</summary>
    None
}
