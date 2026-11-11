using HarmonyLib;
using MiraAPI.MeetingAbilities;
using TownOfUs.Are;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Are;

/// <summary>
///     Gambler's "Bet" (AmonRa Edition): a MiraAPI targeted meeting button, like the Prosecutor's. It appears in the vote
///     popup (next to Confirm / Cancel) of every player the Gambler can bet on and of Skip Vote ("no one is ejected"),
///     while the betting window is open and no bet was placed yet.
///     <para>
///         MiraAPI lays out a popup by counting every targeted button of every role, also the hidden ones, so the
///         third slot depends on which buttons exist, and on Skip Vote the third slot covers the Skip Vote button
///         itself (round 5). So the button is created by MiraAPI but kept inactive, and
///         <see cref="GamblerVotePopupPatch" /> shows it only when the Gambler can bet on that popup, in a fixed slot
///         with the same slide-in animation as Confirm / Cancel: left of Confirm for players, right of Cancel for
///         Skip Vote.
///     </para>
/// </summary>
public sealed class GamblerBetMeetingButton : TargetedMeetingButton
{
    // Slots of MiraAPI's popup layout: x = 1.3 - 0.65 * index, Cancel is index 0 and Confirm index 1.
    private const float PlayerSlotX = 0f;
    private const float SkipSlotX = 1.95f;

    // Our buttons of the current meeting by vote area (player id, 253 = Skip Vote).
    private static readonly Dictionary<byte, MeetingAbilityBehaviour> Created = [];
    private static MeetingHud? _createdFor;

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

    public override MeetingAbilityBehaviour CreateButton(PlayerVoteArea playerVoteArea)
    {
        var behaviour = base.CreateButton(playerVoteArea);
        if (behaviour == null)
        {
            return behaviour!;
        }

        if (_createdFor != MeetingHud.Instance)
        {
            Created.Clear();
            _createdFor = MeetingHud.Instance;
        }

        Created[playerVoteArea.PlayerId.Value] = behaviour;
        behaviour.gameObject.SetActive(false);
        return behaviour;
    }

    protected override void OnClick(PlayerVoteArea playerVoteArea)
    {
        if (PlayerControl.LocalPlayer.Data.Role is GamblerRole gambler)
        {
            gambler.PlaceBet(playerVoteArea.PlayerId.Value);
        }

        HideAll();
    }

    /// <summary>Hides the button of this popup before MiraAPI lays it out.</summary>
    internal static void Hide(PlayerVoteArea area)
    {
        if (Created.TryGetValue(area.PlayerId.Value, out var button) && button)
        {
            button.gameObject.SetActive(false);
        }
    }

    /// <summary>Hides every bet button, also in a popup that is open right now (betting window over, bet placed).</summary>
    public static void HideAll()
    {
        foreach (var button in Created.Values)
        {
            if (button)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>Shows the button in this popup if the Gambler can bet on it, sliding in like Confirm / Cancel.</summary>
    internal static void ShowIfValid(PlayerVoteArea area)
    {
        if (!Created.TryGetValue(area.PlayerId.Value, out var button) || !button || !area.Buttons ||
            !area.Buttons.activeSelf)
        {
            return;
        }

        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data.Role is not GamblerRole { CanBetNow: true } gambler ||
            !gambler.CanBetOn(area))
        {
            return;
        }

        var isSkip = area.PlayerId.Value == GamblerRole.NoEjectionBet;
        var start = area.AnimateButtonsFromLeft ? 0.2f : 1.95f;
        var end = isSkip ? SkipSlotX : PlayerSlotX;

        button.transform.localPosition = Vector2.right * start;
        button.gameObject.SetActive(true);
        area.StartCoroutine(Effects.Lerp(0.45f, (Action<float>)(t =>
        {
            if (button)
            {
                button.transform.localPosition = Vector2.Lerp(Vector2.right * start, Vector2.right * end,
                    Effects.ExpOut(t));
            }
        })));
    }
}

/// <summary>
///     Opening a vote popup: our bet button is hidden while MiraAPI lays the popup out (so it never takes or shifts a
///     slot) and shown afterwards in its own slot, see <see cref="GamblerBetMeetingButton" />.
/// </summary>
[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
public static class GamblerVotePopupPatch
{
    [HarmonyPriority(Priority.First)]
    public static void Prefix(PlayerVoteArea __instance)
    {
        GamblerBetMeetingButton.Hide(__instance);
    }

    public static void Postfix(PlayerVoteArea __instance)
    {
        GamblerBetMeetingButton.ShowIfValid(__instance);
    }
}
