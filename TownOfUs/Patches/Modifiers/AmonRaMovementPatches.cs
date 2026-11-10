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
///     AmonRa player we take that velocity as the *input* of a small vehicle model (see <see cref="ComputeVelocity"/>):
///     speeding up takes time, letting go rolls on, turning is rate-limited and sloppy steering skids. Driving well
///     keeps the momentum and a higher top speed. Movement is client-authoritative in Among Us, so changing the local
///     body velocity is automatically synced to the other players.
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
        __instance.body.velocity = ComputeVelocity(
            _velocityBeforeStep,
            __instance.body.velocity, // what vanilla wants: input * speed, incl. other speed modifiers
            Mathf.Max(__instance.TrueSpeed, 0.01f),
            Time.fixedDeltaTime,
            OptionGroupSingleton<AmonRaOptions>.Instance);
    }

    // Skill model: the chair has a heading and a speed instead of following the input directly.
    // - The heading turns towards the input at a limited rate. The rate is high when slow and low at top speed,
    //   so fast driving needs wide, early turns (or slowing down before a corner).
    // - Speed only grows while the input points where the chair is going (precise steering). Gentle steering
    //   (up to HoldAngle away, e.g. one diagonal step on a keyboard) keeps the speed, so a well-driven chair keeps its
    //   momentum through rooms and corridors.
    // - Yanking the stick far away from the heading skids: the chair loses speed, the more the sharper the input.
    // - Walls take away the speed going into them (the velocity is read back after the physics step).
    private const float AlignAngle = 25f;  // within this the chair accelerates
    private const float HoldAngle = 60f;   // up to this speed is kept while turning
    private const float SkidStrength = 4f; // braking multiplier for a full 180 degree input
    private const float StartSpeedFraction = 0.05f;

    private static Vector2 ComputeVelocity(Vector2 current, Vector2 desired, float baseSpeed, float dt, AmonRaOptions options)
    {
        var maxSpeed = baseSpeed * options.TopSpeed;
        var accelRate = maxSpeed / Mathf.Max(options.AccelerationTime, 0.05f);
        var rollRate = baseSpeed / Mathf.Max(options.BrakingTime, 0.05f);
        var speed = current.magnitude;

        if (desired.sqrMagnitude < 0.0001f)
        {
            // No input: roll on and slowly stop.
            return Vector2.MoveTowards(current, Vector2.zero, rollRate * dt);
        }

        var inputDir = desired.normalized;
        var targetSpeed = Mathf.Min(desired.magnitude * options.TopSpeed, maxSpeed);

        // Standing (almost) still: the chair can start in any direction.
        var heading = speed < baseSpeed * StartSpeedFraction ? inputDir : current / speed;
        var angle = Vector2.SignedAngle(heading, inputDir);
        var absAngle = Mathf.Abs(angle);

        // Turn towards the input, slower the faster we go.
        var turnRate = Mathf.Lerp(AmonRaOptions.SlowTurnSpeed, options.TurnSpeed, Mathf.Clamp01(speed / maxSpeed));
        var turn = Mathf.Clamp(angle, -turnRate * dt, turnRate * dt);
        var newHeading = (Vector2)(Quaternion.Euler(0f, 0f, turn) * heading);

        if (absAngle <= AlignAngle)
        {
            speed = speed < targetSpeed
                ? Mathf.Min(speed + (accelRate * dt), targetSpeed)
                : Mathf.Max(speed - (rollRate * dt), targetSpeed);
        }
        else if (absAngle > HoldAngle)
        {
            // Sloppy steering skids: 0 at HoldAngle, SkidStrength x rolling deceleration at 180 degrees.
            var skid = (absAngle - HoldAngle) / (180f - HoldAngle);
            speed = Mathf.Max(speed - (rollRate * SkidStrength * skid * dt), 0f);
        }

        return Vector2.ClampMagnitude(newHeading * speed, maxSpeed);
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
