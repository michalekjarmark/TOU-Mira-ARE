using System.Collections;
using System.Runtime.CompilerServices;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: the Chairman's orders. The Chairman's client times an order: it notes which of the player's tasks were done
///     (game data, synced to everyone) and waits until one more is done, the time runs out, the player dies or a meeting
///     starts (then the order is dropped without a result). The result goes to everyone
///     (<see cref="ChairmanRole.RpcChairmanOrderResult" />): every client counts the promotion and gives the player
///     <see cref="ChairmanVerdictModifier" /> (bonus or reprimand) for the next meeting, which the host applies to the
///     votes in <c>ChairmanEvents</c>.
/// </summary>
public static class ChairmanSystem
{
    private static Order? _active;

    // Every client: the emergency meetings a Chairman had left when they called the Board Meeting.
    private static readonly Dictionary<byte, int> BoardMeetingCalls = [];

    /// <summary>An order is running (Chairman's client).</summary>
    public static bool OrderActive => _active != null;

    /// <summary>The Board Meeting is being called (every client, before the meeting starts).</summary>
    public static void NoteBoardMeeting(PlayerControl chairman)
    {
        BoardMeetingCalls[chairman.PlayerId] = chairman.RemainingEmergencies;
    }

    /// <summary>A new Chairman (every client): no Board Meeting of theirs is pending.</summary>
    public static void ForgetBoardMeeting(PlayerControl chairman)
    {
        BoardMeetingCalls.Remove(chairman.PlayerId);
    }

    /// <summary>
    ///     The meeting is over (every client). The game takes one of the caller's emergency meetings whenever a meeting
    ///     without a body starts (owner's test 18: the Board Meeting used up the emergency button), so the Chairman gets
    ///     it back: the Board Meeting is a perk of its own.
    /// </summary>
    public static void RefundBoardMeeting()
    {
        foreach (var (id, before) in BoardMeetingCalls)
        {
            var chairman = MiscUtils.PlayerById(id);
            if (chairman != null && chairman.RemainingEmergencies == before - 1)
            {
                chairman.RemainingEmergencies = before;
                Message($"Chairman: {chairman.Data.PlayerName} keeps their emergency meetings ({before})");
            }
        }

        BoardMeetingCalls.Clear();
    }

    public static void CancelOrder()
    {
        if (_active != null)
        {
            Message($"Chairman: order to {_active.TargetName} dropped");
        }

        _active = null;
    }

    /// <summary>Starts an order (Chairman's client, after the button click passed the interaction checks).</summary>
    public static void StartOrder(ChairmanRole role, PlayerControl target)
    {
        var tasks = target.Data.Tasks.ToArray();
        var done = new HashSet<uint>();
        var unfinished = false;
        foreach (var task in tasks)
        {
            if (task.Complete)
            {
                done.Add(task.Id);
            }
            else
            {
                unfinished = true;
            }
        }

        if (!unfinished)
        {
            ChairmanRole.RpcChairmanOrderResult(role.Player, target, (int)ChairmanOrderResult.NoTasks);
            return;
        }

        var time = OptionGroupSingleton<ChairmanOptions>.Instance.OrderTime;
        _active = new Order(role, target, done, Time.time + time);
        ChairmanRole.RpcChairmanOrder(role.Player, target);
        Coroutines.Start(CoRunOrder(_active));
        Message($"Chairman: order to {target.Data.PlayerName} for {time} s");
    }

