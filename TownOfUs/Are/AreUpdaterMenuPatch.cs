using System.Collections;
using System.Globalization;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUs.Patches.AprilFools;
using Twitch;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Shows the progress / result of <see cref="AreUpdater"/> in a popup on the main menu.
///     Nothing is shown when the game is up to date or the check itself failed (e.g. no internet).
/// </summary>
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class AreUpdaterMenuPatch
{
    // Below the Legacy / Discord / Source Code buttons TOU adds to the main menu (AprilFoolsPatches).
    private static readonly Vector2 ButtonAnchor = new(0.815f, 0.52f);

    private static bool _running;
    private static GenericPopup? _popup;

    public static void Postfix(MainMenuManager __instance)
    {
        if (TownOfUsPlugin.IsMobile || AreUpdater.State is AreUpdateState.Disabled or AreUpdateState.UpToDate)
        {
            return;
        }

        Coroutines.Start(CoShowUpdateButton(__instance));
        if (!_running)
        {
            Coroutines.Start(CoShowUpdatePopup());
        }
    }

    /// <summary>
    ///     Main menu button that reopens the popup (it can be closed during the download). Shown once an update was
    ///     found, until the game is restarted.
    /// </summary>
    private static IEnumerator CoShowUpdateButton(MainMenuManager menu)
    {
        while (AreUpdater.State is AreUpdateState.Idle or AreUpdateState.Checking)
        {
            yield return null;
        }

        if (!AreUpdater.UpdateFound || menu == null || menu.newsButton == null)
        {
            yield break;
        }

        PassiveButton button;
        try
        {
            button = menu.newsButton.CloneMenuItem("AreUpdateButton", ButtonAnchor, AreAssets.AreUpdateIcon.LoadAsset(),
                "AreUpdaterButton", "Update");
        }
        catch (Exception e)
        {
            Warning($"ARE Updater: could not create the main menu button: {e.Message}");
            yield break;
        }

        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        button.OnClick.AddListener((Action)(() => ShowOrUpdate(GetText())));
    }

    private static IEnumerator CoShowUpdatePopup()
    {
        _running = true;
        try
        {
            while (AreUpdater.State is AreUpdateState.Idle or AreUpdateState.Checking)
            {
                yield return null;
            }

            if (!AreUpdater.UpdateFound)
            {
                yield break;
            }

            ShowOrUpdate(GetText());
            while (AreUpdater.State == AreUpdateState.Downloading)
            {
                if (_popup != null && _popup.gameObject.activeSelf)
                {
                    _popup.TextAreaTMP.text = GetText();
                }

                yield return new WaitForSeconds(0.25f);
            }

            // Re-open the popup in case it was closed during the download, so the result is always seen.
            ShowOrUpdate(GetText());
        }
        finally
        {
            _running = false;
        }
    }

    private static void ShowOrUpdate(string text)
    {
        if (_popup == null)
        {
            if (TwitchManager.Instance == null || TwitchManager.Instance.TwitchPopup == null)
            {
                Warning($"ARE Updater: no popup available, message: {text}");
                return;
            }

            _popup = Object.Instantiate(TwitchManager.Instance.TwitchPopup);
            _popup.TextAreaTMP.fontSize *= 0.7f;
            _popup.TextAreaTMP.enableAutoSizing = true;
        }

        if (_popup.gameObject.activeSelf)
        {
            _popup.TextAreaTMP.text = text;
        }
        else
        {
            _popup.Show(text);
        }
    }

    private static string GetText()
    {
        var version = AreUpdater.RemoteVersion ?? "?";
        return AreUpdater.State switch
        {
            AreUpdateState.Updated => MiraLocaleManager.Get("AreUpdaterUpdated",
                    "Updated to version <version>!\nRestart the game to play the new version.")
                .Replace("<version>", version),
            AreUpdateState.Failed => MiraLocaleManager.Get("AreUpdaterFailed",
                    "Automatic update to <version> failed:\n<error>\n\nDownload it manually: <url>")
                .Replace("<version>", version)
                .Replace("<error>", $"<noparse>{AreUpdater.LastError}</noparse>")
                .Replace("<url>", AreInfo.ReleasesUrl),
            _ => MiraLocaleManager.Get("AreUpdaterDownloading",
                    "Downloading update <version>... <progress>%\nPlease wait, do not close the game.")
                .Replace("<version>", version)
                .Replace("<progress>", Math.Round(AreUpdater.Progress * 100f).ToString(CultureInfo.InvariantCulture)),
        };
    }
}
