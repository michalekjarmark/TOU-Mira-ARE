using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Handyman (AmonRa Edition, Crewmate Support): at the start of every round gets a random job with one ability for
///     that round: computer service (fix a sabotage from anywhere), pizzeria (sprint), warehouse (admin from anywhere)
///     or stream (cameras from anywhere). Logic: <see cref="HandymanSystem" />.
/// </summary>
public sealed class HandymanRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    IAmonRaContent
{
    public string IdPart => "Handyman";
    public Color RoleColor => TownOfUsColors.Handyman;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Protective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.HandymanIcon, "TouMira.Role.Crewmate.Handyman", 1.45f),
        Icon = AreAssets.HandymanIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.EngineerIntroSound
    };

    /// <summary>This round's job (Handyman's client only, drawn at the start of every round).</summary>
    public HandymanJob Job { get; set; }

    /// <summary>The previous round's job, for the "same job twice in a row" option.</summary>
    public HandymanJob LastJob { get; set; }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(HandymanSystem.JobName(HandymanJob.Service), MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Service.WikiDescription"),
            AreAssets.HandymanServiceButton),
        new(HandymanSystem.JobName(HandymanJob.Pizza), MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Pizza.WikiDescription"),
            AreAssets.HandymanPizzaButton),
        new(HandymanSystem.JobName(HandymanJob.Warehouse), MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Warehouse.WikiDescription"),
            AreAssets.HandymanWarehouseButton),
        new(HandymanSystem.JobName(HandymanJob.Stream), MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Stream.WikiDescription"),
            AreAssets.HandymanStreamButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Job = HandymanJob.None;
        LastJob = HandymanJob.None;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        if (Player == null || !Player.AmOwner || Job == HandymanJob.None)
        {
            return sb;
        }

        sb.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%><color=#BFBFBF>{MiraLocaleManager.Get("TownOfUsMira.Role.HandymanTodayShort").Replace("<job>", HandymanSystem.JobName(Job))}</color></size>");
        return sb;
    }

    /// <summary>
    ///     The part of a "computer service" fix that isn't a ship system update: lights (0), the Fungle's mushroom
    ///     mix-up (1) or Submerged's oxygen (2), on every client.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.HandymanFix)]
    public static void RpcHandymanFix(PlayerControl handyman, byte type)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(handyman);
            return;
        }

        if (handyman.Data.Role is not HandymanRole)
        {
            Error("RpcHandymanFix - Invalid source");
            return;
        }

        HandymanSystem.HandleFix(type);
    }
}
