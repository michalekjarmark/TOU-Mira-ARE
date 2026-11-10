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

        var go = Object.Instantiate(_prefab!, Camera.main.transform, false);
        go.name = "AreFreeplaySettingsMenu";
        go.transform.localPosition = new Vector3(_position.x, _position.y, MenuZ);
        go.SetActive(true);
        _openMenu = go.GetComponent<GameSettingMenu>();
        Info($"ARE Freeplay settings: opened (prefab {_prefab!.name}, lobby position {_position}).");
        Coroutines.Start(CoRestoreWhenClosed(go));
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
        if (_openMenu == null || __instance != _openMenu)
        {
            return true;
        }

        try
        {
            ControllerManager.Instance?.CloseOverlayMenu(__instance.name);
        }
        catch (Exception e)
        {
            Warning($"ARE Freeplay settings: CloseOverlayMenu failed: {e.Message}");
        }

        Object.Destroy(__instance.gameObject);
        _openMenu = null;
        return false;
    }

    private static IEnumerator CoRestoreWhenClosed(GameObject menu)
    {
        while (menu != null)
        {
            yield return null;
        }

        _openMenu = null;
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
