using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

/// <summary>
///     A colour an Informant showed to the local player (AmonRa Edition): red or green on the marked player's name.
///     Added only on the told player's client (nobody else sees it), hidden until the next meeting starts
///     (<c>InformantEvents</c>), then kept until the end of the game, also when someone dies or the marked player
///     changes role (the colour was taken when the player was marked).
/// </summary>
public sealed class InformantRevealModifier(bool isEvil) : BaseRevealModifier
{
    public override string ModifierName => "Informant Reveal";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    // Own backing field, so it starts as false (hidden), unlike the base class.
    public override bool Visible { get; set; }

    public override Color? NameColor { get; set; } = isEvil ? Color.red : Color.green;

    /// <summary>The colour of a later show of the same player replaces this one (one modifier per player).</summary>
    public void SetColor(bool isEvil)
    {
        NameColor = isEvil ? Color.red : Color.green;
    }
}
