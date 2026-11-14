using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Lighthouse Keeper (AmonRa Edition): lanterns are cleared at the start and the end of a game, and (option) go out
///     when their Lighthouse Keeper dies. Every client sees the same deaths, so no RPC is needed.
/// </summary>
public static class LighthouseKeeperEvents
{
    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        LighthouseSystem.Clear();
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent @event)
    {
        LighthouseSystem.Clear();
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player != null && OptionGroupSingleton<LighthouseKeeperOptions>.Instance.LanternsOutOnDeath)
        {
            LighthouseSystem.RemoveLanternsOf(@event.Player.PlayerId);
        }
    }
}
