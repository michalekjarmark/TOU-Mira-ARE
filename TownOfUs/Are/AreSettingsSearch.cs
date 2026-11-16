using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Options;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     ARE (owner's request, 2026-11-14): a search box in the lobby settings menu, because the lists are long. It
///     filters the open tab of the mod's settings (Game, Roles, Modifiers and the custom tab): a group stays when its
///     name matches (with all its options), otherwise only the matching options do. In the Roles tab a role stays when
///     its name or one of its own settings matches. The text stays when switching tabs and is cleared when the menu
///     closes. The box takes the place of the tab description ("What is this?" box on the left); the vanilla Among Us
///     settings page and the presets tab keep the description.
///     <para>
///         MiraAPI lays out the Game and Modifiers tabs every frame (<c>GameOptionsMenuPatch.UpdateGroup</c>); a prefix
///         replaces that layout while a search is active. The Roles tab is laid out once when it is built, so
///         <see cref="RolesUpdatePostfix" /> moves the rows itself (with MiraAPI's spacing) and puts them back when the
///         search is cleared.
///     </para>
/// </summary>
[HarmonyPatch]
public static class AreSettingsSearch
{
    // MiraAPI's layout (GameOptionsMenuPatch.UpdateGroup, RoleSettingMenuPatches.CoQuotaTabPatch).
    private const float GroupHeaderX = -0.903f;
    private const float GroupHeaderStep = 0.58f;
    private const float OptionX = 0.952f;
    private const float OptionStep = 0.45f;
    private const float RoleTop = 0.522f;
    private const float RoleHeaderX = -0.44f;
    private const float RoleHeaderStep = 0.422f;
    private const float RoleRowX = -0.1f;
    private const float RoleRowStep = 0.43f;
    private const float RoleGroupGap = 0.4f;

    // The AmonRa toggle: icon size and distance of its centre from the box's right edge.
    private const float FilterSize = 0.42f;
    private const float FilterInset = 0.3f;

    private static readonly Color SelectedTextColor = new(0.45f, 0.75f, 1f);

    internal static readonly Func<AbstractOptionGroup, CategoryHeaderMasked?> GetHeader =
        AccessTools.MethodDelegate<Func<AbstractOptionGroup, CategoryHeaderMasked?>>(
            AccessTools.PropertyGetter(typeof(AbstractOptionGroup), "Header"));

    private static readonly Func<AbstractOptionGroup, bool> GetReady =
        AccessTools.MethodDelegate<Func<AbstractOptionGroup, bool>>(
            AccessTools.PropertyGetter(typeof(AbstractOptionGroup), "Ready"));

    // Normalized texts (they don't change while the menu is open) and the matches for the current query.
    private static readonly Dictionary<string, string> NormalizedTexts = [];
    private static readonly Dictionary<Type, bool> RoleSettingMatches = [];

    private static IntPtr _menu;
    private static GameObject? _root;
    private static TextBoxTMP? _box;
    private static GameObject? _icon;
    private static SpriteRenderer? _iconRenderer;
    private static BoxCollider2D? _clickArea;
    private static BoxCollider2D? _iconCollider;
    private static Color _textColor = Color.white;
    private static bool _keepFocus;
    private static bool _selectAll;
    private static string _selectedText = string.Empty;
    private static string _lastFrameText = string.Empty;
    private static string? _pendingText;
    private static TextMeshPro? _description;
    private static TextMeshPro? _infoText;
    private static SpriteRenderer? _infoImage;
    private static string[] _tokens = [];
    private static string _rawQuery = string.Empty;
    private static bool _searchTabOpen;
    private static bool _rolesMoved;

    // "AmonRa" toggle (owner's request, 2026-11-16): only the IAmonRaContent roles and modifiers in Roles and Modifiers.
    private static bool _amonRaOnly;
    private static GameObject? _filterButton;
    private static SpriteRenderer? _filterRenderer;
    private static bool _filterHover;
    private static HashSet<object>? _amonRaIcons;
    private static GameSettingMenu? _menuInstance;

    /// <summary>A search (text or the AmonRa toggle) filters the open tab right now.</summary>
    public static bool Active => _searchTabOpen && (_tokens.Length > 0 || AmonRaFilterActive);

    /// <summary>The AmonRa toggle is on and the open tab is one it filters.</summary>
    private static bool AmonRaFilterActive => _amonRaOnly && FilterTabOpen;

    private static bool FilterTabOpen =>
        MenuState.Instance != null && MenuState.Instance.CurrentMenu is MenuCategory.Roles or MenuCategory.Modifiers;

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Update))]
    [HarmonyPostfix]
    public static void GameSettingMenuUpdatePostfix(GameSettingMenu __instance)
    {
        if (__instance == null || MenuState.Instance == null)
        {
            return;
        }

        if (_menu != __instance.Pointer)
        {
            TryBuild(__instance);
        }

        if (_root == null || _box == null || _icon == null)
        {
            return; // no search box for this menu (see the warning in the log)
        }

        var state = MenuState.Instance;
        _searchTabOpen = state.CurrentModIdx != 0 && state.CurrentMenu != MenuCategory.Preset;
        _root.SetActive(_searchTabOpen);
        _icon.SetActive(_searchTabOpen);
        if (_description != null)
        {
            _description.gameObject.SetActive(!_searchTabOpen);
        }

        if (_infoText != null && _infoText != _description)
        {
            _infoText.gameObject.SetActive(!_searchTabOpen);
        }

        if (_infoImage != null)
        {
            _infoImage.enabled = !_searchTabOpen;
        }

        UpdateFilterButton();

        if (_searchTabOpen)
        {
            HandleKeyboard();
        }
        else
        {
            _keepFocus = false;
            SetSelectAll(false);
        }

        var raw = _box.text ?? string.Empty;
        if (raw == _rawQuery)
        {
            return;
        }

        _rawQuery = raw;
        _tokens = Normalize(raw).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        RoleSettingMatches.Clear();
        _iconRenderer!.sprite = raw.Length > 0
            ? MiraAssets.SearchClearIdleSprite.LoadAsset()
            : MiraAssets.SearchIconIdleSprite.LoadAsset();
        ScrollToTop(__instance);
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnDisable))]
    [HarmonyPostfix]
    public static void GameSettingMenuOnDisablePostfix()
    {
        if (_box != null && _box.text.Length > 0)
        {
            _box.Clear();
        }

        _rawQuery = string.Empty;
        _tokens = [];
        _keepFocus = false;
        _amonRaOnly = false;
        SetSelectAll(false);
        RoleSettingMatches.Clear();
        NormalizedTexts.Clear();
    }

    /// <summary>
    ///     Focus and keyboard shortcuts of the search box (the game's text box has neither).
    ///     <para>
    ///         Focus: when the list changes, the game takes the focus away from the box (rows and their buttons turn on
    ///         and off; MiraAPI's own search menus give it back the same way, <c>CustomPaginableMenu.CoRestoreFocus</c>).
    ///         The box keeps it until a click outside the box, Enter or Esc.
    ///     </para>
    ///     <para>
    ///         Shortcuts: Ctrl+A selects the whole text (no caret, so a selection is all or nothing; shown in blue, the
    ///         next key replaces it), Ctrl+C copies it, Ctrl+X cuts it, Ctrl+V pastes at the end, Ctrl+Backspace removes
    ///         the last word (owner's request). Control characters that Ctrl+key may type are removed.
    ///     </para>
    /// </summary>
    private static void HandleKeyboard()
    {
        var box = _box!;
        var text = box.text ?? string.Empty;
        var clean = CleanText(text);
        if (clean != text)
        {
            SetBoxText(clean);
            text = clean;
        }

        // Ctrl+V / Ctrl+Backspace of the last frame: the game's own text box may paste or remove a character too (order
        // of the Updates is unknown), so the result, computed from the text before the key, is set one frame later.
        if (_pendingText != null)
        {
            if (text != _pendingText)
            {
                SetBoxText(_pendingText);
                text = _pendingText;
            }

            _pendingText = null;
        }

        try
        {
            HandleKeys(box, text);
        }
        finally
        {
            _lastFrameText = box.text ?? string.Empty;
        }
    }

    private static void HandleKeys(TextBoxTMP box, string text)
    {
        if (Input.GetMouseButtonDown(0))
        {
            _keepFocus = PointerOverBox();
            SetSelectAll(false);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Escape))
        {
            _keepFocus = false;
            SetSelectAll(false);
        }

        if (_keepFocus && !box.hasFocus)
        {
            box.GiveFocus();
        }

        if (!box.hasFocus)
        {
            return;
        }

        // A key typed over the selection replaces it; Backspace / Delete remove it.
        if (_selectAll)
        {
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
            {
                SetBoxText(string.Empty);
                SetSelectAll(false);
                return;
            }

            if (text != _selectedText)
            {
                SetBoxText(text.StartsWith(_selectedText, StringComparison.Ordinal) && text.Length > _selectedText.Length
                    ? text[_selectedText.Length..]
                    : string.Empty);
                SetSelectAll(false);
                return;
            }
        }

        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SetSelectAll(text.Length > 0);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            GUIUtility.systemCopyBuffer = text;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            GUIUtility.systemCopyBuffer = text;
            SetBoxText(string.Empty);
            SetSelectAll(false);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            var pasted = CleanText((GUIUtility.systemCopyBuffer ?? string.Empty).Replace('\n', ' ').Replace('\r', ' '));
            var result = (_selectAll ? string.Empty : _lastFrameText) + pasted;
            _pendingText = result.Length > box.characterLimit ? result[..box.characterLimit] : result;
            SetSelectAll(false);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _pendingText = WithoutLastWord(_lastFrameText);
        }
    }

    /// <summary>Like Ctrl+Backspace in Windows at the end of a text: "abc def" → "abc ", "abc " → "".</summary>
    private static string WithoutLastWord(string text)
    {
        var end = text.Length;
        while (end > 0 && char.IsWhiteSpace(text[end - 1]))
        {
            end--;
        }

        while (end > 0 && !char.IsWhiteSpace(text[end - 1]))
        {
            end--;
        }

        return text[..end];
    }

    private static bool PointerOverBox()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return false;
        }

        Vector2 point = camera.ScreenToWorldPoint(Input.mousePosition);
        return (_clickArea != null && _clickArea.OverlapPoint(point)) ||
               (_iconCollider != null && _iconCollider.OverlapPoint(point));
    }

    private static void SetSelectAll(bool selected)
    {
        _selectAll = selected;
        _selectedText = selected && _box != null ? _box.text : string.Empty;
        if (_box != null && _box.outputText != null)
        {
            _box.outputText.color = selected ? SelectedTextColor : _textColor;
        }
    }

    private static void SetBoxText(string text)
    {
        if (_box == null)
        {
            return;
        }

        if (text.Length == 0)
        {
            _box.Clear();
        }
        else
        {
            _box.SetText(text, string.Empty);
        }
    }

    private static string CleanText(string text)
    {
        return text.Any(char.IsControl) ? new string(text.Where(c => !char.IsControl(c)).ToArray()) : text;
    }

    /// <summary>
    ///     Game, Modifiers and the custom tabs: MiraAPI's per-frame layout of one option group, filtered.
    /// </summary>
    [HarmonyPatch]
    public static class GroupLayoutPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("MiraAPI.Patches.Options.GameOptionsMenuPatch:UpdateGroup");
        }

        // A modifier's amount and chance rows (in the "Universal Modifiers" group etc.) carry the modifier's icon.
        private static bool IsAmonRaOption(IModdedOption option)
        {
            if (option is not AmountChanceOption { ResourceAsset: { } asset })
            {
                return false;
            }

            _amonRaIcons ??= (MiscUtils.AllModifiers ?? [])
                .Where(x => x is IAmonRaContent && x.ModifierIcon != null)
                .Select(x => (object)x.ModifierIcon!)
                .ToHashSet();
            return _amonRaIcons.Contains(asset);
        }

        public static bool Prefix(AbstractOptionGroup? group, ref float num)
        {
            if (!Active || group == null || !GetReady(group) || group.Children.Count == 0)
            {
                return true;
            }

            var header = GetHeader(group);
            if (header == null || !group.GroupVisible.Invoke())
            {
                return true;
            }

            var groupMatches = Matches(header.Title.text);
            var groupIsAmonRa = IsAmonRaType(group.OptionableType);
            var shown = new List<OptionBehaviour>();
            foreach (var option in group.Children)
            {
                var behaviour = option.OptionBehaviour;
                if (behaviour == null || !behaviour)
                {
                    continue;
                }

                if (option.Visible.Invoke() && (groupMatches || Matches(OptionTitle(option))) &&
                    (!AmonRaFilterActive || groupIsAmonRa || IsAmonRaOption(option)))
                {
                    shown.Add(behaviour);
                }
                else
                {
                    behaviour.gameObject.SetActive(false);
                }
            }

            // With the AmonRa toggle only an AmonRa group stays without options (e.g. closed by a click).
            if (shown.Count == 0 && (!groupMatches || (AmonRaFilterActive && !groupIsAmonRa)))
            {
                header.gameObject.SetActive(false);
                return false;
            }

            // A search shows the matches also in groups closed by a click on their header.
            header.gameObject.SetActive(true);
            header.transform.localScale = Vector3.one * 0.63f;
            header.transform.localPosition = new Vector3(GroupHeaderX, num, -2f);
            num -= GroupHeaderStep;
            foreach (var behaviour in shown)
            {
                behaviour.gameObject.SetActive(true);
                behaviour.transform.localPosition = new Vector3(OptionX, num, -2f);
                num -= OptionStep;
            }

            return false;
        }
    }

    /// <summary>
    ///     Roles tab: MiraAPI places the rows once, so the search moves them (and puts them back afterwards). MiraAPI
    ///     skips the vanilla Update with a prefix; a postfix still runs.
    /// </summary>
    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
    [HarmonyPostfix]
    public static void RolesUpdatePostfix(RolesSettingsMenu __instance)
    {
        if (MenuState.Instance == null || MenuState.Instance.CurrentModIdx == 0 || __instance.RoleChancesSettings == null ||
            !__instance.RoleChancesSettings.activeSelf || (!Active && !_rolesMoved))
        {
            return;
        }

        var parent = __instance.RoleChancesSettings.transform;
        var y = RoleTop;
        CategoryHeaderMasked? header = null;
        var rows = new List<RoleOptionSetting>();
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            var newHeader = child.GetComponent<CategoryHeaderMasked>();
            if (newHeader != null)
            {
                PlaceRoleGroup(header, rows, ref y);
                header = newHeader;
                rows.Clear();
                continue;
            }

            var row = child.GetComponent<RoleOptionSetting>();
            if (row != null)
            {
                rows.Add(row);
            }
        }

        PlaceRoleGroup(header, rows, ref y);
        __instance.scrollBar.SetYBoundsMax(-y - 2f);
        _rolesMoved = Active;
    }

    private static void PlaceRoleGroup(CategoryHeaderMasked? header, List<RoleOptionSetting> rows, ref float y)
    {
        if (header == null)
        {
            return;
        }

        var headerMatches = !Active || Matches(header.Title.text);
        var shown = rows.Where(row => (headerMatches || RoleMatches(row)) &&
                                      (!AmonRaFilterActive || IsAmonRaType(row.Role?.GetType()))).ToList();
        foreach (var row in rows.Except(shown))
        {
            row.gameObject.SetActive(false);
        }

        if (Active && shown.Count == 0 && (!headerMatches || AmonRaFilterActive))
        {
            header.gameObject.SetActive(false);
            return;
        }

        header.gameObject.SetActive(true);
        header.transform.localPosition = new Vector3(RoleHeaderX, y, -2f);
        y -= RoleHeaderStep;
        foreach (var row in shown)
        {
            row.gameObject.SetActive(true);
            row.transform.localPosition = new Vector3(RoleRowX, y, -2f);
            y -= RoleRowStep;
        }

        if (shown.Count > 0)
        {
            y -= RoleGroupGap;
        }
    }

    private static bool IsAmonRaType(Type? type)
    {
        return type != null && typeof(IAmonRaContent).IsAssignableFrom(type);
    }

    private static void ToggleAmonRa()
    {
        _amonRaOnly = !_amonRaOnly;
        UpdatePlaceholder();
        if (_menuInstance != null)
        {
            ScrollToTop(_menuInstance);
        }
    }

    private static void UpdatePlaceholder()
    {
        if (_box?.placeholderText != null)
        {
            _box.placeholderText.text = _amonRaOnly
                ? MiraLocaleManager.Get("AreSettingsSearchPlaceholderAmonRa", "AmonRa only. Search...")
                : MiraLocaleManager.Get("AreSettingsSearchPlaceholder", "Search this tab...");
        }
    }

    /// <summary>The toggle shows in the Roles and Modifiers tabs: bright when on, faded when off.</summary>
    private static void UpdateFilterButton()
    {
        if (_filterButton == null || _filterRenderer == null)
        {
            return;
        }

        var show = _searchTabOpen && FilterTabOpen;
        _filterButton.SetActive(show);
        if (!show)
        {
            return;
        }

        var alpha = _filterHover ? 0.75f : 0.35f;
        _filterRenderer.color = _amonRaOnly ? Color.white : new Color(0.75f, 0.75f, 0.75f, alpha);
    }

    private static bool RoleMatches(RoleOptionSetting row)
    {
        if (Matches(row.titleText.text))
        {
            return true;
        }

        var role = row.Role;
        if (role == null)
        {
            return false;
        }

        // The role's own settings (the cog page).
        var type = role.GetType();
        if (!RoleSettingMatches.TryGetValue(type, out var matches))
        {
            matches = MenuState.Instance.CurrentMod.OptionGroups
                .Where(group => group.OptionableType == type)
                .SelectMany(group => group.Children)
                .Any(option => Matches(OptionTitle(option)));
            RoleSettingMatches[type] = matches;
        }

        return matches;
    }

    private static string OptionTitle(IModdedOption option)
    {
        return TranslationController.Instance.GetString(option.StringName);
    }

    private static bool Matches(string text)
    {
        if (_tokens.Length == 0)
        {
            return true;
        }

        if (!NormalizedTexts.TryGetValue(text, out var normalized))
        {
            normalized = Normalize(text);
            NormalizedTexts[text] = normalized;
        }

        return _tokens.All(token => normalized.Contains(token, StringComparison.Ordinal));
    }

    /// <summary>Lower case, no rich text tags, no Polish (or other) diacritics, so "wygodnis" finds "Wygodniś".</summary>
    private static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var plain = Regex.Replace(text, "<.*?>", string.Empty).ToLowerInvariant().Replace('ł', 'l')
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(plain.Length);
        foreach (var c in plain)
        {
            if (!char.IsControl(c) && CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Trim();
    }

    private static void ScrollToTop(GameSettingMenu menu)
    {
        if (menu.GameSettingsTab != null && menu.GameSettingsTab.scrollBar != null)
        {
            menu.GameSettingsTab.scrollBar.ScrollToTop();
        }

        if (menu.RoleSettingsTab != null && menu.RoleSettingsTab.scrollBar != null)
        {
            menu.RoleSettingsTab.scrollBar.ScrollToTop();
        }
    }

    private static void Clear()
    {
        if (_box != null && _box.text.Length > 0)
        {
            _box.Clear();
        }
    }

    /// <summary>
    ///     The search box goes into the "What is this?" box: the text field where the description was, a magnifier
    ///     (a cross while there is text, click = clear) where the crewmate picture was.
    /// </summary>
    private static void TryBuild(GameSettingMenu menu)
    {
        _menu = menu.Pointer; // one try per menu
        _menuInstance = menu;
        _filterButton = null;
        _filterRenderer = null;
        _root = null;
        _box = null;
        _icon = null;
        _rawQuery = string.Empty;
        _tokens = [];
        var info = menu.transform.Find("What Is This?");
        var template = HudManager.InstanceExists ? HudManager.Instance.Chat?.freeChatField?.textArea : null;
        _description = menu.MenuDescriptionText;
        if (info == null || template == null || _description == null)
        {
            Warning("ARE settings search: the menu has no description box or there is no chat field, no search.");
            return;
        }

        _infoImage = info.Find("InfoImage")?.GetComponent<SpriteRenderer>();
        _infoText = info.Find("InfoText")?.GetComponent<TextMeshPro>();
        var frame = info.Find("Cube");
        var layer = info.gameObject.layer;
        var center = frame != null ? frame.localPosition : _description.transform.localPosition;
        var size = new Vector2(2.18f, 1.07f);
        if (frame != null && frame.GetComponent<SpriteRenderer>() is { } frameRenderer)
        {
            size = new Vector2(frameRenderer.size.x * frame.localScale.x, frameRenderer.size.y * frame.localScale.y);
        }

        _root = new GameObject("AreSettingsSearch") { layer = layer };
        _root.transform.SetParent(info, false);
        _root.transform.localPosition = new Vector3(center.x, center.y, _description.transform.localPosition.z - 0.5f);

        _box = Object.Instantiate(template, _root.transform);
        _box.gameObject.SetActive(true);
        _box.name = "AreSettingsSearchField";
        _box.gameObject.layer = layer;
        _box.transform.localPosition = Vector3.zero;
        _box.transform.localScale = Vector3.one;
        _box.OnEnter = new Button.ButtonClickedEvent();
        _box.OnChange = new Button.ButtonClickedEvent();
        _box.OnFocus = new Button.ButtonClickedEvent();
        _box.OnFocusLost = new Button.ButtonClickedEvent();
        _box.sendButtonGlyph = null;
        _box.SendOnFullChars = false;
        _box.ClearOnFocus = false;
        _box.characterLimit = 30;
        _box.AllowSymbols = true;
        _box.allowAllCharacters = true;
        if (_box.Background)
        {
            _box.Background.enabled = false;
        }

        if (_box.GetComponent<BoxCollider2D>() is { } boxCollider)
        {
            boxCollider.size = size;
            boxCollider.offset = Vector2.zero;
        }

        _textColor = _description.color;
        _keepFocus = false;
        _selectAll = false;
        StyleText(_box.outputText, _textColor, string.Empty);
        StyleText(_box.placeholderText, new Color(0.55f, 0.55f, 0.55f),
            MiraLocaleManager.Get("AreSettingsSearchPlaceholder", "Search this tab..."));
        if (_box.Pipe)
        {
            var pipePosition = _box.Pipe.transform.localPosition;
            _box.Pipe.transform.localPosition = new Vector3(pipePosition.x, _box.outputText.transform.localPosition.y,
                _box.outputText.transform.localPosition.z - 0.1f);
        }

        var clickArea = _clickArea = _root.AddComponent<BoxCollider2D>();
        clickArea.size = size;
        var click = _root.AddComponent<PassiveButton>();
        click.OnClick = new Button.ButtonClickedEvent();
        click.OnClick.AddListener((UnityAction)(() => _box?.GiveFocus()));
        click.OnMouseOver = new UnityEvent();
        click.OnMouseOut = new UnityEvent();
        click.ClickSound = menu.BackButton.GetComponent<PassiveButton>()?.ClickSound;

        _icon = new GameObject("AreSettingsSearchIcon") { layer = layer };
        _icon.transform.SetParent(info, false);
        var iconPosition = _infoImage != null ? _infoImage.transform.localPosition : center + new Vector3(-1.3f, 0f, 0f);
        _icon.transform.localPosition = new Vector3(iconPosition.x, center.y, _root.transform.localPosition.z);
        _iconRenderer = _icon.AddComponent<SpriteRenderer>();
        _iconRenderer.sprite = MiraAssets.SearchIconIdleSprite.LoadAsset();
        var bounds = _iconRenderer.sprite.bounds.size;
        var scale = 0.6f / Mathf.Max(bounds.x, bounds.y, 0.01f);
        _icon.transform.localScale = new Vector3(scale, scale, 1f);
        var iconCollider = _iconCollider = _icon.AddComponent<BoxCollider2D>();
        iconCollider.size = bounds;
        var iconButton = _icon.AddComponent<PassiveButton>();
        iconButton.OnClick = new Button.ButtonClickedEvent();
        iconButton.OnClick.AddListener((UnityAction)Clear);
        iconButton.OnMouseOver = new UnityEvent();
        iconButton.OnMouseOver.AddListener((UnityAction)(() => SetIconHover(true)));
        iconButton.OnMouseOut = new UnityEvent();
        iconButton.OnMouseOut.AddListener((UnityAction)(() => SetIconHover(false)));
        iconButton.ClickSound = click.ClickSound;

        // The AmonRa toggle sits inside the box, at its right end (the text is shortened for it, see StyleText).
        _filterButton = new GameObject("AreSettingsAmonRaFilter") { layer = layer };
        _filterButton.transform.SetParent(info, false);
        _filterButton.transform.localPosition = new Vector3(center.x + size.x / 2f - FilterInset, center.y,
            _root.transform.localPosition.z - 0.2f);
        _filterRenderer = _filterButton.AddComponent<SpriteRenderer>();
        _filterRenderer.sprite = AreAssets.AmonRaIcon.LoadAsset();
        var filterBounds = _filterRenderer.sprite.bounds.size;
        var filterScale = FilterSize / Mathf.Max(filterBounds.x, filterBounds.y, 0.01f);
        _filterButton.transform.localScale = new Vector3(filterScale, filterScale, 1f);
        _filterButton.AddComponent<BoxCollider2D>().size = filterBounds;
        var filterButton = _filterButton.AddComponent<PassiveButton>();
        filterButton.OnClick = new Button.ButtonClickedEvent();
        filterButton.OnClick.AddListener((UnityAction)ToggleAmonRa);
        filterButton.OnMouseOver = new UnityEvent();
        filterButton.OnMouseOver.AddListener((UnityAction)(() => _filterHover = true));
        filterButton.OnMouseOut = new UnityEvent();
        filterButton.OnMouseOut.AddListener((UnityAction)(() => _filterHover = false));
        filterButton.ClickSound = click.ClickSound;
        _filterHover = false;
        UpdatePlaceholder();

        Info($"ARE settings search: box at {center} size {size}, icon at {_icon.transform.localPosition}, " +
             $"AmonRa toggle at {_filterButton.transform.localPosition}.");
    }

    private static void StyleText(TextMeshPro? text, Color color, string value)
    {
        if (text == null || _description == null)
        {
            return;
        }

        text.GetComponent<TextTranslatorTMP>()?.Destroy();
        text.gameObject.layer = _root!.layer;
        text.font = _description.font;
        text.fontSharedMaterial = _description.fontSharedMaterial;
        text.enableAutoSizing = true;
        text.fontSizeMin = 1f;
        text.fontSizeMax = Mathf.Max(_description.fontSizeMax, 2f);
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.color = color;
        text.text = value;
        var rect = text.rectTransform;
        rect.pivot = new Vector2(0.5f, 0.5f);
        // Shorter than the box: the AmonRa toggle sits at its right end.
        rect.sizeDelta = new Vector2(1.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localPosition = new Vector3(-0.15f, 0f, -0.2f);
    }

    private static void SetIconHover(bool hover)
    {
        if (_iconRenderer == null)
        {
            return;
        }

        var hasText = _rawQuery.Length > 0;
        _iconRenderer.sprite = (hasText, hover) switch
        {
            (true, true) => MiraAssets.SearchClearHoverSprite.LoadAsset(),
            (true, false) => MiraAssets.SearchClearIdleSprite.LoadAsset(),
            (false, true) => MiraAssets.SearchIconHoverSprite.LoadAsset(),
            _ => MiraAssets.SearchIconIdleSprite.LoadAsset(),
        };
    }
}
