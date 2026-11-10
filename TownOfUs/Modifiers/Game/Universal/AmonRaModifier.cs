using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Patches;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Game.Universal;

/// <summary>
///     ARE modifier: the player rides a wheelchair. Movement is a small vehicle model (inertia, rate-limited turning,
///     skidding, higher top speed) that rewards good steering. Movement logic lives in
///     <see cref="Patches.Modifiers.AmonRaMovementPatches"/>, this class handles assignment, the wheelchair visual
///     (with a rolling rear wheel) and the rolling sound.
/// </summary>
public sealed class AmonRaModifier : UniversalGameModifier, IWikiDiscoverable
{
    // Visual tuning, relative to the local bounds of the player's body sprite (see tools/are-assets/draw-wheelchair.ps1).
    private const float ChairWidthFactor = 1.3f;  // chair width = body width * factor
    private const float ChairDrawnWidthPx = 200f; // width of the drawn chair inside the 256px canvas
    private const float ChairBottomPx = 122f;     // distance from canvas center to the bottom of the big wheel
    private const float BodyBottomOffset = 0.02f; // fraction of body height to lift the chair by
    private const float CanvasCenterPx = 128f;
    private const float PixelsPerUnit = 100f;     // of all wheelchair sprites
    private const float WheelCenterXPx = 112f;    // big wheel center inside the 256px front canvas
    private const float WheelCenterYPx = 176f;
    private const float WheelRadiusPx = 70f;
    private const float FrontZ = -0.0008f;
    private const float BackZ = 0.0008f;
    private const float WheelZ = -0.0002f;        // relative to the front part

    // Sound tuning.
    private const float MaxRollVolume = 0.45f;
    private const float OthersHearDistance = 6f;  // world units, linear fade to 0
    private const float TeleportDistance = 1.5f;  // per-frame movement above this is a teleport, not rolling

    private GameObject? _front;
    private GameObject? _back;
    private GameObject? _wheel;
    private SpriteRenderer? _frontRend;
    private SpriteRenderer? _backRend;
    private SpriteRenderer? _wheelRend;
    private AudioSource? _audio;

