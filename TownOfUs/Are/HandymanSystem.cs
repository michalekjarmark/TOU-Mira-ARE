using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Handyman (AmonRa Edition): the job drawn at the start of every round and what each job does. Everything here runs
///     on the Handyman's client, except <see cref="HandleFix" /> (every client) and the sprint speed (every client).
/// </summary>
public static class HandymanSystem
{
    public static string JobName(HandymanJob job)
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.Handyman{job}", job.ToString());
    }

    public static LoadableAsset<Sprite> JobSprite(HandymanJob job)
    {
        return job switch
        {
            HandymanJob.Pizza => AreAssets.HandymanPizzaButton,
            HandymanJob.Warehouse => AreAssets.HandymanWarehouseButton,
            HandymanJob.Stream => AreAssets.HandymanStreamButton,
            _ => AreAssets.HandymanServiceButton
        };
    }

    /// <summary>A new job for this round (Handyman's client, at the start of every round).</summary>
    public static void DrawJob(HandymanRole role)
    {
        var opts = OptionGroupSingleton<HandymanOptions>.Instance;
        var pool = new List<HandymanJob>();
        if (opts.ServiceJob)
        {
            pool.Add(HandymanJob.Service);
        }

        if (opts.PizzaJob)
        {
            pool.Add(HandymanJob.Pizza);
        }

        if (opts.WarehouseJob)
        {
            pool.Add(HandymanJob.Warehouse);
        }

        // Maps without cameras (MIRA HQ has only the door log) never get the stream.
        if (opts.StreamJob && FindCamerasConsole() != null)
        {
            pool.Add(HandymanJob.Stream);
        }

        if (!opts.SameJobTwice && pool.Count > 1)
        {
            pool.Remove(role.LastJob);
        }

        role.Job = pool.Count > 0 ? pool.Random() : HandymanJob.None;
        Info($"Handyman: today's job is {role.Job}");
        if (role.Job == HandymanJob.None)
        {
            return;
        }

        role.LastJob = role.Job;
        var notif = Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.HandymanToday").Replace("<job>", JobName(role.Job)).Replace("<what>", MiraLocaleManager.Get($"TownOfUsMira.Role.Handyman{role.Job}Hint"))}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: AreAssets.HandymanIcon.LoadAsset());
        notif.AdjustNotification();
    }

    public static bool AnySabotage()
    {
        return ShipStatus.Instance &&
               ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Sabotage, out var system) &&
               system.Cast<SabotageSystemType>().AnyActive;
    }

    /// <summary>
    ///     "Computer service": fixes every active sabotage from anywhere, like the Engineer's fix (but on any map, by
    ///     looking at the systems the map has).
    /// </summary>
    public static void FixSabotage(PlayerControl handyman)
    {
        var ship = ShipStatus.Instance;
        if (!ship)
        {
            return;
        }

        var systems = ship.Systems;
        if (systems.TryGetValue(SystemTypes.Comms, out var comms))
        {
            var hud = comms.TryCast<HudOverrideSystemType>();
            if (hud != null && hud.IsActive)
            {
                ship.RpcUpdateSystem(SystemTypes.Comms, 0);
            }

            var hq = comms.TryCast<HqHudSystemType>();
            if (hq != null && hq.IsActive)
            {
                ship.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                ship.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
            }
        }

        foreach (var type in new[] { SystemTypes.Reactor, SystemTypes.Laboratory })
        {
            if (systems.TryGetValue(type, out var system))
            {
                var reactor = system.TryCast<ReactorSystemType>();
                if (reactor != null && reactor.IsActive)
                {
                    ship.RpcUpdateSystem(type, 16);
                }
            }
        }

        if (systems.TryGetValue(SystemTypes.LifeSupp, out var lifeSupp))
        {
            var oxygen = lifeSupp.TryCast<LifeSuppSystemType>();
            if (oxygen != null && oxygen.IsActive)
            {
                ship.RpcUpdateSystem(SystemTypes.LifeSupp, 16);
            }
        }

        if (systems.TryGetValue(SystemTypes.HeliSabotage, out var heliSystem))
        {
            var heli = heliSystem.TryCast<HeliSabotageSystem>();
            if (heli != null && heli.IsActive)
            {
                ship.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 0);
                ship.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 1);
            }
        }

        if (systems.TryGetValue(SystemTypes.Electrical, out var electrical))
        {
            var lights = electrical.TryCast<SwitchSystem>();
            if (lights != null && lights.IsActive)
            {
                HandymanRole.RpcHandymanFix(handyman, 0);
            }
        }

        if (systems.TryGetValue(SystemTypes.MushroomMixupSabotage, out var mushroomSystem))
        {
            var mushroom = mushroomSystem.TryCast<MushroomMixupSabotageSystem>();
            if (mushroom != null && mushroom.IsActive)
            {
                HandymanRole.RpcHandymanFix(handyman, 1);
            }
        }

        if (ModCompatibility.IsSubmerged() &&
            PlayerControl.LocalPlayer.myTasks.ToArray().Any(x => x.TaskType == ModCompatibility.RetrieveOxygenMask))
        {
            HandymanRole.RpcHandymanFix(handyman, 2);
        }

        Info("Handyman: fixed the sabotage from afar");
    }

    /// <summary>The part of a fix sent by <see cref="HandymanRole.RpcHandymanFix" /> (every client).</summary>
    public static void HandleFix(byte type)
    {
        if (!ShipStatus.Instance)
        {
            return;
        }

        switch (type)
        {
            case 0:
                var lights = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                lights.ActualSwitches = lights.ExpectedSwitches;
                break;
            case 1:
                var mushroom = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage]
                    .Cast<MushroomMixupSabotageSystem>();
                mushroom.currentSecondsUntilHeal = 0.1f;
                break;
            case 2:
                ModCompatibility.RepairSubOxygen();
                break;
        }
    }

    /// <summary>A console with real cameras (not MIRA HQ's door log), or null.</summary>
    public static SystemConsole? FindCamerasConsole()
    {
        if (!ShipStatus.Instance)
        {
            return null;
        }

        return Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x =>
            x.MinigamePrefab.TryCast<SurveillanceMinigame>() ||
            x.MinigamePrefab.TryCast<PlanetSurveillanceMinigame>() ||
            x.MinigamePrefab.TryCast<FungleSurveillanceMinigame>() || x.UseIcon is ImageNames.CamsButton);
    }

    /// <summary>"Stream": the map's camera screen from anywhere, like the Operative's (Handyman's client).</summary>
    public static Minigame? OpenCameras()
    {
        var console = FindCamerasConsole();
        var cam = Camera.main;
        if (console == null || cam == null)
        {
            Error("Handyman: no cameras on this map");
            return null;
        }

        PlayerControl.LocalPlayer.NetTransform.Halt();
        var minigame = Object.Instantiate(console.MinigamePrefab, cam.transform, false);
        minigame.transform.localPosition = new Vector3(0f, 0f, -50f);

        // Like the Operative: Begin on the exact type, or Android shows no camera feed.
        var fungleGame = minigame.TryCast<FungleSurveillanceMinigame>();
        var planetGame = minigame.TryCast<PlanetSurveillanceMinigame>();
        var camsGame = minigame.TryCast<SurveillanceMinigame>();
        if (fungleGame != null)
        {
            fungleGame.Begin(null);
        }
        else if (planetGame != null)
        {
            planetGame.Begin(null);
        }
        else if (camsGame != null)
        {
            camsGame.Begin(null);
        }
        else
        {
            minigame.Begin(null);
        }

        return minigame;
    }

    /// <summary>"Warehouse": the admin map from anywhere, like the Spy's portable admin (Handyman's client).</summary>
    public static void OpenAdmin(bool canMove)
    {
        if (!ShipStatus.Instance)
        {
            return;
        }

        if (!canMove)
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
        }

        HudManager.Instance.InitMap();
        var opts = GameManager.Instance.GetMapOptions();
        var map = MapBehaviour.Instance;
        map.GenericShow();
        map.countOverlay.gameObject.SetActive(true);
        map.countOverlay.SetOptions(opts.ShowLivePlayerPosition, opts.IncludeDeadBodies);
        map.countOverlayAllowsMovement = canMove;
        map.taskOverlay.Hide();
        map.HerePoint.enabled = !opts.ShowLivePlayerPosition;
        map.TrackedHerePoint.gameObject.SetActive(false);
        if (map.HerePoint.enabled)
        {
            PlayerControl.LocalPlayer.SetPlayerMaterialColors(map.HerePoint);
        }
    }

    public static bool AdminOpen => MapBehaviour.Instance && MapBehaviour.Instance.gameObject.activeSelf;

    /// <summary>"Pizzeria": the sprint speed, for everyone who has the sprint (it is added on every client).</summary>
    [HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetPlayerSpeedMod))]
    public static class SprintSpeedPatch
    {
        public static void Postfix(PlayerControl pc, ref float __result)
        {
            if (pc != null && pc.HasModifier<HandymanSprintModifier>())
            {
                __result *= OptionGroupSingleton<HandymanOptions>.Instance.PizzaSpeed.Value;
            }
        }
    }
}
