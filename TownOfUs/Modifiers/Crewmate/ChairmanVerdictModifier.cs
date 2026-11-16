using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Crewmate;

/// <summary>
///     The Chairman's bonus (<paramref name="bonus" /> = true: the vote counts twice) or reprimand (false: the vote
///     doesn't count) for the next meeting (AmonRa Edition). Added on every client by
///     <c>ChairmanRole.RpcChairmanOrderResult</c>, used by the host in <c>ChairmanSystem.ApplyVotes</c> and removed when
///     the meeting ends.
/// </summary>
public sealed class ChairmanVerdictModifier(bool bonus) : BaseModifier
{
    public override string ModifierName => Bonus ? "Chairman Bonus" : "Chairman Reprimand";
    public override bool HideOnUi => true;

    public bool Bonus { get; } = bonus;

    public override string GetDescription()
    {
        return Bonus ? "Your vote counts twice." : "Your vote doesn't count.";
    }
}
