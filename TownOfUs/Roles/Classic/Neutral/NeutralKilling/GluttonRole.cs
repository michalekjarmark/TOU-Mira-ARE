using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

/// <summary>
///     Glutton / Obżartuch (AmonRa Edition, Neutral Killing): the Pelican of Project: Lotus (its Swallow button icon,
///     GPL-3.0, is used with credit), renamed by the owner. In the AmonRa filter, but the name isn't Polish in English. Swallows a nearby player instead of killing them. The swallowed player
///     disappears inside the Glutton (invisible, not targetable, no abilities, moves with the Glutton) and dies without
///     a body when the next meeting is called (or after a set time). If the Glutton dies first, the swallowed come out
///     alive (option). Wins as the last killer alive. Logic: <see cref="GluttonSystem" />.
/// </summary>
public sealed class GluttonRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    IAmonRaContent
{
    public string IdPart => "Glutton";
    public Color RoleColor => TownOfUsColors.Glutton;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public DoomableType DoomHintType => DoomableType.Relentless;
    public bool HasImpostorVision => OptionGroupSingleton<GluttonOptions>.Instance.ImpostorVision;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.GluttonIcon, "TouMira.Role.Neutral.Glutton", 1.45f),
        CanUseVent = OptionGroupSingleton<GluttonOptions>.Instance.CanVent,
        IntroSound = TouAudio.WarlockIntroSound,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        Icon = AreAssets.GluttonIcon,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Swallow", "Swallow"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Swallow.WikiDescription"),
            AreAssets.GluttonSwallowButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }

        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralKillingTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = ITownOfUsRole.SetNewTabText(this);
        var names = GluttonSystem.SwallowedBy(Player).Select(x => x.Player.Data.PlayerName).ToList();
        if (names.Count > 0)
        {
            text.Append(TownOfUsPlugin.Culture,
                $"\n<b>{MiraLocaleManager.Get("TownOfUsMira.Role.GluttonTabSwallowed").Replace("<players>", string.Join(", ", names))}</b>");
        }

        return text;
    }

    public bool WinConditionMet()
    {
        var gluttonCount = CustomRoleUtils.GetActiveRolesOfType<GluttonRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > gluttonCount || MiscUtils.KillersAliveCount == 0)
        {
            return false;
        }

        return gluttonCount >= MiscUtils.GetImpactfulLivingPlayers().Count - gluttonCount;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    /// <summary>The Glutton swallows <paramref name="target" /> (sent by the Glutton's client, handled everywhere).</summary>
    [MethodRpc((uint)TownOfUsRpc.GluttonSwallow)]
    public static void RpcGluttonSwallow(PlayerControl glutton, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(glutton);
            return;
        }

        if (glutton.Data.Role is not GluttonRole)
        {
            Error("RpcGluttonSwallow - Invalid source");
            return;
        }

        GluttonSystem.HandleSwallow(glutton, target);
    }
}
