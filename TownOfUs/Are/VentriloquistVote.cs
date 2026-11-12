using System.Runtime.CompilerServices;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Voting;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Are;

/// <summary>
///     Vote steal of the Ventriloquist (AmonRa Edition, lobby option). In the meeting the Ventriloquist picks a living
///     player or Skip Vote with the "Steal Vote" button in the vote popup (before their own vote, like the Gambler's bet).
///     When the host counts the votes, every vote of the eavesdropped player goes there instead. That player votes as
///     usual and sees nothing during the meeting; with open voting their icon shows up at the chosen player in the
///     results, so they can notice, but not who did it. A player who hasn't voted counts as one vote (three for a revealed
///     Mayor). In a meeting with a prosecution the stolen vote is void.
/// </summary>
public static class VentriloquistVote
{
    public const byte NoSteal = byte.MaxValue;
    public const byte SkipVote = 253;

    public static VentriloquistVoteSteal Mode => OptionGroupSingleton<VentriloquistOptions>.Instance.VoteSteal.Value;

    /// <summary>Whether the Ventriloquist can still steal the voice's vote in this meeting.</summary>
    public static bool CanSteal(VentriloquistRole role)
    {
        var mode = Mode;
        return mode != VentriloquistVoteSteal.Off && MeetingHud.Instance && role.Player != null &&
               !role.Player.HasDied() && role.StolenVoteFor == NoSteal &&
               VentriloquistChat.CanBeVoiced(role.VoicePlayer) &&
               !(mode == VentriloquistVoteSteal.InsteadOfChat && role.MessagesSent > 0);
    }

    /// <summary>Where the stolen vote can go: Skip Vote or any living player (also the voice or the Ventriloquist).</summary>
    public static bool IsValidTarget(PlayerVoteArea area)
    {
        if (area == null)
        {
            return false;
        }

        if (area.PlayerId == SkipVote)
        {
            return true;
        }

        if (area.AmDead)
        {
            return false;
        }

        var data = GameData.Instance.GetPlayerById(area.PlayerId);
        return data != null && !data.Disconnected && !data.IsDead;
    }

    /// <summary>The name of a vote target for texts ("skip" for Skip Vote).</summary>
    public static string TargetName(byte targetId)
    {
        return targetId == SkipVote
            ? MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistSkip")
            : GameData.Instance.GetPlayerById(targetId)?.PlayerName ?? "?";
    }

    /// <summary>
    ///     Body of <see cref="VentriloquistRole.RpcVentriloquistStealVote" /> on every client (reads options, see CAVEATS).
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleSteal(PlayerControl source, byte targetId)
    {
        if (source.Data.Role is not VentriloquistRole role || !CanSteal(role))
        {
            Error("RpcVentriloquistStealVote - the vote can't be stolen now");
            return;
        }

        if (targetId != SkipVote && GameData.Instance.GetPlayerById(targetId) is not { IsDead: false, Disconnected: false })
        {
            Error($"RpcVentriloquistStealVote - invalid target {targetId}");
            return;
        }

        role.StolenVoteFor = targetId;
        Message($"Ventriloquist: {source.Data.PlayerName} stole the vote of {role.VoicePlayer?.Data.PlayerName} " +
                $"for {TargetName(targetId)}");
    }

    /// <summary>
    ///     Host, when counting: moves the voice's votes. Called first (before the Auditor's freeze, so a frozen vote stays
    ///     frozen, and before the Tiebreaker, the hidden Knighted votes and the Swapper, which work on these votes).
    /// </summary>
    public static void ApplySteals(List<CustomVote> votes)
    {
        foreach (var role in CustomRoleUtils.GetActiveRolesOfType<VentriloquistRole>())
        {
            var targetId = role.StolenVoteFor;
            if (targetId == NoSteal)
            {
                continue;
            }

            var voice = role.VoicePlayer;
            if (role.Player == null || role.Player.HasDied() || !VentriloquistChat.CanBeVoiced(voice) ||
                (targetId != SkipVote && GameData.Instance.GetPlayerById(targetId) is not { IsDead: false }))
            {
                Message($"Ventriloquist: stolen vote void (Ventriloquist, voice or target dead or silenced)");
                continue;
            }

            // A prosecution can't be stolen (owner's decision): its five votes stay on the Prosecutor's pick, and every
            // other vote is gone anyway, so the voice has nothing to give either.
            if (IsProsecution())
            {
                Message("Ventriloquist: stolen vote void (prosecution)");
                continue;
            }

            // Not voted: the vote they would have cast. Don't ask VotesRemaining whether they could: when the voting
            // time runs out MiraAPI sets it to 0 for everyone (CAVEATS).
            var cast = votes.RemoveAll(v => v.Voter == voice!.PlayerId);
            var notVoted = voice!.Data.Role is MayorRole { Revealed: true } ? 3 : 1;
            var count = cast > 0 ? cast : notVoted;
            for (var i = 0; i < count; i++)
            {
                votes.Add(new CustomVote(voice!.PlayerId, targetId));
            }

            Message($"Ventriloquist: {count} vote(s) of {voice!.Data.PlayerName} go to {TargetName(targetId)} " +
                    $"(had cast {cast})");
        }
    }

    // The condition of ProsecutorEvents.VoteEvent, which clears every vote but the Prosecutor's five. HasProsecuted and
    // ProsecutionsCompleted change only at the ejection, so they still hold while the votes are counted.
    private static bool IsProsecution()
    {
        var max = OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
        return CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>().Any(x =>
            x.Player != null && !x.Player.HasDied() && x.HasProsecuted && x.ProsecuteVictim != byte.MaxValue &&
            x.ProsecutionsCompleted < max);
    }
}
