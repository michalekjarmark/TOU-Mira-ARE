using HarmonyLib;
using Il2CppInterop.Runtime;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Polish letters in every game language (owner's report, 2026-11-28): with the game in English, the Polish names of
///     the AmonRa roles (<see cref="AreMammonNames" />) showed boxes instead of ą ć ę ł ń ś ź ż, because none of the
///     game's static TMP fonts (Barlow, Brook, VCR, ...) has them and TMP's global fallback list is empty.
///     <para>
///         Fix: a dynamic font asset made from a source font file the game already has (Barlow-Bold, the source of
///         the dynamic "Barlow-Bold-Outline SDF", else Liberation Sans) is added to <see cref="TMP_Settings" />'s
///         global fallbacks, so every text takes the missing letters from it. Runs on the main menu and the HUD start
///         until it succeeds (the source font must be loaded).
///     </para>
/// </summary>
[HarmonyPatch]
public static class ArePolishLetters
{
    private const string Letters = "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ";

    // Preferred first: the game's UI typeface.
    private static readonly string[] SourceFonts = ["Barlow-Bold", "LiberationSans"];

    private static TMP_FontAsset? _fallback;
    private static bool _gaveUp;

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    public static void MainMenuPostfix()
    {
        Ensure();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void HudPostfix()
    {
        Ensure();
    }

    private static void Ensure()
    {
        if (_fallback || _gaveUp)
        {
            return;
        }

        try
        {
            var fallbacks = TMP_Settings.fallbackFontAssets;
            if (fallbacks == null)
            {
                return;
            }

            var sources = new Dictionary<string, Font>();
            foreach (var obj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<TMP_FontAsset>()))
            {
                var font = obj.TryCast<TMP_FontAsset>();
                if (font && font!.sourceFontFile && !sources.ContainsKey(font.sourceFontFile.name))
                {
                    sources[font.sourceFontFile.name] = font.sourceFontFile;
                }
            }

            foreach (var name in SourceFonts)
            {
                if (!sources.TryGetValue(name, out var source))
                {
                    continue;
                }

                var asset = TMP_FontAsset.CreateFontAsset(source);
                if (!asset)
                {
                    continue;
                }

                // HasCharacter with tryAddCharacter renders the glyph into the dynamic atlas when the source has it.
                var missing = new string(Letters.Where(c => !asset.HasCharacter(c, false, true)).ToArray());
                if (missing.Length > 0)
                {
                    Warning($"ARE Polish letters: {name} has no '{missing}', trying the next font.");
                    Object.Destroy(asset);
                    continue;
                }

                asset.name = $"ARE Polish letters ({name})";
                Keep(asset);
                Keep(asset.material);
                Keep(asset.atlasTexture);
                fallbacks.Add(asset);
                _fallback = asset;
                Info($"ARE Polish letters: added a fallback font made from {name}.");
                return;
            }

            if (sources.Count > 0)
            {
                // The fonts are loaded but none has the letters: no use trying again.
                _gaveUp = true;
                Warning($"ARE Polish letters: no source font with Polish letters among [{string.Join(", ", sources.Keys)}].");
            }
        }
        catch (Exception e)
        {
            _gaveUp = true;
            Error($"ARE Polish letters failed: {e}");
        }
    }

    // A font asset made at runtime is not referenced by any scene, so the scene change cleanup would unload it.
    private static void Keep(Object? obj)
    {
        if (obj)
        {
            obj!.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }
    }
}
