using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Are;

/// <summary>
///     Texts of the AmonRa Edition roles in the places where Town of Us shows the state of its own roles, so ours look
///     the same (round 5 feedback: no notifications for the state of a role).
/// </summary>
public static class AreRoleTexts
{
    /// <summary>
    ///     Symbols after a player's name, called from <c>PlayerRoleTextExtensions.UpdateTargetSymbols</c> (like the
    ///     Executioner's "X"): the player the local Gambler bet on in this meeting gets a "♦", the player whose voice the
    ///     local Ventriloquist has gets a "♪".
    /// </summary>
    public static string AddTargetSymbols(string name, PlayerControl player)
    {
        if (GamblerRole.IsLocalBetTarget(player))
        {
            name += $"{TownOfUsColors.Gambler.ToTextColor()} ♦</color>";
        }

        if (VentriloquistRole.IsLocalVoice(player))
        {
            name += $"{TownOfUsColors.Impostor.ToTextColor()} ♪</color>";
        }

        return name;
    }

    /// <summary>
    ///     Protection symbols, called from <c>PlayerRoleTextExtensions.UpdateProtectionSymbols</c> after the Warden's
    ///     "π": a sulking Comfy gets a "!", seen by the Comfy, by the dead with "The Dead Know" and by everyone with the
    ///     lobby option.
    /// </summary>
    public static string AddProtectionSymbols(string name, PlayerControl player, bool isDead)
    {
        if (player.HasModifier<ComfySulkModifier>() &&
            (player.AmOwner || isDead || OptionGroupSingleton<ComfyOptions>.Instance.SulkVisible))
        {
            name += $"{TownOfUsColors.Comfy.ToTextColor()} !</color>";
        }

        return name;
    }

    /// <summary>
    ///     Alliance symbols, called from <c>PlayerRoleTextExtensions.UpdateAllianceSymbols</c> right after the Lovers'
    ///     "♥" and with the same visibility: a Rival sees "≠" after their own name and their rival's, the dead see it
    ///     with "The Dead Know".
    /// </summary>
    public static string AddAllianceSymbols(string name, PlayerControl player, bool isDead)
    {
        if (player.HasModifier<RivalModifier>() && (PlayerControl.LocalPlayer.HasModifier<RivalModifier>() || isDead))
        {
            name += $"{TownOfUsColors.Rival.ToTextColor()} {RivalModifier.SymbolText}</color>";
        }

        return name;
    }
}

/// <summary>
///     Line under the meeting timer (bottom right of the meeting screen), where Town of Us shows e.g. the Prosecutor's
///     prosecutions or the Doomsayer's guesses (<c>MeetingHudTimerPatch</c>).
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateTimerText))]
public static class AreMeetingTimerTextPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.HasDied())
        {
            return;
        }

        var line = local.Data.Role switch
        {
            GamblerRole gambler => gambler.MeetingStatusText(__instance),
            VentriloquistRole ventriloquist => ventriloquist.MeetingStatusText(),
            ComfyRole => ComfySystem.MeetingStatusText(),
            _ => string.Empty
        };

        if (line.Length > 0)
        {
            __instance.TimerText.text += $"<color=#FFFFFF>\n{line}</color>";
        }
    }
}
