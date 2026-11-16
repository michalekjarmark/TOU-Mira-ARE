using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Universal;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Universal;

/// <summary>
///     ARE: options of the <see cref="StreamerModifier"/>.
/// </summary>
public sealed class StreamerOptions : AbstractTouModifierOptionGroup<StreamerModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Modifier.Streamer", "Streamer");
    public override uint GroupPriority => 38;
    public override Color GroupColor => TownOfUsColors.Streamer;

    // On: an open admin map counts as watching (and "Going offline" hides from it too).
    [ModdedToggleOption("TouOptionStreamerAdminCounts")]
    public bool AdminCounts { get; set; } = false;

    // Off: only "LIVE", without the number of viewers.
    [ModdedToggleOption("TouOptionStreamerShowViewerCount")]
    public bool ShowViewerCount { get; set; } = true;

    [ModdedToggleOption("TouOptionStreamerOfflineButton")]
    public bool OfflineButton { get; set; } = true;

    public ModdedNumberOption OfflineDuration { get; set; } =
        new("TouOptionStreamerOfflineDuration", 10f, 3f, 30f, 1f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => OptionGroupSingleton<StreamerOptions>.Instance.OfflineButton
        };

    public ModdedNumberOption OfflineUses { get; set; } =
        new("TouOptionStreamerOfflineUses", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<StreamerOptions>.Instance.OfflineButton
        };
}
