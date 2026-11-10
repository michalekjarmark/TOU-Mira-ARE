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

    /// <summary>
    ///     Main menu "Update" button icon (reopens the updater popup). 130px like TouAssets.SourceCode.
    ///     Drawn by tools/are-assets/draw-ui-icons.ps1.
    /// </summary>
    public static LoadableAsset<Sprite> AreUpdateIcon { get; } =
        new LoadableResourceAsset($"{CustomPath}.AreUpdateIcon.png", typeof(TownOfUsPlugin).Assembly);

    /// <summary>
    ///     Freeplay "Settings" ability button (opens the lobby settings menu). Drawn by tools/are-assets/draw-ui-icons.ps1.
    /// </summary>
    public static LoadableAsset<Sprite> SettingsButton { get; } =
        new LoadableResourceAsset($"{CustomPath}.AreSettingsButton.png", typeof(TownOfUsPlugin).Assembly);

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

    /// <summary>
    ///     Gambler role icon (chip and die). 128px. Drawn by tools/are-assets/draw-gambler-icons.ps1.
    /// </summary>
    public static LoadableAsset<Sprite> GamblerIcon { get; } =
        new LoadableResourceAsset($"{CustomPath}.GamblerIcon.png", typeof(TownOfUsPlugin).Assembly, 128f);

    /// <summary>
    ///     Gambler's meeting "Bet" chip shown next to players and Skip Vote. 100px, about 0.55 units in game.
    /// </summary>
    public static LoadableAsset<Sprite> GamblerBetButton { get; } =
        new LoadableResourceAsset($"{CustomPath}.GamblerBetButton.png", typeof(TownOfUsPlugin).Assembly, 180f);

    /// <summary>
    ///     Auditor role icon (clipboard and magnifier). 128px. Drawn by tools/are-assets/draw-auditor-icons.ps1.
    /// </summary>
    public static LoadableAsset<Sprite> AuditorIcon { get; } =
        new LoadableResourceAsset($"{CustomPath}.AuditorIcon.png", typeof(TownOfUsPlugin).Assembly, 128f);

    /// <summary>
    ///     Auditor "Audit" ability button and meeting buttons. 100px like the Freeplay settings button.
    /// </summary>
    public static LoadableAsset<Sprite> AuditButton { get; } =
        new LoadableResourceAsset($"{CustomPath}.AuditorButton.png", typeof(TownOfUsPlugin).Assembly, 100f);

    /// <summary>
    ///     Rigger role icon (wrench and lightning). 128px. Drawn by tools/are-assets/draw-rigger-icons.ps1.
    /// </summary>
    public static LoadableAsset<Sprite> RiggerIcon { get; } =
        new LoadableResourceAsset($"{CustomPath}.RiggerIcon.png", typeof(TownOfUsPlugin).Assembly, 128f);

    /// <summary>
    ///     Rigger "Rig" ability button. 100px like the other ARE buttons.
    /// </summary>
    public static LoadableAsset<Sprite> RiggerButton { get; } =
        new LoadableResourceAsset($"{CustomPath}.RiggerButton.png", typeof(TownOfUsPlugin).Assembly, 100f);

    /// <summary>
    ///     Spark over a rigged task console. 64px, about 0.25 units (small on purpose, round 5 feedback).
    /// </summary>
    public static LoadableAsset<Sprite> RigSpark { get; } =
        new LoadableResourceAsset($"{CustomPath}.RigSpark.png", typeof(TownOfUsPlugin).Assembly, 256f);

    /// <summary>
    ///     Insider role icon (plug with a checkmark). 128px. Drawn by tools/are-assets/draw-insider-icon.ps1.
    /// </summary>
    public static LoadableAsset<Sprite> InsiderIcon { get; } =
        new LoadableResourceAsset($"{CustomPath}.InsiderIcon.png", typeof(TownOfUsPlugin).Assembly, 128f);
}
