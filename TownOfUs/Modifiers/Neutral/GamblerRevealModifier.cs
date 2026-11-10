namespace TownOfUs.Modifiers.Neutral;

/// <summary>
///     Shows the Gambler's role on their name (AmonRa Edition), added locally on every client when the Gambler gets close
///     to winning and the "warn others" option is set to reveal. Points are computed the same on every client, so no
///     RPC is needed.
/// </summary>
public sealed class GamblerRevealModifier(RoleBehaviour role) : BaseRevealModifier
{
    public override string ModifierName => "Revealed Gambler";

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override RoleBehaviour? ShownRole { get; set; } = role;
    public override bool RevealRole { get; set; } = true;
}
