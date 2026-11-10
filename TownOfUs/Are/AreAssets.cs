using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Sprites of the AmonRa Edition, embedded from TownOfUs/Resources/Custom (no asset bundle / Unity project needed).
///     The wheelchair PNGs are drawn programmatically, see .claude/docs (generator script) if they need to be redrawn.
/// </summary>
public static class AreAssets
{
    private const string CustomPath = "TownOfUs.Resources.Custom";

    public static LoadableAsset<Sprite> AmonRaIcon { get; } =
        new LoadableResourceAsset($"{CustomPath}.AmonRaIcon.png", typeof(TownOfUsPlugin).Assembly, 128f);

    /// <summary>
    ///     Seat frame, big rear wheel, caster and footrest, drawn in front of the player. Faces right.
    /// </summary>
    public static LoadableAsset<Sprite> WheelchairFront { get; } =
        new LoadableResourceAsset($"{CustomPath}.WheelchairFront.png", typeof(TownOfUsPlugin).Assembly, 100f);

    /// <summary>
    ///     Backrest and push handle, drawn behind the player. Faces right.
    /// </summary>
    public static LoadableAsset<Sprite> WheelchairBack { get; } =
        new LoadableResourceAsset($"{CustomPath}.WheelchairBack.png", typeof(TownOfUsPlugin).Assembly, 100f);
}
