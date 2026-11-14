using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Modifiers.Impostor;

/// <summary>
///     ARE: a player who tried to report a playing-dead Possum (on every client). For a few seconds they can't move
///     (<see cref="Are.PossumSystem" />), use abilities, consoles or the map, or report. They can still be killed.
/// </summary>
public sealed class PossumStunModifier : DisabledModifier
{
    public override string ModifierName => "Stunned";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public override float Duration => OptionGroupSingleton<PossumOptions>.Instance.StunDuration;
    public override bool CanUseAbilities => false;
    public override bool CanUseConsoles => false;
    public override bool CanOpenMap => false;
    public override bool CanReport => false;

    public override void OnMeetingStart()
    {
        RemoveSelf();
    }
}
