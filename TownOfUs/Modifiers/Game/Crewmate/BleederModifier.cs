using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

/// <summary>
///     ARE crewmate modifier (after death): the player's killer leaves a trail of bloody footprints for a while,
///     visible to everyone and without the killer's color. The trail itself is
///     <see cref="Modifiers.Crewmate.BleedingModifier"/> on the killer, added in
///     <see cref="Events.Modifiers.BleederEvents"/>.
/// </summary>
public sealed class BleederModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Bleeder,
        TmpSpriteUtils.CreateSpriteAsset(AreAssets.BleederIcon.LoadAsset(), "TouMira.Modifier.Crewmate.Bleeder", 1.45f));

    public override string IdPart => "Bleeder";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}", "Bleeder");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.BleederIcon;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription").Replace("<time>", TrailTime());
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription").Replace("<time>", TrailTime()) +
               MiscUtils.AppendOptionsText(GetType());
    }

    public static string TrailTime()
    {
        return Math.Round(OptionGroupSingleton<BleederOptions>.Instance.TrailDuration, 1)
            .ToString(TownOfUsPlugin.Culture);
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<CrewmateModifierOptions>.Instance.BleederChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<CrewmateModifierOptions>.Instance.BleederAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
}
