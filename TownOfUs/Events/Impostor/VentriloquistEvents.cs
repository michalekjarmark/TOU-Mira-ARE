using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using TownOfUs.Are;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

/// <summary>
///     Ventriloquist (AmonRa Edition): registers the chat channel and drops the voices when a round starts, so a voice
///     works for one meeting only. Runs on every client (the voice is synced).
/// </summary>
public static class VentriloquistEvents
{
    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        VentriloquistChat.Register();
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        foreach (var role in CustomRoleUtils.GetActiveRolesOfType<VentriloquistRole>())
        {
            role.VoiceId = VentriloquistRole.NoVoice;
            role.MessagesSent = 0;
        }
    }
}
