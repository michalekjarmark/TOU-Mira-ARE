using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Are;

namespace TownOfUs.Events.Modifiers;

/// <summary>
///     Streamer (AmonRa Edition): a meeting closes every camera screen, so nobody watches any more.
/// </summary>
public static class StreamerEvents
{
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        StreamerSystem.OnMeetingStart();
    }
}
