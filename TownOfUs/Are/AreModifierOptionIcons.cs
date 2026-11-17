using System.Reflection;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Modifiers;
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

    // Group header (CategoryHeaderMasked, scale 0.63): title text from x -1.61, left aligned, y -0.165.
    private const float HeaderIconSize = 0.5f;
    private const float HeaderIconX = -1.37f;
    private const float HeaderIconY = -0.165f;
    private const float HeaderTitleShift = 0.5f;

    private static readonly Dictionary<IntPtr, Sprite> Pending = [];
    private static readonly HashSet<IntPtr> DecoratedHeaders = [];

    /// <summary>
    ///     The header of a modifier's own settings group (e.g. "Rivals") gets the modifier's icon before its name
    ///     (owner's feedback). Runs before MiraAPI's per-frame layout of each group, once per header.
    /// </summary>
    [HarmonyPatch]
    public static class GroupHeaderIconPatch
    {
        private static readonly Dictionary<Type, Sprite?> ModifierIcons = [];

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("MiraAPI.Patches.Options.GameOptionsMenuPatch:UpdateGroup");
        }

        public static void Prefix(AbstractOptionGroup? group)
        {
            var type = group?.OptionableType;
            if (type == null || !typeof(BaseModifier).IsAssignableFrom(type))
            {
                return;
            }

            var header = AreSettingsSearch.GetHeader(group!);
            if (header == null || !header || DecoratedHeaders.Contains(header.Pointer))
            {
                return;
            }

            DecoratedHeaders.Add(header.Pointer);
            if (header.transform.Find("AreModifierIcon") != null || IconOf(type) is not { } sprite)
            {
                return;
            }

            var icon = new GameObject("AreModifierIcon") { layer = header.gameObject.layer };
            icon.transform.SetParent(header.transform, false);
            var renderer = icon.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            var bounds = sprite.bounds.size;
            var scale = HeaderIconSize / Mathf.Max(bounds.x, bounds.y, 0.01f);
            icon.transform.localScale = new Vector3(scale, scale, 1f);
            icon.transform.localPosition = new Vector3(HeaderIconX, HeaderIconY, -1.5f);

            if (header.Title != null)
            {
                header.Title.rectTransform.anchoredPosition += new Vector2(HeaderTitleShift, 0f);
            }
        }

        private static Sprite? IconOf(Type modifierType)
        {
            // A cached sprite can be destroyed meanwhile (seen after games in Freeplay): load it again.
            if (!ModifierIcons.TryGetValue(modifierType, out var sprite) || (sprite is not null && !sprite))
            {
                var asset = MiscUtils.AllModifiers?.FirstOrDefault(x => x.GetType() == modifierType)?.ModifierIcon;
                sprite = asset?.LoadAsset();
                ModifierIcons[modifierType] = sprite;
            }

            return sprite;
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnDisable))]
    [HarmonyPostfix]
    public static void GameSettingMenuOnDisablePostfix()
    {
        DecoratedHeaders.Clear();
    }


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
