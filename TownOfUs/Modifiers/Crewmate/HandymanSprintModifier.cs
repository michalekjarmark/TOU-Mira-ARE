using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Modifiers.Crewmate;

/// <summary>
///     The Handyman's "pizzeria" sprint (AmonRa Edition), added on every client. The speed is applied in
///     <see cref="TownOfUs.Are.HandymanSystem.SprintSpeedPatch" />. Ends early at a meeting.
/// </summary>
public sealed class HandymanSprintModifier : TimedModifier
{
    public override string ModifierName => "Pizza Delivery";
    public override bool AutoStart => true;
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<HandymanOptions>.Instance.PizzaDuration.Value;

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        RemoveSelf();
    }
}
