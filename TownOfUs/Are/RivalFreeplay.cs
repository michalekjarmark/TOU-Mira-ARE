using MiraAPI.Hud;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Networking;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: Freeplay "Set Modifiers" support for the <see cref="RivalModifier"/>, like the Lovers' one: after picking
///     the first player and "Rival", a second menu picks the other rival. In Multiplayer Freeplay the request goes to
///     the host (<see cref="MultiplayerFreeplayAction.SetRivals"/>). Unlike the Lovers, dead players are not revived:
///     a rival may be dead.
/// </summary>
public static class RivalFreeplay
{
    public static void OpenPicker(PlayerControl rivalA)
    {
        if (rivalA == null || rivalA.Data == null || rivalA.Data.Disconnected)
        {
            return;
        }

        var menu = CustomPlayerMenu.Create();
        menu.PhoneUI.GetChild(0).GetComponent<SpriteRenderer>().material =
            rivalA.cosmetics.currentBodySprite.BodySprite.material;
        menu.PhoneUI.GetChild(1).GetComponent<SpriteRenderer>().material =
            rivalA.cosmetics.currentBodySprite.BodySprite.material;

        menu.Begin(
            plr2 => plr2 != null && !plr2.Data.Disconnected && plr2 != rivalA,
            plr2 =>
            {
                menu.ForceClose();
                if (plr2 == null)
                {
                    return;
                }

                if (MultiplayerFreeplayMode.Enabled)
                {
                    Rpc<MultiplayerFreeplayRequestRpc>.Instance.Send(
                        PlayerControl.LocalPlayer,
                        new MultiplayerFreeplayRequest(MultiplayerFreeplayAction.SetRivals, rivalA.PlayerId,
                            plr2.PlayerId, 0));
                }
                else
                {
                    RivalModifier.DebugSetRivals(rivalA, plr2);
                }
            });

        foreach (var panel in menu.potentialVictims)
        {
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }

    /// <summary>
    ///     Host side of <see cref="MultiplayerFreeplayAction.SetRivals"/>.
    /// </summary>
    public static void HandleSetRivals(byte rivalAId, byte rivalBId)
    {
        var rivalA = GameData.Instance?.GetPlayerById(rivalAId)?.Object;
        var rivalB = GameData.Instance?.GetPlayerById(rivalBId)?.Object;
        if (rivalA == null || rivalB == null || rivalA.Data.Disconnected || rivalB.Data.Disconnected)
        {
            return;
        }

        RivalModifier.DebugSetRivals(rivalA, rivalB);
    }
}
