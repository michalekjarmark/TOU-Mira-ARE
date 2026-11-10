using HarmonyLib;
using TownOfUs.Patches.Misc;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Adds the AmonRa Edition logo on the startup splash, centered under the Innersloth and AU Avengers logos
///     that <see cref="SplashIntroPatch"/> sets up. Every frame it copies the AU Avengers logo's scale and
///     transparency, so it follows the intro animation (fade in/out) instead of popping in.
/// </summary>
[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
public static class AreSplashLogoPatch
{
    // Tuning: size relative to the AU Avengers logo and gap under the lowest logo (in world units).
    private const float SizeFactor = 0.6f;
    private const float GapBelow = 0.15f;

    private static SpriteRenderer? _logo;

    public static void Postfix()
    {
        if (!SplashIntroPatch.IntroSetup || SplashIntroPatch.PewLogo == null)
        {
            return;
        }

        var pew = SplashIntroPatch.PewLogo.GetComponent<SpriteRenderer>();
        if (pew == null)
        {
            return;
        }

        if (_logo == null)
        {
            var go = new GameObject("AreLogo");
            go.layer = pew.gameObject.layer;
            go.transform.SetParent(pew.transform.parent, false);
            _logo = go.AddComponent<SpriteRenderer>();
            _logo.sprite = AreAssets.AreLogo.LoadAsset();
            _logo.sortingLayerID = pew.sortingLayerID;
            _logo.sortingOrder = pew.sortingOrder;
        }

        _logo.transform.localScale = pew.transform.localScale * SizeFactor;
        _logo.color = pew.color;
        _logo.enabled = pew.enabled && pew.gameObject.activeInHierarchy;

        // Center between both logos, under the lower one (world space, so parent offsets don't matter).
        var sloth = SplashIntroPatch.SlothLogo != null
            ? SplashIntroPatch.SlothLogo.GetComponentInChildren<SpriteRenderer>()
            : null;
        var pewBounds = pew.bounds;
        var centerX = sloth != null ? (sloth.bounds.center.x + pewBounds.center.x) / 2f : pewBounds.center.x;
        var bottom = sloth != null ? Mathf.Min(sloth.bounds.min.y, pewBounds.min.y) : pewBounds.min.y;
        var halfHeight = _logo.bounds.extents.y;
        _logo.transform.position = new Vector3(centerX, bottom - GapBelow - halfHeight, pew.transform.position.z);
    }
}
