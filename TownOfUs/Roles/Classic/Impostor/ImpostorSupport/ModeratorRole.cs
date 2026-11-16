using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Moderator (AmonRa Edition, Impostor Support): bans a nearby player from one thing until the next meeting (or for a
///     while): reporting bodies, the meeting button, tasks, vents, cams/admin/vitals, abilities, or the vote in the next
///     meeting. The type is drawn at random from the enabled ones, or chosen with the second button (option). Logic:
///     <see cref="ModeratorSystem" />.
/// </summary>
public sealed class ModeratorRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    ICrewVariant, IAmonRaContent
{
    public string IdPart => "Moderator";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public DoomableType DoomHintType => DoomableType.Fearmonger;

    // Imitator: the crew role that stops others from using their abilities.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<BarkeeperRole>());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.ModeratorIcon, "TouMira.Role.Impostor.Moderator", 1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.ModeratorIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    /// <summary>Bans given this round (Moderator's client), for the "bans per round" option.</summary>
    public int BansThisRound { get; set; }

    /// <summary>The ban type chosen with the second button (Moderator's client, "Moderator chooses" option).</summary>
    public ModeratorBanType SelectedType { get; set; } = ModeratorBanType.Reports;

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Ban", "Ban"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Ban.WikiDescription"),
            AreAssets.ModeratorButton),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Type", "Ban Type"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Type.WikiDescription"),
            AreAssets.ModeratorTypeButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    /// <summary>Ban types switched on in the lobby, in enum order.</summary>
    [HideFromIl2Cpp]
    public static List<ModeratorBanType> EnabledTypes()
    {
        var opts = OptionGroupSingleton<ModeratorOptions>.Instance;
        return Enum.GetValues<ModeratorBanType>().Where(opts.IsEnabled).ToList();
    }

    /// <summary>The type the next ban gets: the chosen one, or a random enabled one. Null when none is enabled.</summary>
    public ModeratorBanType? NextBanType()
    {
        var enabled = EnabledTypes();
        if (enabled.Count == 0)
        {
            return null;
        }

        if (OptionGroupSingleton<ModeratorOptions>.Instance.ChoosesType)
        {
            if (!enabled.Contains(SelectedType))
            {
                SelectedType = enabled[0];
            }

            return SelectedType;
        }

        return enabled[Random.Range(0, enabled.Count)];
    }

    /// <summary>Second button: the next enabled type.</summary>
    public void CycleSelectedType()
    {
        var enabled = EnabledTypes();
        if (enabled.Count == 0)
        {
            return;
        }

        var index = enabled.IndexOf(SelectedType);
        SelectedType = enabled[(index + 1) % enabled.Count];
    }

    /// <summary>
    ///     A ban on <paramref name="target" />. <paramref name="duration" /> is in seconds, 0 = until the next meeting
    ///     (decided by the Moderator's client, options aren't read in an RPC body).
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.ModeratorBan)]
    public static void RpcModeratorBan(PlayerControl moderator, PlayerControl target, int banType, float duration)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(moderator);
            return;
        }

        if (moderator.Data.Role is not ModeratorRole)
        {
            Error("RpcModeratorBan - Invalid source");
            return;
        }

        ModeratorSystem.HandleBan(moderator, target, banType, duration);
    }
}
