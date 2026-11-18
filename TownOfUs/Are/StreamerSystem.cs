using System.Runtime.CompilerServices;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Streamer (AmonRa Edition). Three parts, all checked a few times a second from <see cref="UpdatePatch" />:
///     <list type="bullet">
///         <item>
///             Watching (every client): which Streamers the local player sees on an open camera feed (or, with the
///             option, on an open admin map). Changes go to the Streamer with <see cref="StreamerModifier.RpcStreamerView" />.
///         </item>
///         <item>Viewers (the Streamer's client): who watches them, shown as "LIVE: N viewers".</item>
///         <item>
///             Offline (every client): the renderers of an offline Streamer move to a layer that only the main camera
///             draws, so camera feeds (anything rendering to a texture) don't show them.
///         </item>
///     </list>
/// </summary>
public static class StreamerSystem
{
    private const float TickTime = 0.2f;

    // The Streamer's client: who is watching them now.
    private static readonly HashSet<byte> Viewers = [];

    // Every client: the Streamers the local player reported watching.
    private static readonly HashSet<byte> Watching = [];

    // Every client: renderers of offline Streamers moved to the hidden layer, with their own layers.
    private static readonly Dictionary<byte, List<(GameObject Go, int Layer)>> Moved = [];

    // Diagnostics: cameras already logged as masked.
    private static readonly HashSet<int> MaskedCameras = [];

    private static float _timer;
    private static int _hiddenLayer = -1;
    private static int _collisionsOf = -1;
    private static GameObject? _label;
    private static TextMeshPro? _labelText;

    /// <summary>Viewers changed (Streamer's client).</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleView(PlayerControl watcher, PlayerControl streamer, bool watching)
    {
        if (watcher == null || streamer == null || !streamer.AmOwner)
        {
            return;
        }

        var changed = watching ? Viewers.Add(watcher.PlayerId) : Viewers.Remove(watcher.PlayerId);
        if (changed)
        {
            Info($"Streamer: {watcher.Data.PlayerName} {(watching ? "started" : "stopped")} watching ({Viewers.Count} viewers)");
        }
    }

    /// <summary>An offline Streamer is back on the cameras (every client).</summary>
    public static void Restore(PlayerControl player)
    {
        if (!Moved.Remove(player.PlayerId, out var list))
        {
            return;
        }

        foreach (var (go, layer) in list)
        {
            // Something else (e.g. dying) changed the layer meanwhile: leave theirs.
            if (go != null && go.layer == _hiddenLayer)
            {
                go.layer = layer;
            }
        }

        Info($"Streamer: {player.Data?.PlayerName} is back on the cameras");
        if (Moved.Count == 0)
        {
            MaskedCameras.Clear();
        }
    }

    /// <summary>The admin map doesn't count an offline Streamer (with the "admin counts" option).</summary>
    public static bool HiddenFromAdmin(PlayerControl player)
    {
        return player.HasModifier<StreamerOfflineModifier>() && OptionGroupSingleton<StreamerOptions>.Instance.AdminCounts;
    }

    private static void UpdateOffline(List<PlayerControl> streamers)
    {
        var any = false;
        foreach (var streamer in streamers)
        {
            if (!streamer.HasModifier<StreamerOfflineModifier>())
            {
                continue;
            }

            any = true;
            Hide(streamer);
        }

        if (any)
        {
            MaskFeeds();
        }
    }

    // Every frame while someone is offline: a camera screen opened since the last tick must not show them either.
    private static void MaskFeeds()
    {
        var bit = 1 << HiddenLayer;
        var main = Camera.main;
        if (main != null)
        {
            main.cullingMask |= bit;
        }

        // Camera feeds render to a texture (map cameras, the Fungle's binoculars, portable views); the Stream Sniper's
        // window is a second camera drawing straight into a corner of the screen.
        foreach (var cam in Camera.allCameras)
        {
            if (cam != null && cam != main && (cam.targetTexture != null || StreamSniperSystem.IsViewCamera(cam)))
            {
                var before = cam.cullingMask;
                cam.cullingMask &= ~bit;

                // Diagnostics (owner's test: a camera screen still showed an offline Streamer), once per camera.
                if (MaskedCameras.Add(cam.GetInstanceID()))
                {
                    Info($"Streamer: masked camera '{cam.name}' (mask {before:X8} -> {cam.cullingMask:X8}, " +
                         $"texture {(cam.targetTexture != null ? cam.targetTexture.name : "none")})");
                }
            }
        }
    }

    private static void Hide(PlayerControl player)
    {
        if (player.cosmetics == null)
        {
            return;
        }

        var first = !Moved.TryGetValue(player.PlayerId, out var list);
        if (list == null)
        {
            list = [];
            Moved[player.PlayerId] = list;
        }

        // Re-checked every tick: cosmetics (or a pet) may be swapped meanwhile. The whole player, not only the
        // cosmetics layer: the body sprites are outside it (owner's test 18: the body stayed on the camera screens).
        HideRenderers(player.gameObject, list, player);
        var pet = player.cosmetics.CurrentPet;
        if (pet != null)
        {
            HideRenderers(pet.gameObject, list, null);
        }

        if (first)
        {
            Info($"Streamer: {player.Data.PlayerName} is offline, {list.Count} renderers moved to layer {HiddenLayer}");
        }
    }

