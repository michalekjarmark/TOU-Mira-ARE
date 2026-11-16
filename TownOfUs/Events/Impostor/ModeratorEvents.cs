using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

/// <summary>
///     Moderator (AmonRa Edition): blocks for banned players (<see cref="ModeratorSystem" />), the vote ban and the per
///     round limit.
/// </summary>
public static class ModeratorEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is ModeratorRole moderator)
        {
            moderator.BansThisRound = 0;
        }
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (ModeratorSystem.BlocksUsable(@event.Usable))
        {
            @event.Cancel();
        }
    }

    // Same place as the Auditor's frozen vote: before everything that works out the ejection from the votes.
    [RegisterEvent(-10)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (ModeratorSystem.IsBanned(player, ModeratorBanType.Vote))
            {
                var id = player.PlayerId;
                @event.Votes.RemoveAll(v => v.Voter == id);
            }
        }
    }

    // The meeting is over: vote bans end (the other bans ended when it started).
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            foreach (var ban in player.GetModifiers<ModeratorBanModifier>().ToList())
            {
                player.RemoveModifier(ban);
            }
        }
    }
}
