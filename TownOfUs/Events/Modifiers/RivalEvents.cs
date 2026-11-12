using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Are;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

/// <summary>
///     ARE: the Rival's start-of-game message, like the Lovers' one. There is no message when the rival dies
///     (owner's decision: the death shows the usual way, by the body or on the meeting screen).
/// </summary>
public static class RivalEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro || !PlayerControl.LocalPlayer ||
            !PlayerControl.LocalPlayer.TryGetModifier<RivalModifier>(out var rival) || rival.RivalData == null)
        {
            return;
        }

        var color = TownOfUsColors.Rival.ToTextColor();
        var notif = Helpers.CreateAndShowNotification(
            MiraLocaleManager.Get("TownOfUsMira.Modifier.RivalIntroMessage")
                .Replace("<modifier>", $"{color}{rival.ModifierName}</color>")
                .Replace("<player>", $"{color}{rival.RivalData.PlayerName}</color>"),
            Color.white, new Vector3(0f, 1f, -20f), spr: AreAssets.RivalIcon);

        notif.AdjustNotification();
    }
}
