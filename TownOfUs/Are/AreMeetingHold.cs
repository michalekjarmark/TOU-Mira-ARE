using System.Collections;
using System.Globalization;
using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: the host waits a moment before it starts a meeting (owner's report, 2026-11-20: a kill made at the same
///     moment as a report or the emergency button "didn't count" and the victim stayed alive in the meeting).
///     <para>
///         The host decides every kill when its request arrives. Vanilla creates the meeting screen on the host right
///         away when it accepts a report (<c>HudManager.OpenMeetingRoom</c>), so a kill request that arrives a few
///         milliseconds later is rejected for everyone, even though the killer pressed the button before they could see
///         the meeting. Holding the meeting start (<c>OpenMeetingRoom</c> and <c>RpcStartMeeting</c>, always called
///         together: reports, the emergency button, Button Barry, the Chairman, the host's keybind) lets such kills
///         arrive first: they happen as normal kills before the meeting, and the meeting then shows the victim as dead
///         on every client.
///     </para>
///     A second meeting called while one is held is dropped, the same as vanilla drops it during a meeting.
/// </summary>
[HarmonyPatch]
public static class AreMeetingHold
{
    private static PlayerControl? _reporter;
    private static NetworkedPlayerInfo? _target;
    private static bool _held;
    private static bool _startCaptured;
    private static float _heldAt;
    private static int _holdId;
    private static bool _releasing;

    /// <summary>The host holds a meeting start right now.</summary>
    public static bool Holding => _held;

    private static float Delay => OptionGroupSingleton<HostSpecificOptions>.Instance.AreMeetingStartDelay.Value;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
    [HarmonyPrefix]
    public static bool OpenMeetingRoomPrefix(PlayerControl __0)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || MeetingHud.Instance || __0 == null)
        {
            return true;
        }

        if (_releasing || TutorialManager.InstanceExists)
        {
            BeforeMeetingOpens();
            return true;
        }

        // Safety net: a hold never lasts longer than its delay plus a moment (see CoRelease).
        if (_held && Time.realtimeSinceStartup - _heldAt > Delay + 3f)
        {
            Warning("ARE meeting hold: a stale hold was cleared");
            Reset();
        }

        if (_held)
        {
            Message($"ARE meeting hold: meeting by {__0.Data?.PlayerName} dropped, one is already starting");
            return false;
        }

        var delay = Delay;
        if (delay <= 0f)
        {
            BeforeMeetingOpens();
            return true;
        }

        _held = true;
        _startCaptured = false;
        _reporter = __0;
        _target = null;
        _heldAt = Time.realtimeSinceStartup;
        Coroutines.Start(CoRelease(delay, ++_holdId));
        return false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcStartMeeting))]
    [HarmonyPrefix]
    public static bool RpcStartMeetingPrefix(PlayerControl __instance, NetworkedPlayerInfo? __0)
    {
        if (_releasing || !_held)
        {
            return true;
        }

        // The start that belongs to the held meeting: keep it for later. Anything else is the dropped second meeting.
        if (__instance == _reporter && !_startCaptured)
        {
            _target = __0;
            _startCaptured = true;
            Message($"ARE meeting hold: {(__0 == null ? "emergency meeting" : $"report of {__0.PlayerName}")} by " +
                    $"{__instance.Data?.PlayerName} held for {Delay.ToString("0.0#", CultureInfo.InvariantCulture)} s");
        }

        return false;
    }

    private static IEnumerator CoRelease(float delay, int holdId)
    {
        yield return new WaitForSeconds(delay);

        if (holdId != _holdId || !_held)
        {
            yield break;
        }

        var reporter = _reporter;
        var target = _target;
        var captured = _startCaptured;
        Reset();

        if (reporter == null || !AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            Warning("ARE meeting hold: nothing to start");
            yield break;
        }

        // The game may have ended in the meantime (a kill that arrived during the hold).
        if (AmongUsClient.Instance.IsGameOver || LobbyBehaviour.Instance || !ShipStatus.Instance || MeetingHud.Instance ||
            ExileController.Instance)
        {
            Message("ARE meeting hold: the held meeting doesn't start (game over or a meeting is on)");
            yield break;
        }

        // Without the start held (the caller didn't call RpcStartMeeting through the patch), the start is already out
        // and every client waits for the meeting screen: create it whatever happened meanwhile.
        if (captured)
        {
            if (reporter.Data == null || reporter.Data.Disconnected)
            {
                Message("ARE meeting hold: the held meeting doesn't start (the reporter left)");
                yield break;
            }

            // As vanilla does right before the meeting starts (tasks may have been finished during the hold).
            if (GameManager.Instance && GameManager.Instance.CheckTaskCompletion())
            {
                yield break;
            }
        }

        _releasing = true;
        try
        {
            // A dropped second report took the reporter's place here (used if someone disconnects before the start).
            MeetingRoomManager.Instance.AssignSelf(reporter, target);
            HudManager.Instance.OpenMeetingRoom(reporter);
            if (captured)
            {
                reporter.RpcStartMeeting(target);
            }
            else
            {
                Warning("ARE meeting hold: the meeting start went out unheld, only the meeting screen was held");
            }
        }
        finally
        {
            _releasing = false;
        }
    }

    /// <summary>
    ///     Host, right before the meeting screen is created from its data: last kills that must be in this meeting
    ///     (the Glutton's swallowed die here, so the meeting shows them dead on every client).
    /// </summary>
    private static void BeforeMeetingOpens()
    {
        try
        {
            GluttonSystem.DigestBeforeMeeting();
        }
        catch (Exception e)
        {
            Error($"ARE meeting hold: {e}");
        }
    }

    private static void Reset()
    {
        _held = false;
        _startCaptured = false;
        _reporter = null;
        _target = null;
    }
}
