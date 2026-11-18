using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Stream Sniper (AmonRa Edition): the view window on the Stream Sniper's client. A copy of the main camera follows
///     the watched player and draws into a corner of the screen, in a frame, like the Parasite's view (same local
///     settings for its corner and size). The window shows; it can't be clicked into.
/// </summary>
public static class StreamSniperSystem
{
    private const string CameraName = "ARE-StreamSniperCam";
    private const float BaseMarginY = 0.04f;
    private const float BaseHeight = 0.3f;
    private const float BaseMarginXAspectFactor = 0.04f;
    private const float BaseWidthAspectFactor = 0.3f;

    private static Camera? _camera;
    private static GameObject? _border;
    private static SpriteRenderer? _borderRenderer;

    /// <summary>The player being watched now (Stream Sniper's client), or null.</summary>
    public static PlayerControl? Target { get; private set; }

    public static bool Watching => Target != null && _camera != null;

    /// <summary>The view's camera, so a Streamer who went offline is hidden from it too.</summary>
    public static bool IsViewCamera(Camera cam)
    {
        return _camera != null && cam == _camera;
    }

    /// <summary>The Stream Sniper's mark, shown after the name on their screen.</summary>
    public static bool IsLocalMark(PlayerControl player)
    {
        return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data?.Role is StreamSniperRole role &&
               role.Marked == player && !player.HasDied();
    }

    public static void Start(PlayerControl target)
    {
        Stop();
        var main = Camera.main;
        if (main == null)
        {
            return;
        }

        Target = target;
        _camera = CopyCamera(main);
        _camera.name = CameraName;
        _camera.orthographicSize = OptionGroupSingleton<StreamSniperOptions>.Instance.ViewSize;
        _camera.nearClipPlane = -1;
        _camera.depth = main.depth + 1;
        _camera.rect = WindowRect();
        _camera.gameObject.SetActive(true);

        if (HudManager.InstanceExists && HudManager.Instance.FullScreen != null)
        {
            _border = Object.Instantiate(TouAssets.ParasiteOverlay.LoadAsset(), HudManager.Instance.FullScreen.transform.parent);
            _border.layer = HudManager.Instance.FullScreen.gameObject.layer;
            _borderRenderer = _border.GetComponent<SpriteRenderer>();
            _borderRenderer.sortingOrder = 1000;
        }

        LateUpdate();
        Info($"StreamSniper: watching {target.Data.PlayerName}");

        if (OptionGroupSingleton<StreamSniperOptions>.Instance.TargetNotified)
        {
            StreamSniperRole.RpcStreamSniperWatch(PlayerControl.LocalPlayer, target);
        }
    }

    public static void Stop()
    {
        Target = null;
        if (_camera != null)
        {
            _camera.gameObject.Destroy();
        }

        if (_border != null)
        {
            _border.Destroy();
        }

        _camera = null;
        _border = null;
        _borderRenderer = null;
    }

    /// <summary>
    ///     A copy of the main camera without its children. The HUD lives under the main camera: a plain copy also copied
    ///     the HUD, whose components woke up as the "real" ones (MiraAPI's modifier panel threw in its Awake, owner's test
    ///     18, and kept pointing to the copy). Copied under an inactive parent, nothing in the copy wakes up before the
    ///     children are gone.
    /// </summary>
    private static Camera CopyCamera(Camera main)
    {
        var holder = new GameObject("ARE-StreamSniperCamHolder");
        holder.SetActive(false);
        var copy = Object.Instantiate(main, holder.transform, false);
        var transform = copy.transform;
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(transform.GetChild(i).gameObject);
        }

        var follower = copy.GetComponent<FollowerCamera>();
        if (follower != null)
        {
            Object.DestroyImmediate(follower);
        }

        transform.SetParent(null, false);
        Object.Destroy(holder);
        return copy;
    }

    /// <summary>Follows the watched player and keeps the frame on the window (Stream Sniper's client).</summary>
    public static void LateUpdate()
    {
        if (_camera == null)
        {
            return;
        }

        if (Target == null || Target.Data == null || Target.Data.Disconnected || Target.HasDied() || MeetingHud.Instance)
        {
            Stop();
            return;
        }

        var pos = Target.transform.position;
        _camera.transform.position = new Vector3(pos.x, pos.y, _camera.transform.position.z);
        _camera.rect = WindowRect();
        UpdateBorder();
    }

    /// <summary>"Someone is watching you" for the watched player (option).</summary>
    public static void HandleWatched(PlayerControl target)
    {
        if (target == null || !target.AmOwner)
        {
            return;
        }

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.StreamSniperWatched")}</b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: AreAssets.StreamSniperIcon.LoadAsset());
        notif.AdjustNotification();
    }

    // The Parasite's window corner and size (local settings), as a viewport rect.
    private static Rect WindowRect()
    {
        var size = ParasitePiPUtilities.GetScaleMultiplier();
        var aspect = (float)Screen.height / Screen.width;
        var width = aspect * BaseWidthAspectFactor * size;
        var height = BaseHeight * size;
        var marginX = aspect * BaseMarginXAspectFactor;
        var marginY = BaseMarginY;

        var location = ParasitePiPUtilities.GetEffectiveLocation();
        var right = location is ParasitePiPLocation.TopRight or ParasitePiPLocation.MiddleRight or ParasitePiPLocation.BottomRight;
        var x = right ? 1f - width - marginX : marginX;
        var y = location switch
        {
            ParasitePiPLocation.TopLeft or ParasitePiPLocation.TopRight => 1f - height - marginY,
            ParasitePiPLocation.MiddleLeft or ParasitePiPLocation.MiddleRight => (1f - height) * 0.5f,
            _ => marginY
        };

        width = Mathf.Clamp01(width);
        height = Mathf.Clamp01(height);
        return new Rect(Mathf.Clamp(x, 0f, 1f - width), Mathf.Clamp(y, 0f, 1f - height), width, height);
    }

    // Same as the Parasite's: the frame sprite stretched around the camera's viewport rect.
    private static void UpdateBorder()
    {
        var hudCam = Camera.main;
        if (_camera == null || _border == null || _borderRenderer == null || _borderRenderer.sprite == null ||
            hudCam == null)
        {
            return;
        }

        var rect = _camera.rect;
        var bottomLeft = hudCam.ScreenToWorldPoint(new Vector3(rect.x * Screen.width, rect.y * Screen.height,
            hudCam.nearClipPlane));
        var topRight = hudCam.ScreenToWorldPoint(new Vector3((rect.x + rect.width) * Screen.width,
            (rect.y + rect.height) * Screen.height, hudCam.nearClipPlane));
        _border.transform.position = new Vector3((bottomLeft.x + topRight.x) * 0.5f, (bottomLeft.y + topRight.y) * 0.5f,
            _border.transform.position.z);

        var spriteSize = _borderRenderer.sprite.bounds.size;
        if (spriteSize.x > 0f && spriteSize.y > 0f)
        {
            const float scaleMultiplier = 1.42f;
            _border.transform.localScale = new Vector3(
                Mathf.Abs(topRight.x - bottomLeft.x) * scaleMultiplier / spriteSize.x,
                Mathf.Abs(topRight.y - bottomLeft.y) * scaleMultiplier / spriteSize.y, 1f);
        }

        _borderRenderer.color = new Color(1f, 1f, 1f, 0.95f);
    }
}
