using System.Globalization;
using System.Text;
using UnityEngine;

namespace TownOfUs.Are;

public enum AuditVerdict : byte
{
    /// <summary>Nothing to judge (only walked around, audit cut short...).</summary>
    Inconclusive,

    /// <summary>Completed a task and nothing irregular happened.</summary>
    Verified,

    /// <summary>Stood at task stations without finishing anything.</summary>
    Unverified,

    /// <summary>Vent, kill or a role ability was used during the audit.</summary>
    Irregular
}

/// <summary>
///     A task station where the audited player stood without finishing a task.
/// </summary>
public readonly record struct AuditStation(TaskTypes Task, SystemTypes Room, int Seconds);

/// <summary>
///     Result of one audit. Kept as data (not text) so a published report can be sent over the network and every
///     player formats it in their own language.
/// </summary>
public sealed class AuditReport
{
    public byte TargetId { get; init; }
    public string TargetName { get; init; } = string.Empty;
    public int TasksCompleted { get; init; }
    public int Irregularities { get; init; }
    public List<AuditStation> IdleStations { get; init; } = [];
    public bool CutByMeeting { get; init; }
    public bool TargetDied { get; init; }

    public AuditVerdict Verdict
    {
        get
        {
            if (Irregularities > 0)
            {
                return AuditVerdict.Irregular;
            }

            if (TasksCompleted > 0)
            {
                return AuditVerdict.Verified;
            }

            return IdleStations.Count > 0 ? AuditVerdict.Unverified : AuditVerdict.Inconclusive;
        }
    }

