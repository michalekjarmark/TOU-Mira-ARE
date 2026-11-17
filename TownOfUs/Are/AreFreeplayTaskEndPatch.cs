using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Modules;
using TownOfUs.Options;

namespace TownOfUs.Are;

/// <summary>
///     ARE: Town of Us' Multiplayer Freeplay (and the dev "no game end" option) stops the game end checks in
///     <c>LogicGameFlowNormal.CheckEndCriteria</c>, but calling a meeting (emergency button, report, Button Barry, the
///     Chairman's "Gathering") runs the game's own task check first, which ended a practice game by tasks once every
///     task was done. Found while testing the Chairman, whose orders finish his tasks.
/// </summary>
[HarmonyPatch]
public static class AreFreeplayTaskEndPatch
{
    private static bool GameEndBlocked =>
        MultiplayerFreeplayMode.Enabled ||
        (TownOfUsPlugin.IsDevBuild && OptionGroupSingleton<HostSpecificOptions>.Instance.NoGameEnd);

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckEndGameViaTasks))]
    [HarmonyPrefix]
    public static bool NoTaskEndPrefix(ref bool __result)
    {
        if (!GameEndBlocked)
        {
            return true;
        }

        __result = false;
        return false;
    }
}
