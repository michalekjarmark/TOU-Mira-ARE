using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using TownOfUs.Are;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Lighthouse Keeper (AmonRa Edition, Crewmate Support): places lanterns. During a lights sabotage everyone within a
///     lantern's radius sees as if the lights were on. Lanterns and the light: <see cref="LighthouseSystem" />.
/// </summary>
public sealed class LighthouseKeeperRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string IdPart => "LighthouseKeeper";
    public Color RoleColor => TownOfUsColors.LighthouseKeeper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Perception;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.LighthouseKeeperIcon,
            "TouMira.Role.Crewmate.LighthouseKeeper", 1.45f),
        Icon = AreAssets.LighthouseKeeperIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ToppatIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Place", "Place Lantern"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic", "Ability"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Place.WikiDescription"),
            AreAssets.LighthouseKeeperButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }
}