    public static string VerdictText(AuditVerdict verdict)
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.AuditorVerdict{verdict}");
    }

    public static Color VerdictColor(AuditVerdict verdict)
    {
        return verdict switch
        {
            AuditVerdict.Verified => new Color32(110, 220, 110, 255),
            AuditVerdict.Unverified => new Color32(255, 200, 80, 255),
            AuditVerdict.Irregular => new Color32(255, 90, 90, 255),
            _ => new Color32(190, 190, 190, 255)
        };
    }

    /// <summary>Report body for the meeting chat, in the local language.</summary>
    public string Format()
    {
        var sb = new StringBuilder();
        var verdict = Verdict;
        sb.AppendLine(CultureInfo.InvariantCulture,
            $"{MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportVerdict")} {VerdictColor(verdict).ToTextColor()}<b>{VerdictText(verdict)}</b></color>");
        sb.AppendLine(CultureInfo.InvariantCulture,
            $"{MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportTasks")} {TasksCompleted}");

        var stations = IdleStations.Count == 0
            ? MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportNone")
            : string.Join(", ", IdleStations.Select(s =>
                $"{TranslationController.Instance.GetString(s.Task)} ({TranslationController.Instance.GetString(s.Room)}) {s.Seconds}s"));
        sb.AppendLine(CultureInfo.InvariantCulture,
            $"{MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportIdle")} {stations}");
        sb.AppendLine(CultureInfo.InvariantCulture,
            $"{MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportIrregular")} {Irregularities}");

        if (TargetDied)
        {
            sb.AppendLine(MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportTargetDied"));
        }
        else if (CutByMeeting)
        {
            sb.AppendLine(MiraLocaleManager.Get("TownOfUsMira.Role.AuditorReportCutByMeeting"));
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>Stations as "task:room:seconds;..." for the publish RPC.</summary>
    public string EncodeStations()
    {
        return string.Join(";", IdleStations.Select(s =>
            string.Create(CultureInfo.InvariantCulture, $"{(int)s.Task}:{(int)s.Room}:{s.Seconds}")));
    }

    public static List<AuditStation> DecodeStations(string encoded)
    {
        var list = new List<AuditStation>();
        foreach (var part in encoded.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var bits = part.Split(':');
            if (bits.Length == 3 &&
                int.TryParse(bits[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var task) &&
                int.TryParse(bits[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var room) &&
                int.TryParse(bits[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
            {
                list.Add(new AuditStation((TaskTypes)task, (SystemTypes)room, seconds));
            }
        }

        return list;
    }
}

/// <summary>
///     One running audit, tracked only on the Auditor's client. Everything it looks at is already synced to every
///     player (position, vent state, task completion, kills, most ability RPCs), so it needs no network traffic.
/// </summary>
public sealed class AuditSession
{
    // Extra margin on top of the console's own use distance (the player stands at the edge of it).
    private const float StationRangeMargin = 0.25f;

    // A kill often also fires an ability event: count events this close together once.
    private const float IrregularDedupeSeconds = 0.75f;

    private readonly float _idleThreshold;
    private readonly float _endTime;
    private readonly Dictionary<(TaskTypes, SystemTypes), float> _idle = [];

    private Console? _visitConsole;
    private float _visitStart;
    private bool _visitProductive;
    private bool _wasInVent;
    private float _lastIrregular = float.MinValue;
    private int _tasks;
    private int _irregular;

    public AuditSession(PlayerControl target, float duration, float idleThreshold)
    {
        Target = target;
        TargetId = target.PlayerId;
        TargetName = target.Data.PlayerName;
        _idleThreshold = idleThreshold;
        _endTime = Time.time + duration;
        _wasInVent = target.inVent;
    }

    public PlayerControl Target { get; }
    public byte TargetId { get; }
    public string TargetName { get; }
    public float TimeLeft => Mathf.Max(0f, _endTime - Time.time);
    public bool TimeIsUp => Time.time >= _endTime;

    public void Tick()
    {
        if (Target == null || Target.Data == null)
        {
            return;
        }

        if (Target.inVent && !_wasInVent)
        {
            AddIrregularity();
        }

        _wasInVent = Target.inVent;

        var console = Target.inVent || Target.Data.IsDead ? null : NearestTaskConsole(Target.GetTruePosition());
        if (console != _visitConsole)
        {
            CloseVisit();
            if (console != null)
            {
                _visitConsole = console;
                _visitStart = Time.time;
                _visitProductive = false;
            }
        }
    }

    public void OnTaskCompleted()
    {
        _tasks++;
        _visitProductive = true;
    }

    public void AddIrregularity()
    {
        if (Time.time - _lastIrregular < IrregularDedupeSeconds)
        {
            return;
        }

        _lastIrregular = Time.time;
        _irregular++;
    }

    public AuditReport Finish(bool cutByMeeting, bool targetDied)
    {
        CloseVisit();
        return new AuditReport
        {
            TargetId = TargetId,
            TargetName = TargetName,
            TasksCompleted = _tasks,
            Irregularities = _irregular,
            IdleStations = _idle
                .Select(x => new AuditStation(x.Key.Item1, x.Key.Item2, Mathf.RoundToInt(x.Value)))
                .OrderByDescending(x => x.Seconds)
                .ToList(),
            CutByMeeting = cutByMeeting,
            TargetDied = targetDied
        };
    }

    private void CloseVisit()
    {
        if (_visitConsole == null)
        {
            return;
        }

        var seconds = Time.time - _visitStart;
        if (!_visitProductive && seconds >= _idleThreshold && _visitConsole.TaskTypes.Length > 0)
        {
            var key = (_visitConsole.TaskTypes[0], _visitConsole.Room);
            _idle[key] = _idle.GetValueOrDefault(key) + seconds;
        }

        _visitConsole = null;
    }

    // Task consoles only: sabotage fix consoles (reactor, lights...) are usable by impostors, task ones aren't.
    private static Console? NearestTaskConsole(Vector2 position)
    {
        if (ShipStatus.Instance == null)
        {
            return null;
        }

        Console? best = null;
        var bestDistance = float.MaxValue;
        foreach (var console in ShipStatus.Instance.AllConsoles)
        {
            if (console == null || console.AllowImpostor || console.TaskTypes == null || console.TaskTypes.Length == 0)
            {
                continue;
            }

            var distance = Vector2.Distance(position, console.transform.position);
            if (distance <= console.UsableDistance + StationRangeMargin && distance < bestDistance)
            {
                best = console;
                bestDistance = distance;
            }
        }

        return best;
    }
}
