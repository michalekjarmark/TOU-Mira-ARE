using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Comfy (AmonRa Edition): the sulk works like the Cleric's barrier (same priority and checks as
///     <c>ClericEvents</c>), plus the locks, the report block and the offloaded tasks coming back. Every client sees the
///     same meetings, departures and modifiers, so no RPC is needed here.
/// </summary>
public static class ComfyEvents
{
    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        ComfySystem.Reset();
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent @event)
    {
        ComfySystem.Reset();
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        ComfySystem.OnRoundStart();
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        ComfySystem.OnMeetingStart();
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        var player = @event.ClientData?.Character;
        if (player != null)
        {
            ComfySystem.OnPlayerLeft(player.PlayerId);
        }
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (ComfySystem.BlocksConsole(@event.Usable))
        {
            @event.Cancel();
        }
    }

    [RegisterEvent(-100)]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (!ComfySystem.BlocksReport(@event.Reporter, @event.Target == null))
        {
            return;
        }

        Message($"Comfy: {@event.Reporter.Data.PlayerName} is sulking, report blocked");
        @event.Cancel();
    }

    [RegisterEvent(-800)]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        CheckForSulk(@event, target, PlayerControl.LocalPlayer, false);
    }

    [RegisterEvent(-800)]
    public static void MiraButtonCancelledEventHandler(MiraButtonCancelledEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || !target.HasModifier<ComfySulkModifier>() ||
            !OptionGroupSingleton<ComfyOptions>.Instance.SulkBlocksAbilities)
        {
            return;
        }

        ResetButtonTimer(PlayerControl.LocalPlayer, button);
    }

    [RegisterEvent(-800)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (CheckForSulk(@event, @event.Target, @event.Source, true))
        {
            ResetButtonTimer(@event.Source);
        }
    }

    private static bool CheckForSulk(MiraCancelableEvent @event, PlayerControl target, PlayerControl source, bool kill)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        if (!target.HasModifier<ComfySulkModifier>() || target.PlayerId == source.PlayerId ||
            @event is BeforeMurderEvent { IgnoreDefense: true } ||
            @event is ExtendedMiraButtonClickEvent { IgnoreDefense: true })
        {
            return false;
        }

        var opts = OptionGroupSingleton<ComfyOptions>.Instance;
        if (kill ? !opts.SulkBlocksKills : !opts.SulkBlocksAbilities)
        {
            return false;
        }

        Message($"Comfy: {target.Data.PlayerName} is sulking, stopping an interaction from {source.Data.PlayerName}");
        @event.Cancel();

        if (source.AmOwner)
        {
            ComfySystem.RpcComfySulkBlocked(source, target);
        }

        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        if (!source.AmOwner)
        {
            return;
        }

        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;
        button?.SetTimer(reset);
        source.SetKillTimer(reset);
    }
}
