using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Are;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Options.Modifiers.Crewmate;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

/// <summary>
///     ARE: the killer of a Bleeder starts bleeding (on every client, like the Frosty's chill).
/// </summary>
public static class BleederEvents
{
    // A killer further than this from the body didn't touch the victim (Arsonist, Bomber, Rigger trap...).
    private const float MaxKillerDistance = 3f;

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;
        if (source == null || target == null || source == target || MeetingHud.Instance ||
            !target.HasModifier<BleederModifier>() || source.Data == null || source.Data.IsDead)
        {
            return;
        }

        var distance = Vector2.Distance(source.GetTruePosition(), target.GetTruePosition());
        if (distance > MaxKillerDistance)
        {
            Message($"Bleeder: {source.Data.PlayerName} killed {target.Data.PlayerName} from {distance:0.0} units, no trail");
            return;
        }

        if (source.TryGetModifier<BleedingModifier>(out var bleeding))
        {
            bleeding.ResetTimer();
            bleeding.StartTimer();
        }
        else
        {
            source.AddModifier<BleedingModifier>();
        }
        Message($"Bleeder: {source.Data.PlayerName} bleeds after killing {target.Data.PlayerName}");

        if (!source.AmOwner || !OptionGroupSingleton<BleederOptions>.Instance.NotifyKiller)
        {
            return;
        }

        var text = MiraLocaleManager.Get("TownOfUsMira.Modifier.BleederTriggeredNotif")
            .Replace("<player>", target.Data.PlayerName)
            .Replace("<modifier>",
                $"{TownOfUsColors.Bleeder.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Modifier.Bleeder")}</color>")
            .Replace("<time>", BleederModifier.TrailTime());
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: AreAssets.BleederIcon.LoadAsset());
        notif.AdjustNotification();
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        BleedingModifier.ClearPrints();
    }
}
