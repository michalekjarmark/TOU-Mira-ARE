using HarmonyLib;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE (owner's request, 2026-11-14): in the lobby settings, a modifier's amount and chance rows show the
///     modifier's icon, like the role rows in the Roles tab, so modifiers are easier to find. The icon is the one
///     <see cref="AmountChanceOption" /> already carries for its change notification. The icon goes into the left end
///     of the row's label (row layout of the game's <c>GameOption_Number</c> prefab: label background from x -2.69 to
///     0.45, title text from -2.38 to 0.27) and the title moves right by the icon's width.
///     <para>
///         The icon is added when the row is initialized, after MiraAPI has tinted the row's sprites with the group
///         color (it would tint the icon too).
///     </para>
/// </summary>
[HarmonyPatch]
public static class AreModifierOptionIcons
{
    private const float IconSize = 0.55f;
    private const float IconX = -2.38f;
    private const float IconY = -0.062f;
    private const float TitleShift = 0.33f;

    private static readonly Dictionary<IntPtr, Sprite> Pending = [];

    [HarmonyPatch(typeof(ModdedNumberOption), nameof(ModdedNumberOption.CreateOption))]
    [HarmonyPostfix]
    public static void CreateOptionPostfix(ModdedNumberOption __instance, OptionBehaviour __result)
    {
        if (__instance is not AmountChanceOption { ResourceAsset: { } asset } || __result == null)
        {
            return;
        }

        var sprite = asset.LoadAsset();
        if (sprite != null)
        {
            Pending[__result.Pointer] = sprite;
        }
    }

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
    [HarmonyPostfix]
    public static void InitializePostfix(NumberOption __instance)
    {
        if (Pending.Count == 0 || !Pending.Remove(__instance.Pointer, out var sprite) ||
            __instance.transform.Find("AreModifierIcon") != null)
        {
            return;
        }

        var icon = new GameObject("AreModifierIcon") { layer = __instance.gameObject.layer };
        icon.transform.SetParent(__instance.transform, false);
        var renderer = icon.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        var bounds = sprite.bounds.size;
        var scale = IconSize / Mathf.Max(bounds.x, bounds.y, 0.01f);
        icon.transform.localScale = new Vector3(scale, scale, 1f);
        icon.transform.localPosition = new Vector3(IconX, IconY, -3f);

        if (__instance.TitleText != null)
        {
            var title = __instance.TitleText.rectTransform;
            title.anchoredPosition += new Vector2(TitleShift / 2f, 0f);
            title.sizeDelta -= new Vector2(TitleShift, 0f);
        }
    }
}
