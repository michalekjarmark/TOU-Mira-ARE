using HarmonyLib;
using TownOfUs.Patches.Misc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     ARE main menu (owner's decision, 2026-11-14):
///     <list type="bullet">
///         <item>
///             our logo ("AmonRa Edition", <c>AreMenuLogo.png</c> from <c>tools/are-assets/make-menu-logo.ps1</c>)
///             instead of TOU's banner (<see cref="LogoPatch" /> sets it first; this postfix runs last);
///         </item>
///         <item>
///             no "Legacy" button (<c>AprilFoolsPatches</c> adds it in a prefix): it switched the banner and the
///             classic Town of Us art of the ability buttons. Legacy mode is turned off for everyone, also when a config
///             still has it on. The Discord and Source Code buttons move up into its place.
///         </item>
///     </list>
/// </summary>
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class AreMainMenuPatch
{
    // Anchors of the buttons AprilFoolsPatches adds (Legacy 0.775, Discord 0.69, Source Code 0.605).
    private const float AnchorX = 0.815f;
    private const float TopY = 0.775f;
    private const float StepY = 0.085f;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(MainMenuManager __instance)
    {
        if (TownOfUsPlugin.LegacyMode.Value != LegacyVisuals.Disabled)
        {
            Info($"ARE: legacy visuals ({TownOfUsPlugin.LegacyMode.Value}) turned off.");
            TownOfUsPlugin.LegacyMode.Value = LegacyVisuals.Disabled;
        }

        if (LogoPatch.GameLogo != null)
        {
            LogoPatch.GameLogo.sprite = AreAssets.AreMenuLogo.LoadAsset();
        }

        RemoveLegacyButton(__instance);
    }

    private static void RemoveLegacyButton(MainMenuManager menu)
    {
        var bounds = GameObject.Find("Main Buttons")?.transform.Find("BottomButtonBounds");
        var legacy = bounds?.Find("LegacyModeButton");
        if (bounds == null || legacy == null)
        {
            return;
        }

        var legacyButton = legacy.GetComponent<PassiveButton>();
        if (legacyButton != null && menu.mainButtons != null)
        {
            var buttons = new Il2CppSystem.Collections.Generic.List<PassiveButton>();
            foreach (var button in menu.mainButtons)
            {
                if (button != null && button.Pointer != legacyButton.Pointer)
                {
                    buttons.Add(button);
                }
            }

            menu.mainButtons = buttons;
            menu.SetUpControllerNav();
        }

        Object.Destroy(legacy.gameObject);
        MoveTo(bounds, "DiscordJoinButton", TopY);
        MoveTo(bounds, "GithubCodeButton", TopY - StepY);
    }

    private static void MoveTo(Transform bounds, string name, float y)
    {
        var positioner = bounds.Find(name)?.GetComponent<AspectPosition>();
        if (positioner == null)
        {
            return;
        }

        positioner.anchorPoint = new Vector2(AnchorX, y);
        positioner.AdjustPosition();
    }
}
