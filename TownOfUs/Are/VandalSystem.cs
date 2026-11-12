using System.Runtime.CompilerServices;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Vandal (AmonRa Edition): breaking finished tasks at a task console.
///     <para>
///         The Vandal's client looks at the console in use range for living players whose tasks count toward the task bar
///         (the Workaholic's rule, so the Snitch's tasks are safe) and who finished a task of this console's type. Task types
///         come from the task objects, "done" from the game data: only those are right on every client (CAVEATS). Task
///         steps exist only on the owner's client, so a task with several stations (Fix Wiring) matches every station of
///         its type.
///     </para>
///     <para>
///         <see cref="VandalRole.RpcVandalBreak" /> goes to everyone. Every client marks the task as not done in the game
///         data and recounts the task bar. The owner's client puts the task object back to its first step (like the Time
///         Lord's undo) and tells the owner. From then on it's an ordinary unstarted task: the owner does it again (the
///         completion is sent the usual way) and a Workaholic can take it.
///     </para>
/// </summary>
public static class VandalSystem
{
    private const float CacheSeconds = 0.25f;

    // Vandal's client: breakable tasks per console index. The button asks every frame, the answer changes rarely.
    private static readonly Dictionary<int, (float Time, List<BreakableTask> Tasks)> Cache = [];

    /// <summary>
    ///     Whose tasks can be broken: living players whose tasks count toward the task bar, not the Snitch
    ///     (<see cref="WorkaholicSystem.IsEligibleOwner" />). Ghosts redo tasks without any risk, and the Haunter's ghost
    ///     tasks have stages of their own, so the dead are left alone.
    /// </summary>
    public static bool IsTaskOwner(PlayerControl? player, byte vandalId)
    {
        return player != null && player && !player.HasDied() && WorkaholicSystem.IsEligibleOwner(player, vandalId);
    }

    /// <summary>Vandal's client, every frame: the finished tasks this console could break (cached for a moment).</summary>
    public static bool HasBreakable(Console console, PlayerControl vandal)
    {
        var index = RiggerSystem.ConsoleIndex(console);
        if (index < 0)
        {
            return false;
        }

        if (!Cache.TryGetValue(index, out var cached) || Time.time - cached.Time > CacheSeconds ||
            Time.time < cached.Time)
        {
            cached = (Time.time, FindBreakable(console, vandal));
            Cache[index] = cached;
        }

        return cached.Tasks.Count > 0;
    }

    public static List<BreakableTask> FindBreakable(Console console, PlayerControl vandal)
    {
        var result = new List<BreakableTask>();
        if (!RiggerSystem.IsTaskConsole(console) || !ShipStatus.Instance)
        {
            return result;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!IsTaskOwner(player, vandal.PlayerId))
            {
                continue;
            }

            foreach (var task in player.myTasks.ToArray())
            {
                var normal = task != null ? task.TryCast<NormalPlayerTask>() : null;
                if (normal == null || WorkaholicSystem.IsCopyId(normal.Id) ||
                    WorkaholicSystem.UsesUpObjects(normal.TaskType) || !HasType(console, normal.TaskType))
                {
                    continue;
                }

                var info = player.Data.FindTaskById(normal.Id);
                if (info != null && info.Complete)
                {
                    result.Add(new BreakableTask(player.PlayerId, normal.Id, normal.TaskType));
                }
            }
        }

