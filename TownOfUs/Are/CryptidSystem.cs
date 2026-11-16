using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Cryptid (AmonRa Edition): the sightings, on every client. A sighting is a blurry grey figure (a sprite on the
///     Players layer, so the cameras show it too) that follows the Cryptid's ghost. The ghost itself stays invisible
///     to the living. Every client removes the figure when a living player comes close; the Cryptid's client also
///     tells everyone, so all agree, and handles the impostor scaring it off.
/// </summary>
public static class CryptidSystem
{
    /// <summary>Why a sighting ended (synced as int).</summary>
    public enum EndReason
    {
        Time,
        Approached,
        ScaredOff
    }

    // The figure's height in world units, about a player's.
    private const float FigureHeight = 0.95f;

    private static readonly Dictionary<byte, SpriteRenderer> Figures = [];

    public static bool IsShowing(PlayerControl player)
    {
        return Figures.TryGetValue(player.PlayerId, out var figure) && figure != null;
    }

    public static void Create(PlayerControl cryptid)
    {
        Remove(cryptid.PlayerId, EndReason.Time);
        var sprite = AreAssets.CryptidFigure.LoadAsset();
        var obj = new GameObject("ARE-CryptidFigure");
        obj.layer = LayerMask.NameToLayer("Players");
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        var scale = FigureHeight / Mathf.Max(0.01f, sprite.bounds.size.y);
        obj.transform.localScale = new Vector3(scale, scale, 1f);
        Figures[cryptid.PlayerId] = renderer;
        Follow(cryptid, renderer);
        Info($"Cryptid: {cryptid.Data.PlayerName} appeared");
    }

    public static void Remove(byte cryptidId, EndReason reason)
    {
        if (!Figures.Remove(cryptidId, out var figure))
        {
            return;
        }

        if (figure != null)
        {
            figure.gameObject.Destroy();
        }

        var cryptid = MiscUtils.PlayerById(cryptidId);
        if (cryptid == null)
        {
            return;
        }

        Info($"Cryptid: {cryptid.Data.PlayerName} vanished ({reason})");
        if (cryptid.AmOwner && reason == EndReason.ScaredOff && cryptid.Data.Role is CryptidRole role)
        {
            role.ScaredOff = true;
            var notif = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.CryptidScaredOff")}</b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: AreAssets.CryptidIcon.LoadAsset());
            notif.AdjustNotification();
        }
    }

    public static void RemoveAll()
    {
        foreach (var id in Figures.Keys.ToArray())
        {
            Remove(id, EndReason.Time);
        }
    }

    /// <summary>Every frame, from <see cref="UpdatePatch" />: the figures follow their ghosts, or vanish.</summary>
    public static void Tick()
    {
        if (Figures.Count == 0)
        {
            return;
        }

        var opts = OptionGroupSingleton<CryptidOptions>.Instance;
        foreach (var (id, figure) in Figures.ToArray())
        {
            var cryptid = MiscUtils.PlayerById(id);
            if (figure == null || cryptid == null || cryptid.Data == null || cryptid.Data.Disconnected ||
                cryptid.Data.Role is not CryptidRole || MeetingHud.Instance)
            {
                Remove(id, EndReason.Time);
                continue;
            }

            Follow(cryptid, figure);

            Vector2 pos = figure.transform.position;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.Disconnected || player.HasDied() ||
                    Vector2.Distance(player.transform.position, pos) >= opts.VanishDistance)
                {
                    continue;
                }

                var reason = opts.ImpostorScaresOff && player.IsImpostorAligned()
                    ? EndReason.ScaredOff
                    : EndReason.Approached;
                Remove(id, reason);
                if (cryptid.AmOwner)
                {
                    CryptidRole.RpcCryptidVanish(cryptid, (int)reason);
                }

                break;
            }
        }
    }

    private static void Follow(PlayerControl cryptid, SpriteRenderer figure)
    {
        var pos = cryptid.transform.position;
        figure.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000f);
        figure.flipX = cryptid.cosmetics != null && cryptid.cosmetics.FlipX;

        // A slow shimmer: it never looks quite solid.
        figure.color = new Color(1f, 1f, 1f, 0.6f + 0.15f * Mathf.Sin(Time.time * 3f));
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class UpdatePatch
    {
        public static void Postfix()
        {
            Tick();
        }
    }
}
