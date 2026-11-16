using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

/// <summary>
///     Blame Shifter (AmonRa Edition, Neutral Afterlife): a dead neutral (like the Spectre, given after a meeting) who
///     picks a living player to blame for their death and possesses them now and then: for a few seconds the player
///     walks after the ghost instead of where they want. Wins when the blamed player is ejected (or, as an option,
///     collects enough votes). Logic: <see cref="BlameShifterSystem" />.
/// </summary>
public sealed class BlameShifterRole(IntPtr cppPtr) : NeutralGhostRole(cppPtr), ITownOfUsRole, IBasicGhostRole,
    IWikiDiscoverable, IAmonRaContent
{
    public string IdPart => "BlameShifter";
    public override string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public override string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public override string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");
    public override Color RoleColor => TownOfUsColors.BlameShifter;
    public override RoleAlignment RoleAlignment => RoleAlignment.NeutralAfterlife;

    // Given after a meeting by GetAvailableGhosts (like the Spectre), not from the pool at death.
    public bool ApplyImmediatelyAfterDeath => false;
    public int SpawnPriority => 1;

    public override CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.BlameShifterIcon, "TouMira.Role.Neutral.BlameShifter", 1.45f),
        Icon = AreAssets.BlameShifterIcon,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        HideSettings = false,
        CanUseVent = false,
        ShowInFreeplay = true
    };

    /// <summary>The player blamed for the death (every client, from <see cref="RpcBlameShifterBlame" />).</summary>
    [HideFromIl2Cpp]
    public PlayerControl? Blamed { get; set; }

    /// <summary>Votes the blamed player got so far ("votes" win mode; counted on every client).</summary>
    public int Votes { get; set; }

    /// <summary>The blamed player was ejected (or got enough votes): counted on every client.</summary>
    public bool Won { get; set; }

    public bool MetWinCon => Won;

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Blame", "Blame"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Blame.WikiDescription"),
            AreAssets.BlameShifterBlameButton),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Possess", "Possess"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Possess.WikiDescription"),
            AreAssets.BlameShifterPossessButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    /// <summary>A dead neutral who hasn't won, like the Spectre (host, after a meeting).</summary>
    [HideFromIl2Cpp]
    public List<PlayerControl> GetAvailableGhosts(List<PlayerControl> basicGhosts, PlayerControl? exiled)
    {
        var data = MiscUtils.GetAssignData((RoleTypes)RoleId.Get<BlameShifterRole>());
        if (CustomRoleUtils.GetActiveRoles().OfType<BlameShifterRole>().Count() >= data.Count ||
            (data.Chance < 100 && HashRandom.Next(101) > data.Chance))
        {
            return basicGhosts;
        }

        var deadNeutral = basicGhosts.Where(x =>
            x.Data.IsDead && x != exiled && x.GetRoleWhenAlive().IsNeutral() &&
            !x.GetRoleWhenAlive().DidWin(GameOverReason.CrewmatesByVote) &&
            x.CanGetGhostRole() &&
            !x.HasModifier<AllianceGameModifier>() &&
            !(x.GetRoleWhenAlive() is ITownOfUsRole touRole && touRole.WinConditionMet())).ToList();
        if (deadNeutral.Count == 0)
        {
            return basicGhosts;
        }

        deadNeutral.Shuffle();
        var player = deadNeutral.TakeFirst();
        if (player != null)
        {
            player.RpcChangeRole(RoleId.Get<BlameShifterRole>());
            basicGhosts.Remove(player);
        }

        return basicGhosts;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Blamed = null;
        Votes = 0;
        Won = false;

        // One ghost role per player.
        if (!player.HasModifier<BasicGhostModifier>())
        {
            player.AddModifier<BasicGhostModifier>();
        }

        // The role comes after the meeting; the HUD shows the button when the round starts, or now in Freeplay.
        if (player.AmOwner && !MeetingHud.Instance && !ExileController.Instance)
        {
            CustomButtonSingleton<BlameShifterButton>.Instance.SetActive(true, this);
        }
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }

        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{RoleColor.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.BlameShifterTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    [HideFromIl2Cpp]
    public new StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        if (Player == null || !Player.AmOwner)
        {
            return sb;
        }

        sb.AppendLine(TownOfUsPlugin.Culture,
            $"<size=60%><color=#BFBFBF>{BlameShifterSystem.StatusText(this)}</color></size>");
        return sb;
    }

    public override bool WinConditionMet()
    {
        return Won && OptionGroupSingleton<BlameShifterOptions>.Instance.WinEndsGame;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return Won;
    }

    /// <summary>The Blame Shifter picked who to blame (every client).</summary>
    [MethodRpc((uint)TownOfUsRpc.BlameShifterBlame)]
    public static void RpcBlameShifterBlame(PlayerControl shifter, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(shifter);
            return;
        }

        if (shifter.Data.Role is not BlameShifterRole role)
        {
            Error("RpcBlameShifterBlame - Invalid source");
            return;
        }

        role.Blamed = target;
        role.Votes = 0;
        Info($"BlameShifter: {shifter.Data.PlayerName} blames {target.Data.PlayerName}");
    }

    /// <summary>The Blame Shifter possesses <paramref name="target" /> (every client).</summary>
    [MethodRpc((uint)TownOfUsRpc.BlameShifterPossess)]
    public static void RpcBlameShifterPossess(PlayerControl shifter, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(shifter);
            return;
        }

        if (shifter.Data.Role is not BlameShifterRole)
        {
            Error("RpcBlameShifterPossess - Invalid source");
            return;
        }

        BlameShifterSystem.HandlePossess(shifter, target);
    }
}