    private Vector3 _lastPosition;
    private bool _hasLastPosition;
    private float _wheelAngle;
    private float _speedEstimate;

    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.AmonRa,
        TmpSpriteUtils.CreateSpriteAsset(AreAssets.AmonRaIcon.LoadAsset(), "TouMira.Modifier.Universal.AmonRa", 1.45f));

    public override string IdPart => "AmonRa";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}", "AmonRa");
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.AmonRaIcon;

    // Passive like Drunk (another movement modifier); "Visibility" is for appearance modifiers (Giant, Mini, Shy).
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    /// <summary>
    ///     Whether the wheelchair (and the "sitting" idle animation) should be shown for this player right now.
    /// </summary>
    public static bool ShouldShowWheelchair(PlayerControl player)
    {
        var options = OptionGroupSingleton<AmonRaOptions>.Instance;
        return options.ShowWheelchair &&
               player.Data != null &&
               !player.Data.IsDead &&
               !(options.HideInCamouflage && HudManagerPatches.CamouflageCommsEnabled);
    }

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription")
            .Replace("<topSpeed>", $"{Math.Round(OptionGroupSingleton<AmonRaOptions>.Instance.TopSpeed, 2)}");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription")
                   .Replace("<topSpeed>", $"{Math.Round(OptionGroupSingleton<AmonRaOptions>.Instance.TopSpeed, 2)}") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.AmonRaChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.AmonRaAmount;
    }

    public override void Update()
    {
        if (Player == null)
        {
            return;
        }

        var rolled = TrackMovement();

        var body = Player.cosmetics?.currentBodySprite?.BodySprite;
        if (body == null || body.sprite == null || !ShouldShowWheelchair(Player))
        {
            SetChairActive(false);
            return;
        }

        EnsureChair(body);
        SetChairActive(true);

        var visible = Player.Visible && body.enabled && body.gameObject.activeInHierarchy;
        _frontRend!.enabled = visible;
        _backRend!.enabled = visible;
        _wheelRend!.enabled = visible;
        if (!visible)
        {
            UpdateSound(false);
            return;
        }

        // Follow facing direction and transparency (Swooper, Phantom, fading...) of the body.
        var flip = body.flipX;
        _frontRend.flipX = flip;
        _backRend.flipX = flip;
        _wheelRend.flipX = flip;
        var color = new Color(1f, 1f, 1f, body.color.a);
        _frontRend.color = color;
        _backRend.color = color;
        _wheelRend.color = color;

        // Size and place the chair relative to the body sprite, so Mini/Giant/body types keep working.
        var bounds = body.sprite.bounds;
        var unitsPerPx = 1f / PixelsPerUnit;
        var scale = bounds.size.x * ChairWidthFactor / (ChairDrawnWidthPx * unitsPerPx);
        var centerY = bounds.min.y + (bounds.size.y * BodyBottomOffset) + (ChairBottomPx * unitsPerPx * scale);

        var localScale = new Vector3(scale, scale, 1f);
        _front!.transform.localScale = localScale;
        _back!.transform.localScale = localScale;
        // flipX mirrors the body around its pivot, not around the center of its bounds.
        var centerX = flip ? -bounds.center.x : bounds.center.x;
        _front.transform.localPosition = new Vector3(centerX, centerY, FrontZ);
        _back.transform.localPosition = new Vector3(centerX, centerY, BackZ);

        // The wheel is a child of the front part: position in front-sprite units, mirrored when facing left.
        var wheelX = (WheelCenterXPx - CanvasCenterPx) * unitsPerPx;
        var wheelY = (CanvasCenterPx - WheelCenterYPx) * unitsPerPx;
        _wheel!.transform.localPosition = new Vector3(flip ? -wheelX : wheelX, wheelY, WheelZ);

        // Roll the wheel: rotation = distance / radius. Facing right (not flipped) rolls clockwise.
        var radiusWorld = WheelRadiusPx * unitsPerPx * Mathf.Abs(_front.transform.lossyScale.x);
        if (radiusWorld > 0.0001f && rolled > 0f)
        {
            _wheelAngle += (flip ? 1f : -1f) * rolled / radiusWorld * Mathf.Rad2Deg;
            _wheelAngle %= 360f;
        }

        _wheel.transform.localRotation = Quaternion.Euler(0f, 0f, _wheelAngle);

        UpdateSound(body.color.a > 0.5f);
    }

    public override void OnDeactivate()
    {
        DestroyChair();
    }

    /// <summary>
    ///     Tracks how far the player moved this frame (works for remote players too) and keeps a smoothed speed.
    /// </summary>
    /// <returns>The distance rolled this frame (0 for teleports).</returns>
    private float TrackMovement()
    {
        var position = Player.transform.position;
        var rolled = 0f;
        if (_hasLastPosition)
        {
            rolled = ((Vector2)(position - _lastPosition)).magnitude;
            if (rolled > TeleportDistance)
            {
                rolled = 0f;
            }
        }

        _lastPosition = position;
        _hasLastPosition = true;

        var dt = Mathf.Max(Time.deltaTime, 0.0001f);
        _speedEstimate = Mathf.Lerp(_speedEstimate, rolled / dt, 1f - Mathf.Exp(-dt * 10f));
        return rolled;
    }

    private void UpdateSound(bool canBeHeard)
    {
        var options = OptionGroupSingleton<AmonRaOptions>.Instance;
        var clip = options.RollingSound ? AreAudio.WheelchairRoll : null;
        var isLocal = Player.AmOwner;
        var volume = 0f;

        if (clip != null && canBeHeard && (isLocal || options.OthersHearRolling))
        {
            var reference = Mathf.Max(Player.MyPhysics != null ? Player.MyPhysics.TrueSpeed : 2.5f, 0.1f);
            var speedFraction = Mathf.Clamp01(_speedEstimate / reference);
            volume = speedFraction * MaxRollVolume;

            if (!isLocal && PlayerControl.LocalPlayer != null)
            {
                var distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, Player.transform.position);
                volume *= Mathf.Clamp01(1f - (distance / OthersHearDistance));
            }

            if (volume > 0.01f)
            {
                EnsureAudio(clip);
                _audio!.volume = volume;
                _audio.pitch = 0.8f + (0.4f * speedFraction);
                if (!_audio.isPlaying)
                {
                    _audio.Play();
                }

                return;
            }
        }

        if (_audio != null && _audio.isPlaying)
        {
            _audio.Pause();
        }
    }

    private void EnsureAudio(AudioClip clip)
    {
        if (_audio != null || _front == null)
        {
            return;
        }

        _audio = _front.AddComponent<AudioSource>();
        _audio.clip = clip;
        _audio.loop = true;
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f;
        if (SoundManager.Instance != null)
        {
            // SFX channel, so the game's sound effects volume applies.
            _audio.outputAudioMixerGroup = SoundManager.Instance.SfxChannel;
        }
    }

    private void EnsureChair(SpriteRenderer body)
    {
        if (_front != null && _back != null && _wheel != null && _front.transform.parent == body.transform)
        {
            return;
        }

        // Body sprite changed (body type swap) or first use: (re)create under the current body sprite.
        DestroyChair();
        _front = CreatePart("AmonRaWheelchairFront", body, body.transform, AreAssets.WheelchairFront.LoadAsset(), out _frontRend);
        _back = CreatePart("AmonRaWheelchairBack", body, body.transform, AreAssets.WheelchairBack.LoadAsset(), out _backRend);
        _wheel = CreatePart("AmonRaWheelchairWheel", body, _front.transform, AreAssets.WheelchairWheel.LoadAsset(), out _wheelRend);
    }

    private static GameObject CreatePart(string name, SpriteRenderer body, Transform parent, Sprite sprite, out SpriteRenderer renderer)
    {
        var go = new GameObject(name);
        go.layer = body.gameObject.layer;
        go.transform.SetParent(parent, false);
        renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerID = body.sortingLayerID;
        renderer.sortingOrder = body.sortingOrder;
        renderer.maskInteraction = body.maskInteraction;
        return go;
    }

    private void SetChairActive(bool active)
    {
        if (_front != null && _front.activeSelf != active)
        {
            _front.SetActive(active); // also silences the AudioSource living on it
        }

        if (_back != null && _back.activeSelf != active)
        {
            _back.SetActive(active);
        }
    }

    private void DestroyChair()
    {
        if (_front != null)
        {
            Object.Destroy(_front); // wheel and audio are children/components of the front part
        }

        if (_back != null)
        {
            Object.Destroy(_back);
        }

        _front = null;
        _back = null;
        _wheel = null;
        _frontRend = null;
        _backRend = null;
        _wheelRend = null;
        _audio = null;
    }
}
