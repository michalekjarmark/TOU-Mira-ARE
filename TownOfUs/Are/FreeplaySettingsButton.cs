using MiraAPI.Hud;
using TownOfUs.Are;
using TownOfUs.Modules;
using UnityEngine;

// ARE: lives in TownOfUs/Are/, but uses the namespace of the other Freeplay buttons on purpose:
// FreeplayButtonsVisibility hides/shows all buttons of this namespace together ("Show practice buttons" setting).
namespace TownOfUs.Buttons.BaseFreeplay;

/// <summary>
///     ARE: Freeplay button that opens the lobby settings menu (see <see cref="AreFreeplaySettings"/>).
/// </summary>
public sealed class FreeplaySettingsButton : TownOfUsButton, IAurialBypassButton
{
    public override string Name => MiraLocaleManager.Get("AreFreeplaySettingsButton", "Settings");
    public override Color TextOutlineColor => new Color32(52, 160, 182, 255);
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => AreAssets.SettingsButton;

    public override bool ZeroIsInfinite { get; set; } = true;
    public override bool UsableInDeath => true;

    // Local Freeplay only: in multiplayer freeplay the settings belong to the lobby host.
    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer && TutorialManager.InstanceExists && !FreeplayButtonsVisibility.Hidden;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        if (Minigame.Instance)
        {
            return;
        }

        AreFreeplaySettings.Open();
    }
}
