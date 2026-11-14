using System.Runtime.CompilerServices;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     ARE: the Possum's fake death. While the Possum plays dead (<see cref="PossumFakeDeathModifier" /> on every
///     client) they are invisible, can't move or use anything, and every client shows a normal dead body with their
///     colors where they lay down (bodies are local objects in Among Us, so nothing more has to be synced). A click on
///     that body (the report button) sends <see cref="PossumRole.RpcPossumReported" /> instead of a report: the
///     Possum gets up and the reporter gets <see cref="PossumStunModifier" />. Other body abilities (Janitor,
///     Undertaker, Amnesiac, Chef, Altruist...) don't see the fake body.
/// </summary>
[HarmonyPatch]
public static class PossumSystem
{
    private static readonly Dictionary<byte, DeadBody> Bodies = [];

    public static bool IsFakeBody(DeadBody body)
    {
        return body != null && Bodies.TryGetValue(body.ParentId, out var fake) && fake == body;
    }

    public static bool IsPlayingDead(PlayerControl player)
    {
        return player != null && player.HasModifier<PossumFakeDeathModifier>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandlePlayDead(PlayerControl possum)
    {
        if (possum == null || possum.Data == null || possum.Data.IsDead || possum.Data.Role is not PossumRole ||
            IsPlayingDead(possum))
        {
            return;
        }

        possum.AddModifier<PossumFakeDeathModifier>();
        Message($"Possum: {possum.Data.PlayerName} plays dead");
    }

    /// <summary>
    ///     The Possum gets up: by the button, at the end of the time limit, or because <paramref name="reporter" />
    ///     tried to report the body (then the reporter is stunned).
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleStandUp(PlayerControl possum, PlayerControl? reporter)
    {
        if (possum == null || !IsPlayingDead(possum))
        {
            return;
        }

        possum.RemoveModifier<PossumFakeDeathModifier>();
        if (reporter == null || reporter.Data == null || reporter.Data.IsDead || reporter == possum)
        {
            Message($"Possum: {possum.Data.PlayerName} got up");
            return;
        }

        var stun = OptionGroupSingleton<PossumOptions>.Instance.StunDuration;
        reporter.AddModifier<PossumStunModifier>();
        Message($"Possum: {reporter.Data.PlayerName} tried to report {possum.Data.PlayerName}, stunned for {stun} s");

        if (possum.AmOwner)
        {
            // The report ended the effect early: start the cooldown like a manual stand-up.
            var button = CustomButtonSingleton<PossumPlayDeadButton>.Instance;
            button.EffectActive = false;
            button.Timer = button.Cooldown;
        }

        if (!reporter.AmOwner && !possum.AmOwner)
        {
            return;
        }

        var key = reporter.AmOwner ? "TownOfUsMira.Role.PossumStunnedNotif" : "TownOfUsMira.Role.PossumCaughtNotif";
        var text = MiraLocaleManager.Get(key)
            .Replace("<player>", reporter.AmOwner ? possum.Data.PlayerName : reporter.Data.PlayerName)
            .Replace("<time>", Math.Round(stun, 1).ToString(TownOfUsPlugin.Culture));
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: AreAssets.PossumIcon.LoadAsset());
        notif.AdjustNotification();
    }

    /// <summary>
    ///     Lays the Possum's body where they stand, built like the one of a real kill (MiraAPI's custom murder).
    /// </summary>
    public static void SpawnBody(PlayerControl possum)
    {
        RemoveBody(possum);
        if (!GameManager.Instance || possum.Data == null)
        {
            return;
        }

        var body = Object.Instantiate(GameManager.Instance.GetDeadBody(possum.Data.Role));
        body.enabled = false;
        body.ParentId = possum.PlayerId;
        foreach (var rend in body.bodyRenderers)
        {
            possum.SetPlayerMaterialColors(rend);
        }

        possum.SetPlayerMaterialColors(body.bloodSplatter);
        var offset = possum.KillAnimations is { Length: > 0 } anims ? anims[0].BodyOffset : Vector3.zero;
        var position = possum.transform.position + offset;
        position.z = position.y / 1000f;
        body.transform.position = position;
        body.enabled = true;
        Bodies[possum.PlayerId] = body;
    }

    public static void RemoveBody(PlayerControl possum)
    {
        if (!Bodies.Remove(possum.PlayerId, out var body))
        {
            return;
        }

        if (body != null)
        {
            body.gameObject.Destroy();
        }
    }

    /// <summary>
    ///     New game: forget bodies of the last one (they were destroyed with the map).
    /// </summary>
    public static void Reset()
    {
        Bodies.Clear();
    }

    // A click on the fake body is the report: get the Possum up instead.
    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    [HarmonyPrefix]
    public static bool DeadBodyOnClickPrefix(DeadBody __instance)
    {
        if (!IsFakeBody(__instance))
        {
            return true;
        }

        var local = PlayerControl.LocalPlayer;
        var possum = MiscUtils.PlayerById(__instance.ParentId);
        if (__instance.Reported || local == null || local.Data == null || local.Data.IsDead || possum == null ||
            local == possum || MeetingHud.Instance || !local.CanMove)
        {
            return false;
        }

        // The same reach as a vanilla report.
        var from = local.GetTruePosition();
        var to = __instance.TruePosition;
        if (Vector2.Distance(from, to) > local.MaxReportDistance ||
            PhysicsHelpers.AnythingBetween(from, to, Constants.ShipAndObjectsMask, false))
        {
            return false;
        }

        __instance.Reported = true;
        PossumRole.RpcPossumReported(local, possum);
        return false;
    }

    // Body abilities (Janitor, Undertaker, Amnesiac, Chef, Altruist, Morphling sample...) ignore the fake body.
    [HarmonyPatch(typeof(MiraAPI.Utilities.Extensions), nameof(MiraAPI.Utilities.Extensions.GetNearestDeadBody))]
    [HarmonyPostfix]
    public static void GetNearestDeadBodyPostfix(ref DeadBody? __result)
    {
        if (__result != null && IsFakeBody(__result))
        {
            __result = null;
        }
    }

    // The Possum lying down and the stunned reporter can't report, so the report button stays dark, also next to the
    // Possum's own fake body (owner's feedback: it lit up, and clicking it did nothing).
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.SetActive))]
    [HarmonyPrefix]
    public static void ReportButtonSetActivePrefix(ref bool isActive)
    {
        var local = PlayerControl.LocalPlayer;
        if (isActive && local != null &&
            (local.HasModifier<PossumFakeDeathModifier>() || local.HasModifier<PossumStunModifier>()))
        {
            isActive = false;
        }
    }

    // The Possum lying down and the stunned reporter can't move. Last, after the other movement patches.
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void FreezePostfix(PlayerPhysics __instance)
    {
        var player = __instance.myPlayer;
        if (__instance.body == null || player == null || !player.AmOwner)
        {
            return;
        }

        if (player.HasModifier<PossumFakeDeathModifier>() || player.HasModifier<PossumStunModifier>())
        {
            __instance.body.velocity = Vector2.zero;
        }
    }
}
