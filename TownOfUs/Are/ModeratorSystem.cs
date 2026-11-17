using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: the Moderator's bans (<see cref="ModeratorBanModifier" /> on every client). An ability ban is Town of Us'
///     "disabled" state; the other types are blocked here, for the local player only (every client blocks its own
///     player): consoles and vents through PlayerCanUseEvent (<c>ModeratorEvents</c>) and the click patches below,
///     reports through the report button and the body click, the portable cams/admin/vitals and the Button Barry
///     through their buttons' <c>CanUse</c>. A vote ban takes the vote out in ProcessVotesEvent.
/// </summary>
[HarmonyPatch]
public static class ModeratorSystem
{
    public const string Symbol = "Ø";

    public static bool IsBanned(PlayerControl? player, ModeratorBanType type)
    {
        return player != null && player.HasModifier<ModeratorBanModifier>(x => x.BanType == type);
    }

    private static bool LocalBanned(ModeratorBanType type)
    {
        return IsBanned(PlayerControl.LocalPlayer, type);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleBan(PlayerControl moderator, PlayerControl target, int banType, float duration)
    {
        if (target == null || target.Data == null || target.Data.IsDead)
        {
            return;
        }

        var type = (ModeratorBanType)banType;

        // The same type again starts over (the timer, not remove + add: MiraAPI removes at the end of the frame).
        // Different types stack (the modifier isn't unique).
        if (target.TryGetModifier<ModeratorBanModifier>(out var old, x => x.BanType == type))
        {
            old.ResetTimer();
            old.StartTimer();
        }
        else
        {
            target.AddModifier<ModeratorBanModifier>(moderator.PlayerId, banType, duration);
        }
        Message($"Moderator: {moderator.Data.PlayerName} banned {target.Data.PlayerName} from {type} " +
                $"({(duration > 0f ? $"{duration} s" : "until the meeting")})");

        var banName = BanName(type);
        if (moderator.AmOwner)
        {
            var text = MiraLocaleManager.Get("TownOfUsMira.Role.ModeratorBanGiven")
                .Replace("<player>", target.Data.PlayerName)
                .Replace("<ban>", banName);
            Notify(text);
        }

        if (target.AmOwner && OptionGroupSingleton<ModeratorOptions>.Instance.TargetNotified)
        {
            var key = "TownOfUsMira.Role.ModeratorBannedUntilMeeting";
            if (type == ModeratorBanType.Vote)
            {
                key = "TownOfUsMira.Role.ModeratorBannedVote";
            }
            else if (duration > 0f)
            {
                key = "TownOfUsMira.Role.ModeratorBannedTimed";
            }

            var text = MiraLocaleManager.Get(key)
                .Replace("<ban>", banName)
                .Replace("<time>", Math.Round(duration).ToString(TownOfUsPlugin.Culture));
            Notify(text);
        }
    }

    public static string BanName(ModeratorBanType type)
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.ModeratorBanName{type}", type.ToString());
    }

