using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Patches.Modifiers;

/// <summary>
///     ARE: drives the local <see cref="SleepwalkerModifier"/> player. Vanilla <see cref="PlayerPhysics.FixedUpdate"/>
///     sets the body velocity from the input; without input (after the idle time) we replace it with the
///     sleepwalking velocity. Like <see cref="AmonRaMovementPatches"/>, the velocity before the step tells us about
///     walls. Both are universal modifiers, so a player never has both.
/// </summary>
[HarmonyPatch(typeof(PlayerPhysics))]
public static class SleepwalkerMovementPatches
{
    private static Vector2 _velocityBeforeStep;
    private static bool _captured;

    [HarmonyPatch(nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPrefix]
    public static void FixedUpdatePrefix(PlayerPhysics __instance)
    {
        _captured = false;
        if (__instance.body == null || __instance.myPlayer == null || !__instance.myPlayer.AmOwner)
        {
            return;
        }

        _velocityBeforeStep = __instance.body.velocity;
        _captured = true;
    }

    [HarmonyPatch(nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerPhysics __instance)
    {
        if (!_captured || __instance.body == null)
        {
            return;
        }

        _captured = false;
        var player = __instance.myPlayer;
        if (!player.TryGetModifier<SleepwalkerModifier>(out var sleepwalker))
        {
            return;
        }

        if (!CanSleepwalk(player))
        {
            sleepwalker.Wake();
            return;
        }

        __instance.body.velocity = sleepwalker.Step(__instance.body.velocity, _velocityBeforeStep,
            Mathf.Max(__instance.TrueSpeed, 0.01f), Time.fixedDeltaTime);
    }

    private static bool CanSleepwalk(PlayerControl player)
    {
        if (player.Data == null || player.Data.IsDead || MeetingHud.Instance || ExileController.Instance ||
            !ShipStatus.Instance)
        {
            return false;
        }

        if (!player.CanMove || player.inVent || player.onLadder || player.inMovingPlat || player.walkingToVent)
        {
            return false;
        }

        // Leave special movement systems alone: Time Lord rewind, Parasite / Puppeteer control (both directions).
        if (TimeLordRewindSystem.IsRewinding || AdvancedMovementUtilities.IsApplyingControlledMovement)
        {
            return false;
        }

        if (player.HasModifier<ParasiteInfectedModifier>() || player.HasModifier<PuppeteerControlModifier>())
        {
            return false;
        }

        return player.Data.Role is not (ParasiteRole { Controlled: not null } or PuppeteerRole { Controlled: not null });
    }
}
