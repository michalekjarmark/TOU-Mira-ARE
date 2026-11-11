using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Are;
using TownOfUs.Networking;
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

    /// <summary>Lethal traps used so far (Rigger's client), limited by MaxTrapKills.</summary>
    public int TrapKills { get; set; }

    /// <summary>
    ///     Kills the crewmate the trap fired on (the trigger delay already ran on their client). Indirect like the
    ///     Bootlegger's poison: no teleport, the Rigger's kill cooldown isn't reset, shields still protect, a Veteran on
    ///     alert doesn't strike back.
    /// </summary>
    [HideFromIl2Cpp]
    private static System.Collections.IEnumerator CoTrapKill(PlayerControl rigger, PlayerControl victim)
    {
        yield return null;

        if (MeetingHud.Instance || ExileController.Instance || rigger == null || rigger.HasDied() ||
            rigger.Data.Role is not RiggerRole || victim == null || victim.HasDied())
        {
            Message($"Rigger: trap kill skipped - meeting {(bool)MeetingHud.Instance || (bool)ExileController.Instance}, " +
                    $"rigger ok {rigger != null && !rigger.HasDied() && rigger.Data.Role is RiggerRole}, " +
                    $"victim alive {victim != null && !victim.HasDied()}");
            yield break;
        }

        Message($"Rigger: trap kills {victim.Data.PlayerName}");
        rigger.RpcSpecialMurder(victim, MeetingCheck.OutsideMeeting, isIndirect: true, resetKillTimer: false,
            teleportMurderer: false, causeOfDeath: "RiggerTrap");
        RiggerSystem.Notify(MiraLocaleManager.Get("TownOfUsMira.Role.RiggerTrapKilled")
            .Replace("<player>", victim.Data.PlayerName));
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
    ///     The trap of a rigged task fired on a crewmate (they kept the task open past the trigger delay): the rig is used
    ///     up, the impostors (or just the Rigger) get an alert and an arrow to the console, and the Rigger's client may
    ///     kill. The work is done in <see cref="HandleTriggered" />, see there why.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.RiggerTriggered)]
    public static void RpcRiggerTriggered(PlayerControl user, int consoleIndex)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(user);
            return;
        }

        HandleTriggered(user, consoleIndex);
    }

    /// <summary>
    ///     Never read attribute options (<c>[ModdedToggleOption] public bool X</c>) directly in a <c>[MethodRpc]</c> body.
    ///     Reactor runs RPC bodies from a copy compiled while the plugin loads, before MiraAPI patches the option getters,
    ///     so the copy has the getter inlined and always sees the initializer value (round 7: TrapsKill was always false).
    ///     This method is compiled on its first call instead, and NoInlining keeps it out of that early copy.
    /// </summary>
    [HideFromIl2Cpp]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void HandleTriggered(PlayerControl user, int consoleIndex)
    {
        var riggerId = RiggerSystem.RemoveRig(consoleIndex);
        var console = RiggerSystem.ConsoleAt(consoleIndex);
        var local = PlayerControl.LocalPlayer;
        Message($"Rigger: trap at console {consoleIndex} fired on {user?.Data?.PlayerName ?? "?"}, rig of player {riggerId}" +
                (riggerId == byte.MaxValue ? " (not rigged here)" : string.Empty));
        if (riggerId == byte.MaxValue || console == null || local == null || local.Data == null || local.Data.IsDead ||
            !local.Data.Role.IsImpostor)
        {
            return;
        }

        var opts = OptionGroupSingleton<RiggerOptions>.Instance;

        // Lethal trap: the Rigger's own client does the kill, like any other kill of theirs.
        if (local.PlayerId == riggerId)
        {
            var rigger = local.Data.Role as RiggerRole;
            var maxKills = (int)opts.MaxTrapKills.Value;
            Message($"Rigger: trap check - role {local.Data.Role.GetType().Name}, traps kill {opts.TrapsKill}, " +
                    $"trap kills {rigger?.TrapKills.ToString(TownOfUsPlugin.Culture) ?? "-"}/{maxKills}, " +
                    $"victim alive {user != null && !user.HasDied()}");
            if (rigger != null && opts.TrapsKill && rigger.TrapKills < maxKills && user != null && !user.HasDied())
            {
                rigger.TrapKills++;
                Coroutines.Start(CoTrapKill(local, user));
            }
        }
        if (!opts.AlertAllImpostors && local.PlayerId != riggerId)
        {
            return;
        }

        RiggerSystem.Notify(MiraLocaleManager.Get("TownOfUsMira.Role.RiggerTriggered")
            .Replace("<console>", RiggerSystem.ConsoleName(console)));
        Coroutines.Start(RiggerSystem.CoArrowTo(console, opts.ArrowDuration));
    }
}
