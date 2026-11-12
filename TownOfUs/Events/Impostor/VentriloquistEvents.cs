using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Roles;
using TownOfUs.Are;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

/// <summary>
///     Ventriloquist (AmonRa Edition): registers the chat channel, moves a stolen vote when the host counts the votes and
///     drops the voices when a round starts, so a voice works for one meeting only. The state is synced, so this runs the
///     same on every client.
/// </summary>
public static class VentriloquistEvents
{
    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        VentriloquistChat.Register();
    }

    // First of all vote handlers: before the Auditor's freeze (-10, a frozen vote stays frozen even when stolen) and
    // before everything that works out the ejection from the votes (Tiebreaker, hidden Knighted votes, Swapper).
    [RegisterEvent(-20)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        VentriloquistVote.ApplySteals(@event.Votes);
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        foreach (var role in CustomRoleUtils.GetActiveRolesOfType<VentriloquistRole>())
        {
            role.VoiceId = VentriloquistRole.NoVoice;
            role.MessagesSent = 0;
            role.StolenVoteFor = VentriloquistVote.NoSteal;
        }
    }
}
