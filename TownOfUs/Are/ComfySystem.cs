using System.Collections;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Comfy (AmonRa Edition, Crewmate Support, the owner's idea): when things don't go their way, they get annoyed.
///     <para>
///         Closing an own task before finishing it (no step done, no timer started) locks that task for the Comfy for a
///         while (<see cref="ComfyOptions.LockDuration" />) and, if the sulk is ready, starts a sulk
///         (<see cref="ComfySulkModifier" />): kills and abilities on the Comfy fail, but they can't do tasks or report.
///         Watching the task window and the locks happen on the Comfy's client only.
///     </para>
///     <para>
///         "Offload Task" gives the task that is locked right now to a player nearby (<see cref="RpcComfyOffload" />): the
///         Comfy's task leaves their list (it still counts in the game data) and the receiver's client gets a copy, the
///         same way as the Workaholic's copies (ids from <see cref="CopyIdBase" />, inside
///         <see cref="WorkaholicSystem.IsCopyId" />). When the receiver finishes the copy
///         (<see cref="RpcComfyOffloadDone" />), the Comfy's client completes the real task. A task that isn't done comes
///         back to the Comfy at the start of the next meeting (option), so nobody can hold the crew's task win.
///     </para>
/// </summary>
public static class ComfySystem
{
    /// <summary>Copies of offloaded tasks get ids from here up (above the Workaholic's copies, see CopyIdBase there).</summary>
    public const uint CopyIdBase = 0x48000000u;

    // A meeting or a death closes the task window too: wait a moment before calling it the Comfy's own doing.
    private const float CloseCheckDelay = 0.3f;

    // All clients: offloaded tasks that aren't done yet.
    private static readonly List<Offload> Offloads = [];

    // Comfy's client: own tasks locked after closing them unfinished, the most recent last (task id, lock end time).
    private static readonly List<(uint TaskId, float Until)> Locks = [];

    // Comfy's client: own tasks that were offloaded, out of the task list until they are done or come back.
    private static readonly Dictionary<(byte Comfy, uint Task), HiddenTask> Hidden = [];

    // Comfy's client: what happened to the offloaded tasks since the last meeting (line under the meeting timer).
    private static readonly List<string> Results = [];

    private static float sulkReadyAt;
    private static uint nextCopyId = CopyIdBase;

    public static bool IsComfyCopyId(uint id)
    {
        return id is >= CopyIdBase and not uint.MaxValue;
    }

    /// <summary>Start and end of a game, new HUD (lobby, Freeplay): forget everything.</summary>
    public static void Reset()
    {
        foreach (var offload in Offloads)
        {
            if (offload.Copy != null && offload.Copy)
            {
                Object.Destroy(offload.Copy.gameObject);
            }
        }

        foreach (var hidden in Hidden.Values)
        {
            if (hidden.Task)
            {
                Object.Destroy(hidden.Task.gameObject);
            }
        }

        Offloads.Clear();
        Hidden.Clear();
        Locks.Clear();
        Results.Clear();
        sulkReadyAt = 0f;
    }

    // ---------------------------------------------------------------- Comfy's client: annoyance, locks, sulk

    private static bool IsLocalComfy(out PlayerControl local)
    {
        local = PlayerControl.LocalPlayer;
        return local && local.Data != null && !local.HasDied() && local.Data.Role is ComfyRole;
    }

    /// <summary>Console.Use postfix: the Comfy opened one of their own tasks, watch how the window gets closed.</summary>
    public static void OnConsoleUsed()
    {
        if (!IsLocalComfy(out var local))
        {
            return;
        }

        var minigame = Minigame.Instance;
        var task = minigame ? minigame.MyNormTask : null;
        if (minigame == null || task == null || task.Owner != local || task.IsComplete ||
            WorkaholicSystem.IsCopyId(task.Id))
        {
            return;
        }

        // A timed task that is still waiting (e.g. Inspect Sample): looking at it isn't a try.
        if (task.TimerStarted == NormalPlayerTask.TimerState.Started)
        {
            return;
        }

        Coroutines.Start(CoWatchTaskWindow(minigame, task));
    }

    private static IEnumerator CoWatchTaskWindow(Minigame minigame, NormalPlayerTask task)
    {
        var startStep = task.TaskStep;
        var startTimer = task.TimerStarted;
        while (minigame && Minigame.Instance == minigame && minigame.amClosing == Minigame.CloseState.None)
        {
            yield return null;
        }

        for (var t = 0f; t < CloseCheckDelay; t += Time.deltaTime)
        {
            yield return null;
        }

        if (!task || !IsLocalComfy(out var local) || MeetingHud.Instance || ExileController.Instance)
        {
            yield break;
        }

        // A step done, the task finished or its timer started: that was progress, nothing to be annoyed about.
        if (task.IsComplete || task.TaskStep != startStep || task.TimerStarted != startTimer)
        {
            yield break;
        }

        OnAnnoyed(local, task);
    }

    private static void OnAnnoyed(PlayerControl local, NormalPlayerTask task)
    {
        var opts = OptionGroupSingleton<ComfyOptions>.Instance;
        if (opts.LockDuration > 0f)
        {
            Locks.RemoveAll(x => x.TaskId == task.Id);
            Locks.Add((task.Id, Time.time + opts.LockDuration));
        }

        var sulk = opts.SulkDuration > 0f && Time.time >= sulkReadyAt && !local.HasModifier<ComfySulkModifier>();
        if (sulk)
        {
            local.RpcAddModifier<ComfySulkModifier>();
            sulkReadyAt = Time.time + opts.SulkDuration + opts.SulkCooldown;
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Comfy));
        }

        Info($"Comfy: task {task.Id} ({task.TaskType}) closed unfinished, locked for {opts.LockDuration}s, sulk {sulk}");
    }

    /// <summary>Comfy's client, every frame (from the button): forget the locks that ran out.</summary>
    public static void LocalTick()
    {
        if (Locks.Count > 0)
        {
            Locks.RemoveAll(x => Time.time >= x.Until);
        }
    }

    private static bool IsLocked(uint taskId)
    {
        foreach (var (id, until) in Locks)
        {
            if (id == taskId && Time.time < until)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     PlayerCanUseEvent (local player): a locked task's console can't be used by the Comfy, and no task console
    ///     during a sulk (option). Sabotage consoles, doors and ladders stay usable.
    /// </summary>
    public static bool BlocksConsole(IUsable usable)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.Data.Role is not ComfyRole)
        {
            return false;
        }

        var console = usable.TryCast<Console>();
        if (console == null || !RiggerSystem.IsTaskConsole(console))
        {
            return false;
        }

        if (OptionGroupSingleton<ComfyOptions>.Instance.SulkBlocksTasks && local.HasModifier<ComfySulkModifier>())
        {
            return true;
        }

        if (Locks.Count == 0)
        {
            return false;
        }

        var task = console.FindTask(local);
        return task != null && IsLocked(task.Id);
    }

    /// <summary>ReportBodyEvent: no reports while sulking (options for bodies and for the meeting button).</summary>
    public static bool BlocksReport(PlayerControl? reporter, bool emergency)
    {
        if (reporter == null || !reporter.HasModifier<ComfySulkModifier>())
        {
            return false;
        }

        var opts = OptionGroupSingleton<ComfyOptions>.Instance;
        return emergency ? opts.SulkBlocksMeetingButton : opts.SulkBlocksReports;
    }

    /// <summary>Role tab of the Comfy: whether the sulk is ready.</summary>
    public static string SulkStatusText(PlayerControl local)
    {
        if (OptionGroupSingleton<ComfyOptions>.Instance.SulkDuration <= 0f)
        {
            return string.Empty;
        }

        if (local.HasModifier<ComfySulkModifier>())
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.ComfySulkActive");
        }

        var left = sulkReadyAt - Time.time;
        return left <= 0f
            ? MiraLocaleManager.Get("TownOfUsMira.Role.ComfySulkReady")
            : MiraLocaleManager.Get("TownOfUsMira.Role.ComfySulkCooldown")
                .Replace("<time>", $"{Mathf.CeilToInt(left)}");
    }

    // ---------------------------------------------------------------- Comfy's client: offloading

    /// <summary>The most recently locked task that can be offloaded (not one that uses up objects on the map).</summary>
    private static NormalPlayerTask? TaskToOffload(PlayerControl local)
    {
        for (var i = Locks.Count - 1; i >= 0; i--)
        {
            var (id, until) = Locks[i];
            if (Time.time >= until)
            {
                continue;
            }

            var task = WorkaholicSystem.FindOwnTask(local, id);
            var info = local.Data.FindTaskById(id);
            if (task == null || task.IsComplete || info == null || info.Complete ||
                WorkaholicSystem.UsesUpObjects(task.TaskType))
            {
                continue;
            }

            return task;
        }

        return null;
    }

    public static bool HasTaskToOffload(PlayerControl local)
    {
        return Locks.Count > 0 && TaskToOffload(local) != null;
    }

    /// <summary>"Offload Task" button.</summary>
    public static void StartOffload(PlayerControl local, PlayerControl receiver)
    {
        var task = TaskToOffload(local);
        if (task == null)
        {
            Warning("Comfy: nothing to offload");
            return;
        }

        RpcComfyOffload(local, receiver.PlayerId, task.Id, (int)task.TaskType);
    }

    // Out of myTasks: no console accepts it, no arrow, no map icon. The task list shows it greyed out instead.
    private static void HideOwnTask(PlayerControl comfy, uint taskId, byte receiverId)
    {
        var task = WorkaholicSystem.FindOwnTask(comfy, taskId);
        if (task == null)
        {
            return;
        }

        var index = comfy.myTasks.IndexOf(task);
        comfy.myTasks.Remove(task);
        if (task.Arrow)
        {
            task.Arrow.gameObject.SetActive(false);
        }

        task.gameObject.SetActive(false);
        Hidden[(comfy.PlayerId, taskId)] = new HiddenTask(task, index, receiverId);
    }

    private static NormalPlayerTask? UnhideOwnTask(PlayerControl comfy, uint taskId)
    {
        if (!Hidden.Remove((comfy.PlayerId, taskId), out var hidden) || !hidden.Task)
        {
            return null;
        }

        hidden.Task.gameObject.SetActive(true);
        comfy.myTasks.Insert(Math.Clamp(hidden.Index, 0, comfy.myTasks.Count), hidden.Task);
        hidden.Task.UpdateArrowAndLocation();
        return hidden.Task;
    }

    // ---------------------------------------------------------------- Receiver's client: the copy

    private static NormalPlayerTask? CreateCopy(PlayerControl receiver, PlayerControl comfy, uint taskId, TaskTypes type)
    {
        // The exact variant only comes from the game data on the host (CAVEATS), otherwise any task of that type.
        var info = comfy.Data.FindTaskById(taskId);
        var prefab = info != null && ShipStatus.Instance ? ShipStatus.Instance.GetTaskById(info.TypeId) : null;
        if (prefab == null || prefab.TaskType != type)
        {
            prefab = WorkaholicSystem.FindTaskPrefab(type);
        }

        if (prefab == null)
        {
            Error($"Comfy: no task prefab for task {taskId} ({type}) of player {comfy.PlayerId}");
            return null;
        }

        // The same steps as the game uses when it hands out tasks at the start (PlayerControl.SetTasks).
        var copy = Object.Instantiate(prefab, receiver.transform);
        copy.Id = nextCopyId++;
        copy.Owner = receiver;
        copy.Initialize();
        receiver.myTasks.Add(copy);
        Info($"Comfy: player {receiver.PlayerId} got task {taskId} ({type}) of player {comfy.PlayerId}, copy {copy.Id}");
        return copy;
    }

    /// <summary>The receiver finished a copy (caught in <see cref="WorkaholicRpcCompleteTaskPatch" />).</summary>
    public static void OnCopyCompleted(PlayerControl receiver, uint copyId)
    {
        var offload = Offloads.Find(x => x.CopyId == copyId);
        if (offload == null)
        {
            Warning($"Comfy: completed copy {copyId} isn't an offloaded task");
            return;
        }

        RpcComfyOffloadDone(receiver, offload.ComfyId, offload.TaskId);
        if (offload.Copy != null)
        {
            Coroutines.Start(CoRemoveCopyLater(receiver, offload.Copy));
        }
    }

    // The minigame is still closing when the copy is completed: remove the copy once it's closed.
    private static IEnumerator CoRemoveCopyLater(PlayerControl receiver, NormalPlayerTask copy)
    {
        for (var t = 0f; t < 5f && Minigame.Instance && copy && Minigame.Instance.MyTask == copy; t += Time.deltaTime)
        {
            yield return null;
        }

        if (receiver && copy)
        {
            receiver.myTasks.Remove(copy);
            Object.Destroy(copy.gameObject);
        }
    }

    private static void DropCopy(Offload offload)
    {
        var copy = offload.Copy;
        offload.Copy = null;
        if (copy == null || !copy)
        {
            return;
        }

        if (Minigame.Instance && Minigame.Instance.MyTask == copy)
        {
            Minigame.Instance.Close();
        }

        var receiver = MiscUtils.PlayerById(offload.ReceiverId);
        if (receiver)
        {
            receiver!.myTasks.Remove(copy);
        }

        Object.Destroy(copy.gameObject);
    }

    // ---------------------------------------------------------------- All clients

    /// <summary>
    ///     Start of a meeting (every client): offloaded tasks that aren't done come back to the Comfy, unlocked (option),
    ///     and the Comfy's locks end.
    /// </summary>
    public static void OnMeetingStart()
    {
        Locks.Clear();
        if (!OptionGroupSingleton<ComfyOptions>.Instance.TaskReturns)
        {
            return;
        }

        foreach (var offload in Offloads.ToList())
        {
            var receiver = MiscUtils.PlayerById(offload.ReceiverId);
            if (ReturnToComfy(offload) && receiver != null && receiver.Data != null)
            {
                Results.Add(MiraLocaleManager.Get("TownOfUsMira.Role.ComfyResultNotDone")
                    .Replace("<player>", receiver.Data.PlayerName));
            }
        }
    }

    /// <summary>Round start (every client): the results were shown at the meeting.</summary>
    public static void OnRoundStart()
    {
        Results.Clear();
    }

    /// <summary>A player left: their copies come back to the Comfy, a leaving Comfy's copies are dropped.</summary>
    public static void OnPlayerLeft(byte playerId)
    {
        foreach (var offload in Offloads.Where(x => x.ReceiverId == playerId || x.ComfyId == playerId).ToList())
        {
            ReturnToComfy(offload);
        }
    }

    /// <summary>No longer a Comfy (death, role change): every offloaded task comes back. Runs on every client.</summary>
    public static void ReleaseAllOf(byte comfyId)
    {
        foreach (var offload in Offloads.Where(x => x.ComfyId == comfyId).ToList())
        {
            ReturnToComfy(offload);
        }

        var local = PlayerControl.LocalPlayer;
        if (local && local.PlayerId == comfyId)
        {
            Locks.Clear();
        }
    }

    // Returns true on the Comfy's client when the task is back on their list.
    private static bool ReturnToComfy(Offload offload)
    {
        Offloads.Remove(offload);
        DropCopy(offload);

        var comfy = MiscUtils.PlayerById(offload.ComfyId);
        if (comfy == null || !comfy.AmOwner)
        {
            return false;
        }

        var task = UnhideOwnTask(comfy, offload.TaskId);
        Info($"Comfy: task {offload.TaskId} of player {offload.ComfyId} came back from player {offload.ReceiverId}");
        return task != null;
    }

    /// <summary>Line under the meeting timer for the Comfy: what the receivers did with the offloaded tasks.</summary>
    public static string MeetingStatusText()
    {
        return Results.Count == 0
            ? string.Empty
            : $"{MiraLocaleManager.Get("TownOfUsMira.Role.ComfyResults")} {string.Join(", ", Results)}";
    }

    /// <summary>
    ///     Local task list: locked tasks with the time left, offloaded tasks greyed out, received copies marked.
    /// </summary>
    public static string DecorateTaskText(string text)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || (Locks.Count == 0 && Hidden.Count == 0 && Offloads.Count == 0))
        {
            return text;
        }

        var color = TownOfUsColors.Comfy.ToTextColor();
        foreach (var (id, until) in Locks)
        {
            var left = until - Time.time;
            var task = left > 0f ? WorkaholicSystem.FindOwnTask(local, id) : null;
            var line = task != null ? TaskLine(task) : string.Empty;
            if (line.Length > 0 && text.Contains(line, StringComparison.Ordinal))
            {
                var suffix = MiraLocaleManager.Get("TownOfUsMira.Role.ComfyLockedSuffix")
                    .Replace("<time>", $"{Mathf.CeilToInt(left)}");
                text = text.Replace(line, $"{line} {color}{suffix}</color>", StringComparison.Ordinal);
            }
        }

        foreach (var offload in Offloads)
        {
            if (offload.ReceiverId != local.PlayerId || offload.Copy == null || !offload.Copy)
            {
                continue;
            }

            var line = TaskLine(offload.Copy);
            if (line.Length > 0 && text.Contains(line, StringComparison.Ordinal))
            {
                var suffix = ReceiverText(offload.ComfyId, "TownOfUsMira.Role.ComfyCopySuffix",
                    "TownOfUsMira.Role.ComfyCopySuffixNamed");
                text = text.Replace(line, $"{line} {color}{suffix}</color>", StringComparison.Ordinal);
            }
        }

        foreach (var (key, hidden) in Hidden)
        {
            if (key.Comfy != local.PlayerId || !hidden.Task)
            {
                continue;
            }

            var receiver = MiscUtils.PlayerById(hidden.ReceiverId);
            var label = MiraLocaleManager.Get("TownOfUsMira.Role.ComfyOffloadedSuffix")
                .Replace("<player>", receiver != null && receiver.Data != null ? receiver.Data.PlayerName : "?");
            var name = WorkaholicSystem.TaskName(hidden.Task);
            text = $"{text.TrimEnd('\n', '\r')}\n<color=#8C8C8C>{name}</color> {color}{label}</color>\n";
        }

        return text;
    }

    private static string TaskLine(PlayerTask task)
    {
        var sb = new Il2CppSystem.Text.StringBuilder();
        task.AppendTaskText(sb);
        return sb.ToString().TrimEnd('\n', '\r');
    }

    private static string ReceiverText(byte comfyId, string anonymousKey, string namedKey)
    {
        var comfy = MiscUtils.PlayerById(comfyId);
        return OptionGroupSingleton<ComfyOptions>.Instance.ReceiverSeesName && comfy != null && comfy.Data != null
            ? MiraLocaleManager.Get(namedKey).Replace("<player>", comfy.Data.PlayerName)
            : MiraLocaleManager.Get(anonymousKey);
    }

    private static void Notify(string text)
    {
        if (!HudManager.InstanceExists || !ShipStatus.Instance ||
            (AmongUsClient.Instance && AmongUsClient.Instance.IsGameOver))
        {
            return;
        }

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.Comfy.ToTextColor()}{text}</color></b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: AreAssets.ComfyIcon);
        notif.AdjustNotification();
    }

    // ---------------------------------------------------------------- RPC
    // The bodies live in [NoInlining] methods: Reactor compiles a copy of every RPC method body before MiraAPI patches
    // the getters of attribute options, so an option read directly in the RPC would always see its initial value.

    /// <summary>The Comfy offloads one of their tasks (id and type of the task object) to a player.</summary>
    [MethodRpc((uint)TownOfUsRpc.ComfyOffload)]
    public static void RpcComfyOffload(PlayerControl comfy, byte receiverId, uint taskId, int taskType)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(comfy);
            return;
        }

        HandleOffload(comfy, receiverId, taskId, (TaskTypes)taskType);
    }

    /// <summary>The receiver finished the copy: the Comfy's client completes the real task.</summary>
    [MethodRpc((uint)TownOfUsRpc.ComfyOffloadDone)]
    public static void RpcComfyOffloadDone(PlayerControl receiver, byte comfyId, uint taskId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(receiver);
            return;
        }

        HandleOffloadDone(receiver, comfyId, taskId);
    }

    /// <summary>The sulk stopped a kill or an ability: a flash for the attacker and (option) for the Comfy.</summary>
    [MethodRpc((uint)TownOfUsRpc.ComfySulkBlocked)]
    public static void RpcComfySulkBlocked(PlayerControl source, PlayerControl comfy)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        HandleSulkBlocked(source, comfy);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleOffload(PlayerControl comfy, byte receiverId, uint taskId, TaskTypes taskType)
    {
        var receiver = MiscUtils.PlayerById(receiverId);
        if (comfy.Data.Role is not ComfyRole || receiver == null || receiver.Data == null ||
            Offloads.Exists(x => x.ComfyId == comfy.PlayerId && x.TaskId == taskId))
        {
            Warning($"Comfy: offload of task {taskId} from {comfy.PlayerId} to {receiverId} ignored");
            return;
        }

        var offload = new Offload(comfy.PlayerId, taskId, receiverId);
        Offloads.Add(offload);
        Info($"Comfy: player {comfy.PlayerId} offloads task {taskId} ({taskType}) to player {receiverId}");

        if (comfy.AmOwner)
        {
            HideOwnTask(comfy, taskId, receiverId);
            Locks.RemoveAll(x => x.TaskId == taskId);
        }

        if (!receiver.AmOwner)
        {
            return;
        }

        var copy = CreateCopy(receiver, comfy, taskId, taskType);
        offload.Copy = copy;
        offload.CopyId = copy != null ? copy.Id : uint.MaxValue;
        if (copy != null && receiver == PlayerControl.LocalPlayer && !receiver.HasDied())
        {
            Notify(ReceiverText(comfy.PlayerId, "TownOfUsMira.Role.ComfyYouGotTask",
                    "TownOfUsMira.Role.ComfyYouGotTaskNamed")
                .Replace("<task>", WorkaholicSystem.TaskName(copy)));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleOffloadDone(PlayerControl receiver, byte comfyId, uint taskId)
    {
        var offload = Offloads.Find(x => x.ComfyId == comfyId && x.TaskId == taskId);
        if (offload == null || offload.ReceiverId != receiver.PlayerId)
        {
            Warning($"Comfy: player {receiver.PlayerId} finished task {taskId} of player {comfyId} it didn't have");
            return;
        }

        Offloads.Remove(offload);

        // Every client: the completion that follows was done by the receiver (the Auditor counts it for them).
        WorkaholicSystem.MarkDoneByHelper(comfyId, taskId);
        AuditorEvents.OnTaskDoneByWorkaholic(receiver);

        var comfy = MiscUtils.PlayerById(comfyId);
        if (comfy == null || !comfy.AmOwner)
        {
            return;
        }

        var task = UnhideOwnTask(comfy, taskId) ?? WorkaholicSystem.FindOwnTask(comfy, taskId);
        var info = comfy.Data.FindTaskById(taskId);
        if (task != null && info != null && !info.Complete)
        {
            // Sent by the Comfy's client, like the Taskmaster's completion: counts for the Comfy everywhere.
            comfy.RpcCompleteTask(taskId);
            if (!task.IsComplete)
            {
                task.taskStep = task.MaxStep;
                task.UpdateArrowAndLocation();
            }
        }

        Info($"Comfy: player {receiver.PlayerId} did task {taskId} of player {comfyId}");
        if (comfy == PlayerControl.LocalPlayer && receiver.Data != null)
        {
            Results.Add(MiraLocaleManager.Get("TownOfUsMira.Role.ComfyResultDone")
                .Replace("<player>", receiver.Data.PlayerName));
            Notify(MiraLocaleManager.Get("TownOfUsMira.Role.ComfyTaskDone")
                .Replace("<player>", receiver.Data.PlayerName)
                .Replace("<task>", task != null ? WorkaholicSystem.TaskName(task) : "?"));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleSulkBlocked(PlayerControl source, PlayerControl comfy)
    {
        Info($"Comfy: the sulk of player {comfy.PlayerId} stopped player {source.PlayerId}");
        if (source.AmOwner ||
            (comfy.AmOwner && OptionGroupSingleton<ComfyOptions>.Instance.AttackNotif))
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Comfy));
        }
    }

    private sealed class Offload(byte comfyId, uint taskId, byte receiverId)
    {
        public byte ComfyId { get; } = comfyId;
        public uint TaskId { get; } = taskId;
        public byte ReceiverId { get; } = receiverId;

        // Receiver's client only.
        public NormalPlayerTask? Copy { get; set; }
        public uint CopyId { get; set; } = uint.MaxValue;
    }

    private sealed class HiddenTask(NormalPlayerTask task, int index, byte receiverId)
    {
        public NormalPlayerTask Task { get; } = task;
        public int Index { get; } = index;
        public byte ReceiverId { get; } = receiverId;
    }
}

/// <summary>Opening a task: the Comfy's client watches how the window gets closed.</summary>
[HarmonyPatch(typeof(Console), nameof(Console.Use))]
public static class ComfyConsoleUsePatch
{
    public static void Postfix()
    {
        ComfySystem.OnConsoleUsed();
    }
}

/// <summary>New HUD = lobby or Freeplay loaded: forget the previous game (Freeplay has no game end event).</summary>
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class ComfyHudStartPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        ComfySystem.Reset();
    }
}

/// <summary>Local task list: locked and offloaded tasks of the Comfy, received copies.</summary>
[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
public static class ComfyTaskTextPatch
{
    [HarmonyPrefix]
    public static void Prefix(TaskPanelBehaviour __instance, ref string str)
    {
        if (HudManager.InstanceExists && __instance == HudManager.Instance.TaskPanel)
        {
            str = ComfySystem.DecorateTaskText(str);
        }
    }
}
