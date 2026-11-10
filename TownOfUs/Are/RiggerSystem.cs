using System.Collections;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Rigged task consoles of the Rigger (AmonRa Edition), kept the same on every client (RPCs in
///     <see cref="RiggerRole" />). A console is identified by its index in <see cref="ShipStatus.AllConsoles" />,
///     which is the same for everyone on the same map.
/// </summary>
public static class RiggerSystem
{
    // Crewmates notice the sparks only this close to the console.
    private const float SparkVisibleDistance = 1.75f;

    private static readonly Dictionary<int, Rig> Rigs = [];
    private static readonly Dictionary<int, float> JammedUntil = [];
    private static bool _sparkLoopRunning;

    public static int ConsoleIndex(Console console)
    {
        if (ShipStatus.Instance == null || console == null)
        {
            return -1;
        }

        var all = ShipStatus.Instance.AllConsoles;
        for (var i = 0; i < all.Length; i++)
        {
            if (all[i] == console)
            {
                return i;
            }
        }

        return -1;
    }

    public static Console? ConsoleAt(int index)
    {
        if (ShipStatus.Instance == null || index < 0 || index >= ShipStatus.Instance.AllConsoles.Length)
        {
            return null;
        }

        return ShipStatus.Instance.AllConsoles[index];
    }

    /// <summary>Task consoles only: sabotage fix consoles are usable by impostors, task ones aren't.</summary>
    public static bool IsTaskConsole(Console? console)
    {
        return console != null && !console.AllowImpostor && console.TaskTypes != null && console.TaskTypes.Length > 0;
    }

    public static bool IsRigged(int index)
    {
        return Rigs.ContainsKey(index);
    }

    public static int ActiveRigsOf(byte riggerId)
    {
        return Rigs.Values.Count(x => x.RiggerId == riggerId);
    }

    public static string ConsoleName(Console console)
    {
        var task = console.TaskTypes.Length > 0 ? TranslationController.Instance.GetString(console.TaskTypes[0]) : "?";
        return $"{task} ({TranslationController.Instance.GetString(console.Room)})";
    }

    public static void AddRig(int index, byte riggerId)
    {
        var console = ConsoleAt(index);
        if (console == null || Rigs.ContainsKey(index))
        {
            return;
        }

        var spark = new GameObject($"RiggerSpark{index}");
        if (ShipStatus.Instance != null)
        {
            spark.transform.SetParent(ShipStatus.Instance.transform, true);
        }

        var pos = console.transform.position;
        spark.transform.position = new Vector3(pos.x, pos.y + 0.3f, pos.z - 0.5f);
        var renderer = spark.AddComponent<SpriteRenderer>();
        renderer.sprite = AreAssets.RigSpark.LoadAsset();
        renderer.enabled = false;

        Rigs[index] = new Rig(riggerId, spark, renderer);
        if (!_sparkLoopRunning)
        {
            _sparkLoopRunning = true;
            Coroutines.Start(CoUpdateSparks());
        }
    }

    /// <summary>Removes a rig and returns who placed it (255 if it wasn't rigged).</summary>
    public static byte RemoveRig(int index)
    {
        if (!Rigs.Remove(index, out var rig))
        {
            return byte.MaxValue;
        }

        if (rig.Spark)
        {
            rig.Spark.Destroy();
        }

        return rig.RiggerId;
    }

    public static void Clear()
    {
        foreach (var index in Rigs.Keys.ToList())
        {
            RemoveRig(index);
        }

        JammedUntil.Clear();
    }

    /// <summary>Sparks: always visible to impostors and the dead, to crewmates only up close (option).</summary>
    private static IEnumerator CoUpdateSparks()
    {
        while (Rigs.Count > 0)
        {
            var local = PlayerControl.LocalPlayer;
            var seesAll = local == null || local.Data == null || local.Data.IsDead || local.Data.Role.IsImpostor;
            var sparksOn = OptionGroupSingleton<RiggerOptions>.Instance.SparksVisible;
            var flicker = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 7f));

            foreach (var rig in Rigs.Values)
            {
                if (!rig.Renderer)
                {
                    continue;
                }

                var visible = seesAll || (sparksOn && local != null &&
                    Vector2.Distance(local.GetTruePosition(), rig.Spark.transform.position) <= SparkVisibleDistance);
                rig.Renderer.enabled = visible && !MeetingHud.Instance;
                rig.Renderer.color = new Color(1f, 1f, 1f, flicker);
            }

            yield return null;
        }

        _sparkLoopRunning = false;
    }

    /// <summary>
    ///     Console.Use postfix on the user's client: a crewmate opened a rigged task, alert the impostors and maybe jam it.
    /// </summary>
    public static void OnConsoleUsed(Console console)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || local.Data.IsDead || local.Data.Role.IsImpostor)
        {
            return;
        }

        var index = ConsoleIndex(console);
        var minigame = Minigame.Instance;
        if (index < 0 || !IsRigged(index) || minigame == null)
        {
            return;
        }

        RiggerRole.RpcRiggerTriggered(local, index);

        var opts = OptionGroupSingleton<RiggerOptions>.Instance;
        if (opts.JamConsole)
        {
            Coroutines.Start(CoJam(minigame, index, opts.JamDelay.Value, opts.JamLockout.Value));
        }
    }

    private static IEnumerator CoJam(Minigame minigame, int index, float delay, float lockout)
    {
        yield return new WaitForSeconds(delay);

        if (minigame == null || !minigame || Minigame.Instance != minigame)
        {
            yield break;
        }

        minigame.Close();
        JammedUntil[index] = Time.time + lockout;
        Notify(MiraLocaleManager.Get("TownOfUsMira.Role.RiggerJammed"));
    }

    public static bool IsJammedForLocal(Console console)
    {
        var index = ConsoleIndex(console);
        return index >= 0 && JammedUntil.TryGetValue(index, out var until) && Time.time < until;
    }

    public static void Notify(string text, LoadableAsset<Sprite>? icon = null)
    {
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: icon ?? AreAssets.RiggerIcon);
        notif.AdjustNotification();
    }

    /// <summary>Arrow from the local player to the console for a few seconds.</summary>
    public static IEnumerator CoArrowTo(Console console, float duration)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || console == null)
        {
            yield break;
        }

        var arrow = MiscUtils.CreateArrow(local.transform, Palette.ImpostorRed);
        arrow.target = console.transform.position;
        var end = Time.time + duration;
        while (Time.time < end && arrow && !MeetingHud.Instance)
        {
            yield return null;
        }

        if (arrow)
        {
            UObject.Destroy(arrow.gameObject);
        }
    }

    private sealed record Rig(byte RiggerId, GameObject Spark, SpriteRenderer Renderer);
}

/// <summary>
///     Hooks the task consoles for the Rigger: a jammed console can't be opened for a moment, and opening a rigged one
///     triggers the rig.
/// </summary>
[HarmonyPatch(typeof(Console), nameof(Console.Use))]
public static class RiggerConsoleUsePatch
{
    public static bool Prefix(Console __instance)
    {
        if (!RiggerSystem.IsJammedForLocal(__instance))
        {
            return true;
        }

        RiggerSystem.Notify(MiraLocaleManager.Get("TownOfUsMira.Role.RiggerStillJammed"));
        return false;
    }

    public static void Postfix(Console __instance)
    {
        RiggerSystem.OnConsoleUsed(__instance);
    }
}
