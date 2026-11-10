using System.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     ARE: opens the lobby settings menu (<see cref="GameSettingMenu"/>) in Freeplay, so options can be tweaked
///     while practicing instead of going back to a lobby. Used by <see cref="Buttons.BaseFreeplay.FreeplaySettingsButton"/>.
///     The vanilla menu is created by <see cref="GameStartManager.ClickEdit"/> from the
///     <see cref="GameStartManager.PlayerOptionsMenu"/> prefab. There is no lobby in Freeplay, so we look the prefab up
///     (the Tutorial scene is built from the online game scene) or remember it from the last lobby, open it ourselves
///     and close it ourselves, skipping the vanilla close (it expects the lobby UI).
///     Mod options are saved to the config as soon as they change (we are the host in Freeplay).
/// </summary>
[HarmonyPatch]
public static class AreFreeplaySettings
{
    private const float MenuZ = -50f; // in front of the HUD, like minigames (camera local position)

    private static GameObject? _prefab;
    private static Vector3 _position;
    private static GameSettingMenu? _openMenu;

    public static bool IsOpen => _openMenu != null;

    /// <summary>
    ///     Opens the settings menu in Freeplay. Does nothing if a settings menu is already open.
    /// </summary>
    public static void Open()
    {
        if (GameSettingMenu.Instance != null || _openMenu != null || HudManager.Instance == null || Camera.main == null)
        {
            return;
        }

        if (!TryFindPrefab())
        {
            Warning("ARE Freeplay settings: GameSettingMenu prefab not found.");
            HudManager.Instance.ShowPopUp(MiraLocaleManager.Get("AreFreeplaySettingsUnavailable",
                "The settings menu is not available here. Open a local lobby once and try again."));
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (player != null)
        {
            player.NetTransform.Halt();
            player.moveable = false;
        }

        // Keep the clone's name: OnEnable registers the menu in ControllerManager under it, and the overlay has to be
        // closed under the same name (a rename left a stale "open menu" that blocked all input).
        var go = Object.Instantiate(_prefab!, Camera.main.transform, false);
        go.transform.localPosition = new Vector3(_position.x, _position.y, MenuZ);
        go.SetActive(true);
        _openMenu = go.GetComponent<GameSettingMenu>() ?? go.GetComponentInChildren<GameSettingMenu>(true);
        if (_openMenu == null)
        {
            Warning("ARE Freeplay settings: the prefab has no GameSettingMenu, closing.");
            Object.Destroy(go);
            RestorePlayer();
            return;
        }

        Info($"ARE Freeplay settings: opened {go.name} (lobby position {_position}).");
        Coroutines.Start(CoRestoreWhenClosed(go, _openMenu.name));
    }

    /// <summary>
    ///     Vanilla Update closes the menu right away outside of a lobby (seen in Player.log: Update ->
    ///     CloseOverlayMenu + destroy, bypassing the Close patch). Our menu needs none of it: tabs and the close button
    ///     work through their own button events, Esc through the ControllerManager back button.
    /// </summary>
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Update))]
    [HarmonyPrefix]
    public static bool GameSettingMenuUpdatePrefix(GameSettingMenu __instance)
    {
        return !IsOurs(__instance);
    }

    /// <summary>
    ///     Remember the prefab whenever a lobby is seen, so Freeplay can use it later in this session.
    /// </summary>
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPostfix]
    public static void GameStartManagerStartPostfix(GameStartManager __instance)
    {
        Remember(__instance);
    }

    /// <summary>
    ///     Our menu: close it ourselves (the vanilla close updates the lobby UI). Other menus: vanilla.
    /// </summary>
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Close))]
    [HarmonyPrefix]
    public static bool GameSettingMenuClosePrefix(GameSettingMenu __instance)
    {
        if (!IsOurs(__instance))
        {
            return true;
        }

        CloseOverlay(__instance.name);
        Object.Destroy(__instance.gameObject);
        return false;
    }

    private static bool IsOurs(GameSettingMenu menu)
    {
        return _openMenu != null && menu != null && menu.Pointer == _openMenu.Pointer;
    }

    private static IEnumerator CoRestoreWhenClosed(GameObject menu, string overlayName)
    {
        while (menu != null)
        {
            yield return null;
        }

        // However it was closed, never leave the overlay registered (it blocks the HUD and movement).
        CloseOverlay(overlayName);
        _openMenu = null;
        RestorePlayer();
        Info("ARE Freeplay settings: closed.");
    }

    private static void CloseOverlay(string overlayName)
    {
        try
        {
            ControllerManager.Instance?.CloseOverlayMenu(overlayName);
        }
        catch (Exception e)
        {
            Warning($"ARE Freeplay settings: CloseOverlayMenu failed: {e.Message}");
        }
    }

    private static void RestorePlayer()
    {
        if (PlayerControl.LocalPlayer != null)
        {
            PlayerControl.LocalPlayer.moveable = true;
        }
    }

    private static bool TryFindPrefab()
    {
        if (_prefab != null)
        {
            return true;
        }

        // 1. A GameStartManager (also inactive ones) that knows the prefab.
        foreach (var obj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<GameStartManager>()))
        {
            var manager = obj.TryCast<GameStartManager>();
            if (manager != null && manager.PlayerOptionsMenu != null)
            {
                Remember(manager);
                return true;
            }
        }

        // 2. The prefab itself, if it is loaded (a prefab is not part of any scene).
        foreach (var obj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<GameSettingMenu>()))
        {
            var menu = obj.TryCast<GameSettingMenu>();
            if (menu != null && !menu.gameObject.scene.IsValid())
            {
                _prefab = menu.gameObject;
                _prefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                _position = Vector3.zero;
                Info("ARE Freeplay settings: using a loaded GameSettingMenu prefab.");
                return true;
            }
        }

        return false;
    }

    private static void Remember(GameStartManager manager)
    {
        if (manager == null || manager.PlayerOptionsMenu == null)
        {
            return;
        }

        _prefab = manager.PlayerOptionsMenu;
        _position = manager.GameOptionsPosition;
        if (!_prefab.scene.IsValid())
        {
            // A prefab asset: keep it loaded after the lobby scene is unloaded.
            _prefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }
    }
}
