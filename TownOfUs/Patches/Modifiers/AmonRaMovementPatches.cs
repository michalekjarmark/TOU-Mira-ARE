using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Patches.Modifiers;

/// <summary>
///     ARE: wheelchair movement for <see cref="AmonRaModifier"/>.
///     Vanilla <see cref="PlayerPhysics.FixedUpdate"/> sets the body velocity straight from the input. For the local
///     AmonRa player we take that velocity as the *target* and move the real velocity towards it with limited
///     acceleration: speeding up takes time, letting go rolls on, and sideways speed decays slowly (sliding in turns).
///     In exchange the top speed on straight lines is higher. Movement is client-authoritative in Among Us, so changing
///     the local body velocity is automatically synced to the other players.
/// </summary>
[HarmonyPatch(typeof(PlayerPhysics))]
public static class AmonRaMovementPatches
{
    /// <summary>
    ///     Velocity of the local body before vanilla overwrote it in this physics step. It already contains the result
    ///     of collisions from the previous step, so the chair stops at walls instead of storing "phantom" speed.
    /// </summary>
    private static Vector2 _velocityBeforeStep;
    private static bool _captured;

    [HarmonyPatch(nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPrefix]
    public static void FixedUpdatePrefix(PlayerPhysics __instance)
    {
        _captured = false;
        if (__instance.body == null || !IsActiveForLocalPlayer(__instance))
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
        if (!_captured || __instance.body == null || !IsActiveForLocalPlayer(__instance))
        {
            return;
        }

        _captured = false;
        var options = OptionGroupSingleton<AmonRaOptions>.Instance;
        var dt = Time.fixedDeltaTime;

        var baseSpeed = Mathf.Max(__instance.TrueSpeed, 0.01f);
        var desired = __instance.body.velocity; // what vanilla wants (input * speed, incl. other speed modifiers)
        var target = desired * options.TopSpeed;
        var current = _velocityBeforeStep;

        var accelRate = baseSpeed * options.TopSpeed / Mathf.Max(options.AccelerationTime, 0.05f);
        var brakeRate = baseSpeed / Mathf.Max(options.BrakingTime, 0.05f);

        Vector2 next;
        if (target.sqrMagnitude < 0.0001f)
        {
            // No input: roll to a stop.
            next = Vector2.MoveTowards(current, Vector2.zero, brakeRate * dt);
        }
        else
        {
            var dir = target.normalized;
            var targetSpeed = target.magnitude;
            var forward = Vector2.Dot(current, dir);
            var sideways = current - (dir * forward);

            // Forward: accelerate towards the target speed. Moving backwards (sharp U-turn) brakes + accelerates.
            float newForward;
            if (forward < targetSpeed)
            {
                var rate = forward < 0f ? accelRate + brakeRate : accelRate;
                newForward = Mathf.Min(forward + (rate * dt), targetSpeed);
            }
            else
            {
                newForward = Mathf.Max(forward - (brakeRate * dt), targetSpeed);
            }

            // Sideways: old momentum fades slowly -> sliding when turning.
            var newSideways = Vector2.MoveTowards(sideways, Vector2.zero, brakeRate * options.TurnGrip * dt);
            next = (dir * newForward) + newSideways;
        }

        next = Vector2.ClampMagnitude(next, baseSpeed * options.TopSpeed);
        __instance.body.velocity = next;
    }

    /// <summary>
    ///     Players in a wheelchair don't run: keep the idle ("sitting") body animation while moving and skip footsteps.
    ///     Runs for every player on every client, as remote animations are computed locally.
    /// </summary>
    [HarmonyPatch(nameof(PlayerPhysics.HandleAnimation))]
    [HarmonyPostfix]
    public static void HandleAnimationPostfix(PlayerPhysics __instance, bool amDead)
    {
        var player = __instance.myPlayer;
        if (amDead || player == null || player.Data == null || __instance.Animations == null)
        {
            return;
        }

        if (player.inVent || player.onLadder || player.inMovingPlat || player.walkingToVent ||
            !player.HasModifier<AmonRaModifier>() || !AmonRaModifier.ShouldShowWheelchair(player))
        {
            return;
        }

        if (__instance.Animations.IsPlayingRunAnimation())
        {
            __instance.Animations.PlayIdleAnimation();
            player.cosmetics.AnimateSkinIdle();
        }

        if (player.FootSteps != null && player.FootSteps.isPlaying)
        {
            player.FootSteps.Stop();
        }
    }

    private static bool IsActiveForLocalPlayer(PlayerPhysics physics)
    {
        var player = physics.myPlayer;
        if (player == null || !player.AmOwner || player.Data == null || player.Data.IsDead)
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

        if (player.Data.Role is ParasiteRole { Controlled: not null } or PuppeteerRole { Controlled: not null })
        {
            return false;
        }

        return player.HasModifier<AmonRaModifier>();
    }
}
