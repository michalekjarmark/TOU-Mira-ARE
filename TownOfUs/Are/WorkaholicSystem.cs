using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Workaholic (AmonRa Edition): taking over other crewmates' tasks.
///     <para>
///         Flow: the Workaholic asks the crewmate with the most tasks left (<see cref="RpcWorkaholicRequest" />). That
///         player's client picks one of its own tasks that isn't started yet (only it knows the task steps) and answers
///         (<see cref="RpcWorkaholicAssign" />). The owner's task is removed from their task list until it's done or
///         comes back, and the Workaholic gets a fresh copy of the same task. When the copy is done
///         (<see cref="RpcWorkaholicDone" />), the owner's client completes the real task itself, like the Taskmaster does,
///         so the task bar, the win check and every "task completed" effect work as usual.
///     </para>
///     <para>
///         The copy has an id the game never gives out (<see cref="CopyIdBase" /> and up) and isn't in the game data, so
///         its completion is caught before the game sends it (<see cref="WorkaholicRpcCompleteTaskPatch" />).
///     </para>
/// </summary>
public static class WorkaholicSystem
{
    /// <summary>Copies of taken tasks get ids from here up, far above the ids the game gives (0, 1, 2...).</summary>
    public const uint CopyIdBase = 0x40000000u;

    private const uint NoTask = uint.MaxValue;
    private const byte Nobody = byte.MaxValue;
    private const float RequestTimeout = 2.5f;
    private const float CheckInterval = 0.5f;

    // Tasks that use up objects on the map (towels, records, guns, vegetables, ore, shells). If the Workaholic already
    // did one of them, there could be nothing left to pick up on their screen, so these are never taken.
    private static readonly HashSet<TaskTypes> ExcludedTypes =
    [
        TaskTypes.PickUpTowels, TaskTypes.SortRecords, TaskTypes.PutAwayPistols, TaskTypes.PutAwayRifles,
        TaskTypes.CollectVegetables, TaskTypes.MineOres, TaskTypes.CollectShells
    ];

    // All clients: (owner, owner's task id) -> Workaholic doing it.
    private static readonly Dictionary<(byte Owner, uint Task), byte> Taken = [];

    // Owner's client (the client that owns the owner's PlayerControl: the owner's own game, or the host for the
    // Freeplay dummies): tasks taken by a Workaholic, out of the owner's task list until they are done or come back.
    private static readonly Dictionary<(byte Owner, uint Task), HiddenTask> Hidden = [];

    // Owner's client: the step each task started with (not every task starts at step 0).
    private static readonly Dictionary<(byte Owner, uint Task), int> InitialSteps = [];

    // All clients: completions sent by the owner's client for a Workaholic (until the completion arrives). The Auditor
    // counts these for the Workaholic, not for the owner.
    private static readonly HashSet<(byte Owner, uint Task)> HelperCompletions = [];

    // Workaholic's client.
    private static readonly List<byte> PendingQueue = [];
    private static HeldTask? held;
    private static byte pendingOwner = Nobody;
    private static float pendingSince;
    private static float nextCheck;
    private static uint nextCopyId = CopyIdBase;

    /// <summary>Workaholic's client: someone has a task to take (refreshed every half second).</summary>
    public static bool HasCandidate { get; private set; }

    /// <summary>Workaholic's client: tasks done for others in this game.</summary>
    public static int HelpedTasks { get; private set; }

    /// <summary>Workaholic's client: holding a task or waiting for an answer.</summary>
    public static bool IsBusy => held != null || pendingOwner != Nobody;

    public static bool IsCopyId(uint id)
    {
        return id is >= CopyIdBase and not NoTask;
    }

    public static bool OwnTasksDone(PlayerControl player)
    {
        return player != null && player.Data != null && player.Data.Tasks != null &&
               player.Data.Tasks.ToArray().All(t => t == null || t.Complete);
    }

    /// <summary>Start of a game and end of a game: forget everything.</summary>
    public static void Reset()
    {
        foreach (var hidden in Hidden.Values)
        {
            if (hidden.Task)
            {
                Object.Destroy(hidden.Task.gameObject);
            }
        }

        var local = PlayerControl.LocalPlayer;
        if (held != null && local)
        {
            DropCopy(local, held.Copy);
        }

        Hidden.Clear();
        Taken.Clear();
        InitialSteps.Clear();
        HelperCompletions.Clear();
        PendingQueue.Clear();
        held = null;
        pendingOwner = Nobody;
        HasCandidate = false;
        HelpedTasks = 0;
    }

