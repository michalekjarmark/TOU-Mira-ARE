using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Utilities;
using TownOfUs.Are;
using TownOfUs.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

/// <summary>
///     The Glutton's "Swallow" (AmonRa Edition), its kill button. A kill button for Town of Us, so shields, the
///     Veteran's alert and the Guardian Angel stop it the same way they stop a kill (they react to the click).
/// </summary>
public sealed class GluttonSwallowButton : TownOfUsKillRoleButton<GluttonRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.GluttonSwallow", "Swallow");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Glutton;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<GluttonOptions>.Instance.SwallowCooldown + MapCooldown, 5f, 120f);

    public override LoadableAsset<Sprite> Sprite => AreAssets.GluttonSwallowButton;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, false));
    }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override bool CanUse()
    {
        var max = (int)OptionGroupSingleton<GluttonOptions>.Instance.MaxSwallowed;
        return base.CanUse() && (max <= 0 || GluttonSystem.SwallowedBy(PlayerControl.LocalPlayer).Count() < max);
    }

    public override PlayerControl? GetTarget()
    {
        // Not someone in a vent or on a ladder, zipline or platform: the swallow freezes them where they are.
        var noLovers = !OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther &&
                       PlayerControl.LocalPlayer.IsLover();
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
            x => !x.inVent && !x.IsInTargetingAnimState() && (!noLovers || !x.IsLover()));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Glutton Swallow: Target is null");
            return;
        }

        // Someone who can't be killed (the Pestilence): a normal kill attempt, so they react to it the usual way.
        if (Target.HasModifier<InvulnerabilityModifier>())
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
            return;
        }

        GluttonRole.RpcGluttonSwallow(PlayerControl.LocalPlayer, Target);
    }
}
