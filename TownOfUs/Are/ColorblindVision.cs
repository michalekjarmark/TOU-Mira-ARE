using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     ARE: what a <see cref="Modifiers.Game.Universal.ColorblindModifier"/> player sees. Runs only on that player's
///     client: every other living player further away than the range gets the Town of Us camouflage look (grey, no
///     name, by default no hat, skin, visor or pet), set with the same appearance type as the comms camouflage, so
///     the rest of TOU treats it like one. Up close the real appearance comes back. Nothing is sent over the network.
///     <see cref="Modules.Components.HudManagerHelper.UpdateCamouflageComms"/> leaves players hidden here alone.
/// </summary>
public static class ColorblindVision
{
    private static readonly HashSet<byte> Hidden = [];

    public static bool IsHidden(PlayerControl player)
    {
        return Hidden.Count > 0 && player != null && Hidden.Contains(player.PlayerId);
    }

    /// <summary>
    ///     Called every frame from the local player's modifier.
    /// </summary>
    public static void Update(PlayerControl local)
    {
        if (local == null || local.Data == null || local.Data.IsDead || MeetingHud.Instance || ExileController.Instance ||
            !ShipStatus.Instance)
        {
            RestoreAll();
            return;
        }

        if (local.IsHysteria())
        {
            // The Hypnotist's hysteria sets everyone's look on this client; don't fight it.
            Hidden.Clear();
            return;
        }

        var options = OptionGroupSingleton<ColorblindOptions>.Instance;
        var range = options.Range;
        var origin = local.GetTruePosition();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player == local || player.Data == null)
            {
                continue;
            }

            var skip = player.Data.Disconnected || player.Data.IsDead || player.Data.Role is IGhostRole ||
                       player.GetAppearanceType() == TownOfUsAppearances.Swooper;
            var far = !skip && Vector2.Distance(origin, player.GetTruePosition()) > range;
            if (far)
            {
                if (player.GetAppearanceType() != TownOfUsAppearances.Camouflage)
                {
                    Apply(player, options.HideCosmetics);
                }

                Hidden.Add(player.PlayerId);
            }
            else if (Hidden.Remove(player.PlayerId))
            {
                Restore(player);
            }
        }
    }

    /// <summary>
    ///     Gives every hidden player their real look back (meeting, death, modifier removed).
    /// </summary>
    public static void RestoreAll()
    {
        if (Hidden.Count == 0)
        {
            return;
        }

        foreach (var id in Hidden.ToArray())
        {
            var player = MiscUtils.PlayerById(id);
            if (player != null)
            {
                Restore(player);
            }
        }

        Hidden.Clear();
    }

    private static void Apply(PlayerControl player, bool hideCosmetics)
    {
        var current = player.GetAppearance();
        player.RawSetAppearance(new VisualAppearance(current, TownOfUsAppearances.Camouflage)
        {
            ColorId = player.Data.DefaultOutfit.ColorId,
            HatId = hideCosmetics ? "hat_NoHat" : current.HatId,
            SkinId = hideCosmetics ? "skin_None" : current.SkinId,
            VisorId = hideCosmetics ? "visor_EmptyVisor" : current.VisorId,
            PetId = hideCosmetics ? "pet_EmptyPet" : current.PetId,
            PlayerName = string.Empty,
            NameVisible = false,
            PlayerMaterialColor = Color.grey,
            ColorBlindTextColor = Color.clear,
            Size = OptionGroupSingleton<AdvancedSabotageOptions>.Instance.HidePlayerSizeInCamo
                ? new Vector3(0.7f, 0.7f, 1f)
                : current.Size
        });
    }

    private static void Restore(PlayerControl player)
    {
        if (player.GetAppearanceType() != TownOfUsAppearances.Camouflage)
        {
            return;
        }

        // Still camouflaged for everyone (comms sabotage, Venerer): keep the TOU look.
        if (HudManagerPatches.CommsSaboActive() || player.HasModifier<VenererCamouflageModifier>())
        {
            player.SetCamouflage(false);
            player.SetCamouflage();
            return;
        }

        // The look the player has right now (Morphling, Glitch...), not just the default one.
        player.RawSetAppearance(player.GetAppearance());
        player.cosmetics.ToggleNameVisible(true);
    }
}
