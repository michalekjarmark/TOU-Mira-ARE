namespace TownOfUs.Modifiers.Crewmate;

/// <summary>
///     Shows the Auditor's role to everyone after a public audit (AmonRa Edition), like the revealed Mayor.
/// </summary>
public sealed class AuditorRevealModifier(RoleBehaviour role) : BaseRevealModifier
{
    public override string ModifierName => "Revealed Auditor";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override RoleBehaviour? ShownRole { get; set; } = role;
    public override bool RevealRole { get; set; } = true;
}
