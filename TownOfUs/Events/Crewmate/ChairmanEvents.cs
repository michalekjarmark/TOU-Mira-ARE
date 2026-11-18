using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers.Crewmate;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Chairman (AmonRa Edition): bonus and reprimand votes, the King's double vote.
/// </summary>
public static class ChairmanEvents
{
    // After the Ventriloquist's steal (-20) and the frozen/banned votes (-10), before the Tiebreaker, hidden Knighted
    // votes and the Swapper (0 and up).
    [RegisterEvent(-8)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        ChairmanSystem.ApplyVotes(@event.Votes);
    }

    // The meeting is over: bonuses and reprimands end, a Board Meeting gives the emergency meeting back.
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        ChairmanSystem.RefundBoardMeeting();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            foreach (var verdict in player.GetModifiers<ChairmanVerdictModifier>().ToList())
            {
                player.RemoveModifier(verdict);
            }
        }
    }
}
