using System.Text.RegularExpressions;
using HarmonyLib;

namespace TownOfUs.Are;

/// <summary>
///     ARE: the AmonRa roles (<see cref="IAmonRaContent" />) keep their Polish names in every language (owner's
///     request, 2026-11-20; on by default, a local setting in Preferences: <c>AreMammonPolishNames</c>). In the texts of
///     those roles (name, descriptions, notices, options) every English name of an AmonRa role or Chairman title is
///     replaced by its Polish name, e.g. "The Chairman ..." becomes "The Prezes ...". The pairs come from the en_US and
///     pl_PL names, so a renamed role needs no change here; a new AmonRa role needs its IdPart below. Documents for
///     players (the English README and changelog) still use the English names.
/// </summary>
[HarmonyPatch]
public static class AreMammonNames
{
    private static readonly string[] RoleParts =
    [
        "Expert", "Chairman", "Handyman", "Comfy", "Cryptid", "Fabulist", "Moderator", "StreamSniper", "BlameShifter",
        "Informant", "Glutton"
    ];

    private static readonly string[] ModifierParts = ["AmonRa", "Streamer"];

    // Texts of other features that name an AmonRa role (the Donosiciel and Seer role exclusion).
    private static readonly string[] ExtraTextKeys = ["TouOptionExclusiveInformingRoles"];

    // Titles that are names too (the Chairman's promotions; the top one is the role's own name).
    private static readonly string[] ExtraNameKeys =
    [
        "TownOfUsMira.Role.ChairmanRankManager", "TownOfUsMira.Role.ChairmanRankDirector",
        "TownOfUsMira.Role.ChairmanRankPresident", "TownOfUsMira.Role.ChairmanRankKing"
    ];

    private static readonly Dictionary<string, bool> KeyIsAmonRa = [];
    private static readonly Dictionary<string, string> SwappedTexts = [];
    private static string[]? _prefixes;
    private static Dictionary<string, string>? _polishNames;
    private static Regex? _englishNames;
    private static bool _busy;

    private static bool Enabled
    {
        get
        {
            try
            {
                return LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance?.AreMammonPolishNames.Value ?? true;
            }
            catch
            {
                return true;
            }
        }
    }

    // Get(language, key, fallback) is the one every lookup ends in (the other overload calls it).
    [HarmonyPatch(typeof(MiraLocaleManager), nameof(MiraLocaleManager.Get), typeof(MiraLanguage), typeof(string),
        typeof(string))]
    [HarmonyPostfix]
    public static void GetPostfix(MiraLanguage language, string key, ref string __result)
    {
        if (_busy || language == MiraLanguage.Polish || string.IsNullOrEmpty(__result) || string.IsNullOrEmpty(key) ||
            !IsAmonRaKey(key) || !Enabled)
        {
            return;
        }

        if (SwappedTexts.TryGetValue(__result, out var swapped))
        {
            __result = swapped;
            return;
        }

        if (!TryBuildNames())
        {
            return;
        }

        swapped = _englishNames!.Replace(__result, match => _polishNames![match.Value]);
        SwappedTexts[__result] = swapped;
        __result = swapped;
    }

    private static bool IsAmonRaKey(string key)
    {
        if (KeyIsAmonRa.TryGetValue(key, out var known))
        {
            return known;
        }

        _prefixes ??= RoleParts.SelectMany(x => new[] { $"TownOfUsMira.Role.{x}", $"TouOption{x}" })
            .Concat(ModifierParts.SelectMany(x => new[] { $"TownOfUsMira.Modifier.{x}", $"TouOption{x}" }))
            .ToArray();

        var result = ExtraTextKeys.Contains(key) || _prefixes.Any(x => key.StartsWith(x, StringComparison.Ordinal));
        KeyIsAmonRa[key] = result;
        return result;
    }

    // English name -> Polish name of every AmonRa role (only those that differ), from the loaded locales.
    private static bool TryBuildNames()
    {
        if (_englishNames != null)
        {
            return true;
        }

        _busy = true;
        try
        {
            var names = new Dictionary<string, string>();
            var keys = RoleParts.Select(x => $"TownOfUsMira.Role.{x}")
                .Concat(ModifierParts.Select(x => $"TownOfUsMira.Modifier.{x}"))
                .Concat(ExtraNameKeys);
            foreach (var key in keys)
            {
                var english = MiraLocaleManager.Get(MiraLanguage.English, key, string.Empty);
                var polish = MiraLocaleManager.Get(MiraLanguage.Polish, key, string.Empty);
                if (english.Length == 0)
                {
                    // The locales aren't loaded yet: try again on the next lookup.
                    return false;
                }

                if (polish.Length > 0 && polish != english)
                {
                    names[english] = polish;
                }
            }

            _polishNames = names;
            _englishNames = names.Count == 0
                ? new Regex("(?!)")
                : new Regex(
                    @"(?<!\p{L})(" + string.Join("|", names.Keys.OrderByDescending(x => x.Length).Select(Regex.Escape)) +
                    @")(?!\p{L})", RegexOptions.CultureInvariant);
            return true;
        }
        finally
        {
            _busy = false;
        }
    }
}