        return result;
    }

    private static bool HasType(Console console, TaskTypes type)
    {
        var types = console.TaskTypes;
        for (var i = 0; i < types.Length; i++)
        {
            if (types[i] == type)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Vandal's client ("Break" button): breaks a finished task of one random player at this console, or of everyone
    ///     (option). False if there was nothing to break any more.
    /// </summary>
    public static bool Break(PlayerControl vandal, Console console)
    {
        var index = RiggerSystem.ConsoleIndex(console);
        var tasks = index < 0 ? new List<BreakableTask>() : FindBreakable(console, vandal);
        Cache.Remove(index);
        if (tasks.Count == 0)
        {
            Warning($"Vandal: nothing to break at console {index} any more");
            return false;
        }

        if (OptionGroupSingleton<VandalOptions>.Instance.Targets.Value == VandalTargets.OneRandom)
        {
            tasks = [tasks[UnityEngine.Random.Range(0, tasks.Count)]];
        }

        for (var i = 0; i < tasks.Count; i++)
        {
            VandalRole.RpcVandalBreak(vandal, index, tasks[i].Owner, tasks[i].Task, i == 0);
        }

        var owners = tasks.Select(x => x.Owner).Distinct().Count();
        var label = TaskLabel(tasks[0].Type, console);
        Notify(owners == 1
            ? MiraLocaleManager.Get("TownOfUsMira.Role.VandalBroken").Replace("<task>", label)
            : MiraLocaleManager.Get("TownOfUsMira.Role.VandalBrokenMany").Replace("<task>", label)
                .Replace("<count>", $"{owners}"));
        return true;
    }

    /// <summary>"Admin: Swipe Card": the room of the console where it was broken, like the task list writes it.</summary>
    public static string TaskLabel(TaskTypes type, Console? console)
    {
        var task = TranslationController.Instance.GetString(type);
        return console != null ? $"{TranslationController.Instance.GetString(console.Room)}: {task}" : task;
    }

    /// <summary>The body of <see cref="VandalRole.RpcVandalBreak" />, on every client.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleBreak(PlayerControl vandal, int consoleIndex, byte ownerId, uint taskId, bool firstOfUse)
    {
        if (vandal.Data.Role is not VandalRole role)
        {
            Error("RpcVandalBreak - Invalid vandal");
            return;
        }

        // One use, one ability event, also when it breaks the task of several players (the Auditor counts it once).
        if (firstOfUse)
        {
            var touAbilityEvent = new TouAbilityEvent(AbilityType.VandalBreak, vandal);
            touAbilityEvent.InvokeEvent();
        }

        var owner = MiscUtils.PlayerById(ownerId);
        var info = owner != null && owner.Data != null ? owner.Data.FindTaskById(taskId) : null;
        if (owner == null || owner.Data == null || info == null || !info.Complete)
        {
            Warning($"Vandal: task {taskId} of player {ownerId} isn't done, nothing to break");
            return;
        }

        info.Complete = false;
        role.BrokenTasks++;
        if (GameData.Instance)
        {
            GameData.Instance.RecomputeTaskCounts();
        }

        Info($"Vandal: {vandal.Data.PlayerName} broke task {taskId} of {owner.Data.PlayerName} at console {consoleIndex}");

        if (owner.AmOwner)
        {
            ResetOwnTask(owner, taskId, RiggerSystem.ConsoleAt(consoleIndex));
        }
    }

    // Owner's client: back to the step the task started with, as if it was never touched (the Time Lord's undo and
    // MiscUtils.AdjustGhostTasks do the same). Initialize also picks the stations of the task again.
    private static void ResetOwnTask(PlayerControl owner, uint taskId, Console? console)
    {
        var task = WorkaholicSystem.FindOwnTask(owner, taskId);
        if (task == null)
        {
            Warning($"Vandal: task {taskId} of player {owner.PlayerId} not found in the task list");
            return;
        }

        if (owner == PlayerControl.LocalPlayer && Minigame.Instance && Minigame.Instance.MyTask == task)
        {
            Minigame.Instance.Close();
        }

        var start = WorkaholicSystem.InitialStep(owner.PlayerId, taskId);
        task.TimerStarted = NormalPlayerTask.TimerState.NotStarted;
        task.TaskTimer = 0f;
        task.taskStep = start;
        task.Initialize();
        task.taskStep = start;
        task.UpdateArrowAndLocation();
        Info($"Vandal: task {taskId} ({task.TaskType}) of player {owner.PlayerId} is back at step {start}/{task.MaxStep}");

        if (owner != PlayerControl.LocalPlayer)
        {
            return;
        }

        switch (OptionGroupSingleton<VandalOptions>.Instance.OwnerNotice.Value)
        {
            case VandalOwnerNotice.WithTaskName:
                Notify(MiraLocaleManager.Get("TownOfUsMira.Role.VandalYourTaskBroken")
                    .Replace("<task>", TaskLabel(task.TaskType, console)));
                break;
            case VandalOwnerNotice.WithoutTaskName:
                Notify(MiraLocaleManager.Get("TownOfUsMira.Role.VandalYourTaskBrokenAnonymous"));
                break;
        }
    }

    private static void Notify(string text)
    {
        if (!HudManager.InstanceExists || !ShipStatus.Instance ||
            (AmongUsClient.Instance && AmongUsClient.Instance.IsGameOver))
        {
            return;
        }

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.Impostor.ToTextColor()}{text}</color></b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: AreAssets.VandalIcon);
        notif.AdjustNotification();
    }

    public readonly record struct BreakableTask(byte Owner, uint Task, TaskTypes Type);
}
