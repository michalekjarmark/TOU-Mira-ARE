using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Roles;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

/// <summary>
///     Gambler (AmonRa Edition): resolves the bets when the ejection starts (on every client, so the points match
///     everywhere) and tells the Gambler the result when the next round starts. Win conditions are not checked while
///     the ejection is shown, so a winning Gambler ends the game right after it (before crew / impostor wins).
/// </summary>
public static class GamblerEvents
{
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer;
        foreach (var gambler in CustomRoleUtils.GetActiveRolesOfType<GamblerRole>())
        {
            gambler.ResolveBet(exiled);
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        foreach (var gambler in CustomRoleUtils.GetActiveRolesOfType<GamblerRole>())
        {
            gambler.OnRoundStart();
        }
    }
}
