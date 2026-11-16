using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Are;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Cryptid (AmonRa Edition): a meeting ends every sighting.
/// </summary>
public static class CryptidEvents
{
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        CryptidSystem.RemoveAll();
    }
}
