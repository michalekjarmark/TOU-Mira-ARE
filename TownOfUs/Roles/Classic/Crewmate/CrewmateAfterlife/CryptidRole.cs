using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Cryptid (AmonRa Edition, Crewmate Afterlife): a crewmate's ghost (given at death, like the vanilla Guardian
///     Angel) who can show up to the living for a few seconds as a blurry grey figure, without a name. Nobody can get
///     close: the figure is gone when a living player comes near (an impostor scares it off for good, option). The dead
///     can't talk, so this is how the Cryptid points at things. Logic: <see cref="CryptidSystem" />.
/// </summary>
public sealed class CryptidRole(IntPtr cppPtr) : CrewmateGhostRole(cppPtr), ITownOfUsRole, IWikiDiscoverable,
    IAmonRaContent
{
    private Minigame _hauntMenu = null!;

    public string IdPart => "Cryptid";
    public Color RoleColor => TownOfUsColors.Cryptid;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateAfterlife;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.CryptidIcon, "TouMira.Role.Crewmate.Cryptid", 1.45f),
        Icon = AreAssets.CryptidIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 1,
        CanUseVent = false,
        ShowInFreeplay = true
    };

    // Needed because MiraAPI roles inherit from RoleBehaviour (like Town of Polus' ghosts).
    public override bool IsDead => true;
    public override bool IsAffectedByComms => false;

    /// <summary>An impostor scared the Cryptid off: no more sightings (Cryptid's client).</summary>
    public bool ScaredOff { get; set; }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Appear", "Appear"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Appear.WikiDescription"),
            AreAssets.CryptidAppearButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    // The vanilla ghost's "Haunt" stays (the ability button), like Town of Polus' ghosts.
    public void Awake()
    {
        var crewGhost = RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).Cast<CrewmateGhostRole>();
        _hauntMenu = crewGhost.HauntMenu;
        Ability = crewGhost.Ability;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ScaredOff = false;

        // One ghost role per player: no Haunter later.
        if (!player.HasModifier<BasicGhostModifier>())
        {
            player.AddModifier<BasicGhostModifier>();
        }

        // The role comes after death, when the HUD has already been set up.
        if (player.AmOwner)
        {
            CustomButtonSingleton<CryptidAppearButton>.Instance.SetActive(!MeetingHud.Instance, this);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        CryptidSystem.Remove(targetPlayer.PlayerId, CryptidSystem.EndReason.Time);
    }

    public override void UseAbility()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return;
        }

        if (Minigame.Instance)
        {
            if (Minigame.Instance.TryCast<HauntMenuMinigame>())
            {
                Minigame.Instance.Close();
            }

            return;
        }

        var minigame = Instantiate(_hauntMenu, HudManager.Instance.AbilityButton.transform, false);
        minigame.transform.SetLocalZ(-5f);
        minigame.Begin(null);
        HudManager.Instance.AbilityButton.SetDisabled();
    }

    /// <summary>The Cryptid shows up: every client draws the figure.</summary>
    [MethodRpc((uint)TownOfUsRpc.CryptidAppear)]
    public static void RpcCryptidAppear(PlayerControl cryptid)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(cryptid);
            return;
        }

        if (cryptid.Data.Role is not CryptidRole)
        {
            Error("RpcCryptidAppear - Invalid source");
            return;
        }

        CryptidSystem.Create(cryptid);
    }

    /// <summary>The figure is gone (<see cref="CryptidSystem.EndReason" /> as int), sent by the Cryptid's client.</summary>
    [MethodRpc((uint)TownOfUsRpc.CryptidVanish)]
    public static void RpcCryptidVanish(PlayerControl cryptid, int reason)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(cryptid);
            return;
        }

        CryptidSystem.Remove(cryptid.PlayerId, (CryptidSystem.EndReason)reason);
    }
}
