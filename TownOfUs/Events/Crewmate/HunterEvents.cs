using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Are;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Events.Crewmate;

public static class HunterEvents
{
    public static int ActiveStalkTaskCount;
    public static PlayerControl HunterToAnnounceNext;
    private static string _shameVictimToAnnounce = string.Empty; // AmonRa Edition
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            if (!HunterToAnnounceNext)
            {
                return;
            }

            // AmonRa Edition: texts that say why (owner's report: a Hunter didn't know why they were dead), with the
            // victim's name, a longer notice and a chat line the Hunter can read later.
            if (HunterToAnnounceNext.AmOwner)
            {
                AreDeathNotice.Show(
                    AreDeathNotice.Title(MiraLocaleManager.Get("TownOfUsMira.Role.Hunter"), TownOfUsColors.Hunter),
                    MiraLocaleManager.Get("TownOfUsMira.Role.Hunter.AreShameSelf")
                        .Replace("<player>", _shameVictimToAnnounce),
                    TouRoleIcons.Hunter.LoadAsset());
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.Hunter.AreShame").Replace("<player>", HunterToAnnounceNext.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hunter);

                notif1.AdjustNotification();
            }

            HunterToAnnounceNext = null!;
            return; // Only run when game starts.
        }

        HunterToAnnounceNext = null!;
        ActiveStalkTaskCount = 0;

        var hunterStalk = CustomButtonSingleton<HunterStalkButton>.Instance;
        hunterStalk.ExtraUses = 0;
        hunterStalk.SetUses(OptionGroupSingleton<HunterOptions>.Instance.StalkUses);
        if (!hunterStalk.LimitedUses)
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(false);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(true);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }
    }

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        var opt = OptionGroupSingleton<HunterOptions>.Instance;
        var stalkButton = CustomButtonSingleton<HunterStalkButton>.Instance;
        if (@event.Player.AmOwner)
        {
            ++ActiveStalkTaskCount;
            if (@event.Player.Data.Role is not HunterRole)
            {
                return;
            }

            if (stalkButton.LimitedUses &&
                opt.StalkPerTasks != 0 && opt.StalkPerTasks <= ActiveStalkTaskCount)
            {
                ++stalkButton.UsesLeft;
                ++stalkButton.ExtraUses;
                stalkButton.SetUses(stalkButton.UsesLeft);
                ActiveStalkTaskCount = 0;
            }
        }
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button;
        var source = PlayerControl.LocalPlayer;

        if (button == null || !button.CanClick())
        {
            return;
        }

        CheckForHunterStalked(source, button is CustomActionButton<PlayerControl>);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        CheckForHunterStalked(source, true);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        CheckForHunterStalked(source, true);

        if (source.Data.Role is not HunterRole role)
        {
            return;
        }

        if (source.HasModifier<AllianceGameModifier>(x => !x.GetsPunished))
        {
            return;
        }

        var target = @event.Target;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (!target.IsCrewmate() ||
                target.HasModifier<AllianceGameModifier>(x => !x.GetsPunished))
            {
                stats.CorrectKills += 1;
            }
            else if (source != target)
            {
                stats.IncorrectKills += 1;
            }
        }

        // AmonRa Edition: AreKillSync.StartedInMeeting instead of MeetingHud.Instance: this event comes after the kill
        // animation, and a meeting called meanwhile made only some clients send the Hunter away.
        if (OptionGroupSingleton<HunterOptions>.Instance.SameRoundKillPunishment
            && !AreKillSync.StartedInMeeting(target)
            && IsIncorrectKill(source, target)
            && role.CaughtRounds.TryGetValue(target.PlayerId, out var caughtRound)
            && caughtRound == HudManagerHelper.Instance.CurrentRound)
        {
            role.PendingShame = true;
            role.ShameVictim = target.Data.PlayerName;
        }
    }

    private static bool IsIncorrectKill(PlayerControl hunter, PlayerControl target)
    {
        if (hunter.HasModifier<AllianceGameModifier>(allyMod => !allyMod.GetsPunished))
        {
            return false;
        }

        if (target.HasModifier<AllianceGameModifier>(allyMod2 => !allyMod2.GetsPunished))
        {
            return false;
        }

        if (hunter == target)
        {
            return false;
        }

        return target.IsCrewmate();
    }

    [RegisterEvent(400)]
    public static void ShameWrapUpEvent(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        foreach (var hunter in CustomRoleUtils.GetActiveRolesOfType<HunterRole>())
        {
            if (!hunter.PendingShame)
            {
                continue;
            }

            hunter.PendingShame = false;

            if (hunter.Player.HasDied() || exiled == hunter.Player)
            {
                continue;
            }

            HunterToAnnounceNext = hunter.Player;
            _shameVictimToAnnounce = hunter.ShameVictim; // AmonRa Edition
            if (hunter.Player.TryGetModifier<CelebrityModifier>(out var celeb))
            {
                celeb.Announced = true;
            }
            GameHistory.UpdatePlayerDeathData(hunter.Player.PlayerId, MiraLocaleManager.Get("DiedToPunishment"), 0,
                HudManagerHelper.Instance.CurrentRound, DeathHandlerOverride.SetFalse,
                lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);

            hunter.Player.Exiled();
        }
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent _)
    {
        if (!OptionGroupSingleton<HunterOptions>.Instance.RetributionOnVote)
        {
            return;
        }
        var states = MeetingHudGetVotesPatch.States;
        var hunters = CustomRoleUtils.GetActiveRolesOfType<HunterRole>();
        if (!hunters.Any())
        {
            return;
        }
        foreach (var state in states)
        {
            if (state.SkippedVote || state.AmDead)
            {
                continue;
            }
            foreach (var hunter in hunters)
            {
                var voter = MiscUtils.PlayerById(state.VoterId);
                if (hunter.Player.PlayerId != state.VoterId && hunter.Player.PlayerId == state.VotedForId && voter != null)
                {
                    hunter.LastVoted = voter;
                }
            }
        }
    }


    [RegisterEvent(300)]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (!OptionGroupSingleton<HunterOptions>.Instance.RetributionOnVote)
        {
            return;
        }

        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        if (exiled?.Data.Role is not HunterRole hunter)
        {
            return;
        }

        var target = hunter.LastVoted!;
        if (CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>().FirstOrDefault() is { HasProsecuted: true } pros)
        {
            target = pros.Player;
        }

        HunterRole.Retribution(hunter.Player, target);
    }

    private static void CheckForHunterStalked(PlayerControl source, bool isInteraction)
    {
        if (MeetingHud.Instance || ExileController.Instance || !isInteraction && OptionGroupSingleton<HunterOptions>.Instance.StalkTriggeredBy.Value is StalkTriggered.Interactions)
        {
            return;
        }

        if (!source.TryGetModifier<HunterStalkedModifier>(out var mod))
        {
            return;
        }

        if (mod.Hunter == null || !(TutorialManager.InstanceExists || source.AmOwner))
        {
            return;
        }

        HunterRole.RpcCatchPlayer(source, mod.Hunter, isInteraction);
    }
}