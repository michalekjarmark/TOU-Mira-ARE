using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

/// <summary>
///     Options of the Rigger (AmonRa Edition).
/// </summary>
public sealed class RiggerOptions : AbstractRoleOptionGroup<RiggerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Rigger", "Rigger");

    [ModdedNumberOption("TouOptionRiggerRigCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RigCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionRiggerMaxActiveRigs", 1f, 4f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxActiveRigs { get; set; } = 2f;

    [ModdedToggleOption("TouOptionRiggerAlertAllImpostors")]
    public bool AlertAllImpostors { get; set; } = true;

    [ModdedNumberOption("TouOptionRiggerArrowDuration", 2f, 15f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float ArrowDuration { get; set; } = 6f;

    [ModdedToggleOption("TouOptionRiggerJamConsole")]
    public bool JamConsole { get; set; } = true;

    public ModdedNumberOption JamDelay { get; set; } =
        new("TouOptionRiggerJamDelay", 1.5f, 0.5f, 5f, 0.5f, MiraNumberSuffixes.Seconds, "0.0")
        {
            Visible = () => OptionGroupSingleton<RiggerOptions>.Instance.JamConsole
        };

    public ModdedNumberOption JamLockout { get; set; } =
        new("TouOptionRiggerJamLockout", 5f, 0f, 15f, 1f, MiraNumberSuffixes.Seconds, "0")
        {
            Visible = () => OptionGroupSingleton<RiggerOptions>.Instance.JamConsole
        };

    [ModdedToggleOption("TouOptionRiggerSparksVisible")]
    public bool SparksVisible { get; set; } = true;
}
