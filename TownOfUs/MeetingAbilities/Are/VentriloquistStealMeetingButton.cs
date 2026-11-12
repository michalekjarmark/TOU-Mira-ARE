using HarmonyLib;
using MiraAPI.MeetingAbilities;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Are;

/// <summary>
///     Ventriloquist's "Steal Vote" (AmonRa Edition): the eavesdropped player's vote goes to the player (or Skip Vote) whose
///     vote popup the button is in. Built like the Gambler's bet button (<see cref="GamblerBetMeetingButton" />, see
///     there why): created by MiraAPI but kept inactive, and shown by <see cref="VentriloquistVotePopupPatch" /> in a
///     fixed slot only when the vote can be stolen: left of Confirm for players, right of Cancel for Skip Vote.
/// </summary>
public sealed class VentriloquistStealMeetingButton : TargetedMeetingButton
{
    // Slots of MiraAPI's popup layout: x = 1.3 - 0.65 * index, Cancel is index 0 and Confirm index 1.
    private const float PlayerSlotX = 0f;
    private const float SkipSlotX = 1.95f;

    // Our buttons of the current meeting by vote area (player id, 253 = Skip Vote).
    private static readonly Dictionary<byte, MeetingAbilityBehaviour> Created = [];
    private static MeetingHud? _createdFor;

    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.VentriloquistStealVote", "Steal Vote");
    public override float Cooldown => 0.0001f;
    public override float InitialCooldown => 0.0001f;

    // Unlimited for MiraAPI: one steal per meeting is enforced by VentriloquistVote.CanSteal.
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => AreAssets.VentriloquistStealButton;
    public override Color OutlineColor => TownOfUsColors.Impostor;

    // Decided once when the meeting starts (see CAVEATS): the role and the option. The rest is checked per popup.
    public override bool Enabled(RoleBehaviour r)
    {
        return r is VentriloquistRole && VentriloquistVote.Mode != VentriloquistVoteSteal.Off;
    }

    public override bool IsTargetValid(PlayerVoteArea playerVoteArea)
    {
        return VentriloquistVote.IsValidTarget(playerVoteArea);
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

        // Outside the player list mask (Skip Vote) the hover highlight needs no mask (see CAVEATS).
        if (playerVoteArea.PlayerId.Value == VentriloquistVote.SkipVote)
        {
            foreach (var renderer in behaviour.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.gameObject != behaviour.gameObject)
                {
                    renderer.maskInteraction = SpriteMaskInteraction.None;
                }
            }
        }

        behaviour.gameObject.SetActive(false);
        return behaviour;
    }

    protected override void OnClick(PlayerVoteArea playerVoteArea)
    {
        if (PlayerControl.LocalPlayer.Data.Role is VentriloquistRole role && VentriloquistVote.CanSteal(role))
        {
            if (VentriloquistVote.Mode == VentriloquistVoteSteal.InsteadOfChat)
            {
                // The voice channel closes now: leave it first.
                VentriloquistChat.LeaveChannelIfSelected();
            }

            VentriloquistRole.RpcVentriloquistStealVote(PlayerControl.LocalPlayer, playerVoteArea.PlayerId.Value);
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

    /// <summary>Hides every steal button, also in a popup that is open right now.</summary>
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

    /// <summary>Shows the button in this popup if the vote can be stolen for it, sliding in like Confirm / Cancel.</summary>
    internal static void ShowIfValid(PlayerVoteArea area)
    {
        if (!Created.TryGetValue(area.PlayerId.Value, out var button) || !button || !area.Buttons ||
            !area.Buttons.activeSelf)
        {
            return;
        }

        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data.Role is not VentriloquistRole role ||
            !VentriloquistVote.CanSteal(role) || !VentriloquistVote.IsValidTarget(area))
        {
            return;
        }

        var isSkip = area.PlayerId.Value == VentriloquistVote.SkipVote;
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
///     Opening a vote popup: the steal button is hidden while MiraAPI lays the popup out and shown afterwards in its own
///     slot, see <see cref="VentriloquistStealMeetingButton" />.
/// </summary>
[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
public static class VentriloquistVotePopupPatch
{
    [HarmonyPriority(Priority.First)]
    public static void Prefix(PlayerVoteArea __instance)
    {
        VentriloquistStealMeetingButton.Hide(__instance);
    }

    public static void Postfix(PlayerVoteArea __instance)
    {
        VentriloquistStealMeetingButton.ShowIfValid(__instance);
    }
}
