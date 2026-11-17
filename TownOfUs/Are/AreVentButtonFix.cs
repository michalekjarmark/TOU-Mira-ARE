using TownOfUs.Modules.Components;

namespace TownOfUs.Are;

/// <summary>
///     ARE: Town of Us shows a "vents disabled" sprite on the vent button when few players are alive. At a round start
///     the sprite was missing once (a client in a Freeplay test): the NullReferenceException stopped the rest of
///     <c>TownOfUsEventHandlers.RoundStartHandler</c>, which also unlocks the buttons locked in the first round.
///     Creates the sprite when it is missing, like Town of Us' own vent event does.
/// </summary>
public static class AreVentButtonFix
{
    public static void SetDisabledSprite(bool disabled)
    {
        var helper = HudManagerHelper.Instance;
        if (helper == null || !helper)
        {
            return;
        }

        if (!helper.VentButtonDisabledSprite)
        {
            if (!HudManager.InstanceExists || HudManager.Instance.ImpostorVentButton == null)
            {
                return;
            }

            helper.VentButtonDisabledSprite = HudManager.Instance.ImpostorVentButton.CreateDeathDisabledSprite();
        }

        helper.VentButtonDisabledSprite.SetActive(disabled);
    }
}
