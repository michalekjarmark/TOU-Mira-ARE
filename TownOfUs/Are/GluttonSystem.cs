using System.Collections;
using System.Runtime.CompilerServices;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: the Glutton's swallowing (<see cref="GluttonRole" />, <see cref="GluttonSwallowedModifier" />).
///     <para>
///         The host decides: the Glutton's client asks (<c>RpcGluttonSwallow</c>), the host checks and confirms to
///         everyone (<see cref="RpcGluttonConfirmSwallow" />), so every client swallows in the host's order relative
///         to a meeting start. The host also digests (a kill without a body, cause "Glutton"): right before it opens a
///         meeting (<see cref="AreMeetingHold" />, so the meeting shows them dead and they can't talk in it), after the
///         digestion time, and at once when everyone left alive is swallowed (so the Glutton's win isn't held up).
///     </para>
///     When the Glutton dies or leaves, its swallowed come out alive next to it (or die with it, option).
/// </summary>
public static class GluttonSystem
{
    public static IEnumerable<GluttonSwallowedModifier> SwallowedBy(PlayerControl? glutton)
    {
        if (glutton == null)
        {
            return [];
        }

        var id = glutton.PlayerId;
        return ModifierUtils.GetActiveModifiers<GluttonSwallowedModifier>(x => x.GluttonId == id);
    }

    /// <summary>Swallowed, also when the modifier is still waiting to be added (MiraAPI adds it on the next update).</summary>
    public static bool IsSwallowed(PlayerControl? player)
    {
        return player != null && player.GetModifierComponent().HasModifier<GluttonSwallowedModifier>(true);
    }

