using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

/// <summary>
///     ARE universal modifier (negative): other players further away than a few steps look camouflaged to this
///     player (grey, no name, by default no cosmetics); up close they look normal. Only this player's view changes,
///     see <see cref="ColorblindVision"/>. Meetings look normal.
/// </summary>
public sealed class ColorblindModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Colorblind,
        TmpSpriteUtils.CreateSpriteAsset(AreAssets.ColorblindIcon.LoadAsset(), "TouMira.Modifier.Universal.Colorblind",
            1.45f));

    public override string IdPart => "Colorblind";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}", "Colorblind");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.ColorblindIcon;
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.ColorblindChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.ColorblindAmount;
    }

    public override void Update()
    {
        if (Player != null && Player.AmOwner)
        {
            ColorblindVision.Update(Player);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        if (Player != null && Player.AmOwner)
        {
            ColorblindVision.RestoreAll();
        }
    }

    public override void OnDeactivate()
    {
        if (Player != null && Player.AmOwner)
        {
            ColorblindVision.RestoreAll();
        }
    }
}
