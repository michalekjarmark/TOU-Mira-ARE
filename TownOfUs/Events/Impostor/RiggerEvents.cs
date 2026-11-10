using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Are;

namespace TownOfUs.Events.Impostor;

/// <summary>
///     Rigger (AmonRa Edition): rigs last until the next meeting, and nothing is carried over between games.
/// </summary>
public static class RiggerEvents
{
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        RiggerSystem.Clear();
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            RiggerSystem.Clear();
        }
    }
}