    private static IEnumerator CoRunOrder(Order order)
    {
        while (_active == order)
        {
            var target = order.Target;
            if (target == null || target.Data == null || target.Data.Disconnected || target.HasDied() ||
                order.Role.Player == null || order.Role.Player.HasDied())
            {
                CancelOrder();
                yield break;
            }

            if (target.Data.Tasks.ToArray().Any(x => x.Complete && !order.DoneBefore.Contains(x.Id)))
            {
                Finish(order, ChairmanOrderResult.Done);
                yield break;
            }

            if (Time.time >= order.Until)
            {
                Finish(order, ChairmanOrderResult.Slacked);
                yield break;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private static void Finish(Order order, ChairmanOrderResult result)
    {
        _active = null;
        ChairmanRole.RpcChairmanOrderResult(order.Role.Player, order.Target, (int)result);

        // The order is over before the countdown on the button: start the cooldown now.
        var button = CustomButtonSingleton<ChairmanOrderButton>.Instance;
        if (button.EffectActive)
        {
            button.EffectActive = false;
            button.Timer = button.Cooldown;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleOrder(PlayerControl chairman, PlayerControl target)
    {
        if (!target.AmOwner)
        {
            return;
        }

        var opts = OptionGroupSingleton<ChairmanOptions>.Instance;
        var key = opts.TargetSeesName ? "TownOfUsMira.Role.ChairmanOrderGivenNamed" : "TownOfUsMira.Role.ChairmanOrderGiven";
        Notify(MiraLocaleManager.Get(key)
            .Replace("<player>", chairman.Data.PlayerName)
            .Replace("<time>", Math.Round(opts.OrderTime).ToString(TownOfUsPlugin.Culture)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleResult(ChairmanRole role, PlayerControl target, ChairmanOrderResult result)
    {
        if (target == null || target.Data == null)
        {
            return;
        }

        var opts = OptionGroupSingleton<ChairmanOptions>.Instance;
        var chairman = role.Player;
        Message($"Chairman: {chairman.Data.PlayerName}'s order to {target.Data.PlayerName}: {result}");

        var rankBefore = role.Rank;
        if (result == ChairmanOrderResult.Done)
        {
            role.OrdersDone++;
        }

        var verdict = result switch
        {
            ChairmanOrderResult.Done when opts.Bonus => true,
            ChairmanOrderResult.Slacked when opts.Reprimand => false,
            _ => (bool?)null
        };

        if (verdict != null)
        {
            // A new verdict replaces the old one in place: MiraAPI removes modifiers at the end of the frame, so
            // remove + add would drop both.
            if (target.TryGetModifier<ChairmanVerdictModifier>(out var old))
            {
                old.Bonus = verdict.Value;
            }
            else
            {
                target.AddModifier<ChairmanVerdictModifier>(verdict.Value);
            }
        }

        if (chairman.AmOwner)
        {
            var key = result switch
            {
                ChairmanOrderResult.Done => "TownOfUsMira.Role.ChairmanResultDone",
                ChairmanOrderResult.Slacked => "TownOfUsMira.Role.ChairmanResultSlacked",
                _ => "TownOfUsMira.Role.ChairmanResultNoTasks"
            };
            Notify(MiraLocaleManager.Get(key).Replace("<player>", target.Data.PlayerName));

            if (result == ChairmanOrderResult.Done && opts.FreeTask)
            {
                CompleteOwnTask(chairman);
            }
        }

        if (target.AmOwner && result != ChairmanOrderResult.NoTasks)
        {
            var key = (result, verdict) switch
            {
                (ChairmanOrderResult.Done, true) => "TownOfUsMira.Role.ChairmanBonusGiven",
                (ChairmanOrderResult.Done, _) => "TownOfUsMira.Role.ChairmanOrderDone",
                (_, false) => "TownOfUsMira.Role.ChairmanReprimandGiven",
                _ => "TownOfUsMira.Role.ChairmanOrderMissed"
            };
            Notify(MiraLocaleManager.Get(key));
        }

        var rankAfter = role.Rank;
        if (rankAfter == rankBefore)
        {
            return;
        }

        var title = RankName(rankAfter);
        if (chairman.AmOwner)
        {
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.ChairmanPromoted")
                .Replace("<title>", title)
                .Replace("<perk>", MiraLocaleManager.Get($"TownOfUsMira.Role.ChairmanPerk{rankAfter}")));

            // MiraAPI shows buttons again only when the HUD is switched on (SetHudActive): show the new one now.
            CustomButtonSingleton<ChairmanMeetingButton>.Instance.SetActive(!MeetingHud.Instance, role);
        }
        else if (opts.PromotionsPublic)
        {
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.ChairmanPromotedPublic")
                .Replace("<player>", chairman.Data.PlayerName)
                .Replace("<title>", title));
        }
    }

    // Credit for someone else's work: one of the Chairman's own unfinished tasks gets done (like the Taskmaster's).
    private static void CompleteOwnTask(PlayerControl chairman)
    {
        var tasks = chairman.myTasks.ToArray()
            .Select(x => x.TryCast<NormalPlayerTask>())
            .Where(x => x != null && !x.IsComplete && chairman.Data.FindTaskById(x.Id) is { Complete: false })
            .ToList();
        if (tasks.Count == 0)
        {
            return;
        }

        var task = tasks[UnityEngine.Random.Range(0, tasks.Count)]!;
        HudManager.Instance.ShowTaskComplete();
        chairman.RpcCompleteTask(task.Id);

        // RpcCompleteTask marks the task done in the game data (task bar) but leaves the task's own steps: without
        // this the task list showed it unfinished again (like the Comfy's offloaded tasks).
        if (!task.IsComplete)
        {
            task.taskStep = task.MaxStep;
            task.UpdateArrowAndLocation();
        }
    }

    /// <summary>The highest switched-on title reached with <paramref name="ordersDone" /> done orders.</summary>
    public static ChairmanRank RankFor(int ordersDone)
    {
        var opts = OptionGroupSingleton<ChairmanOptions>.Instance;
        if (opts.King && ordersDone >= opts.KingAt.Value)
        {
            return ChairmanRank.King;
        }

        if (opts.President && ordersDone >= opts.PresidentAt.Value)
        {
            return ChairmanRank.President;
        }

        if (opts.Director && ordersDone >= opts.DirectorAt.Value)
        {
            return ChairmanRank.Director;
        }

        return ChairmanRank.Manager;
    }

    /// <summary>A perk is there when its title is switched on and reached (higher titles keep the lower perks).</summary>
    public static bool HasPerk(ChairmanRole role, ChairmanRank perk)
    {
        var opts = OptionGroupSingleton<ChairmanOptions>.Instance;
        return perk switch
        {
            ChairmanRank.Director => opts.Director && role.OrdersDone >= opts.DirectorAt.Value,
            ChairmanRank.President => opts.President && role.OrdersDone >= opts.PresidentAt.Value,
            ChairmanRank.King => opts.King && role.OrdersDone >= opts.KingAt.Value,
            _ => true
        };
    }

    public static string RankName(ChairmanRank rank)
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.ChairmanRank{rank}", rank.ToString());
    }

    /// <summary>"Manager, 1/2 to the next promotion" (task tab and meeting).</summary>
    public static string StatusText(ChairmanRole role)
    {
        var opts = OptionGroupSingleton<ChairmanOptions>.Instance;
        var next = new[]
            {
                (opts.Director, opts.DirectorAt.Value), (opts.President, opts.PresidentAt.Value),
                (opts.King, opts.KingAt.Value)
            }
            .Where(x => x.Item1 && x.Item2 > role.OrdersDone)
            .Select(x => (int)x.Item2)
            .DefaultIfEmpty(0)
            .Min();

        var title = RankName(role.Rank);
        if (next == 0)
        {
            return title;
        }

        return MiraLocaleManager.Get("TownOfUsMira.Role.ChairmanStatus")
            .Replace("<title>", title)
            .Replace("<done>", role.OrdersDone.ToString(TownOfUsPlugin.Culture))
            .Replace("<next>", next.ToString(TownOfUsPlugin.Culture));
    }

    /// <summary>Line under the meeting timer: the Chairman's title, and a bonus or reprimand of the local player.</summary>
    public static string MeetingStatusText()
    {
        var local = PlayerControl.LocalPlayer;
        var lines = new List<string>();
        if (local.Data.Role is ChairmanRole role)
        {
            lines.Add($"{TownOfUsColors.Chairman.ToTextColor()}{StatusText(role)}</color>");
        }

        if (local.TryGetModifier<ChairmanVerdictModifier>(out var verdict))
        {
            var key = verdict.Bonus ? "TownOfUsMira.Role.ChairmanBonusMeeting" : "TownOfUsMira.Role.ChairmanReprimandMeeting";
            lines.Add($"{TownOfUsColors.Chairman.ToTextColor()}{MiraLocaleManager.Get(key)}</color>");
        }

        return string.Join("\n", lines);
    }

    /// <summary>The Chairman with the "President" title sees the voters' colours (like the Prosecutor).</summary>
    public static bool LocalSeesVotes()
    {
        return PlayerControl.LocalPlayer?.Data?.Role is ChairmanRole role && HasPerk(role, ChairmanRank.President);
    }

    /// <summary>
    ///     Host, ProcessVotesEvent: a reprimanded player's vote is taken out; a bonus player's and a King's vote is
    ///     counted twice.
    /// </summary>
    public static void ApplyVotes(List<CustomVote> votes)
    {
        var doubled = new HashSet<byte>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                continue;
            }

            if (player.TryGetModifier<ChairmanVerdictModifier>(out var verdict))
            {
                if (verdict.Bonus)
                {
                    doubled.Add(player.PlayerId);
                }
                else
                {
                    var id = player.PlayerId;
                    votes.RemoveAll(v => v.Voter == id);
                }
            }

            if (player.Data.Role is ChairmanRole role && HasPerk(role, ChairmanRank.King))
            {
                doubled.Add(player.PlayerId);
            }
        }

        if (doubled.Count == 0)
        {
            return;
        }

        votes.AddRange(votes.Where(v => doubled.Contains(v.Voter)).ToList());
    }

    private static void Notify(string text)
    {
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: AreAssets.ChairmanIcon.LoadAsset());
        notif.AdjustNotification();
    }

    private sealed class Order(ChairmanRole role, PlayerControl target, HashSet<uint> doneBefore, float until)
    {
        public ChairmanRole Role { get; } = role;
        public PlayerControl Target { get; } = target;
        public string TargetName { get; } = target.Data.PlayerName;
        public HashSet<uint> DoneBefore { get; } = doneBefore;
        public float Until { get; } = until;
    }
}
