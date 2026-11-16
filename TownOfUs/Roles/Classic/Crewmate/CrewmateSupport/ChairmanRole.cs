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
///     Chairman (AmonRa Edition, Crewmate Support): gives a nearby player an order to finish any task in a short time.
///     A done order brings a promotion (Manager → Director → Chairman → King, each with a perk) and, as options, a free
///     own task for the Chairman and a double vote for the player in the next meeting; a missed one means the player's
///     vote doesn't count. Logic: <see cref="ChairmanSystem" />.
/// </summary>
public sealed class ChairmanRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    IAmonRaContent
{
    public string IdPart => "Chairman";
    public Color RoleColor => TownOfUsColors.Chairman;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Default;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.ChairmanIcon, "TouMira.Role.Crewmate.Chairman", 1.45f),
        Icon = AreAssets.ChairmanIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ToppatIntroSound
    };

    /// <summary>Done orders (counted by every client in <see cref="RpcChairmanOrderResult" />).</summary>
    public int OrdersDone { get; set; }

    /// <summary>The Director's meeting from anywhere was used (Chairman's client).</summary>
    public bool MeetingUsed { get; set; }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Order", "Order"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Order.WikiDescription"),
            AreAssets.ChairmanOrderButton),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Meeting", "Board Meeting"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Meeting.WikiDescription"),
            AreAssets.ChairmanMeetingButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        OrdersDone = 0;
        MeetingUsed = false;
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        if (Player.AmOwner)
        {
            ChairmanSystem.CancelOrder();
        }
    }

    /// <summary>The highest title reached with <see cref="OrdersDone" /> (titles switched off are skipped).</summary>
    public ChairmanRank Rank => ChairmanSystem.RankFor(OrdersDone);

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        if (Player == null || !Player.AmOwner)
        {
            return sb;
        }

        sb.AppendLine(TownOfUsPlugin.Culture, $"<size=60%><color=#BFBFBF>{ChairmanSystem.StatusText(this)}</color></size>");
        return sb;
    }

    /// <summary>An order to <paramref name="target" />: their client shows it (the Chairman's client times it).</summary>
    [MethodRpc((uint)TownOfUsRpc.ChairmanOrder)]
    public static void RpcChairmanOrder(PlayerControl chairman, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(chairman);
            return;
        }

        if (chairman.Data.Role is not ChairmanRole)
        {
            Error("RpcChairmanOrder - Invalid source");
            return;
        }

        ChairmanSystem.HandleOrder(chairman, target);
    }

    /// <summary>
    ///     How an order ended (<see cref="ChairmanOrderResult" /> as int), sent by the Chairman's client: every client
    ///     counts the promotion and gives the bonus or the reprimand for the next meeting.
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.ChairmanOrderResult)]
    public static void RpcChairmanOrderResult(PlayerControl chairman, PlayerControl target, int result)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(chairman);
            return;
        }

        if (chairman.Data.Role is not ChairmanRole role)
        {
            Error("RpcChairmanOrderResult - Invalid source");
            return;
        }

        ChairmanSystem.HandleResult(role, target, (ChairmanOrderResult)result);
    }

    /// <summary>The Director's meeting from anywhere (the host starts it, like the Button Barry's).</summary>
    [MethodRpc((uint)TownOfUsRpc.ChairmanMeeting)]
    public static void RpcChairmanMeeting(PlayerControl chairman)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(chairman);
            return;
        }

        if (chairman.Data.Role is not ChairmanRole || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        MeetingRoomManager.Instance.AssignSelf(chairman, null);
        if (GameManager.Instance.CheckTaskCompletion())
        {
            return;
        }

        HudManager.Instance.OpenMeetingRoom(chairman);
        chairman.RpcStartMeeting(null);
    }
}
