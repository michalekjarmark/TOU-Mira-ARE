using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Sprites of the AmonRa Edition, embedded from TownOfUs/Resources/Custom (no asset bundle / Unity project needed).
///     The wheelchair PNGs are drawn programmatically, see .claude/docs (generator script) if they need to be redrawn.
/// </summary>
public static class AreAssets
{
    private const string CustomPath = "TownOfUs.Resources.Custom";

    /// <summary>
    ///     AmonRa Edition logo shown on the startup splash under the Innersloth and AU Avengers logos.
    ///     1600x800, transparent, same pixels-per-unit as the AU Avengers logo. Currently a placeholder
    ///     (tools/are-assets/draw-logo-placeholder.ps1) - just replace the PNG with the real logo.
    /// </summary>
    public static LoadableAsset<Sprite> AreLogo { get; } =
        new LoadableResourceAsset($"{CustomPath}.AreLogo.png", typeof(TownOfUsPlugin).Assembly, 200f);

    public static LoadableAsset<Sprite> AmonRaIcon { get; } =
        new LoadableResourceAsset($"{CustomPath}.AmonRaIcon.png", typeof(TownOfUsPlugin).Assembly, 128f);

    /// <summary>
    ///     Seat frame, caster and footrest, drawn in front of the player. Faces right. 256px canvas.
    /// </summary>
    public static LoadableAsset<Sprite> WheelchairFront { get; } =
        new LoadableResourceAsset($"{CustomPath}.WheelchairFront.png", typeof(TownOfUsPlugin).Assembly, 100f);

    /// <summary>
    ///     Big rear wheel (rotates while moving), drawn in front of the frame. 160px canvas, wheel centered.
    /// </summary>
    public static LoadableAsset<Sprite> WheelchairWheel { get; } =
        new LoadableResourceAsset($"{CustomPath}.WheelchairWheel.png", typeof(TownOfUsPlugin).Assembly, 100f);

    /// <summary>
    ///     Backrest and push handle, drawn behind the player. Faces right.
    /// </summary>
    public static LoadableAsset<Sprite> WheelchairBack { get; } =
        new LoadableResourceAsset($"{CustomPath}.WheelchairBack.png", typeof(TownOfUsPlugin).Assembly, 100f);
}
