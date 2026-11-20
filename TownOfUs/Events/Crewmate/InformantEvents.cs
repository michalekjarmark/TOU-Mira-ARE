using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Informant (AmonRa Edition): a shown colour waits for the next meeting. When a meeting starts, every colour the local
///     player was told becomes visible on the name and stays (only on that player's client, nothing is synced).
/// </summary>
public static class InformantEvents
{
    [RegisterEvent]
    public static void StartMeetingHandler(StartMeetingEvent @event)
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player.TryGetModifier<InformantRevealModifier>(out var reveal) && !reveal.Visible)
            {
                reveal.Visible = true;
            }
        }
    }
}
