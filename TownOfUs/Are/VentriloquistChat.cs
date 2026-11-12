using System.Runtime.CompilerServices;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches.Options;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Are;

/// <summary>
///     Chat side of the Ventriloquist (AmonRa Edition). In a meeting the Ventriloquist gets a team chat channel
///     "Voice: &lt;player&gt;" (TOU's extension team chat system, switched with the same button as the impostor chat).
///     A message sent there reaches everyone as a normal public chat message of the eavesdropped player: TOU's
///     <c>ChatController.AddChat</c> prefix draws it like any other message of that player (name, color, sound, voted
///     mark, censoring; on that player's own screen it's on the right like their own). Among Us has no "is typing"
///     indicator, so nothing gives the author away. Dead players with "The Dead Know" see a note with the real author.
/// </summary>
public static class VentriloquistChat
{
    // After TOU's own team chats (Jailor 10, Jailee 20, Impostor 30, Vampire 40).
    private const int ChannelPriority = 90;

    private const int MaxLength = 200;

    private static readonly TeamChatPatches.ExtensionTeamChatHandler Handler = new()
    {
        Priority = ChannelPriority,
        IsChatAvailable = LocalChannelOpen,
        SendMessage = Send,
        GetDisplayText = ChannelName,
        DisplayTextColor = TownOfUsColors.Impostor
    };

    public static int MessagesPerMeeting =>
        Math.Max(1, (int)OptionGroupSingleton<VentriloquistOptions>.Instance.MessagesPerMeeting);

    /// <summary>Adds the channel to TOU's team chats (once; the registry ignores the same handler again).</summary>
    public static void Register()
    {
        TeamChatPatches.ExtensionTeamChatRegistry.RegisterHandler(Handler);
    }

    /// <summary>
    ///     Whether a player's voice can be used right now: alive and able to write in the public chat. A message from a
    ///     blackmailed player (or a jailed one who can't use the public chat) would be impossible and give the
    ///     Ventriloquist away.
    /// </summary>
    public static bool CanBeVoiced(PlayerControl? player)
    {
        if (player == null || player.Data == null || player.Data.Disconnected || player.HasDied())
        {
            return false;
        }

        if (player.HasModifier<BlackmailedModifier>())
        {
            return false;
        }

        return !player.IsJailed() || OptionGroupSingleton<JailorOptions>.Instance.JaileePublicChat;
    }

    private static VentriloquistRole? LocalVentriloquist()
    {
        var local = PlayerControl.LocalPlayer;
        return local != null && local.Data != null && !local.HasDied() && local.Data.Role is VentriloquistRole role
            ? role
            : null;
    }

    // The channel stays open for the whole meeting once there is a voice, even when its owner can't speak any more
    // (then sending only shows a note): when the selected channel disappears, TOU falls back to another team chat, or
    // with none left sends the text to the public chat under the Ventriloquist's own name.
    private static bool LocalChannelOpen()
    {
        var role = LocalVentriloquist();
        return MeetingHud.Instance && role != null && role.VoiceId != VentriloquistRole.NoVoice &&
               role.MessagesSent < MessagesPerMeeting && !PlayerControl.LocalPlayer.HasModifier<BlackmailedModifier>();
    }

    private static string ChannelName()
    {
        var voice = LocalVentriloquist()?.VoicePlayer;
        var key = CanBeVoiced(voice)
            ? "TownOfUsMira.Role.VentriloquistChannel"
            : "TownOfUsMira.Role.VentriloquistChannelLost";
        return MiraLocaleManager.Get(key).Replace("<player>", voice?.Data?.PlayerName ?? "?");
    }

    private static void Send(PlayerControl sender, string message)
    {
        var role = LocalVentriloquist();
        var text = message.Trim();
        if (role == null || text.Length == 0)
        {
            return;
        }

        var voice = role.VoicePlayer;
        if (!CanBeVoiced(voice))
        {
            // Nothing is sent, only a private note in the channel.
            MiscUtils.AddTeamChat(sender.Data, ChannelName(),
                MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistVoiceLost")
                    .Replace("<player>", voice?.Data?.PlayerName ?? "?"),
                onLeft: false, bubbleType: BubbleType.Other);
            return;
        }

        VentriloquistRole.RpcVentriloquistSay(sender, voice!, text.Length > MaxLength ? text[..MaxLength] : text);

        // Last message for this meeting: back to the public chat before the channel closes (see LocalChannelOpen).
        if (role.MessagesSent >= MessagesPerMeeting)
        {
            TeamChatPatches.ForceNormalChat();
            TeamChatPatches.UpdateChat();
        }
    }

    /// <summary>
    ///     Body of <see cref="VentriloquistRole.RpcVentriloquistSay" /> on every client (reads options, see CAVEATS).
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleSay(PlayerControl source, PlayerControl voice, string text)
    {
        if (!MeetingHud.Instance || source.HasDied() || source.Data.Role is not VentriloquistRole role)
        {
            Error("RpcVentriloquistSay - invalid source or no meeting");
            return;
        }

        if (voice == source || voice.PlayerId != role.VoiceId || !CanBeVoiced(voice))
        {
            Error($"RpcVentriloquistSay - {voice.Data.PlayerName} can't be voiced");
            return;
        }

        if (role.MessagesSent >= MessagesPerMeeting || string.IsNullOrWhiteSpace(text))
        {
            Error("RpcVentriloquistSay - message limit reached or empty message");
            return;
        }

        if (text.Length > MaxLength)
        {
            text = text[..MaxLength];
        }

        role.MessagesSent++;
        Message($"Ventriloquist: {source.Data.PlayerName} wrote as {voice.Data.PlayerName} " +
                $"({role.MessagesSent}/{MessagesPerMeeting})");

        var shown = text;
        if (TeamChatPatches.LocalPlayerDeadKnows)
        {
            var note = MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistDeadNote")
                .Replace("<player>", source.Data.PlayerName);
            shown += $"\n<size=70%>{TownOfUsColors.Impostor.ToTextColor()}{note}</color></size>";
        }

        HudManager.Instance.Chat.AddChat(voice, shown, true);

        if (source.AmOwner)
        {
            var header = MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistSentAs")
                .Replace("<player>", voice.Data.PlayerName);
            MiscUtils.AddTeamChat(voice.Data, $"{TownOfUsColors.Impostor.ToTextColor()}{header}</color>", text,
                onLeft: false, bubbleType: BubbleType.Other);
        }
    }
}
