using System.Collections;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Lighthouse Keeper (AmonRa Edition, Crewmate Support): lanterns on the map. During a lights sabotage, a player
///     within a lantern's radius sees as if the lights were on (<c>VisionPatch</c>, like the Torch). Lanterns are placed
///     on every client by <see cref="RpcLighthousePlaceLantern" /> and stay until the end of the game (option: until the
///     Lighthouse Keeper dies).
/// </summary>
public static class LighthouseSystem
{
    // The lantern sprite is 0.48 units high and its pivot is the centre: lift it so it stands on the spot.
    private const float LanternHalfHeight = 0.24f;

    // A player's feet (GetTruePosition) are this far below their transform, whose y sets their depth (z = y / 1000).
    private const float PlayerFeetOffset = 0.3636f;

    private static readonly List<Lantern> Lanterns = [];
    private static bool loopRunning;

    public static void Clear()
    {
        foreach (var lantern in Lanterns)
        {
            if (lantern.Root)
            {
                Object.Destroy(lantern.Root);
            }
        }

        Lanterns.Clear();
    }

    public static void RemoveLanternsOf(byte keeperId)
    {
        foreach (var lantern in Lanterns.Where(x => x.KeeperId == keeperId).ToList())
        {
            if (lantern.Root)
            {
                Object.Destroy(lantern.Root);
            }

            Lanterns.Remove(lantern);
        }
    }

    /// <summary>VisionPatch: the player stands in the light of a lantern.</summary>
    public static bool IsInLanternLight(PlayerControl? player)
    {
        if (Lanterns.Count == 0 || player == null || !player)
        {
            return false;
        }

        var opts = OptionGroupSingleton<LighthouseKeeperOptions>.Instance;
        var radius = opts.LanternRadius;
        var position = player.GetTruePosition();
        foreach (var lantern in Lanterns)
        {
            if (Vector2.Distance(position, lantern.Position) > radius)
            {
                continue;
            }

            if (opts.WallsBlockLight &&
                PhysicsHelpers.AnythingBetween(lantern.Position, position, Constants.ShipOnlyMask, false))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static void AddLantern(byte keeperId, Vector2 position)
    {
        if (!ShipStatus.Instance)
        {
            return;
        }

        // The same depth rule as players (z = y / 1000 of a player standing on this spot), so whoever stands behind the
        // lantern is drawn behind it and whoever stands in front, in front.
        var root = new GameObject($"ARE Lantern {Lanterns.Count}");
        root.transform.SetParent(ShipStatus.Instance.transform, true);
        root.transform.position = new Vector3(position.x, position.y, (position.y + PlayerFeetOffset) / 1000f);

        var lamp = new GameObject("Lamp");
        lamp.transform.SetParent(root.transform, false);
        lamp.transform.localPosition = new Vector3(0f, LanternHalfHeight, 0f);
        var lampRenderer = lamp.AddComponent<SpriteRenderer>();
        lampRenderer.sprite = AreAssets.LanternSprite.LoadAsset();

        // The glow lies on the floor, under everything (like the blood splats of the crime scenes).
        var glow = new GameObject("Glow");
        glow.transform.SetParent(root.transform, false);
        glow.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        var radius = OptionGroupSingleton<LighthouseKeeperOptions>.Instance.LanternRadius;
        glow.transform.localScale = new Vector3(radius, radius, 1f);
        var glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = AreAssets.LanternGlow.LoadAsset();
        glowRenderer.enabled = false;

        Lanterns.Add(new Lantern(keeperId, position, root, lampRenderer, glowRenderer));
        if (!loopRunning)
        {
            loopRunning = true;
            Coroutines.Start(CoUpdateLanterns());
        }
    }

    // Who sees the lanterns, and the glow while the lights are out.
    private static IEnumerator CoUpdateLanterns()
    {
        while (Lanterns.Count > 0)
        {
            var local = PlayerControl.LocalPlayer;
            var seesLanterns = OptionGroupSingleton<LighthouseKeeperOptions>.Instance.LanternsVisible ||
                               local == null || local.Data == null || local.HasDied() ||
                               local.Data.Role is LighthouseKeeperRole;
            var show = seesLanterns && !MeetingHud.Instance;
            var dark = LightsOut();
            var time = Time.time;

            for (var i = 0; i < Lanterns.Count; i++)
            {
                var lantern = Lanterns[i];
                if (!lantern.Lamp || !lantern.Glow)
                {
                    continue;
                }

                lantern.Lamp.enabled = show;
                lantern.Glow.enabled = show && dark;
                if (dark)
                {
                    // A slight flicker, every lantern at its own pace.
                    var alpha = 0.32f + 0.05f * Mathf.Sin(time * 6f + i * 1.7f);
                    lantern.Glow.color = new Color(1f, 0.85f, 0.5f, alpha);
                }
            }

            yield return null;
        }

        loopRunning = false;
    }

    private static bool LightsOut()
    {
        if (!ShipStatus.Instance || ShipStatus.Instance.Systems == null ||
            !ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Electrical, out var system))
        {
            return false;
        }

        var switches = system.TryCast<SwitchSystem>();
        return switches != null && switches.Level < 0.99f;
    }

    /// <summary>"Place Lantern" button: the spot under the Lighthouse Keeper's feet.</summary>
    public static void PlaceLantern(PlayerControl keeper)
    {
        RpcLighthousePlaceLantern(keeper, keeper.GetTruePosition());
    }

    [MethodRpc((uint)TownOfUsRpc.LighthousePlaceLantern)]
    public static void RpcLighthousePlaceLantern(PlayerControl keeper, Vector2 position)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(keeper);
            return;
        }

        HandlePlaceLantern(keeper, position);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandlePlaceLantern(PlayerControl keeper, Vector2 position)
    {
        if (keeper.Data == null || keeper.Data.Role is not LighthouseKeeperRole)
        {
            Warning($"LighthouseKeeper: lantern from player {keeper.PlayerId}, who isn't a Lighthouse Keeper, ignored");
            return;
        }

        AddLantern(keeper.PlayerId, position);
        Info($"LighthouseKeeper: player {keeper.PlayerId} placed lantern {Lanterns.Count} at {position}");
    }

    private sealed record Lantern(byte KeeperId, Vector2 Position, GameObject Root, SpriteRenderer Lamp,
        SpriteRenderer Glow);
}

/// <summary>New HUD = lobby or Freeplay loaded: no lanterns from the previous game.</summary>
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class LighthouseHudStartPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        LighthouseSystem.Clear();
    }
}
