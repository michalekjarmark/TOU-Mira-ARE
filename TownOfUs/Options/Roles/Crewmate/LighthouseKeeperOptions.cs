using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

/// <summary>
///     Options of the Lighthouse Keeper (AmonRa Edition).
/// </summary>
public sealed class LighthouseKeeperOptions : AbstractRoleOptionGroup<LighthouseKeeperRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.LighthouseKeeper", "Lighthouse Keeper");

    [ModdedNumberOption("TouOptionLighthouseKeeperLanternCooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float LanternCooldown { get; set; } = 15f;

    // 0 = no limit.
    [ModdedNumberOption("TouOptionLighthouseKeeperMaxLanterns", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxLanterns { get; set; } = 3f;

    // In map units, measured from the lantern to the player's feet. A player's normal vision is about 3 units.
    [ModdedNumberOption("TouOptionLighthouseKeeperLanternRadius", 1f, 6f, 0.25f, MiraNumberSuffixes.None, "0.00")]
    public float LanternRadius { get; set; } = 2.5f;

    // On: the light doesn't reach through walls (a lantern in Electrical doesn't light the corridor behind the wall).
    [ModdedToggleOption("TouOptionLighthouseKeeperWallsBlockLight")]
    public bool WallsBlockLight { get; set; } = true;

    // Off: only the Lighthouse Keeper and the dead see the lanterns (the light works for everyone either way).
    [ModdedToggleOption("TouOptionLighthouseKeeperLanternsVisible")]
    public bool LanternsVisible { get; set; } = true;

    [ModdedToggleOption("TouOptionLighthouseKeeperLanternsOutOnDeath")]
    public bool LanternsOutOnDeath { get; set; } = false;
}
