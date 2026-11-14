using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE (owner's feedback, round 14): in a "Local" game the meeting results wait for the host's "Proceed" button,
///     and a dead host couldn't click it, so the meeting hung until the game's own time limit (about 60 s). Online
///     games go on by themselves a few seconds after the results (TOU turns the button into a "Host: name" label
///     there), so friends never see this; it only slowed down our two-window tests. A dead host in a local game now
///     goes on by itself after the results, like an online game.
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class AreLocalMeetingProceed
{
    private const float ResultsSeconds = 5f;

    private static IntPtr _meeting;
    private static float _resultsSince = -1f;
    private static bool _done;

    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        if (__instance.Pointer != _meeting)
        {
            _meeting = __instance.Pointer;
            _resultsSince = -1f;
            _done = false;
        }

        if (_done || __instance.state != MeetingHud.MeetingStates.Results || AmongUsClient.Instance == null ||
            AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || !local.Data.IsDead)
        {
            return;
        }

        if (_resultsSince < 0f)
        {
            _resultsSince = Time.time;
            return;
        }

        if (Time.time - _resultsSince < ResultsSeconds)
        {
            return;
        }

        _done = true;
        Info("ARE: local game, the host is dead, the meeting goes on after the results.");
        __instance.HandleProceed();
    }
}
