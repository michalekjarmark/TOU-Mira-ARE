using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

/// <summary>
///     Options of the Glutton (AmonRa Edition, inspired by Project: Lotus's Pelican).
/// </summary>
public sealed class GluttonOptions : AbstractRoleOptionGroup<GluttonRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Glutton", "Glutton");

    [ModdedNumberOption("TouOptionGluttonSwallowCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwallowCooldown { get; set; } = 30f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionGluttonMaxSwallowed", 0f, 5f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxSwallowed { get; set; } = 0f;

    // 0 = the swallowed die when the next meeting is called.
    [ModdedNumberOption("TouOptionGluttonDigestTime", 0f, 60f, 5f, MiraNumberSuffixes.Seconds, "0", zeroInfinity: true)]
    public float DigestTime { get; set; } = 0f;

    // On: the swallowed come out alive when the Glutton dies. Off: they die with it.
    [ModdedToggleOption("TouOptionGluttonReleaseOnDeath")]
    public bool ReleaseOnDeath { get; set; } = true;

    // On: the swallowed see the world around the Glutton (dimmed). Off: a black screen.
    [ModdedToggleOption("TouOptionGluttonSwallowedSeeOutside")]
    public bool SwallowedSeeOutside { get; set; } = true;

    [ModdedToggleOption("TouOptionGluttonCanVent")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("TouOptionGluttonImpostorVision")]
    public bool ImpostorVision { get; set; } = true;
}
