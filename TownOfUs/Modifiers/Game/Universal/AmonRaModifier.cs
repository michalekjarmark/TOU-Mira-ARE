using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Patches;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Game.Universal;

/// <summary>
///     ARE modifier: the player rides a wheelchair. Movement has inertia (gradual acceleration, rolling to a stop,
///     sliding in turns) but reaches a higher top speed on straight lines. Movement logic lives in
///     <see cref="Patches.Modifiers.AmonRaMovementPatches"/>, this class handles assignment and the wheelchair visual.
/// </summary>
public sealed class AmonRaModifier : UniversalGameModifier, IWikiDiscoverable
{
    // Visual tuning, relative to the local bounds of the player's body sprite.
    private const float ChairWidthFactor = 1.3f;  // chair width = body width * factor
    private const float ChairDrawnWidthPx = 200f; // width of the drawn chair inside the 256px canvas
    private const float ChairBottomPx = 119f;     // distance from canvas center to the bottom of the wheels
    private const float BodyBottomOffset = 0.02f; // fraction of body height to lift the chair by
    private const float FrontZ = -0.0008f;
    private const float BackZ = 0.0008f;

    private GameObject? _front;
    private GameObject? _back;
    private SpriteRenderer? _frontRend;
    private SpriteRenderer? _backRend;

    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.AmonRa,
        TmpSpriteUtils.CreateSpriteAsset(AreAssets.AmonRaIcon.LoadAsset(), "TouMira.Modifier.Universal.AmonRa", 1.45f));

    public override string IdPart => "AmonRa";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}", "AmonRa");
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.AmonRaIcon;

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;
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
        if (!visible)
        {
            return;
        }

        // Follow facing direction and transparency (Swooper, Phantom, fading...) of the body.
        var flip = body.flipX;
        _frontRend.flipX = flip;
        _backRend.flipX = flip;
        var color = new Color(1f, 1f, 1f, body.color.a);
        _frontRend.color = color;
        _backRend.color = color;

        // Size and place the chair relative to the body sprite, so Mini/Giant/body types keep working.
        var bounds = body.sprite.bounds;
        var chairUnitsPerPx = 1f / _frontRend.sprite.pixelsPerUnit;
        var scale = bounds.size.x * ChairWidthFactor / (ChairDrawnWidthPx * chairUnitsPerPx);
        var centerY = bounds.min.y + (bounds.size.y * BodyBottomOffset) + (ChairBottomPx * chairUnitsPerPx * scale);

        var localScale = new Vector3(scale, scale, 1f);
        _front!.transform.localScale = localScale;
        _back!.transform.localScale = localScale;
        _front.transform.localPosition = new Vector3(bounds.center.x, centerY, FrontZ);
        _back.transform.localPosition = new Vector3(bounds.center.x, centerY, BackZ);
    }

    public override void OnDeactivate()
    {
        DestroyChair();
    }

    private void EnsureChair(SpriteRenderer body)
    {
        if (_front != null && _back != null && _front.transform.parent == body.transform)
        {
            return;
        }

        // Body sprite changed (body type swap) or first use: (re)create under the current body sprite.
        DestroyChair();
        _front = CreatePart("AmonRaWheelchairFront", body, AreAssets.WheelchairFront.LoadAsset(), out _frontRend);
        _back = CreatePart("AmonRaWheelchairBack", body, AreAssets.WheelchairBack.LoadAsset(), out _backRend);
    }

    private static GameObject CreatePart(string name, SpriteRenderer body, Sprite sprite, out SpriteRenderer renderer)
    {
        var go = new GameObject(name);
        go.layer = body.gameObject.layer;
        go.transform.SetParent(body.transform, false);
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
            _front.SetActive(active);
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
            Object.Destroy(_front);
        }

        if (_back != null)
        {
            Object.Destroy(_back);
        }

        _front = null;
        _back = null;
        _frontRend = null;
        _backRend = null;
    }
}
