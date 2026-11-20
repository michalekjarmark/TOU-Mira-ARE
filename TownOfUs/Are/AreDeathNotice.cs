using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: tells the local player why they died when a rule killed them without a kill of their own to see (owner's
///     report, 2026-11-20: a Hunter didn't know why they were dead). A notification plus a line in the chat, which stays
///     there to read later. A death at the end of a meeting is told at the start of the next round, when the player can
///     see it.
/// </summary>
public static class AreDeathNotice
{
    private static (string Title, string Text, Sprite? Icon)? _nextRound;

    /// <summary>Show the notice now.</summary>
    public static void Show(string title, string text, Sprite? icon)
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }

        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: icon);
        notif.AdjustNotification();
        notif.alphaTimer = 10f;
        MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, text, false, true);
    }

    /// <summary>Show the notice when the next round starts (after the ejection).</summary>
    public static void ShowNextRound(string title, string text, Sprite? icon)
    {
        _nextRound = (title, text, icon);
    }

    /// <summary>A title in the given colour, for the chat line.</summary>
    public static string Title(string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro || _nextRound is not { } notice)
        {
            _nextRound = null;
            return;
        }

        _nextRound = null;
        Show(notice.Title, notice.Text, notice.Icon);
    }
}
