using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class CelebrityEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (target.TryGetModifier<CelebrityModifier>(out var celeb))
        {
            // AmonRa Edition: the meeting state from the kill's start (Are.AreKillSync): this event comes after the
            // kill animation, so a meeting called meanwhile kept the announcement from some clients only.
            if (!AreKillSync.StartedInMeeting(target, false))
            {
                CelebrityModifier.CelebrityKilled(source, target);

                // The meeting started before this event: its intro (which announces) is over here, so announce in
                // the chat, like the intro does (otherwise this client would announce one meeting late).
                if (MeetingHud.Instance && !celeb.Announced && celeb.DeathMessage != string.Empty)
                {
                    celeb.Announced = true;
                    var title = $"<color=#{ColorUtility.ToHtmlStringRGBA(TownOfUsColors.Celebrity)}>" +
                                $"{MiraLocaleManager.Get("TownOfUsMira.Role.CelebrityMessageTitle")}</color>";
                    var message = celeb.DeathMessage.Replace("<timeAgo>",
                        Math.Round(celeb.DeathTimeMilliseconds / 1000).ToString(TownOfUsPlugin.Culture));
                    MiscUtils.AddFakeChat(celeb.Player.Data, title, message, false, true);
                }
            }
            else
            {
                celeb.Announced = true;
            }
        }
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason != DeathReason.Exile)
        {
            return;
        }

        if (@event.Player.TryGetModifier<CelebrityModifier>(out var celeb))
        {
            celeb.Announced = true;
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Reporter.AmOwner)
        {
            var celebrity = ModifierUtils.GetActiveModifiers<CelebrityModifier>(x => x.Player.HasDied() && !x.Announced)
                .FirstOrDefault();
            if (celebrity != null)
            {
                // AmonRa Edition: a report right after the kill comes before CelebrityKilled set the time: 0, not
                // the time since year 1.
                var milliSeconds = celebrity.DeathTime == default
                    ? 0f
                    : (float)(DateTime.UtcNow - celebrity.DeathTime).TotalMilliseconds;

                CelebrityModifier.RpcUpdateCelebrityKilled(celebrity.Player, milliSeconds);
            }
        }
    }

    [RegisterEvent]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var player = @event.ExileController.initData.networkedPlayer?.Object;
        if (player == null)
        {
            return;
        }

        if (player.TryGetModifier<CelebrityModifier>(out var celeb))
        {
            celeb.Announced = true;
        }
    }
}