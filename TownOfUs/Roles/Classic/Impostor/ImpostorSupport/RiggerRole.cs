using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Rigger (AmonRa Edition, Impostor Support): rigs real task consoles. When a crewmate opens a rigged task, the
///     impostors get an alert with an arrow to it, and (option) the task jams and closes after a moment. Rigged consoles
///     spark up close, so crewmates can notice them. Rigs are cleared at every meeting.
/// </summary>
public sealed class RiggerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    ICrewVariant
{
    public string IdPart => "Rigger";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public DoomableType DoomHintType => DoomableType.Fearmonger;

    // Imitator: the crew role that also works with the map's equipment.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.RiggerIcon, "TouMira.Role.Impostor.Rigger", 1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.RiggerIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rig", "Rig"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rig.WikiDescription"),
            AreAssets.RiggerButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [MethodRpc((uint)TownOfUsRpc.RiggerRig)]
    public static void RpcRiggerRig(PlayerControl rigger, int consoleIndex)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(rigger);
            return;
        }

        if (rigger.Data.Role is not RiggerRole)
        {
            Error("RpcRiggerRig - Invalid rigger");
            return;
        }

        if (!RiggerSystem.IsTaskConsole(RiggerSystem.ConsoleAt(consoleIndex)))
        {
            Error($"RpcRiggerRig - console {consoleIndex} is not a task console");
            return;
        }

        RiggerSystem.AddRig(consoleIndex, rigger.PlayerId);
    }

    /// <summary>
    ///     A crewmate opened a rigged task: the rig is used up, and the impostors (or just the Rigger) get an alert and an
    ///     arrow to the console.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.RiggerTriggered)]
    public static void RpcRiggerTriggered(PlayerControl user, int consoleIndex)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(user);
            return;
        }

        var riggerId = RiggerSystem.RemoveRig(consoleIndex);
        var console = RiggerSystem.ConsoleAt(consoleIndex);
        var local = PlayerControl.LocalPlayer;
        if (riggerId == byte.MaxValue || console == null || local == null || local.Data == null || local.Data.IsDead ||
            !local.Data.Role.IsImpostor)
        {
            return;
        }

        var opts = OptionGroupSingleton<RiggerOptions>.Instance;
        if (!opts.AlertAllImpostors && local.PlayerId != riggerId)
        {
            return;
        }

        RiggerSystem.Notify(MiraLocaleManager.Get("TownOfUsMira.Role.RiggerTriggered")
            .Replace("<console>", RiggerSystem.ConsoleName(console)));
        Coroutines.Start(RiggerSystem.CoArrowTo(console, opts.ArrowDuration));
    }
}
