using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Auditor's "Audit" button (AmonRa Edition). A normal player-targeting button on purpose: the click goes through
///     MiraButtonClickEvent, so a Veteran on alert kills the Auditor (and cancels the audit), a Plaguebearer infects
///     them, a Lookout sees them etc., like with every other interaction.
/// </summary>
public sealed class AuditorAuditButton : TownOfUsRoleButton<AuditorRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.AuditorAudit", "Audit");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Auditor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<AuditorOptions>.Instance.AuditCooldown + MapCooldown, 5f, 120f);

    // The duration only drives the countdown on the button; the audit itself is timed by AuditSession.
    public override float EffectDuration => OptionGroupSingleton<AuditorOptions>.Instance.AuditDuration;

    public override int MaxUses => (int)OptionGroupSingleton<AuditorOptions>.Instance.InitialAudits;

    public override LoadableAsset<Sprite> Sprite => AreAssets.AuditButton;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role is { ActiveAudit: null };
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Audit: Target is null");
            return;
        }

        Role.StartAudit(Target);
    }
}
