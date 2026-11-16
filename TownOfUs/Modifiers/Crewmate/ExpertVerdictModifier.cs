namespace TownOfUs.Modifiers.Crewmate;

/// <summary>
///     The Expert's result on a player's name (AmonRa Edition). Added only on the Expert's client, so nobody else sees
///     it. <paramref name="role" /> may be a made-up role. It stays when the player changes role later (the Expert saw
///     what they saw).
/// </summary>
public sealed class ExpertVerdictModifier(RoleBehaviour role) : BaseRevealModifier
{
    public override string ModifierName => "Expert Verdict";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override RoleBehaviour? ShownRole { get; set; } = role;
    public override bool RevealRole { get; set; } = true;
}