    public static string BanShortName(ModeratorBanType type)
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.ModeratorBanShort{type}", type.ToString());
    }

    private static void Notify(string text)
    {
        var notif = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
            spr: AreAssets.ModeratorIcon.LoadAsset());
        notif.AdjustNotification();
    }

    /// <summary>"Ø" after a banned player's name, for the Moderator who banned them and for the player (if told).</summary>
    public static bool ShowsSymbol(PlayerControl player)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return false;
        }

        return player.HasModifier<ModeratorBanModifier>(x =>
            x.ModeratorId == local.PlayerId ||
            (player.AmOwner && OptionGroupSingleton<ModeratorOptions>.Instance.TargetNotified));
    }

    /// <summary>Line under the meeting timer for a player whose vote doesn't count.</summary>
    public static string MeetingStatusText()
    {
        return LocalBanned(ModeratorBanType.Vote) && OptionGroupSingleton<ModeratorOptions>.Instance.TargetNotified
            ? $"{TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.ModeratorVoteBanned")}</color>"
            : string.Empty;
    }

    /// <summary>
    ///     PlayerCanUseEvent and console clicks (local player): what a ban switches off. Sabotage repairs, doors and
    ///     ladders stay usable, and a player in a vent can always get out.
    /// </summary>
    public static bool BlocksUsable(IUsable usable)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || !local.HasModifier<ModeratorBanModifier>())
        {
            return false;
        }

        if (usable.TryCast<Vent>() != null)
        {
            return !local.inVent && LocalBanned(ModeratorBanType.Vents);
        }

        if (usable.TryCast<MapConsole>() != null)
        {
            return LocalBanned(ModeratorBanType.Equipment);
        }

        var systemConsole = usable.TryCast<SystemConsole>();
        if (systemConsole != null)
        {
            var prefab = systemConsole.MinigamePrefab;
            if (prefab == null)
            {
                return false;
            }

            if (prefab.TryCast<EmergencyMinigame>())
            {
                return LocalBanned(ModeratorBanType.MeetingButton);
            }

            return (prefab.TryCast<SurveillanceMinigame>() || prefab.TryCast<PlanetSurveillanceMinigame>() ||
                    prefab.TryCast<FungleSurveillanceMinigame>() || prefab.TryCast<VitalsMinigame>() ||
                    prefab.TryCast<SecurityLogGame>()) && LocalBanned(ModeratorBanType.Equipment);
        }

        var console = usable.TryCast<Console>();
        return console != null && RiggerSystem.IsTaskConsole(console) && LocalBanned(ModeratorBanType.Tasks);
    }

    // A mouse click on a console calls Use without asking CanUse (CAVEATS).
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    [HarmonyPrefix]
    public static bool ConsoleUsePrefix(Console __instance)
    {
        return !BlocksUsable(__instance.Cast<IUsable>());
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
    [HarmonyPrefix]
    public static bool SystemConsoleUsePrefix(SystemConsole __instance)
    {
        return !BlocksUsable(__instance.Cast<IUsable>());
    }

    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
    [HarmonyPrefix]
    public static bool MapConsoleUsePrefix(MapConsole __instance)
    {
        return !BlocksUsable(__instance.Cast<IUsable>());
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
    [HarmonyPrefix]
    public static bool VentUsePrefix(Vent __instance)
    {
        return !BlocksUsable(__instance.Cast<IUsable>());
    }

    // A click on a body is a report too (the report button itself is blocked by Town of Us through CanReport).
    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    [HarmonyPrefix]
    public static bool DeadBodyOnClickPrefix()
    {
        return !LocalBanned(ModeratorBanType.Reports);
    }

    // The report button stays dark while reports are banned.
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.SetActive))]
    [HarmonyPrefix]
    public static void ReportButtonSetActivePrefix(ref bool isActive)
    {
        if (isActive && LocalBanned(ModeratorBanType.Reports))
        {
            isActive = false;
        }
    }

    /// <summary>
    ///     The Button Barry's and the Chairman's meeting buttons (meeting ban) and the portable cams/admin/vitals
    ///     (equipment ban).
    /// </summary>
    [HarmonyPatch]
    public static class GadgetButtonsPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BarryButton), nameof(BarryButton.CanUse));
            yield return AccessTools.Method(typeof(ChairmanMeetingButton), nameof(ChairmanMeetingButton.CanUse));
            yield return AccessTools.Method(typeof(SecurityButton), nameof(SecurityButton.CanUse));
            yield return AccessTools.Method(typeof(ScientistButton), nameof(ScientistButton.CanUse));
            yield return AccessTools.Method(typeof(SpyAdminTableModifierButton), nameof(SpyAdminTableModifierButton.CanUse));
            yield return AccessTools.Method(typeof(SpyAdminTableRoleButton), nameof(SpyAdminTableRoleButton.CanUse));
            yield return AccessTools.Method(typeof(SentryPortableCameraButtonBase), nameof(SentryPortableCameraButtonBase.CanUse));
        }

        public static void Postfix(object __instance, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            var type = __instance is BarryButton or ChairmanMeetingButton
                ? ModeratorBanType.MeetingButton
                : ModeratorBanType.Equipment;
            if (LocalBanned(type))
            {
                __result = false;
            }
        }
    }
}
