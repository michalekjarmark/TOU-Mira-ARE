using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using TownOfUs.Are;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Workaholic (AmonRa Edition): keeps the taken tasks consistent on every client. A Workaholic who dies or leaves gives
///     the held task back (no RPC needed, every client sees the same death / disconnect). Role changes are handled in
///     <c>WorkaholicRole.Deinitialize</c>.
/// </summary>
public static class WorkaholicEvents
{
    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        WorkaholicSystem.Reset();
    }

    [RegisterEvent]
    public static void IntroEndEventHandler(IntroEndEvent @event)
    {
        WorkaholicSystem.RecordInitialSteps();
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        WorkaholicSystem.RecordInitialSteps();
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent @event)
    {
        WorkaholicSystem.Reset();
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player != null)
        {
            WorkaholicSystem.ReleaseAllHeldBy(@event.Player.PlayerId);
        }
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        var player = @event.ClientData?.Character;
        if (player != null)
        {
            WorkaholicSystem.ReleaseAllHeldBy(player.PlayerId);
        }
    }

    [RegisterEvent]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        if (@event.Player != null && @event.Task != null)
        {
            WorkaholicSystem.OnTaskCompleted(@event.Player, @event.Task.Id);
        }
    }
}
