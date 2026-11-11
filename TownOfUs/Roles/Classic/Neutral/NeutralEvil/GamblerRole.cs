using System.Collections;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Interfaces;
using TownOfUs.MeetingAbilities.Are;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

/// <summary>
///     Gambler (AmonRa Edition, Neutral Evil): during every meeting, before the votes are revealed, bets on who gets
///     ejected (or on no ejection). A correct bet is +1 point, a wrong one -1 (optional, never below 0). Reaching the
///     goal while alive wins. The bet is sent to everyone (RPC) and every client resolves it on its own when the
///     ejection starts, so the points are the same everywhere and the host can end the game.
/// </summary>
public sealed class GamblerRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IProgressTally
{
    public const byte NoBet = byte.MaxValue;

    /// <summary>Player id of <see cref="MeetingHud.SkipVoteButton" /> (vanilla skipped vote, set in MeetingHudGetVotesPatch).</summary>
    public const byte NoEjectionBet = 253;

    public int Points { get; set; }

    /// <summary>Player id the Gambler bet on this meeting, <see cref="NoEjectionBet" /> or <see cref="NoBet" />.</summary>
    public byte CurrentBet { get; set; } = NoBet;

    public bool ReachedGoal { get; set; }

    /// <summary>Won "no one is ejected" bets, limited by the NoEjectionWinsLimit option.</summary>
    public int NoEjectionWins { get; set; }

    public bool CanBetOnNoEjection =>
        NoEjectionWins < (int)OptionGroupSingleton<GamblerOptions>.Instance.NoEjectionWinsLimit;

    /// <summary>Result of the last resolved bet, shown to the Gambler when the next round starts.</summary>
    public GamblerBetResult LastResult { get; set; }

    /// <summary>Others were already warned that this Gambler is close to winning (reset when the points drop).</summary>
    public bool WarnedOthers { get; set; }

    /// <summary>The warning is shown when the next round starts (after the ejection screen).</summary>
    public bool PendingWarning { get; set; }

    private int PointsLeft => PointsToWin - Points;

    public static int PointsToWin => (int)OptionGroupSingleton<GamblerOptions>.Instance.PointsToWin;

    public string IdPart => "Gambler";
    public Color RoleColor => TownOfUsColors.Gambler;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public DoomableType DoomHintType => DoomableType.Trickster;

    // Imitator: closest crew role is the one that also plays the meeting votes.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SwapperRole>());

    public string RoleLongDescription =>
        MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription").Replace("<points>", $"{PointsToWin}");

