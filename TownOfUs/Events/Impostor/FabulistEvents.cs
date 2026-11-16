using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Are;

namespace TownOfUs.Events.Impostor;

/// <summary>
///     Fabulist (AmonRa Edition): a meeting ends every alibi, like a fake body disappears.
/// </summary>
public static class FabulistEvents
{
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        FabulistSystem.RemoveAll();
    }
}
