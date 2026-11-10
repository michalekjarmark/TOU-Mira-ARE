using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Roles;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

/// <summary>
///     Auditor (AmonRa Edition): feeds the running audit with synced events (task completion, kills, role abilities),
///     gives audits for own tasks and applies the sanction when the host counts the votes.
/// </summary>
public static class AuditorEvents
{
    // The audit runs only on the Auditor's own client.
    private static AuditorRole? LocalAuditor => PlayerControl.LocalPlayer?.Data?.Role as AuditorRole;

    [RegisterEvent]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        var auditor = LocalAuditor;
        if (auditor == null)
        {
            return;
        }

        if (@event.Player.AmOwner)
        {
            auditor.OnOwnTaskCompleted();
        }
        else if (auditor.ActiveAudit is { } audit && audit.TargetId == @event.Player.PlayerId)
        {
            audit.OnTaskCompleted();
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (LocalAuditor?.ActiveAudit is { } audit && @event.Source != null &&
            audit.TargetId == @event.Source.PlayerId && @event.Source != @event.Target)
        {
            audit.AddIrregularity();
        }
    }

    [RegisterEvent]
    public static void TouAbilityEventHandler(TouAbilityEvent @event)
    {
        if (LocalAuditor?.ActiveAudit is { } audit && @event.Player != null && audit.TargetId == @event.Player.PlayerId)
        {
            audit.AddIrregularity();
        }
    }

    // Before the Swapper (10): a frozen vote is gone before anything moves it around.
    [RegisterEvent(5)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        foreach (var auditor in CustomRoleUtils.GetActiveRolesOfType<AuditorRole>())
        {
            var frozen = auditor.SanctionedThisMeeting;
            if (frozen != byte.MaxValue)
            {
                @event.Votes.RemoveAll(v => v.Voter == frozen);
            }
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        foreach (var auditor in CustomRoleUtils.GetActiveRolesOfType<AuditorRole>())
        {
            auditor.SanctionedThisMeeting = byte.MaxValue;
        }
    }
}
