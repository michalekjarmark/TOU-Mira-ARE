using HarmonyLib;
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
            _ => string.Empty
        };

        if (line.Length > 0)
        {
            __instance.TimerText.text += $"<color=#FFFFFF>\n{line}</color>";
        }
    }
}
