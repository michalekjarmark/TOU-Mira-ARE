using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Fabulist (AmonRa Edition): the alibi copies, one per Fabulist, on every client. A copy is Town of Us'
///     <see cref="FakePlayer" /> (the still figure also used for statues): no collider, so it can't be killed, reported or
///     targeted, and the admin map doesn't count it (unless the option says so). Every client removes a copy when another
///     living player comes close; the Fabulist's client also sends that to everyone, so all agree.
/// </summary>
public static class FabulistSystem
{
    private static readonly Dictionary<byte, FakePlayer> Alibis = [];

    public static bool HasAlibi(PlayerControl player)
    {
        return Alibis.TryGetValue(player.PlayerId, out var alibi) && alibi.body != null;
    }

    /// <summary>Next to a task console (impostors' fake tasks included, sabotage consoles not).</summary>
    public static bool NearTaskConsole(PlayerControl player)
    {
        if (!ShipStatus.Instance)
        {
            return false;
        }

        var pos = player.GetTruePosition();
        foreach (var console in ShipStatus.Instance.AllConsoles)
        {
            if (RiggerSystem.IsTaskConsole(console) &&
                Vector2.Distance(pos, console.transform.position) <= console.UsableDistance)
            {
                return true;
            }
        }

        return false;
    }

    public static void Create(PlayerControl fabulist, Vector2 position)
    {
        Remove(fabulist.PlayerId, false);
        if (fabulist.HasDied() || fabulist.cosmetics == null)
        {
            return;
        }

        // FakePlayer takes the skin off the player it copies (and off their outfit data): put it back.
        var defaultSkin = fabulist.Data.DefaultOutfit.SkinId;
        var shownOutfit = fabulist.CurrentOutfit;
        var shownSkin = shownOutfit.SkinId;
        var shownColor = shownOutfit.ColorId;

        var alibi = new FakePlayer(fabulist);

        fabulist.Data.DefaultOutfit.SkinId = defaultSkin;
        shownOutfit.SkinId = shownSkin;
        fabulist.cosmetics.SetSkin(shownSkin, shownColor);

        alibi.body.transform.position = new Vector3(position.x, position.y, position.y / 1000f);
        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            alibi.Camo();
        }

        Alibis[fabulist.PlayerId] = alibi;
        Info($"Fabulist: {fabulist.Data.PlayerName} left an alibi at {position}");
    }

    public static void Remove(byte fabulistId, bool broken)
    {
        if (!Alibis.Remove(fabulistId, out var alibi))
        {
            return;
        }

        FakePlayer.FakePlayers.Remove(alibi);
        alibi.Destroy();

        var fabulist = MiscUtils.PlayerById(fabulistId);
        if (fabulist == null)
        {
            return;
        }

        Info($"Fabulist: {fabulist.Data.PlayerName}'s alibi is gone{(broken ? " (someone came close)" : string.Empty)}");
        if (broken && fabulist.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.FabulistAlibiBroken")}</b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: AreAssets.FabulistIcon.LoadAsset());
            notif.AdjustNotification();
        }
    }

    public static void RemoveAll()
    {
        foreach (var id in Alibis.Keys.ToArray())
        {
            Remove(id, false);
        }
    }

    /// <summary>The admin map counts copies too (option): a second dot in the Fabulist's colour.</summary>
    public static void AddAlibisInRoom(Collider2D roomArea, List<int> colorMap)
    {
        if (Alibis.Count == 0 || !OptionGroupSingleton<FabulistOptions>.Instance.CopyOnAdmin)
        {
            return;
        }

        foreach (var (id, alibi) in Alibis)
        {
            var fabulist = MiscUtils.PlayerById(id);
            if (alibi.body != null && fabulist != null && roomArea.OverlapPoint(alibi.body.transform.position))
            {
                colorMap.Add(fabulist.Data.DefaultOutfit.ColorId);
            }
        }
    }

    /// <summary>Every frame, from <see cref="UpdatePatch" />: copies whose story fell apart.</summary>
    public static void Tick()
    {
        if (Alibis.Count == 0)
        {
            return;
        }

        var distance = OptionGroupSingleton<FabulistOptions>.Instance.BreakDistance;
        foreach (var (id, alibi) in Alibis.ToArray())
        {
            var fabulist = MiscUtils.PlayerById(id);

            // Town of Us clears every FakePlayer at the start of a round.
            if (alibi.body == null || fabulist == null || fabulist.Data == null || fabulist.Data.Disconnected ||
                fabulist.HasDied())
            {
                Remove(id, false);
                continue;
            }

            Vector2 pos = alibi.body.transform.position;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player == fabulist || player.Data == null || player.Data.Disconnected ||
                    player.HasDied() || Vector2.Distance(player.transform.position, pos) >= distance)
                {
                    continue;
                }

                Remove(id, true);
                if (fabulist.AmOwner)
                {
                    FabulistRole.RpcFabulistAlibiEnd(fabulist, true);
                }

                break;
            }
        }
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