    public string RoleWikiDescription => GetAdvancedDescription();

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.GamblerIcon, "TouMira.Role.Neutral.Gambler", 1.45f),
        IntroSound = TouAudio.QuestionSound,
        Icon = AreAssets.GamblerIcon,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool MetWinCon => ReachedGoal;

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Bet", "Bet"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Bet.WikiDescription"),
            AreAssets.GamblerBetButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription")
                   .Replace("<points>", $"{PointsToWin}") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (amOwner || localDead)
        {
            var color = ReachedGoal ? TownOfUsColors.Gambler : Color.yellow;
            progress = $"{color.ToTextColor()}({Points}/{PointsToWin})</color>";
            return true;
        }

        progress = string.Empty;
        return false;
    }

    public string ProgressOnSummaryNormal => string.Empty;
    public string ProgressOnSummaryDetailed => string.Empty;

    public bool WinConditionMet()
    {
        return ReachedGoal && !Player.HasDied() && OptionGroupSingleton<GamblerOptions>.Instance.WinEndsGame;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return ReachedGoal;
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }

        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralEvilTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (!OptionGroupSingleton<GamblerOptions>.Instance.CanButton)
        {
            player.RemainingEmergencies = 0;
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        _bettingOpen = false;

        // A Gambler who already won (game continues) must not turn into a ghost role.
        if (ReachedGoal && !Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        CurrentBet = NoBet;
        _bettingOpen = false;
        _betDeadline = 0f;

        if (!Player.AmOwner)
        {
            return;
        }

        var meeting = MeetingHud.Instance;
        if (meeting == null || Player.HasDied() || Player.IsJailed() || ReachedGoal)
        {
            return;
        }

        _bettingOpen = true;
        Coroutines.Start(CoBetWindow(meeting));
    }

    // Local (Gambler only): betting is allowed in this meeting and the window from the options is not over yet.
    private bool _bettingOpen;

    // Local: Time.time when the time limit window closes (0 = the voting hasn't started yet).
    private float _betDeadline;

    /// <summary>
    ///     Local check for <see cref="MeetingAbilities.Are.GamblerBetMeetingButton" />: the "Bet" button shows up in a
    ///     player's (or Skip Vote's) vote popup only while this is true.
    /// </summary>
    public bool CanBetNow =>
        _bettingOpen && CurrentBet == NoBet && !ReachedGoal && !Player.HasDied() && MeetingHud.Instance != null &&
        IsBettingOpen(MeetingHud.Instance);

    public bool CanBetOn(PlayerVoteArea area)
    {
        if (area == null)
        {
            return false;
        }

        if (area.PlayerId == NoEjectionBet)
        {
            return CanBetOnNoEjection;
        }

        if (area.PlayerId == Player.PlayerId || area.AmDead)
        {
            return false;
        }

        var data = GameData.Instance.GetPlayerById(area.PlayerId);
        return data != null && !data.Disconnected && !data.IsDead;
    }

    /// <summary>
    ///     The Gambler's line under the meeting timer (where Town of Us shows e.g. the Prosecutor's prosecutions left):
    ///     points and the state of this meeting's bet. Shown instead of notifications (round 5 feedback).
    /// </summary>
    public string MeetingStatusText(MeetingHud meeting)
    {
        var points = MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingPoints")
            .Replace("<points>", $"{Points}").Replace("<goal>", $"{PointsToWin}");

        string status;
        if (ReachedGoal)
        {
            status = MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingGoalReached");
        }
        else if (CurrentBet == NoEjectionBet)
        {
            status = MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingBetNoEjection");
        }
        else if (CurrentBet != NoBet)
        {
            status = MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingBet")
                .Replace("<player>", GameData.Instance.GetPlayerById(CurrentBet)?.PlayerName ?? "?");
        }
        else if (_bettingOpen && meeting.state is MeetingHud.MeetingStates.Animating
                     or MeetingHud.MeetingStates.Discussion)
        {
            status = MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingWaitForVoting");
        }
        else if (CanBetNow && !HasVoted(meeting))
        {
            var opts = OptionGroupSingleton<GamblerOptions>.Instance;
            var seconds = _betDeadline > 0f ? _betDeadline - Time.time : opts.BetTime.Value;
            status = opts.BetWindow is GamblerBetWindow.Timer
                ? MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingTimeLeft")
                    .Replace("<seconds>", $"{Mathf.Max(0, Mathf.CeilToInt(seconds))}")
                : MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingBeforeVote");
        }
        else
        {
            status = MiraLocaleManager.Get("TownOfUsMira.Role.GamblerMeetingClosed");
        }

        return $"{points} | {status}";
    }

    private bool HasVoted(MeetingHud meeting)
    {
        foreach (var area in meeting.playerStates)
        {
            if (area != null && area.PlayerId.Value == Player.PlayerId)
            {
                return area.DidVote;
            }
        }

        return false;
    }

    /// <summary>
    ///     The vote popups only open once the voting starts, so the betting window starts there too, and closes after
    ///     BetTime seconds (time limit mode). The state is shown in <see cref="MeetingStatusText" />.
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoBetWindow(MeetingHud meeting)
    {
        while (meeting && _bettingOpen && meeting.state is MeetingHud.MeetingStates.Animating
                   or MeetingHud.MeetingStates.Discussion)
        {
            yield return null;
        }

        if (!meeting || !_bettingOpen || !IsBettingOpen(meeting))
        {
            yield break;
        }

        var opts = OptionGroupSingleton<GamblerOptions>.Instance;
        Message($"Gambler: betting open (meeting state {meeting.state}, window {opts.BetWindow})");

        if (opts.BetWindow is not GamblerBetWindow.Timer)
        {
            yield break;
        }

        _betDeadline = Time.time + opts.BetTime.Value;
        while (meeting && _bettingOpen && CurrentBet == NoBet && IsBettingOpen(meeting) && Time.time < _betDeadline)
        {
            yield return null;
        }

        if (!meeting || !_bettingOpen || CurrentBet != NoBet)
        {
            yield break;
        }

        Message($"Gambler: betting window over, meeting state {meeting.state}");
        _bettingOpen = false;

        // Also from a vote popup that is open right now (round 5: the button stayed there, clicking it did nothing).
        GamblerBetMeetingButton.HideAll();
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);
        _bettingOpen = false;
        if (Player.AmOwner)
        {
            GamblerBetMeetingButton.HideAll();
        }
    }

    public static bool IsBettingOpen(MeetingHud meeting)
    {
        return meeting.state is MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted;
    }

    public void PlaceBet(byte targetId)
    {
        var meeting = MeetingHud.Instance;
        if (meeting == null || !CanBetNow)
        {
            return;
        }

        Message($"Gambler: bet placed on {targetId} (meeting state {meeting.state})");
        _bettingOpen = false;

        // The bet shows up as a symbol next to the player's name and in the line under the meeting timer.
        RpcGamblerPlaceBet(Player, targetId);
    }

    /// <summary>
    ///     Name symbol of the player the local Gambler bet on in this meeting (like the Executioner's "X").
    /// </summary>
    public static bool IsLocalBetTarget(PlayerControl player)
    {
        return MeetingHud.Instance && player != null && PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.Data != null &&
               PlayerControl.LocalPlayer.Data.Role is GamblerRole { CurrentBet: not NoBet } gambler &&
               gambler.CurrentBet == player.PlayerId;
    }

    /// <summary>
    ///     Called on every client when the ejection starts. <paramref name="exiled" /> is null when no one is ejected
    ///     (skip or tie).
    /// </summary>
    public void ResolveBet(NetworkedPlayerInfo? exiled)
    {
        var bet = CurrentBet;
        CurrentBet = NoBet;
        LastResult = GamblerBetResult.None;

        if (bet == NoBet || ReachedGoal || Player.HasDied())
        {
            return;
        }

        // Ejected Gambler: dead at the resolution, the bet doesn't count.
        if (exiled != null && exiled.PlayerId == Player.PlayerId)
        {
            return;
        }

        var correct = bet == NoEjectionBet ? exiled == null : exiled != null && exiled.PlayerId == bet;
        if (correct)
        {
            Points++;
            LastResult = GamblerBetResult.Won;
            if (bet == NoEjectionBet)
            {
                NoEjectionWins++;
            }
        }
        else if (bet != NoEjectionBet && GameData.Instance.GetPlayerById(bet) is not { IsDead: false, Disconnected: false })
        {
            // The player died or left during the meeting (e.g. guessed), so they could not be ejected anymore.
            LastResult = GamblerBetResult.Void;
        }
        else
        {
            LastResult = GamblerBetResult.Lost;
            if (OptionGroupSingleton<GamblerOptions>.Instance.WrongBetLosesPoint && Points > 0)
            {
                Points--;
            }
        }

        if (Points >= PointsToWin)
        {
            ReachedGoal = true;
        }

        UpdateWarning();

        Message($"Gambler {Player.Data.PlayerName}: bet {bet}, exiled {exiled?.PlayerId.ToString(TownOfUsPlugin.Culture) ?? "none"} -> {LastResult}, points {Points}/{PointsToWin}");
    }

    // Same result on every client, because the points are.
    private void UpdateWarning()
    {
        var opts = OptionGroupSingleton<GamblerOptions>.Instance;
        var close = !ReachedGoal && Points > 0 && opts.WarnOthers is not GamblerWarning.Off &&
                    PointsLeft <= (int)opts.WarnAtPointsLeft.Value;
        if (!close)
        {
            // Dropped back (lost bet): warn again if they get close again.
            WarnedOthers = false;
            return;
        }

        if (!WarnedOthers)
        {
            WarnedOthers = true;
            PendingWarning = true;
        }
    }

    /// <summary>
    ///     Called on every client when a round starts after a meeting: the result for the Gambler, and the
    ///     "close to winning" warning (with the optional reveal) for everyone.
    /// </summary>
    public void OnRoundStart()
    {
        ShowPendingWarning();
        ShowLastResult();
    }

    private void ShowPendingWarning()
    {
        if (!PendingWarning)
        {
            return;
        }

        PendingWarning = false;
        if (Player.HasDied() || ReachedGoal)
        {
            return;
        }

        var reveal = OptionGroupSingleton<GamblerOptions>.Instance.WarnOthers is GamblerWarning.Reveal;
        if (reveal && !Player.HasModifier<GamblerRevealModifier>())
        {
            Player.AddModifier<GamblerRevealModifier>(this);
        }

        if (Player.AmOwner)
        {
            Notify(MiraLocaleManager.Get(reveal
                ? "TownOfUsMira.Role.GamblerWarnedSelfReveal"
                : "TownOfUsMira.Role.GamblerWarnedSelf"), 0.7f);
            return;
        }

        var text = MiraLocaleManager.Get(reveal
                ? "TownOfUsMira.Role.GamblerWarningReveal"
                : "TownOfUsMira.Role.GamblerWarning")
            .Replace("<player>", Player.Data.PlayerName)
            .Replace("<left>", $"{PointsLeft}");
        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Gambler, alpha: 0.5f));
        Notify(text);
    }

    /// <summary>
    ///     Shows the Gambler the result of the last bet (after the ejection screen).
    /// </summary>
    private void ShowLastResult()
    {
        if (!Player.AmOwner || LastResult == GamblerBetResult.None)
        {
            return;
        }

        var key = LastResult switch
        {
            GamblerBetResult.Won => "TownOfUsMira.Role.GamblerBetWon",
            GamblerBetResult.Lost => "TownOfUsMira.Role.GamblerBetLost",
            _ => "TownOfUsMira.Role.GamblerBetVoid"
        };
        Notify(MiraLocaleManager.Get(key).Replace("<points>", $"{Points}").Replace("<goal>", $"{PointsToWin}"));

        if (ReachedGoal && !OptionGroupSingleton<GamblerOptions>.Instance.WinEndsGame)
        {
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.GamblerGoalReached"), 0.85f);
        }

        LastResult = GamblerBetResult.None;
    }

    private static void Notify(string text, float y = 1f)
    {
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, y, -20f),
            spr: AreAssets.GamblerIcon);
        notif.AdjustNotification();
    }

    [MethodRpc((uint)TownOfUsRpc.GamblerPlaceBet)]
    public static void RpcGamblerPlaceBet(PlayerControl gambler, byte targetId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(gambler);
            return;
        }

        if (gambler.Data.Role is not GamblerRole role)
        {
            Error("RpcGamblerPlaceBet - Invalid gambler");
            return;
        }

        // Not checking the meeting state here: the Gambler checked it before sending, and a client that already got
        // the vote results a moment earlier must still accept the bet, or the points would differ between clients.
        if (MeetingHud.Instance == null || role.CurrentBet != NoBet)
        {
            Warning($"RpcGamblerPlaceBet - bet from {gambler.Data.PlayerName} ignored (no meeting or already bet)");
            return;
        }

        role.CurrentBet = targetId;
    }
}

public enum GamblerBetResult
{
    None,
    Won,
    Lost,
    Void
}
