using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using TownOfUs.Are;
using TownOfUs.Events.TouEvents;

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
        WorkaholicSystem.LogTasks(PlayerControl.LocalPlayer, "game start");
    }

    // Diagnostics (round 11): a role change through "Set Roles" left task objects and game data out of step.
    [RegisterEvent(100)]
    public static void ChangeRoleEventHandler(ChangeRoleEvent @event)
    {
        if (@event.Player != null && @event.Player.AmOwner)
        {
            WorkaholicSystem.LogTasks(@event.Player, $"role change {@event.OldRole?.GetType().Name} -> {@event.NewRole?.GetType().Name}");
        }
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

    // Last (handlers run from the lowest priority): the Auditor's handler must still see that a Workaholic did the task.
    [RegisterEvent(100)]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        if (@event.Player != null && @event.Task != null)
        {
            WorkaholicSystem.OnTaskCompleted(@event.Player, @event.Task.Id);
        }
    }
}
