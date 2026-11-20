using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Networking;
using TownOfUs.Networking;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: kills at the same moment as something else (owner's report, 2026-11-20). Every kill is decided by the host
///     (MiraAPI's and TOU's confirm RPCs), so all clients agree on who died; the fixes here are about what the host
///     accepts and about checks that run late on each client.
/// </summary>
[HarmonyPatch]
public static class AreKillSync
{
    // Every client: the state when each player's last kill started here (meeting on, ejection on, victim's position).
    private static readonly Dictionary<byte, (bool Meeting, bool Ejection, Vector2 Position, float At)> KillStarts = [];

    /// <summary>
    ///     Whether the kill of <paramref name="target" /> started during a meeting (or, with
    ///     <paramref name="countEjection" />, the ejection). Use it instead of MeetingHud.Instance in AfterMurderEvent
    ///     handlers: that event comes after the kill animation, and a meeting called in the meantime has already started on
    ///     some clients and not on others, so a check of the current state gives different results on different clients
    ///     (e.g. the Hunter left in shame only for some).
    /// </summary>
    public static bool StartedInMeeting(PlayerControl target, bool countEjection = true)
    {
        if (TryGetStart(target, out var start))
        {
            return start.Meeting || (countEjection && start.Ejection);
        }

        return MeetingHud.Instance || (countEjection && ExileController.Instance);
    }

    /// <summary>
    ///     Where <paramref name="target" /> was when their kill started: after the animation a meeting may already have
    ///     moved everyone to the table (the Celebrity's death room).
    /// </summary>
    public static Vector2 KillStartPosition(PlayerControl target)
    {
        return TryGetStart(target, out var start) ? start.Position : target.GetTruePosition();
    }

    private static bool TryGetStart(PlayerControl target,
        out (bool Meeting, bool Ejection, Vector2 Position, float At) start)
    {
        start = default;
        return target != null && KillStarts.TryGetValue(target.PlayerId, out start) &&
               Time.realtimeSinceStartup - start.At < 10f;
    }

    // The host decides a kill when its request arrives and lets every client run it (CustomMurder), so here the state
    // is the same on every client: the kill was accepted before the host started a meeting.
    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.CustomMurder), typeof(PlayerControl),
        typeof(PlayerControl), typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool),
        typeof(bool))]
    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.CustomMurder), typeof(PlayerControl),
        typeof(PlayerControl), typeof(PlayerControl), typeof(bool), typeof(bool), typeof(MurderResultFlags),
        typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static void CustomMurderPrefix(PlayerControl target)
    {
        NoteKillStart(target);
    }

    // Kills in a meeting (guesses, the Jailor...) take their own way; noted too, so an older record isn't used.
    [HarmonyPatch(typeof(CustomTouMurderRpcs), nameof(CustomTouMurderRpcs.MeetingMurder))]
    [HarmonyPrefix]
    public static void MeetingMurderPrefix(PlayerControl target)
    {
        NoteKillStart(target);
    }

    private static void NoteKillStart(PlayerControl target)
    {
        if (target != null)
        {
            KillStarts[target.PlayerId] = ((bool)MeetingHud.Instance, (bool)ExileController.Instance, target.GetTruePosition(),
                Time.realtimeSinceStartup);
        }
    }

    // A killer who died a moment earlier can't kill any more (vanilla doesn't let dead players kill either). Without
    // this two killers who strike at the same moment both die. Only kills meant outside meetings (kill buttons and the
    // like): kills by the dead (ghost roles, a bomb after the Bomber's death, the Hunter's retribution) don't use it.
    // Every client runs the check; the host's result is the one that counts.
    [RegisterEvent(-1000)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        if (@event.InMeeting is not MeetingCheck.OutsideMeeting || source == null || source == @event.Target)
        {
            return;
        }

        // For the logs: a kill that came after the host had started the meeting (the murder RPC cancels it) or while
        // the start was held (it counts, see AreMeetingHold).
        if (AmongUsClient.Instance.AmHost && (MeetingHud.Instance || ExileController.Instance))
        {
            Message($"ARE kill sync: {source.Data?.PlayerName} -> {@event.Target?.Data?.PlayerName} cancelled, " +
                    "the meeting had already started");
        }
        else if (AmongUsClient.Instance.AmHost && AreMeetingHold.Holding)
        {
            Message($"ARE kill sync: {source.Data?.PlayerName} -> {@event.Target?.Data?.PlayerName} came while the " +
                    "meeting start was held");
        }

        if (!source.HasDied())
        {
            return;
        }

        @event.Cancel();
        if (AmongUsClient.Instance.AmHost)
        {
            Message($"ARE kill sync: {source.Data?.PlayerName} -> {@event.Target?.Data?.PlayerName} cancelled, " +
                    "the killer died first");
        }
    }
}
