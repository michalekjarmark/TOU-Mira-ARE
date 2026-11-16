using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

/// <summary>
///     Options of the Stream Sniper (AmonRa Edition).
/// </summary>
public sealed class StreamSniperOptions : AbstractRoleOptionGroup<StreamSniperRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.StreamSniper", "Stream Sniper");

    [ModdedNumberOption("TouOptionStreamSniperWatchCooldown", 10f, 90f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float WatchCooldown { get; set; } = 30f;

    [ModdedNumberOption("TouOptionStreamSniperWatchDuration", 3f, 20f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float WatchDuration { get; set; } = 8f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionStreamSniperMaxWatches", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxWatches { get; set; } = 0f;

    // How far the view reaches around the player (camera size).
    [ModdedNumberOption("TouOptionStreamSniperViewSize", 1.5f, 4f, 0.25f, MiraNumberSuffixes.None, "0.00")]
    public float ViewSize { get; set; } = 2.5f;

    [ModdedEnumOption<StreamSniperTargetMode>("TouOptionStreamSniperTargetMode",
        ["TouOptionStreamSniperTargetModeEnumList", "TouOptionStreamSniperTargetModeEnumMarked"])]
    public StreamSniperTargetMode TargetMode { get; set; } = StreamSniperTargetMode.List;

    public ModdedNumberOption MarkCooldown { get; set; } =
        new("TouOptionStreamSniperMarkCooldown", 10f, 2.5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<StreamSniperOptions>.Instance.TargetMode == StreamSniperTargetMode.Marked
        };

    // On: the watched player gets "Someone is watching you".
    [ModdedToggleOption("TouOptionStreamSniperTargetNotified")]
    public bool TargetNotified { get; set; } = false;
}

/// <summary>Who the Stream Sniper can watch.</summary>
public enum StreamSniperTargetMode
{
    // Any living player, chosen from a list.
    List,

    // Only the player marked earlier with the "Mark" button (next to them).
    Marked
}
