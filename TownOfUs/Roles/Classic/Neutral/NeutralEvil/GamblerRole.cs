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
    public const byte NoEjectionBet = GamblerBetMenu.SkipId;

    private GamblerBetMenu? _betMenu;

    public int Points { get; set; }

    /// <summary>Player id the Gambler bet on this meeting, <see cref="NoEjectionBet" /> or <see cref="NoBet" />.</summary>
    public byte CurrentBet { get; set; } = NoBet;

    public bool ReachedGoal { get; set; }

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

        _betMenu?.Hide();
        _betMenu = null;

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

        if (!Player.AmOwner)
        {
            return;
        }

        var meeting = MeetingHud.Instance;
        if (meeting == null || Player.HasDied() || Player.IsJailed() || ReachedGoal)
        {
            return;
        }

        _betMenu ??= new GamblerBetMenu(PlaceBet);
        _betMenu.Show(meeting, CanBetOn, OptionGroupSingleton<GamblerOptions>.Instance.CanBetOnNoEjection);
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);
        _betMenu?.Hide();
    }

    private bool CanBetOn(PlayerVoteArea area)
    {
        if (area.PlayerId == Player.PlayerId || area.AmDead)
        {
            return false;
        }

        var data = GameData.Instance.GetPlayerById(area.PlayerId);
        return data != null && !data.Disconnected && !data.IsDead;
    }

    public static bool IsBettingOpen(MeetingHud meeting)
    {
        return meeting.state is MeetingHud.MeetingStates.Discussion or MeetingHud.MeetingStates.NotVoted
            or MeetingHud.MeetingStates.Voted;
    }

    private void PlaceBet(byte targetId)
    {
        var meeting = MeetingHud.Instance;
        if (meeting == null || CurrentBet != NoBet || !IsBettingOpen(meeting) || Player.HasDied())
        {
            return;
        }

        RpcGamblerPlaceBet(Player, targetId);
        _betMenu?.Lock(targetId);

        var text = targetId == NoEjectionBet
            ? MiraLocaleManager.Get("TownOfUsMira.Role.GamblerBetPlacedNoEjection")
            : MiraLocaleManager.Get("TownOfUsMira.Role.GamblerBetPlaced")
                .Replace("<player>", GameData.Instance.GetPlayerById(targetId)?.PlayerName ?? "?");
        Notify(text);
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