    private static void HideRenderers(GameObject root, List<(GameObject Go, int Layer)> list, PlayerControl? player)
    {
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            var go = renderer.gameObject;
            if (go.layer != HiddenLayer && (player == null || !KeepsLayer(player, go)))
            {
                if (go.GetComponent<Collider2D>() != null)
                {
                    MatchCollisions(go.layer);
                }

                list.Add((go, go.layer));
                go.layer = HiddenLayer;
            }
        }
    }

    /// <summary>
    ///     A layer also decides collisions, and an unused layer collides with everything: the pet (it has a collider)
    ///     started pushing its owner around (owner's test 19). The hidden layer gets the collisions of the layer the
    ///     collider came from, so it collides exactly as before.
    /// </summary>
    private static void MatchCollisions(int layer)
    {
        if (_collisionsOf == layer)
        {
            return;
        }

        if (_collisionsOf >= 0)
        {
            Warning($"Streamer: colliders from layers {_collisionsOf} and {layer} hidden, using the collisions of {layer}");
        }

        _collisionsOf = layer;
        var hidden = HiddenLayer;
        for (var i = 0; i < 32; i++)
        {
            Physics2D.IgnoreLayerCollision(hidden, i, Physics2D.GetIgnoreLayerCollision(layer, i));
        }

        Physics2D.IgnoreLayerCollision(hidden, hidden, Physics2D.GetIgnoreLayerCollision(layer, layer));
        Info($"Streamer: layer {hidden} collides like layer {layer}");
    }

    // The player's own light and flashlight (local player) belong to the shadow camera: moving them darkens the screen.
    // The player object itself keeps its layer (the game sets it, e.g. for ghosts, and it decides collisions).
    private static bool KeepsLayer(PlayerControl player, GameObject go)
    {
        return go == player.gameObject ||
               (player.lightSource != null && go.transform.IsChildOf(player.lightSource.transform)) ||
               (player.TargetFlashlight != null && go.transform.IsChildOf(player.TargetFlashlight.transform));
    }

    private static int HiddenLayer
    {
        get
        {
            if (_hiddenLayer >= 0)
            {
                return _hiddenLayer;
            }

            // A layer without a name, so nothing in the game uses it.
            _hiddenLayer = 31;
            for (var i = 31; i >= 20; i--)
            {
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    _hiddenLayer = i;
                    break;
                }
            }

            Info($"Streamer: hidden layer {_hiddenLayer}");
            return _hiddenLayer;
        }
    }

    private static void UpdateWatching(PlayerControl local, List<PlayerControl> streamers)
    {
        var seen = new HashSet<byte>();
        if (!MeetingHud.Instance && !local.AreCommsAffected())
        {
            var feeds = Feeds();
            var adminOpen = OptionGroupSingleton<StreamerOptions>.Instance.AdminCounts && MapBehaviour.Instance &&
                            MapBehaviour.Instance.IsOpen && MapBehaviour.Instance.countOverlay != null &&
                            MapBehaviour.Instance.countOverlay.gameObject.activeInHierarchy;
            foreach (var streamer in streamers)
            {
                if (streamer.AmOwner || streamer.Data.IsDead || streamer.HasModifier<StreamerOfflineModifier>())
                {
                    continue;
                }

                // A Stream Sniper watching them counts too (their window follows them, so it always shows them).
                if (adminOpen || StreamSniperSystem.Target == streamer ||
                    (streamer.Visible && !streamer.inVent && InFeeds(streamer, feeds)))
                {
                    seen.Add(streamer.PlayerId);
                }
            }
        }

        foreach (var id in seen)
        {
            if (Watching.Add(id))
            {
                Send(local, id, true);
            }
        }

        foreach (var id in Watching.ToArray())
        {
            if (!seen.Contains(id))
            {
                Watching.Remove(id);
                Send(local, id, false);
            }
        }
    }

    private static void Send(PlayerControl local, byte streamerId, bool watching)
    {
        var streamer = MiscUtils.PlayerById(streamerId);
        if (streamer != null)
        {
            StreamerModifier.RpcStreamerView(local, streamer, watching);
        }
    }

    private static bool InFeeds(PlayerControl player, List<Rect> feeds)
    {
        var pos = (Vector2)player.transform.position;
        foreach (var rect in feeds)
        {
            if (rect.Contains(pos))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>What the open camera screen shows right now, as world rectangles (empty when nothing is open).</summary>
    private static List<Rect> Feeds()
    {
        var feeds = new List<Rect>();
        var minigame = Minigame.Instance;
        if (minigame == null)
        {
            return feeds;
        }

        // Polus and the Airship: one camera at a time.
        var planet = minigame.TryCast<PlanetSurveillanceMinigame>();
        if (planet != null)
        {
            if (!planet.isStatic)
            {
                AddFeed(feeds, planet.Camera);
            }

            return feeds;
        }

        // The Fungle: the binoculars' camera lives on the map.
        var fungle = minigame.TryCast<FungleSurveillanceMinigame>();
        if (fungle != null)
        {
            if (fungle.securityCamera != null)
            {
                AddFeed(feeds, fungle.securityCamera.cam);
            }

            return feeds;
        }

        // The Skeld style screen: several cameras, each rendering to one of the minigame's textures.
        var screen = minigame.TryCast<SurveillanceMinigame>();
        if (screen != null && !screen.isStatic && screen.textures != null)
        {
            var textures = screen.textures;
            foreach (var cam in Camera.allCameras)
            {
                if (cam == null || cam.targetTexture == null)
                {
                    continue;
                }

                for (var i = 0; i < textures.Length; i++)
                {
                    if (textures[i] != null && textures[i] == cam.targetTexture)
                    {
                        AddFeed(feeds, cam);
                        break;
                    }
                }
            }
        }

        return feeds;
    }

    private static void AddFeed(List<Rect> feeds, Camera? cam)
    {
        if (cam == null || !cam.isActiveAndEnabled)
        {
            return;
        }

        var halfHeight = cam.orthographicSize;
        var halfWidth = halfHeight * cam.aspect;
        var center = cam.transform.position;
        feeds.Add(new Rect(center.x - halfWidth, center.y - halfHeight, 2f * halfWidth, 2f * halfHeight));
    }

    private static void UpdateLabel(PlayerControl local)
    {
        // Viewers who left the game don't count.
        Viewers.RemoveWhere(id => MiscUtils.PlayerById(id) is not { Data.Disconnected: false });

        var offline = local.HasModifier<StreamerOfflineModifier>();
        var show = local.HasModifier<StreamerModifier>() && !local.Data.IsDead && !MeetingHud.Instance &&
                   (Viewers.Count > 0 || offline);
        if (!show)
        {
            if (_label != null)
            {
                _label.SetActive(false);
            }

            return;
        }

        EnsureLabel();
        if (_label == null || _labelText == null)
        {
            return;
        }

        string text;
        if (offline)
        {
            text = $"<color=#9A9A9A>{MiraLocaleManager.Get("TownOfUsMira.Modifier.StreamerOfflineLabel")}</color>";
        }
        else if (OptionGroupSingleton<StreamerOptions>.Instance.ShowViewerCount)
        {
            text = $"<color=#FF4040>●</color> {MiraLocaleManager.Get("TownOfUsMira.Modifier.StreamerLiveCount").Replace("<count>", Viewers.Count.ToString(TownOfUsPlugin.Culture))}";
        }
        else
        {
            text = $"<color=#FF4040>●</color> {MiraLocaleManager.Get("TownOfUsMira.Modifier.StreamerLive")}";
        }

        _labelText.text = $"<size=160%><b>{text}</b></size>";
        _label.SetActive(true);
    }

    private static void EnsureLabel()
    {
        if (_label != null || !HudManager.InstanceExists)
        {
            return;
        }

        var pingTracker = Object.FindObjectOfType<PingTracker>(true);
        if (pingTracker == null)
        {
            return;
        }

        _label = Object.Instantiate(pingTracker.gameObject, HudManager.Instance.transform);
        _label.GetComponent<PingTracker>().Destroy();
        _label.name = "AreStreamerLive";
        var position = _label.GetComponent<AspectPosition>();
        position.Alignment = AspectPosition.EdgeAlignments.Top;
        position.DistanceFromEdge = new Vector3(0f, 0.75f, -20f);
        position.AdjustPosition();
        _labelText = _label.GetComponent<TextMeshPro>();
        _labelText.alignment = TextAlignmentOptions.Center;
    }

    private static void Clear()
    {
        Viewers.Clear();
        Watching.Clear();
        foreach (var id in Moved.Keys.ToArray())
        {
            var player = MiscUtils.PlayerById(id);
            if (player != null)
            {
                Restore(player);
            }
        }

        Moved.Clear();
        MaskedCameras.Clear();
        if (_label != null)
        {
            _label.SetActive(false);
        }
    }

    /// <summary>A meeting: nobody watches anything any more.</summary>
    public static void OnMeetingStart()
    {
        Viewers.Clear();
        Watching.Clear();
    }

    /// <summary>A few times a second, from <see cref="UpdatePatch" />.</summary>
    public static void Tick()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || !ShipStatus.Instance || LobbyBehaviour.Instance)
        {
            Clear();
            return;
        }

        if (Moved.Count > 0)
        {
            MaskFeeds();
        }

        _timer -= Time.deltaTime;
        if (_timer > 0f)
        {
            return;
        }

        _timer = TickTime;

        var streamers = new List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.Data != null && !player.Data.Disconnected && player.HasModifier<StreamerModifier>())
            {
                streamers.Add(player);
            }
        }

        UpdateOffline(streamers);
        UpdateWatching(local, streamers);
        UpdateLabel(local);
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class UpdatePatch
    {
        public static void Postfix()
        {
            Tick();
        }
    }
}
