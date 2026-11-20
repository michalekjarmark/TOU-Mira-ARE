using UnityEngine;

namespace TownOfUs.Modules.ControlSystem;

/// <summary>
/// Per-client state for Parasite control. This is intentionally client-local:
/// - The Parasite sends the desired direction for a controlled player via RPC.
/// - The controlled player's owner applies that direction inside a movement patch.
/// </summary>
public static class ParasiteControlState
{
    // After initial control begins, different clients may briefly disagree on transform state.
    // During this grace window we avoid applying any victim movement input to prevent desync.
    public const float InitialControlSyncGraceSeconds = 1.0f;

    private static readonly Dictionary<byte, byte> ControlledBy = [];
    private static readonly Dictionary<byte, Vector2> ControlledDirection = [];
    private static readonly Dictionary<byte, Vector2> ControlledPosition = [];
    private static readonly Dictionary<byte, Vector2> ControlledVelocity = [];
    private static readonly Dictionary<byte, float> ControlledSince = [];

    // AmonRa Edition: when each position arrived (see GetPosition).
    private static readonly Dictionary<byte, float> ControlledPositionAt = [];
    private const float MaxPositionExtrapolationSeconds = 0.2f;

    public static void SetControl(byte controlledId, byte controllerId)
    {
        ControlledBy[controlledId] = controllerId;
        ControlledDirection[controlledId] = Vector2.zero;
        ControlledPosition[controlledId] = Vector2.zero;
        ControlledVelocity[controlledId] = Vector2.zero;
        ControlledSince[controlledId] = Time.time;
    }

    public static void ClearControl(byte controlledId)
    {
        ControlledBy.Remove(controlledId);
        ControlledDirection.Remove(controlledId);
        ControlledPosition.Remove(controlledId);
        ControlledVelocity.Remove(controlledId);
        ControlledSince.Remove(controlledId);
        ControlledPositionAt.Remove(controlledId); // AmonRa Edition
    }

    public static bool IsControlled(byte controlledId, out byte controllerId)
    {
        return ControlledBy.TryGetValue(controlledId, out controllerId);
    }

    public static void SetDirection(byte controlledId, Vector2 direction)
    {
        ControlledDirection[controlledId] = direction;
    }

    public static Vector2 GetDirection(byte controlledId)
    {
        return ControlledDirection.TryGetValue(controlledId, out var dir) ? dir : Vector2.zero;
    }

    public static void SetMovementState(byte controlledId, Vector2 position, Vector2 velocity)
    {
        ControlledPosition[controlledId] = position;
        ControlledVelocity[controlledId] = velocity;
        ControlledPositionAt[controlledId] = Time.time; // AmonRa Edition
    }

    // AmonRa Edition: the last position moved on by its velocity for the time since it arrived (at most 0.2 s).
    // It is only used to correct a client's own simulation of the controlled player, every physics frame: compared
    // with the bare last position, the gap grew until the next packet (they are unreliable, so some get lost), and
    // past 0.5 the player was snapped back, which looked like teleporting on everyone's screen but the controller's
    // (owner's report, 2026-11-20: the controlled host saw their character jump around).
    public static Vector2 GetPosition(byte controlledId)
    {
        if (!ControlledPosition.TryGetValue(controlledId, out var pos) || pos == Vector2.zero)
        {
            return Vector2.zero;
        }

        if (!ControlledPositionAt.TryGetValue(controlledId, out var at) ||
            !ControlledVelocity.TryGetValue(controlledId, out var vel))
        {
            return pos;
        }

        return pos + vel * Mathf.Clamp(Time.time - at, 0f, MaxPositionExtrapolationSeconds);
    }

    public static Vector2 GetVelocity(byte controlledId)
    {
        return ControlledVelocity.TryGetValue(controlledId, out var vel) ? vel : Vector2.zero;
    }

    public static float GetControlElapsedSeconds(byte controlledId)
    {
        return ControlledSince.TryGetValue(controlledId, out var since) ? Mathf.Max(0f, Time.time - since) : float.PositiveInfinity;
    }

    public static bool IsInInitialGrace(byte controlledId)
    {
        return GetControlElapsedSeconds(controlledId) < InitialControlSyncGraceSeconds;
    }

    public static void ClearMovementState(byte controlledId)
    {
        ControlledPosition[controlledId] = Vector2.zero;
        ControlledVelocity[controlledId] = Vector2.zero;
        ControlledDirection[controlledId] = Vector2.zero;
    }

    public static void ClearAll()
    {
        ControlledBy.Clear();
        ControlledDirection.Clear();
        ControlledPosition.Clear();
        ControlledVelocity.Clear();
        ControlledSince.Clear();
        ControlledPositionAt.Clear(); // AmonRa Edition
    }
}