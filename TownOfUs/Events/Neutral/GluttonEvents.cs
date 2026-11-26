using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Are;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Networking;

namespace TownOfUs.Events.Neutral;

/// <summary>
///     Glutton (AmonRa Edition): the swallowed come out when the Glutton dies or leaves, and the host checks whether
///     everyone left alive is swallowed. Logic: <see cref="GluttonSystem" />.
/// </summary>
public static class GluttonEvents
{
    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player == null)
        {
            return;
        }

        GluttonSystem.OnGluttonGone(@event.Player.PlayerId);
        Coroutines.Start(CoCheckNextFrame());
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        var player = @event.ClientData.Character;
        if (!player)
        {
            return;
        }

        GluttonSystem.OnGluttonGone(player.PlayerId);
        Coroutines.Start(CoCheckNextFrame());
    }

    /// <summary>
    ///     Safety net (host): a meeting that started without the host's meeting hook (it digests everyone before the
    ///     meeting screen exists). Anyone still swallowed dies at once, like a guess.
    /// </summary>
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }

        foreach (var mod in ModifierUtils.GetActiveModifiers<GluttonSwallowedModifier>().ToList())
        {
            if (mod.Player == null || mod.Player.HasDied() || mod.DigestSent)
            {
                continue;
            }

            mod.DigestSent = true;
            var glutton = mod.Glutton;
            var source = glutton != null && !glutton.HasDied() ? glutton : mod.Player;
            Warning($"Glutton: {mod.Player.Data.PlayerName} was still swallowed when the meeting started");
            source.RpcMeetingMurder(mod.Player, MeetingAnimation.PlayerNameplateAnimation,
                CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate), causeOfDeath: "Glutton");
        }
    }

    private static IEnumerator CoCheckNextFrame()
    {
        yield return null;
        GluttonSystem.CheckEveryoneSwallowed();
    }
}
