using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

/// <summary>
///     Insider (AmonRa Edition): applies the task-reduced kill cooldown wherever the game resets the kill timer (after a
///     kill, after a meeting). Runs after Underdog (priority 1), whose cooldown is already part of the base value.
/// </summary>
public static class InsiderEvents
{
    [RegisterEvent]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is InsiderRole insider)
        {
            insider.OnOwnTaskCompleted();
        }
    }

    [RegisterEvent(5)]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        if (source != null && source.AmOwner && source.Data.Role is InsiderRole insider)
        {
            source.SetKillTimer(insider.KillCooldown);
        }
    }

    [RegisterEvent(5)]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        // The first round keeps the normal short start cooldown.
        if (@event.TriggeredByIntro)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local != null && local.Data.Role is InsiderRole insider)
        {
            local.SetKillTimer(insider.KillCooldown);
        }
    }
}
