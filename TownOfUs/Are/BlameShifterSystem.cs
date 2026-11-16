using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Blame Shifter (AmonRa Edition): possession and the win.
///     <list type="bullet">
///         <item>
///             Possession: <see cref="BlameShifterPossessedModifier" /> on every client; on the possessed player's own
///             client their velocity is replaced (after the normal physics step, like the Sleepwalker's) by walking
///             towards the Blame Shifter's ghost.
///         </item>
///         <item>
///             The win: every client reads the final votes and the ejected player from
///             <c>MeetingHud.VotingComplete</c>, so all of them agree without an extra RPC.
///         </item>
///     </list>
/// </summary>
public static class BlameShifterSystem
{
    // Close enough to the ghost: stand still.
    private const float StopDistance = 0.35f;

    public static bool IsPossessed(PlayerControl? player)
    {
        return player != null && player.HasModifier<BlameShifterPossessedModifier>();
    }

    /// <summary>The player who killed <paramref name="victim" />, if the game knows one.</summary>
    public static byte? KillerOf(PlayerControl victim)
    {
        var kill = GameHistory.KilledPlayers.LastOrDefault(x => x.VictimId == victim.PlayerId);
        return kill == null || kill.KillerId == victim.PlayerId ? null : kill.KillerId;
    }

    /// <summary>The blamed player can be possessed right now (Blame Shifter's client).</summary>
    public static bool CanPossess(PlayerControl shifter, PlayerControl? target)
    {
        return target != null && target.Data != null && !target.Data.Disconnected && !target.HasDied() &&
               !target.inVent && !target.onLadder && !target.inMovingPlat && !target.walkingToVent &&
               !IsPossessed(target) &&
               Vector2.Distance(shifter.transform.position, target.transform.position) <=
               OptionGroupSingleton<BlameShifterOptions>.Instance.PossessRange;
    }

    public static void HandlePossess(PlayerControl shifter, PlayerControl target)
    {
        if (target == null || target.HasDied())
        {
            return;
        }

        var duration = OptionGroupSingleton<BlameShifterOptions>.Instance.PossessDuration;
        target.AddModifier<BlameShifterPossessedModifier>(shifter.PlayerId, duration);
        Info($"BlameShifter: {shifter.Data.PlayerName} possessed {target.Data.PlayerName} for {duration} s");

        if (!target.AmOwner)
        {
            return;
        }

        // Out of whatever they were doing.
        if (Minigame.Instance != null)
        {
            Minigame.Instance.Close();
        }

        if (MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
        {
            MapBehaviour.Instance.Close();
        }

        if (OptionGroupSingleton<BlameShifterOptions>.Instance.VictimNotified)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.BlameShifter));
            var notif = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterPossessed")}</b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: AreAssets.BlameShifterIcon.LoadAsset());
            notif.AdjustNotification();
        }
    }

    /// <summary>The line in the Blame Shifter's task tab.</summary>
    public static string StatusText(BlameShifterRole role)
    {
        if (role.Won)
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterStatusWon");
        }

        if (role.Blamed == null || role.Blamed.HasDied())
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterStatusNone");
        }

        var opts = OptionGroupSingleton<BlameShifterOptions>.Instance;
        var text = MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterStatus")
            .Replace("<player>", role.Blamed.Data.PlayerName);
        if (opts.WinMode == BlameShifterWinMode.Votes)
        {
            text += " " + MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterStatusVotes")
                .Replace("<votes>", role.Votes.ToString(TownOfUsPlugin.Culture))
                .Replace("<needed>", ((int)opts.VotesNeeded.Value).ToString(TownOfUsPlugin.Culture));
        }

        return text;
    }

    /// <summary>After the votes (every client, from <see cref="VotesPatch" />): ejected, or the votes counted.</summary>
    public static void CountVotes(Il2CppStructArray<MeetingHud.VoterState> states, NetworkedPlayerInfo? exiled)
    {
        var opts = OptionGroupSingleton<BlameShifterOptions>.Instance;
        foreach (var role in CustomRoleUtils.GetActiveRoles().OfType<BlameShifterRole>())
        {
            var blamed = role.Blamed;
            if (role.Won || blamed == null || blamed.Data == null || blamed.Data.IsDead || role.Player == null)
            {
                continue;
            }

            if (opts.WinMode == BlameShifterWinMode.Ejected)
            {
                role.Won = exiled != null && exiled.PlayerId == blamed.PlayerId;
            }
            else
            {
                if (states != null)
                {
                    foreach (var state in states)
                    {
                        if (!state.SkippedVote && state.VotedForId == blamed.PlayerId)
                        {
                            role.Votes++;
                        }
                    }
                }

                role.Won = role.Votes >= (int)opts.VotesNeeded.Value;
            }

            Info($"BlameShifter: {role.Player.Data.PlayerName} - {blamed.Data.PlayerName} has {role.Votes} votes, " +
                 $"ejected: {exiled?.PlayerId == blamed.PlayerId}, won: {role.Won}");
            if (role.Won && role.Player.AmOwner)
            {
                var notif = Helpers.CreateAndShowNotification(
                    $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterWon")}</b>", Color.white,
                    new Vector3(0f, 1f, -20f), spr: AreAssets.BlameShifterIcon.LoadAsset());
                notif.AdjustNotification();
            }
        }
    }

    /// <summary>The final votes and the ejected player, on every client.</summary>
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static class VotesPatch
    {
        public static void Prefix([HarmonyArgument(0)] Il2CppStructArray<MeetingHud.VoterState> states,
            [HarmonyArgument(1)] NetworkedPlayerInfo? exiled)
        {
            CountVotes(states, exiled);
        }
    }

    /// <summary>The possessed player's own client: walk towards the ghost instead of following the input.</summary>
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PossessionMovementPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            var player = __instance.myPlayer;
            if (__instance.body == null || player == null || !player.AmOwner ||
                !player.TryGetModifier<BlameShifterPossessedModifier>(out var possession))
            {
                return;
            }

            var shifter = MiscUtils.PlayerById(possession.ShifterId);
            if (shifter == null || MeetingHud.Instance || player.inVent || player.onLadder || player.inMovingPlat ||
                player.walkingToVent)
            {
                return;
            }

            var toGhost = (Vector2)(shifter.transform.position - player.transform.position);
            __instance.body.velocity = toGhost.magnitude <= StopDistance
                ? Vector2.zero
                : toGhost.normalized * Mathf.Max(__instance.TrueSpeed, 0.01f);
        }
    }
}
