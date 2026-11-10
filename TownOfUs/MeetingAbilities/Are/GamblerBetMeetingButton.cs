using MiraAPI.MeetingAbilities;
using TownOfUs.Are;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Are;

/// <summary>
///     Gambler's "Bet" (AmonRa Edition): a MiraAPI targeted meeting button, like the Prosecutor's. It appears in the vote
///     popup (next to Confirm / Cancel) of every player the Gambler can bet on and of Skip Vote ("no one is ejected"),
///     while the betting window is open and no bet was placed yet. Visibility is checked every time a popup opens.
/// </summary>
public sealed class GamblerBetMeetingButton : TargetedMeetingButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.GamblerBet", "Bet");
    public override float Cooldown => 0.0001f;
    public override float InitialCooldown => 0.0001f;

    // Unlimited for MiraAPI: one bet per meeting is enforced by GamblerRole.CanBetNow.
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => AreAssets.GamblerBetButton;
    public override Color OutlineColor => TownOfUsColors.Gambler;

    public override bool Enabled(RoleBehaviour r)
    {
        return r is GamblerRole { CanBetNow: true };
    }

    public override bool IsTargetValid(PlayerVoteArea playerVoteArea)
    {
        return PlayerControl.LocalPlayer.Data.Role is GamblerRole gambler && gambler.CanBetOn(playerVoteArea);
    }

    protected override void OnClick(PlayerVoteArea playerVoteArea)
    {
        if (PlayerControl.LocalPlayer.Data.Role is GamblerRole gambler)
        {
            gambler.PlaceBet(playerVoteArea.PlayerId);
        }
    }
}
