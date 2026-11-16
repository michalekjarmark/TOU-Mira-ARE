using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Are;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Handyman (AmonRa Edition): a new job at the start of every round (<see cref="HandymanSystem.DrawJob" />), and
///     right away for a player who becomes the Handyman during a round.
/// </summary>
public static class HandymanEvents
{
    // A round is going on (not the intro, a meeting or the time between games).
    private static bool _roundLive;

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        _roundLive = true;
        var local = PlayerControl.LocalPlayer;
        if (local != null && local.Data?.Role is HandymanRole handyman && !local.HasDied())
        {
            HandymanSystem.DrawJob(handyman);
        }
    }

    [RegisterEvent]
    public static void SetRoleEventHandler(SetRoleEvent @event)
    {
        if (_roundLive && @event.Player != null && @event.Player.AmOwner &&
            @event.Player.Data?.Role is HandymanRole handyman && !@event.Player.HasDied())
        {
            HandymanSystem.DrawJob(handyman);
        }
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        _roundLive = false;
    }

    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        _roundLive = false;
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent @event)
    {
        _roundLive = false;
    }
}
