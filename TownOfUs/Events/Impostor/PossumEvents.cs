using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Are;

namespace TownOfUs.Events.Impostor;

/// <summary>
///     ARE: Possum bookkeeping. The report of the fake body is caught earlier (<see cref="PossumSystem" />, click on
///     the body); this is only a safety net for any other way of reporting it.
/// </summary>
public static class PossumEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            PossumSystem.Reset();
        }
    }

    [RegisterEvent(-100)]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        var target = @event.Target?.Object;
        if (target == null || !PossumSystem.IsPlayingDead(target))
        {
            return;
        }

        Message($"Possum: report of {target.Data.PlayerName}'s fake body by {@event.Reporter?.Data?.PlayerName} blocked");
        @event.Cancel();
    }
}
