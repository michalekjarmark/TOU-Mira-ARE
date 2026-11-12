using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

/// <summary>
///     Ventriloquist (AmonRa Edition, Impostor Support): eavesdrops on a player during the round (an interaction, so the
///     Veteran, Warden, Sixth Sense, Auditor etc. react to it). At the next meeting they can write in the public chat with
///     that player's voice: the message looks exactly like one written by that player. The voice is lost when the next
///     round starts. Chat side: <see cref="VentriloquistChat" />.
/// </summary>
public sealed class VentriloquistRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable,
    IDoomable, ICrewVariant
{
    public const byte NoVoice = byte.MaxValue;

    public string IdPart => "Ventriloquist";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public DoomableType DoomHintType => DoomableType.Trickster;

    // Imitator: the crew role that listens in on others.
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<AurialRole>());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(AreAssets.VentriloquistIcon, "TouMira.Role.Impostor.Ventriloquist",
            1.45f),
        UseVanillaKillButton = true,
        Icon = AreAssets.VentriloquistIcon,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Eavesdrop", "Eavesdrop"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Eavesdrop.WikiDescription"),
            AreAssets.VentriloquistButton),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Speak", "Speak With Their Voice"),
            MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Speak.WikiDescription"),
            AreAssets.VentriloquistIcon)
    ];

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
               MiscUtils.AppendOptionsText(GetType());
    }

    /// <summary>The eavesdropped player (synced): whose voice can be used at the next meeting.</summary>
    public byte VoiceId { get; set; } = NoVoice;

    /// <summary>Messages written with the current voice (synced), limited by the MessagesPerMeeting option.</summary>
    public int MessagesSent { get; set; }

    public PlayerControl? VoicePlayer => VoiceId == NoVoice ? null : GameData.Instance.GetPlayerById(VoiceId)?.Object;

    /// <summary>The player whose voice the local (living) Ventriloquist has: gets a "♪" after the name.</summary>
    public static bool IsLocalVoice(PlayerControl player)
    {
        var local = PlayerControl.LocalPlayer;
        return local != null && local.Data != null && !local.HasDied() &&
               local.Data.Role is VentriloquistRole role && role.VoiceId == player.PlayerId;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        if (Player == null || !Player.AmOwner)
        {
            return sb;
        }

        var voice = VoicePlayer;
        var line = voice != null
            ? MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistTabVoice")
                .Replace("<player>", voice.Data.PlayerName)
            : MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistTabNoVoice");
        sb.AppendLine(TownOfUsPlugin.Culture, $"<size=60%><color=#BFBFBF>{line}</color></size>");
        return sb;
    }

    /// <summary>Line under the meeting timer (<c>AreMeetingTimerTextPatch</c>).</summary>
    public string MeetingStatusText()
    {
        var voice = VoicePlayer;
        if (voice == null)
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistMeetingNoVoice");
        }

        if (!VentriloquistChat.CanBeVoiced(voice))
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistMeetingVoiceLost")
                .Replace("<player>", voice.Data.PlayerName);
        }

        return MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistMeetingVoice")
            .Replace("<player>", voice.Data.PlayerName)
            .Replace("<left>", $"{Math.Max(0, VentriloquistChat.MessagesPerMeeting - MessagesSent)}");
    }

    [MethodRpc((uint)TownOfUsRpc.VentriloquistEavesdrop)]
    public static void RpcVentriloquistEavesdrop(PlayerControl source, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (source.Data.Role is not VentriloquistRole role)
        {
            Error("RpcVentriloquistEavesdrop - Invalid source");
            return;
        }

        role.VoiceId = target.PlayerId;
        role.MessagesSent = 0;
        Message($"Ventriloquist: {source.Data.PlayerName} eavesdropped on {target.Data.PlayerName}");

        var touAbilityEvent = new TouAbilityEvent(AbilityType.VentriloquistEavesdrop, source, target);
        touAbilityEvent.InvokeEvent();
    }

    /// <summary>A message written with the voice. The work is done in <see cref="VentriloquistChat.HandleSay" />.</summary>
    [MethodRpc((uint)TownOfUsRpc.VentriloquistSay)]
    public static void RpcVentriloquistSay(PlayerControl source, PlayerControl voice, string text)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        VentriloquistChat.HandleSay(source, voice, text);
    }
}
