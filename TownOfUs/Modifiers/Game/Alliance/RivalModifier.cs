using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using TownOfUs.Are;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Other;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Modifiers.Game.Alliance;

/// <summary>
///     ARE alliance modifier, the opposite of the Lovers: two players know each other as rivals (a "≠" after the
///     name, no role) and each of them wins only if the other one doesn't survive until the end of the game. This is
///     an extra condition on top of the player's normal win (team or own role), see <see cref="DidWin"/>. A rival who
///     left the game counts as not surviving.
/// </summary>
public sealed class RivalModifier : AllianceGameModifier, IWikiDiscoverable, IAssignableTargets
{
    public const string SymbolText = "≠";

    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Rival,
        TmpSpriteUtils.CreateSpriteAsset(AreAssets.RivalIcon.LoadAsset(), "TouMira.Modifier.Alliance.Rival", 1.45f));

    public override string IdPart => "Rival";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}", "Rival");
    public override string IntroInfo => RivalString();
    public override string Symbol => SymbolText;
    public override float IntroSize => 3f;
    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.RivalIcon;

    /// <summary>
    ///     PlayerId of the other rival, <see cref="byte.MaxValue"/> until the pair is set. An id instead of a
    ///     PlayerControl, so a rival who left the game can still be looked up in GameData.
    /// </summary>
    public byte RivalId { get; set; } = byte.MaxValue;

    public NetworkedPlayerInfo? RivalData =>
        RivalId == byte.MaxValue || !GameData.Instance ? null : GameData.Instance.GetPlayerById(RivalId);

    public override int CustomAmount =>
        (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.RivalsChance.Value != 0 ? 2 : 0;

    public override int CustomChance => (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.RivalsChance.Value;

    // After the Lovers (4), so a Lover is never picked as a rival.
    public int Priority { get; set; } = 5;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        // The wiki lists a modifier that isn't on anyone: the general rule instead of "You have no rival.".
        return Player == null
            ? MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription")
            : RivalString();
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription")
            .Replace("<symbol>", $"{TownOfUsColors.Rival.ToTextColor()}{SymbolText}</color>") +
               MiscUtils.AppendOptionsText(GetType());
    }

    public string RivalString()
    {
        var rival = RivalData;
        if (rival == null)
        {
            return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}InfoNone");
        }

        var key = rival.Disconnected ? $"TownOfUsMira.Modifier.{IdPart}InfoDisconnected" : $"TownOfUsMira.Modifier.{IdPart}Info";
        return MiraLocaleManager.Get(key).Replace("<player>", rival.PlayerName);
    }

    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override int GetAssignmentChance()
    {
        return 0;
    }

    /// <summary>
    ///     True while the other rival is alive and still in the game, i.e. while this player can't win.
    /// </summary>
    public bool RivalAlive()
    {
        var rival = RivalData;
        return rival != null && !rival.Disconnected && !rival.IsDead;
    }

    /// <summary>
    ///     The extra condition: with the rival alive at the end the player loses whatever the result, otherwise
    ///     <see langword="null"/> lets the player's role (team) decide.
    /// </summary>
    public override bool? DidWin(GameOverReason reason)
    {
        var alive = RivalAlive();
        Message($"Rival: {Player.Data.PlayerName}'s rival {RivalData?.PlayerName ?? "(none)"} " +
                $"{(alive ? "survived, so they lose" : "didn't survive, team decides")} ({reason})");
        return alive ? false : null;
    }

    /// <summary>
    ///     Picks the pair on the host after the roles are assigned (see <see cref="RivalsOptions"/>): with the
    ///     "killer" chance one of them is a killer (Impostor, or Neutral Killing if neutrals may be rivals), the other
    ///     one is not, unless "both can be killers" is on. Otherwise neither is a killer.
    /// </summary>
    public void AssignTargets()
    {
        if (!RoleOptions.IsClassicRoleAssignment)
        {
            return;
        }

        foreach (var rival in PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasModifier<RivalModifier>()))
        {
            rival.RpcRemoveModifier<RivalModifier>();
        }

        Random rnd = new();
        if (rnd.Next(1, 101) > (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.RivalsChance.Value)
        {
            return;
        }

        var opts = OptionGroupSingleton<RivalsOptions>.Instance;
        var pool = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && !x.HasModifier<ExecutionerTargetModifier>() &&
                        !x.HasModifier<AllianceGameModifier>() &&
                        !SpectatorRole.TrackedSpectators.Contains(x.Data.PlayerName) &&
                        (opts.NeutralRivals || !x.IsNeutral()))
            .ToList();
        var killers = pool.Where(IsKiller).ToList();
        var others = pool.Where(x => !IsKiller(x)).ToList();

        var withKiller = rnd.Next(0, 100) < (int)opts.KillerPercent;
        PlayerControl? first = null;
        List<PlayerControl> secondPool;
        if (withKiller)
        {
            first = killers.Count > 0 ? killers[rnd.Next(killers.Count)] : null;
            secondPool = opts.BothKillers ? pool.Where(x => x != first).ToList() : others;
        }
        else
        {
            first = others.Count > 0 ? others[rnd.Next(others.Count)] : null;
            secondPool = others.Where(x => x != first).ToList();
        }

        if (first == null || secondPool.Count == 0)
        {
            Message($"Rival: not enough players for a pair (with killer: {withKiller}, killers {killers.Count}, " +
                    $"others {others.Count})");
            return;
        }

        var second = secondPool[rnd.Next(secondPool.Count)];
        Message($"Rival: pair {first.Data.PlayerName} ({first.Data.Role.GetType().Name}) and " +
                $"{second.Data.PlayerName} ({second.Data.Role.GetType().Name}), with killer: {withKiller}");
        RpcSetRivals(first, second);
    }

    private static bool IsKiller(PlayerControl player)
    {
        return player.IsImpostor() || player.Is(RoleAlignment.NeutralKilling);
    }

    /// <summary>
    ///     Freeplay helper: makes the two players rivals (removing the current pair first).
    /// </summary>
    public static void DebugSetRivals(PlayerControl rivalA, PlayerControl rivalB)
    {
        if (rivalA == null || rivalB == null || rivalA == rivalB)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray()
                     .Where(x => x != null && x.HasModifier<RivalModifier>()))
        {
            player.RpcRemoveModifier<RivalModifier>();
        }

        RpcSetRivals(rivalA, rivalB);
    }

    [MethodRpc((uint)TownOfUsRpc.SetRivals)]
    private static void RpcSetRivals(PlayerControl player, PlayerControl target)
    {
        if (PlayerControl.AllPlayerControls.ToArray().Any(x => x.HasModifier<RivalModifier>()))
        {
            Error("RpcSetRivals - Rivals Already Spawned!");
            return;
        }

        var targetModifier = target.AddModifier<RivalModifier>();
        var sourceModifier = player.AddModifier<RivalModifier>();
        if (targetModifier == null || sourceModifier == null)
        {
            Error("RpcSetRivals - could not add the modifier");
            return;
        }

        targetModifier.RivalId = player.PlayerId;
        sourceModifier.RivalId = target.PlayerId;
    }
}
