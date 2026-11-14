using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     AmonRa Edition, owner's feedback (2026-11-14): Town of Us draws a running effect like a cooldown (shading that
///     fills up and a white number), also when the button can end the effect right now: Swooper "Unswoop", Morphling
///     "Unmorph", Venerer, Glitch "Unmimic", Medium, Monarch knighting, Chameleon, Possum "Stand Up". Players took it
///     for a cooldown. Such a button is drawn ready instead: no shading, and the time left in green. Effects that can't
///     be ended keep the Town of Us look. Called at the end of the button drawing in
///     <c>TownOfUsButton.FixedUpdateHandler</c> and <c>TownOfUsTargetButton.FixedUpdateHandler</c>.
/// </summary>
public static class AreButtonLook
{
    // The game's own colour of the cooldown number (the same on every button), read before we ever change it.
    private static Color? normalTimerColor;

    /// <param name="button">The drawn button.</param>
    /// <param name="cancellableEffect">An effect is running and the button can end it now (it is usable).</param>
    public static void Apply(ActionButton button, bool cancellableEffect)
    {
        var text = button.cooldownTimerText;
        if (text == null)
        {
            return;
        }

        normalTimerColor ??= text.color;
        if (!cancellableEffect)
        {
            text.color = normalTimerColor.Value;
            return;
        }

        button.isCoolingDown = false;
        button.SetCooldownFill(0f);
        text.color = Palette.AcceptedGreen;
    }
}
