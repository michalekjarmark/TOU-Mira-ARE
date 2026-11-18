using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

/// <summary>
///     Informant (AmonRa Edition, Crewmate Support): marks a player (their colour is taken right then, red or green by
///     the Seer's rules), then shows that colour to another player. Only that player sees it, on the marked player's
///     name from the next meeting on, until the end of the game (<see cref="InformantRevealModifier" />). The Informant
///     learns nothing. One button with two modes, see <c>InformantButton</c>.
/// </summary>
public sealed class InformantRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public const byte NoMark = byte.MaxValue;

    public string IdPart => "Informant";
    public Color RoleColor => TownOfUsColors.Informant;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Insight;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.InformantIcon, "TouMira.Role.Crewmate.Informant", 1.45f),
        Icon = AreAssets.InformantIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.SpyIntroSound
    };

    /// <summary>Marked player (Informant's client only), <see cref="NoMark" /> = the button is in the "Mark" mode.</summary>
    public byte MarkedPlayerId { get; set; } = NoMark;

    /// <summary>Colour of the marked player, taken when they were marked (true = red).</summary>
    public bool MarkedIsEvil { get; set; }

    public bool HasMark => MarkedPlayerId != NoMark;

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Mark", "Mark"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Mark.WikiDescription"),
            AreAssets.InformantMarkButton),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Show", "Show"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Show.WikiDescription"),
            AreAssets.InformantShowButton)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var text = ITownOfUsRole.SetNewTabText(this);
        if (HasMark && Player.AmOwner)
        {
            var name = GameData.Instance.GetPlayerById(MarkedPlayerId)?.PlayerName ?? "?";
            text.AppendLine(TownOfUsPlugin.Culture,
                $"<b>{MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabMarked").Replace("<player>", name)}</b>");
        }

        return text;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        MarkedPlayerId = NoMark;
        MarkedIsEvil = false;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        MarkedPlayerId = NoMark;
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        // Died before showing: the mark is lost. Colours already shown stay with their players.
        MarkedPlayerId = NoMark;
    }

    /// <summary>Marks a player (Informant's client). The colour is decided now, a later role change doesn't matter.</summary>
    public void Mark(PlayerControl target)
    {
        MarkedPlayerId = target.PlayerId;
        MarkedIsEvil = IsEvil(target);
        Message($"Informant: marked {target.Data.PlayerName} ({(MarkedIsEvil ? "red" : "green")})");
    }

    /// <summary>Shows the marked player's colour to <paramref name="receiver" /> and clears the mark (Informant's client).</summary>
    public void Show(PlayerControl receiver)
    {
        if (!HasMark)
        {
            return;
        }

        RpcInformantShow(Player, receiver.PlayerId, MarkedPlayerId, MarkedIsEvil);
        MarkedPlayerId = NoMark;
    }

    /// <summary>Red or green by the Seer's rules, with the Informant's own copy of the Seer's colour switches.</summary>
    public static bool IsEvil(PlayerControl target)
    {
        var options = OptionGroupSingleton<InformantOptions>.Instance;
        return (target.Is(RoleAlignment.CrewmateKilling) && options.ShowCrewmateKillingAsRed) ||
               (target.Is(RoleAlignment.NeutralBenign) && options.ShowNeutralBenignAsRed) ||
               (target.Is(RoleAlignment.NeutralEvil) && options.ShowNeutralEvilAsRed) ||
               (target.Is(RoleAlignment.NeutralKilling) && options.ShowNeutralKillingAsRed) ||
               (target.Is(RoleAlignment.NeutralOutlier) && options.ShowNeutralOutlierAsRed) ||
               (target.IsImpostor() && !target.IsTraitor()) ||
               (target.IsTraitor() && options.SwapTraitorColors);
    }

    /// <summary>
    ///     Sent to everyone: every client fires the ability event (the Auditor counts a show as an irregularity), only
    ///     the told player keeps the colour. The colour is decided on the Informant's client and sent as a value, so no
    ///     option is read in here (CAVEATS: options in RPC bodies).
    /// </summary>
    [MethodRpc((uint)TownOfUsRpc.InformantShow)]
    public static void RpcInformantShow(PlayerControl informant, byte receiverId, byte targetId, bool isEvil)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(informant);
            return;
        }

        if (informant.Data.Role is not InformantRole)
        {
            Error("RpcInformantShow - Invalid informant");
            return;
        }

        var receiver = MiscUtils.PlayerById(receiverId);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.InformantShow, informant, receiver);
        touAbilityEvent.InvokeEvent();

        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.PlayerId != receiverId)
        {
            return;
        }

        var target = MiscUtils.PlayerById(targetId);
        if (target == null)
        {
            Error("RpcInformantShow - Invalid target");
            return;
        }

        // One modifier per player (CAVEATS: no remove-then-add of a unique modifier): a later show changes the colour.
        if (target.TryGetModifier<InformantRevealModifier>(out var reveal))
        {
            reveal.SetColor(isEvil);
        }
        else
        {
            target.AddModifier<InformantRevealModifier>(isEvil);
        }

        Message($"Informant: told {target.Data.PlayerName} is {(isEvil ? "red" : "green")}, visible from the next meeting");
    }
}
