namespace TownOfUs.Modifiers.Neutral;

/// <summary>
///     Possessed by a Blame Shifter (AmonRa Edition), added on every client: no abilities, consoles, map or reports,
///     and on the player's own client <see cref="TownOfUs.Are.BlameShifterSystem" /> walks them towards the ghost. Ends
///     after its time, at a meeting or at death.
/// </summary>
public sealed class BlameShifterPossessedModifier(byte shifterId, float duration) : DisabledModifier
{
    public override string ModifierName => "Possessed";
    public override float Duration => duration;
    public override bool AutoStart => true;

    public byte ShifterId { get; } = shifterId;

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        RemoveSelf();
    }
}
