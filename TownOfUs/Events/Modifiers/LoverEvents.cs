using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using TownOfUs.AnimationSystem.BasicKills;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.Are;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Networking;
using TownOfUs.Options.Modifiers.Alliance;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class LoverEvents
{
    public static bool NextKillFromLovers;

    [RegisterEvent]
    public static void BeforeKillAnim(BeforeKillAnimationEvent @event)
    {
        if (NextKillFromLovers)
        {
            @event.FinalAnimation = SuffocateKillAnimation.CreateKillAnimation(@event.Killer, @event.Victim);
            @event.ShouldAlterAnimationAfter = false;
        }

        NextKillFromLovers = false;
    }
    [RegisterEvent(400)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!@event.Player)
        {
            return;
        }

        if (!@event.Player.TryGetModifier<LoverModifier>(out var loveMod)
            || !OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie || loveMod.LoverDisconnected || loveMod.OtherLover == null
            || loveMod.OtherLover.HasDied() || loveMod.OtherLover.HasModifier<InvulnerabilityModifier>())
        {
            return;
        }

        switch (@event.DeathReason)
        {
            case DeathReason.Exile:
                NoticeHeartbreak(loveMod, @event.Player, true); // AmonRa Edition
                GameHistory.UpdatePlayerDeathData(loveMod.OtherLover, MiraLocaleManager.Get("DiedToHeartbreak"), 0, HudManagerHelper.Instance.CurrentRound,
                    DeathHandlerOverride.SetTrue,
                    lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);
                loveMod.OtherLover.Exiled();
                break;
            case DeathReason.Kill:
                NoticeHeartbreak(loveMod, @event.Player, ExileController.Instance); // AmonRa Edition
                if (PlayerControl.LocalPlayer.IsHost() && MeetingHud.Instance)
                {
                    loveMod.OtherLover.RpcMeetingMurder(loveMod.OtherLover, MeetingAnimation.PlayerNameplateAnimation, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                        causeOfDeath: "Heartbreak");
                }
                else if (!MeetingHud.Instance)
                {
                    var showAnim = !ExileController.Instance;
                    NextKillFromLovers = showAnim && loveMod.OtherLover.AmOwner;
                    var murderResultFlags2 = MurderResultFlags.DecisionByHost | MurderResultFlags.Succeeded;

                    GameHistory.UpdatePlayerDeathData(loveMod.OtherLover, MiraLocaleManager.Get("DiedToHeartbreak"), 0, HudManagerHelper.Instance.CurrentRound,
                        showAnim
                            ? DeathHandlerOverride.SetTrue
                            : DeathHandlerOverride.SetFalse,
                        lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);
                    loveMod.OtherLover.CustomMurder(
                        loveMod.OtherLover,
                        murderResultFlags2,
                        false,
                        showAnim,
                        false,
                        showAnim,
                        false);
                }
                break;
        }
    }

    // AmonRa Edition: tell the other Lover why they died (after an ejection there's no kill of their own to see).
    private static void NoticeHeartbreak(LoverModifier loveMod, PlayerControl deadLover, bool nextRound)
    {
        if (loveMod.OtherLover == null || !loveMod.OtherLover.AmOwner)
        {
            return;
        }

        var title = AreDeathNotice.Title(loveMod.ModifierName, TownOfUsColors.Lover);
        var text = MiraLocaleManager.Get("TownOfUsMira.Modifier.Lover.AreHeartbreak")
            .Replace("<player>", deadLover.Data.PlayerName);
        if (nextRound)
        {
            AreDeathNotice.ShowNextRound(title, text, TouModifierIcons.Lover.LoadAsset());
        }
        else
        {
            AreDeathNotice.Show(title, text, TouModifierIcons.Lover.LoadAsset());
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        ModifierUtils.GetActiveModifiers<LoverModifier>().Do(x => x.OnRoundStart());
        if (@event.TriggeredByIntro && PlayerControl.LocalPlayer.TryGetModifier<LoverModifier>(out var lover) &&
            lover.OtherLover != null)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                MiraLocaleManager.Get("TownOfUsMira.Modifier.LoverIntroMessage")
                    .Replace("<modifier>", $"{TownOfUsColors.Lover.ToTextColor()}{lover.ModifierName}</color>")
                    .Replace("<player>", $"{TownOfUsColors.Lover.ToTextColor()}{lover.OtherLover.Data.PlayerName}</color>"),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Lover);

            notif1.AdjustNotification();
        }
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var player = @event.Player;

        if (player.TryGetModifier<LoverModifier>(out var lover) && !@event.NewRole.IsCrewmate())
        {
            lover.ForceDisableTasks = true;
            if (lover.OtherLover != null && lover.OtherLover.TryGetModifier<LoverModifier>(out var lover2))
            {
                lover2.ForceDisableTasks = true;
            }
        }
    }
}