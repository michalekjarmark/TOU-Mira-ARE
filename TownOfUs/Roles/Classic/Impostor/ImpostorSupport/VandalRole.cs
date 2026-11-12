using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Vandal (AmonRa Edition, Impostor Support): at a task console, breaks a task that someone has already finished. The
///     task goes back to its owner as not done (from the first step) and the task bar drops. By default one random owner
///     per use, with the lobby option everyone who finished it. Logic: <see cref="VandalSystem" />.
/// </summary>
public sealed class VandalRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    ICrewVariant
{
    public string IdPart => "Vandal";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public DoomableType DoomHintType => DoomableType.Trickster;

    // Imitator: the crew role that repairs things.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.VandalIcon, "TouMira.Role.Impostor.Vandal", 1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.VandalIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Break", "Break"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Break.WikiDescription"),
            AreAssets.VandalButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    /// <summary>Tasks broken so far (counted by every client in <see cref="RpcVandalBreak" />).</summary>
    public int BrokenTasks { get; set; }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        if (Player == null || !Player.AmOwner)
        {
            return sb;
        }

        var line = MiraLocaleManager.Get("TownOfUsMira.Role.VandalTabBroken").Replace("<count>", $"{BrokenTasks}");
        sb.AppendLine(TownOfUsPlugin.Culture, $"<size=60%><color=#BFBFBF>{line}</color></size>");
        return sb;
    }

    /// <summary>
    ///     One broken task (one use can send several, <paramref name="firstOfUse" /> marks the first). The work is done in
    ///     <see cref="VandalSystem.HandleBreak" />.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.VandalBreak)]
    public static void RpcVandalBreak(PlayerControl vandal, int consoleIndex, byte ownerId, uint taskId,
        bool firstOfUse)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(vandal);
            return;
        }

        VandalSystem.HandleBreak(vandal, consoleIndex, ownerId, taskId, firstOfUse);
    }
}