    /// <summary>The Glutton's request (every client gets it, only the host acts).</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleSwallow(PlayerControl glutton, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (target == null || target.Data == null || target.HasDied() || target.Data.Disconnected ||
            glutton.HasDied() || target == glutton || IsSwallowed(target) || IsSwallowed(glutton))
        {
            Message($"Glutton: swallow of {target?.Data?.PlayerName} by {glutton.Data.PlayerName} refused");
            return;
        }

        // Same rule as a kill: nothing happens once the host has a meeting or an ejection on.
        if (MeetingHud.Instance || ExileController.Instance)
        {
            Message($"Glutton: swallow of {target.Data.PlayerName} refused, the meeting had already started");
            return;
        }

        var max = (int)OptionGroupSingleton<GluttonOptions>.Instance.MaxSwallowed;
        if (max > 0 && SwallowedBy(glutton).Count() >= max)
        {
            Message($"Glutton: swallow of {target.Data.PlayerName} refused, the Glutton is full");
            return;
        }

        RpcGluttonConfirmSwallow(PlayerControl.LocalPlayer, glutton, target);
    }

    [MethodRpc((uint)TownOfUsRpc.GluttonConfirmSwallow)]
    public static void RpcGluttonConfirmSwallow(PlayerControl host, PlayerControl glutton, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(host);
            return;
        }

        if (!host.IsHost())
        {
            Error("RpcGluttonConfirmSwallow - Not sent by the host");
            return;
        }

        ApplySwallow(glutton, target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ApplySwallow(PlayerControl glutton, PlayerControl target)
    {
        if (target == null || target.HasDied() || IsSwallowed(target))
        {
            return;
        }

        target.AddModifier<GluttonSwallowedModifier>(glutton.PlayerId);
        new TouAbilityEvent(AbilityType.GluttonSwallow, glutton, target).InvokeEvent();
        Message($"Glutton: {glutton.Data.PlayerName} swallowed {target.Data.PlayerName}");

        if (glutton.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Glutton.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.GluttonSwallowedSomeone").Replace("<player>", target.Data.PlayerName)}</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: AreAssets.GluttonIcon.LoadAsset());
            notif.AdjustNotification();
            Coroutines.Start(CoScannerGlow(glutton));
        }

        if (AmongUsClient.Instance.AmHost)
        {
            Coroutines.Start(CoCheckEveryoneSwallowed());
        }
    }

    // ---- Animation (every client) ----

    private const float SwallowSeconds = 0.45f;
    private static readonly Dictionary<byte, Vector3> ScaleBefore = [];

    /// <summary>
    ///     The victim is pulled into the Glutton spinning and shrinking, then the Glutton gulps. The whole player is
    ///     moved (the body sprites aren't under <c>cosmetics</c>): the victim's movement and sync are off from the
    ///     swallow on (<see cref="GluttonSwallowedModifier" />), so this is local on every client and fights nothing.
    /// </summary>
    public static IEnumerator CoSwallowAnimation(GluttonSwallowedModifier mod)
    {
        var victim = mod.Player;
        var glutton = mod.Glutton;
        if (victim == null || glutton == null)
        {
            mod.EnterStomach();
            yield break;
        }

        var tr = victim.transform;
        ScaleBefore[victim.PlayerId] = tr.localScale;
        var startScale = tr.localScale;
        var start = tr.position;

        for (var t = 0f; t < SwallowSeconds; t += Time.deltaTime)
        {
            if (victim == null || glutton == null || victim.HasDied() || mod.Removed)
            {
                break;
            }

            var k = t / SwallowSeconds;
            var e = k * k;
            var end = glutton.transform.position;
            var pos = Vector3.Lerp(start, end, e);
            tr.position = new Vector3(pos.x, pos.y, start.z);
            victim.MyPhysics.body.position = pos;
            tr.localScale = startScale * (1f - 0.9f * e);
            tr.localEulerAngles = new Vector3(0f, 0f, 540f * e);
            yield return null;
        }

        if (victim != null)
        {
            RestoreShape(victim);
        }

        mod.EnterStomach();

        if (glutton != null)
        {
            yield return CoGulp(glutton.transform);
        }
    }

    // Squash and stretch of the Glutton (a gulp).
    private static IEnumerator CoGulp(Transform cos)
    {
        var baseScale = cos.localScale;
        Vector3[] steps = [new(1.18f, 0.84f, 1f), new(0.9f, 1.12f, 1f), new(1.05f, 0.96f, 1f), Vector3.one];
        var from = Vector3.one;
        foreach (var to in steps)
        {
            for (var t = 0f; t < 0.1f; t += Time.deltaTime)
            {
                if (cos == null)
                {
                    yield break;
                }

                cos.localScale = Vector3.Scale(baseScale, Vector3.Lerp(from, to, t / 0.1f));
                yield return null;
            }

            from = to;
        }

        if (cos != null)
        {
            cos.localScale = baseScale;
        }
    }

    // The Glutton glows like the scanner while swallowing (as the Pelican of Project: Lotus does), on their own screen only.
    private static IEnumerator CoScannerGlow(PlayerControl glutton)
    {
        glutton.SetScanner(true, ++glutton.scannerCount);
        yield return new WaitForSeconds(0.8f);
        if (glutton != null)
        {
            glutton.SetScanner(false, ++glutton.scannerCount);
        }
    }

    /// <summary>Size and rotation from before the swallow animation (if it ran).</summary>
    public static void RestoreShape(PlayerControl player)
    {
        if (!ScaleBefore.Remove(player.PlayerId, out var scale))
        {
            return;
        }

        player.transform.localScale = scale;
        player.transform.localEulerAngles = Vector3.zero;
    }

    // ---- Digestion and release (host decides) ----

    /// <summary>Host, right before it opens a meeting (<see cref="AreMeetingHold" />): everyone swallowed dies now.</summary>
    public static void DigestBeforeMeeting()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var mod in ModifierUtils.GetActiveModifiers<GluttonSwallowedModifier>().ToList())
        {
            Digest(mod, "the meeting was called");
        }
    }

    /// <summary>Host, every frame per swallowed player: the digestion time.</summary>
    public static void CheckDigestTimer(GluttonSwallowedModifier mod)
    {
        if (!AmongUsClient.Instance.AmHost || mod.DigestSent || MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var time = OptionGroupSingleton<GluttonOptions>.Instance.DigestTime;
        if (time > 0f && mod.SwallowedFor >= time)
        {
            Digest(mod, "the digestion time is over");
        }
    }

    private static void Digest(GluttonSwallowedModifier mod, string why)
    {
        var target = mod.Player;
        if (mod.DigestSent || target == null || target.HasDied())
        {
            return;
        }

        mod.DigestSent = true;
        var glutton = mod.Glutton;
        var source = glutton != null && !glutton.HasDied() ? glutton : target;
        Message($"Glutton: {target.Data.PlayerName} digested ({why})");
        source.RpcSpecialMurder(target, isIndirect: true, ignoreShield: true, didSucceed: true, resetKillTimer: false,
            createDeadBody: false, teleportMurderer: false, showKillAnim: false, playKillSound: false,
            causeOfDeath: "Glutton");
    }

    // Everyone alive is swallowed (or a Glutton): digest them now, nothing else can happen until a meeting.
    private static IEnumerator CoCheckEveryoneSwallowed()
    {
        yield return null;
        CheckEveryoneSwallowed();
    }

    public static void CheckEveryoneSwallowed()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || !ShipStatus.Instance ||
            MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var alive = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.Data != null && !x.Data.Disconnected && !x.HasDied()).ToList();
        if (alive.Count == 0 || !alive.Any(IsSwallowed) ||
            !alive.All(x => IsSwallowed(x) || (x.Data.Role is GluttonRole && !IsSwallowed(x))))
        {
            return;
        }

        foreach (var mod in ModifierUtils.GetActiveModifiers<GluttonSwallowedModifier>().ToList())
        {
            Digest(mod, "everyone left alive is swallowed");
        }
    }

    /// <summary>Every client: the Glutton died or left. Its swallowed come out (or the host kills them, option).</summary>
    public static void OnGluttonGone(byte gluttonId)
    {
        var mods = ModifierUtils.GetActiveModifiers<GluttonSwallowedModifier>(x => x.GluttonId == gluttonId).ToList();
        if (mods.Count == 0)
        {
            return;
        }

        if (OptionGroupSingleton<GluttonOptions>.Instance.ReleaseOnDeath)
        {
            foreach (var mod in mods)
            {
                Message($"Glutton: {mod.Player.Data.PlayerName} came out of the Glutton");
                mod.Player.RemoveModifier(mod);
                if (mod.Player.AmOwner)
                {
                    var notif = Helpers.CreateAndShowNotification(
                        $"<b>{TownOfUsColors.Glutton.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.GluttonReleased")}</color></b>",
                        Color.white, new Vector3(0f, 1f, -20f), spr: AreAssets.GluttonIcon.LoadAsset());
                    notif.AdjustNotification();
                }
            }

            return;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            foreach (var mod in mods)
            {
                Digest(mod, "the Glutton is gone");
            }
        }
    }
}
