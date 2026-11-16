using TownOfUs.Options.Roles.Impostor;

namespace TownOfUs.Modifiers.Impostor;

/// <summary>
///     A Moderator's ban (AmonRa Edition), added on every client by <c>ModeratorRole.RpcModeratorBan</c>. An ability ban
///     uses Town of Us' "disabled" state (every ability button, kill and sabotage blocked); the other types only switch
///     off what they name, in <see cref="TownOfUs.Are.ModeratorSystem" />. Bans end at the next meeting (or after their
///     time), a vote ban at the end of that meeting. The Cleric's Cleanse removes them.
/// </summary>
public sealed class ModeratorBanModifier(byte moderatorId, int banType, float duration) : DisabledModifier
{
    // "Until the meeting": long enough to never run out before one.
    public const float UntilMeeting = 100000f;

    public override string ModifierName => "Moderator Ban";

    public byte ModeratorId { get; } = moderatorId;
    public ModeratorBanType BanType { get; } = (ModeratorBanType)banType;

    public override float Duration => duration > 0f ? duration : UntilMeeting;
    public override bool AutoStart => true;

    public override bool CanUseAbilities => BanType != ModeratorBanType.Abilities;
    public override bool CanUseConsoles => true;
    public override bool CanOpenMap => true;
    public override bool CanReport => BanType != ModeratorBanType.Reports;

    public override string GetDescription()
    {
        return "You are banned!";
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        if (BanType != ModeratorBanType.Vote)
        {
            RemoveSelf();
        }
    }
}