    /// <summary>Remembers the starting step of the tasks of our players (start of the game and of every round).</summary>
    public static void RecordInitialSteps()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || !player.AmOwner || player.myTasks == null)
            {
                continue;
            }

            foreach (var task in player.myTasks.ToArray())
            {
                var normal = task != null ? task.TryCast<NormalPlayerTask>() : null;
                if (normal != null && !IsCopyId(normal.Id))
                {
                    InitialSteps.TryAdd((player.PlayerId, normal.Id), normal.TaskStep);
                }
            }
        }
    }

    /// <summary>
    ///     The same rules as the TOU task count (<c>LogicGameFlowPatches</c>, RecomputeTaskCounts): only tasks that count
    ///     towards the task win can be taken. The Snitch's tasks are never taken (owner's decision): finishing them
    ///     faster would reveal the impostors sooner.
    /// </summary>
    public static bool IsEligibleOwner(PlayerControl? player, byte workaholicId)
    {
        if (player == null || !player || player.PlayerId == workaholicId || player.Data == null ||
            player.Data.Disconnected || player.Data.Tasks == null || !player.TryGetComponent<ModifierComponent>(out _))
        {
            return false;
        }

        if (player.Data.IsDead && !GhostsDoTasks())
        {
            return false;
        }

        if (player.IsImpostor())
        {
            return false;
        }

        bool tasksCount;
        try
        {
            tasksCount = player.Data.Role != null && player.Data.Role.TasksCountTowardProgress;
        }
        catch
        {
            tasksCount = true;
        }

        return tasksCount && !player.HasModifier<AllianceGameModifier>(ally => !ally.DoesTasks) &&
               player.GetRoleWhenAlive() is not SnitchRole;
    }

    // ---------------------------------------------------------------- Workaholic's client

    /// <summary>"Take Task" button: ask the crewmates in order until someone has an unstarted task to give.</summary>
    public static void StartTake(PlayerControl local)
    {
        PendingQueue.Clear();
        PendingQueue.AddRange(BuildCandidates(local));
        RequestNext(local);
    }

    /// <summary>Called by the button every frame (Workaholic alive): answer timeouts and checks of the held task.</summary>
    public static void LocalTick(PlayerControl local)
    {
        if (pendingOwner != Nobody && Time.time - pendingSince > RequestTimeout)
        {
            Warning($"Workaholic: no answer from player {pendingOwner}, asking the next one");
            RequestNext(local);
        }

        if (Time.time < nextCheck)
        {
            return;
        }

        nextCheck = Time.time + CheckInterval;

        // The copy can only disappear together with the player's tasks (end of a game): nothing is held any more.
        if (held != null && !held.Copy)
        {
            held = null;
        }
        HasCandidate = BuildCandidates(local).Count > 0;

        if (held == null)
        {
            return;
        }

        // The task stopped counting (the owner left, died without "ghosts do tasks", changed team) or got done some
        // other way: give it back. Not the Workaholic's fault, so no cooldown and the use comes back.
        var owner = MiscUtils.PlayerById(held.OwnerId);
        var info = owner != null && owner.Data != null ? owner.Data.FindTaskById(held.OwnerTaskId) : null;
        if (!IsEligibleOwner(owner, local.PlayerId) || info == null || info.Complete)
        {
            Info($"Workaholic: task {held.OwnerTaskId} of player {held.OwnerId} isn't needed any more");
            RpcWorkaholicRelease(local, held.OwnerId, held.OwnerTaskId);
            GiveBackUse();
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicTaskNotNeeded"));
        }
    }

    private static List<byte> BuildCandidates(PlayerControl local)
    {
        var list = new List<(byte Id, int Count, bool Alive, float Roll)>();
        if (!ShipStatus.Instance)
        {
            return [];
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!IsEligibleOwner(player, local.PlayerId))
            {
                continue;
            }

            var count = AvailableTaskCount(player);
            if (count > 0)
            {
                list.Add((player.PlayerId, count, !player.Data.IsDead, UnityEngine.Random.value));
            }
        }

        // Most tasks left first (that's what holds the task bar back, e.g. an AFK ghost), the living win ties (their
        // time is worth more: every walk to a task is a chance to get killed).
        return list.OrderByDescending(x => x.Count).ThenByDescending(x => x.Alive).ThenBy(x => x.Roll)
            .Select(x => x.Id).ToList();
    }

    private static int AvailableTaskCount(PlayerControl owner)
    {
        var count = 0;
        foreach (var info in owner.Data.Tasks.ToArray())
        {
            if (info == null || info.Complete || Taken.ContainsKey((owner.PlayerId, info.Id)))
            {
                continue;
            }

            var prefab = ShipStatus.Instance.GetTaskById(info.TypeId);
            if (prefab != null && !ExcludedTypes.Contains(prefab.TaskType))
            {
                count++;
            }
        }

        return count;
    }

    private static void RequestNext(PlayerControl local)
    {
        if (PendingQueue.Count == 0)
        {
            pendingOwner = Nobody;

            // Everyone asked had only started tasks: nothing was taken, so the use comes back. The cooldown still
            // starts by default (owner's decision, round 9: otherwise the button can be spammed), there's an option.
            GiveBackUse(OptionGroupSingleton<WorkaholicOptions>.Instance.CooldownAfterFailedTake);
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicNothingToTake"));
            return;
        }

        pendingOwner = PendingQueue[0];
        PendingQueue.RemoveAt(0);
        pendingSince = Time.time;
        RpcWorkaholicRequest(local, pendingOwner);
    }

    // Nothing was taken (or the taken task stopped counting): the use comes back.
    private static void GiveBackUse(bool startCooldown = false)
    {
        var button = CustomButtonSingleton<WorkaholicTakeButton>.Instance;
        if (button == null)
        {
            return;
        }

        if (button.LimitedUses)
        {
            button.IncreaseUses();
        }

        button.SetTimer(startCooldown ? button.Cooldown : 0f);
    }

    private static bool CreateCopy(PlayerControl local, PlayerControl owner, uint ownerTaskId)
    {
        var info = owner.Data.FindTaskById(ownerTaskId);
        var prefab = info != null && ShipStatus.Instance ? ShipStatus.Instance.GetTaskById(info.TypeId) : null;
        if (prefab == null)
        {
            Error($"Workaholic: no task prefab for task {ownerTaskId} of player {owner.PlayerId}");
            return false;
        }

        // The same steps as the game uses when it hands out tasks at the start (PlayerControl.SetTasks).
        var copy = Object.Instantiate(prefab, local.transform);
        copy.Id = nextCopyId++;
        copy.Owner = local;
        copy.Initialize();
        local.myTasks.Add(copy);
        held = new HeldTask(owner.PlayerId, ownerTaskId, copy);

        Info($"Workaholic: took task {ownerTaskId} ({copy.TaskType}) of player {owner.PlayerId}, copy {copy.Id}");
        Notify(MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicNewTask").Replace("<task>", TaskName(copy)));
        return true;
    }

    /// <summary>The copy is done (caught in <see cref="WorkaholicRpcCompleteTaskPatch" />).</summary>
    public static void OnCopyCompleted(PlayerControl local, uint copyId)
    {
        if (held == null || held.CopyId != copyId)
        {
            Warning($"Workaholic: completed copy {copyId} isn't the held task");
            return;
        }

        var done = held;
        held = null;
        HelpedTasks++;
        Info($"Workaholic: done task {done.OwnerTaskId} of player {done.OwnerId}");
        RpcWorkaholicDone(local, done.OwnerId, done.OwnerTaskId);
        Coroutines.Start(CoRemoveCopyLater(local, done.Copy));

        var button = CustomButtonSingleton<WorkaholicTakeButton>.Instance;
        button?.SetTimer(button.Cooldown);
    }

    // The minigame is still closing when the copy is completed: remove the copy once it's closed.
    private static IEnumerator CoRemoveCopyLater(PlayerControl local, NormalPlayerTask copy)
    {
        for (var t = 0f; t < 5f && Minigame.Instance && copy && Minigame.Instance.MyTask == copy; t += Time.deltaTime)
        {
            yield return null;
        }

        if (local && copy)
        {
            local.myTasks.Remove(copy);
            Object.Destroy(copy.gameObject);
        }
    }

    private static void DropCopy(PlayerControl local, NormalPlayerTask? copy)
    {
        if (copy == null || !copy)
        {
            return;
        }

        if (Minigame.Instance && Minigame.Instance.MyTask == copy)
        {
            Minigame.Instance.Close();
        }

        local.myTasks.Remove(copy);
        Object.Destroy(copy.gameObject);
    }

    // ---------------------------------------------------------------- Owner's client

    private static uint PickTaskToGive(PlayerControl owner, PlayerControl workaholic)
    {
        if (workaholic.HasDied() || workaholic.Data.Role is not WorkaholicRole ||
            !IsEligibleOwner(owner, workaholic.PlayerId))
        {
            return NoTask;
        }

        // The task the owner is doing right now (only the local player can have a minigame open).
        var open = owner == PlayerControl.LocalPlayer && Minigame.Instance ? Minigame.Instance.MyTask : null;
        var options = new List<uint>();
        foreach (var task in owner.myTasks.ToArray())
        {
            var normal = task != null ? task.TryCast<NormalPlayerTask>() : null;
            if (normal == null || normal.IsComplete || IsCopyId(normal.Id) || (open != null && open == task) ||
                ExcludedTypes.Contains(normal.TaskType) || Taken.ContainsKey((owner.PlayerId, normal.Id)))
            {
                continue;
            }

            var info = owner.Data.FindTaskById(normal.Id);
            var start = InitialSteps.TryGetValue((owner.PlayerId, normal.Id), out var step) ? step : 0;

            // Started tasks stay with the owner: nobody loses their progress.
            if (info == null || info.Complete || normal.TaskStep != start ||
                normal.TimerStarted != NormalPlayerTask.TimerState.NotStarted)
            {
                continue;
            }

            options.Add(normal.Id);
        }

        return options.Count == 0 ? NoTask : options[UnityEngine.Random.Range(0, options.Count)];
    }

    private static PlayerTask? FindOwnTask(PlayerControl owner, uint taskId)
    {
        foreach (var task in owner.myTasks.ToArray())
        {
            if (task != null && task.Id == taskId)
            {
                return task;
            }
        }

        return null;
    }

    // Out of myTasks: no console accepts it, no arrow, no map icon, not in the vanilla task list. The task list shows it
    // greyed out instead (DecorateTaskText).
    private static PlayerTask? HideOwnTask(PlayerControl owner, uint taskId, byte workaholicId)
    {
        var task = FindOwnTask(owner, taskId);
        if (task == null)
        {
            return null;
        }

        var index = owner.myTasks.IndexOf(task);
        owner.myTasks.Remove(task);
        var normal = task.TryCast<NormalPlayerTask>();
        if (normal != null && normal.Arrow)
        {
            normal.Arrow.gameObject.SetActive(false);
        }

        task.gameObject.SetActive(false);
        Hidden[(owner.PlayerId, taskId)] = new HiddenTask(task, index, workaholicId);
        return task;
    }

    private static PlayerTask? UnhideOwnTask(PlayerControl owner, uint taskId)
    {
        if (!Hidden.Remove((owner.PlayerId, taskId), out var hidden) || !hidden.Task)
        {
            return null;
        }

        hidden.Task.gameObject.SetActive(true);
        owner.myTasks.Insert(Math.Clamp(hidden.Index, 0, owner.myTasks.Count), hidden.Task);
        var normal = hidden.Task.TryCast<NormalPlayerTask>();
        if (normal != null)
        {
            normal.UpdateArrowAndLocation();
        }

        return hidden.Task;
    }

    private static string HelperText(string anonymousKey, string namedKey, byte workaholicId)
    {
        var helper = MiscUtils.PlayerById(workaholicId);
        return OptionGroupSingleton<WorkaholicOptions>.Instance.ShowHelperName && helper != null && helper.Data != null
            ? MiraLocaleManager.Get(namedKey).Replace("<player>", helper.Data.PlayerName)
            : MiraLocaleManager.Get(anonymousKey);
    }

    // ---------------------------------------------------------------- All clients

    /// <summary>A Workaholic died, left or changed role: every task they held goes back to its owner.</summary>
    public static void ReleaseAllHeldBy(byte workaholicId)
    {
        foreach (var key in Taken.Where(x => x.Value == workaholicId).Select(x => x.Key).ToList())
        {
            Taken.Remove(key);
            ReturnToOwner(key.Owner, key.Task);
        }

        var local = PlayerControl.LocalPlayer;
        if (local && local.PlayerId == workaholicId)
        {
            if (held != null)
            {
                DropCopy(local, held.Copy);
            }

            held = null;
            pendingOwner = Nobody;
            PendingQueue.Clear();
        }
    }

    /// <summary>The owner's completion of this task was done by a Workaholic (see <see cref="HelperCompletions" />).</summary>
    public static bool WasDoneByHelper(byte ownerId, uint taskId)
    {
        return HelperCompletions.Contains((ownerId, taskId));
    }

    /// <summary>
    ///     Every completed task (after the other handlers, so the Auditor still sees <see cref="WasDoneByHelper" />). A
    ///     taken task completed outside of this system: the Workaholic's copy isn't needed any more.
    /// </summary>
    public static void OnTaskCompleted(PlayerControl owner, uint taskId)
    {
        HelperCompletions.Remove((owner.PlayerId, taskId));
        if (!Taken.Remove((owner.PlayerId, taskId), out var workaholicId))
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local && local.PlayerId == workaholicId && held != null && held.OwnerId == owner.PlayerId &&
            held.OwnerTaskId == taskId)
        {
            DropCopy(local, held.Copy);
            held = null;
            GiveBackUse();
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicTaskNotNeeded"));
        }
    }

    // On the owner's client: the task goes back on the owner's list.
    private static void ReturnToOwner(byte ownerId, uint taskId)
    {
        var owner = MiscUtils.PlayerById(ownerId);
        if (owner == null || !owner.AmOwner)
        {
            return;
        }

        var task = UnhideOwnTask(owner, taskId);
        if (task != null && owner == PlayerControl.LocalPlayer && !owner.HasDied())
        {
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicTaskReturned").Replace("<task>", TaskName(task)));
        }
    }

    /// <summary>Task list of the local player: taken tasks greyed out at the end, the held copy marked.</summary>
    public static string DecorateTaskText(string text)
    {
        var local = PlayerControl.LocalPlayer;
        if ((Hidden.Count == 0 && held == null) || !local)
        {
            return text;
        }

        var color = TownOfUsColors.Workaholic.ToTextColor();
        if (held != null && held.Copy)
        {
            var line = TaskLine(held.Copy);
            if (line.Length > 0 && text.Contains(line, StringComparison.Ordinal))
            {
                text = text.Replace(line,
                    $"{line} {color}{MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicCopySuffix")}</color>",
                    StringComparison.Ordinal);
            }
        }

        foreach (var (key, hidden) in Hidden)
        {
            if (key.Owner != local.PlayerId || !hidden.Task)
            {
                continue;
            }

            var label = HelperText("TownOfUsMira.Role.WorkaholicTakenSuffix",
                "TownOfUsMira.Role.WorkaholicTakenSuffixNamed", hidden.WorkaholicId);
            text = $"{text.TrimEnd('\n', '\r')}\n<color=#8C8C8C>{TaskName(hidden.Task)}</color> {color}{label}</color>\n";
        }

        return text;
    }

    private static string TaskLine(PlayerTask task)
    {
        var sb = new Il2CppSystem.Text.StringBuilder();
        task.AppendTaskText(sb);
        return sb.ToString().TrimEnd('\n', '\r');
    }

    /// <summary>Task name without colours and progress, e.g. "Electrical: Fix Wiring".</summary>
    public static string TaskName(PlayerTask task)
    {
        var text = Regex.Replace(TaskLine(task), "<.*?>", string.Empty);
        text = Regex.Replace(text, @" \(.*?\)", string.Empty);
        return text.Replace("\r", string.Empty).Replace("\n", " ").Trim();
    }

    private static bool GhostsDoTasks()
    {
        try
        {
            return GameOptionsManager.Instance.currentNormalGameOptions != null &&
                   GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks;
        }
        catch
        {
            return false;
        }
    }

    private static void Notify(string text)
    {
        // Roles are cleaned up when the game ends: no "task returned" popups on the end screen.
        if (!HudManager.InstanceExists || !ShipStatus.Instance ||
            (AmongUsClient.Instance && AmongUsClient.Instance.IsGameOver))
        {
            return;
        }

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.Workaholic.ToTextColor()}{text}</color></b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: AreAssets.WorkaholicIcon);
        notif.AdjustNotification();
    }

    // ---------------------------------------------------------------- RPC
    // The bodies live in [NoInlining] methods: Reactor compiles a copy of every RPC method body before MiraAPI patches
    // the getters of attribute options, so an option read directly in the RPC would always see its initial value.

    /// <summary>Workaholic asks a crewmate for a task. Only that crewmate's client answers.</summary>
    [MethodRpc((uint)TownOfUsRpc.WorkaholicRequest)]
    public static void RpcWorkaholicRequest(PlayerControl workaholic, byte ownerId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(workaholic);
            return;
        }

        HandleRequest(workaholic, ownerId);
    }

    /// <summary>Owner's answer: the given task is now the Workaholic's (<see cref="NoTask" /> = nothing to give).</summary>
    [MethodRpc((uint)TownOfUsRpc.WorkaholicAssign)]
    public static void RpcWorkaholicAssign(PlayerControl owner, byte workaholicId, uint taskId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(owner);
            return;
        }

        HandleAssign(owner, workaholicId, taskId);
    }

    /// <summary>The Workaholic finished the copy: the owner's client completes the real task.</summary>
    [MethodRpc((uint)TownOfUsRpc.WorkaholicDone)]
    public static void RpcWorkaholicDone(PlayerControl workaholic, byte ownerId, uint taskId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(workaholic);
            return;
        }

        HandleDone(workaholic, ownerId, taskId);
    }

    /// <summary>The Workaholic gives a task back (it stopped counting, or an answer came too late).</summary>
    [MethodRpc((uint)TownOfUsRpc.WorkaholicRelease)]
    public static void RpcWorkaholicRelease(PlayerControl workaholic, byte ownerId, uint taskId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(workaholic);
            return;
        }

        HandleRelease(workaholic, ownerId, taskId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleRequest(PlayerControl workaholic, byte ownerId)
    {
        // Answered by the client that owns the asked player (their own game; in Freeplay also the dummies).
        var owner = MiscUtils.PlayerById(ownerId);
        if (owner == null || !owner.AmOwner)
        {
            return;
        }

        var taskId = PickTaskToGive(owner, workaholic);
        Info(taskId == NoTask
            ? $"Workaholic: player {owner.PlayerId} has no unstarted task for {workaholic.PlayerId}"
            : $"Workaholic: player {owner.PlayerId} gives task {taskId} to {workaholic.PlayerId}");
        RpcWorkaholicAssign(owner, workaholic.PlayerId, taskId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleAssign(PlayerControl owner, byte workaholicId, uint taskId)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local)
        {
            return;
        }

        if (taskId == NoTask)
        {
            if (local.PlayerId == workaholicId && pendingOwner == owner.PlayerId)
            {
                RequestNext(local);
            }

            return;
        }

        // A Workaholic who died meanwhile gets nothing (the same check on every client).
        var workaholic = MiscUtils.PlayerById(workaholicId);
        if (workaholic == null || workaholic.HasDied() || workaholic.Data.Role is not WorkaholicRole ||
            !Taken.TryAdd((owner.PlayerId, taskId), workaholicId))
        {
            return;
        }

        if (owner.AmOwner)
        {
            var task = HideOwnTask(owner, taskId, workaholicId);
            if (task != null && owner == local)
            {
                Notify(HelperText("TownOfUsMira.Role.WorkaholicYourTaskTaken",
                    "TownOfUsMira.Role.WorkaholicYourTaskTakenNamed", workaholicId).Replace("<task>", TaskName(task)));
            }
        }

        if (workaholic != local)
        {
            return;
        }

        if (held == null && pendingOwner == owner.PlayerId && CreateCopy(workaholic, owner, taskId))
        {
            pendingOwner = Nobody;
            PendingQueue.Clear();
            return;
        }

        // Late answer (already asked someone else) or the task couldn't be copied: give it back.
        RpcWorkaholicRelease(workaholic, owner.PlayerId, taskId);
        if (pendingOwner == owner.PlayerId)
        {
            RequestNext(workaholic);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleDone(PlayerControl workaholic, byte ownerId, uint taskId)
    {
        if (!Taken.TryGetValue((ownerId, taskId), out var holder) || holder != workaholic.PlayerId)
        {
            Warning($"Workaholic: {workaholic.PlayerId} finished task {taskId} of player {ownerId} it didn't hold");
            return;
        }

        Taken.Remove((ownerId, taskId));

        // Every client: the owner's completion that follows was done by the Workaholic, and an Auditor auditing the
        // Workaholic sees the task done right now, where the Workaholic stands.
        HelperCompletions.Add((ownerId, taskId));
        AuditorEvents.OnTaskDoneByWorkaholic(workaholic);

        var owner = MiscUtils.PlayerById(ownerId);
        if (owner == null || !owner.AmOwner)
        {
            return;
        }

        var task = UnhideOwnTask(owner, taskId) ?? FindOwnTask(owner, taskId);
        var info = owner.Data.FindTaskById(taskId);
        if (task == null || info == null || info.Complete)
        {
            return;
        }

        // Sent by the owner's client, like the Taskmaster's completion: counts for the owner everywhere.
        owner.RpcCompleteTask(taskId);
        if (owner == PlayerControl.LocalPlayer)
        {
            Notify(HelperText("TownOfUsMira.Role.WorkaholicYourTaskDone",
                "TownOfUsMira.Role.WorkaholicYourTaskDoneNamed", workaholic.PlayerId).Replace("<task>", TaskName(task)));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleRelease(PlayerControl workaholic, byte ownerId, uint taskId)
    {
        if (!Taken.TryGetValue((ownerId, taskId), out var holder) || holder != workaholic.PlayerId)
        {
            return;
        }

        Taken.Remove((ownerId, taskId));
        ReturnToOwner(ownerId, taskId);

        var local = PlayerControl.LocalPlayer;
        if (local && local.PlayerId == workaholic.PlayerId && held != null && held.OwnerId == ownerId &&
            held.OwnerTaskId == taskId)
        {
            DropCopy(local, held.Copy);
            held = null;
        }
    }

    private sealed class HiddenTask(PlayerTask task, int index, byte workaholicId)
    {
        public PlayerTask Task { get; } = task;
        public int Index { get; } = index;
        public byte WorkaholicId { get; } = workaholicId;
    }

    private sealed class HeldTask(byte ownerId, uint ownerTaskId, NormalPlayerTask copy)
    {
        public byte OwnerId { get; } = ownerId;
        public uint OwnerTaskId { get; } = ownerTaskId;
        public NormalPlayerTask Copy { get; } = copy;
        public uint CopyId { get; } = copy.Id;
    }
}

/// <summary>
///     The Workaholic's copy of a task is done: tell the owner instead of sending a completion of a task the game doesn't
///     know (<see cref="WorkaholicSystem.CopyIdBase" />). NormalPlayerTask.NextStep calls this from native code, which
///     the Harmony patch catches.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcCompleteTask))]
public static class WorkaholicRpcCompleteTaskPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(PlayerControl __instance, uint idx)
    {
        if (!WorkaholicSystem.IsCopyId(idx))
        {
            return true;
        }

        if (__instance.AmOwner)
        {
            WorkaholicSystem.OnCopyCompleted(__instance, idx);
        }

        return false;
    }
}

/// <summary>Safety net: a copy's id must never reach the game data.</summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class WorkaholicCompleteTaskPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(uint idx)
    {
        return !WorkaholicSystem.IsCopyId(idx);
    }
}

/// <summary>
///     New HUD = lobby or Freeplay loaded: forget the previous game. Freeplay has no intro and no game end event, so
///     without this a held task from the previous Freeplay session would block the button.
/// </summary>
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class WorkaholicHudStartPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        WorkaholicSystem.Reset();
    }
}

/// <summary>Local task list: taken tasks greyed out, the Workaholic's copy marked.</summary>
[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
public static class WorkaholicTaskTextPatch
{
    [HarmonyPrefix]
    public static void Prefix(TaskPanelBehaviour __instance, ref string str)
    {
        if (HudManager.InstanceExists && __instance == HudManager.Instance.TaskPanel)
        {
            str = WorkaholicSystem.DecorateTaskText(str);
        }
    }
}
