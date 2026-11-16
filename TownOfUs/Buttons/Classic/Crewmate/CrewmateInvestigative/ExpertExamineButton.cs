using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Expert's "Examine" button (AmonRa Edition). A normal player-targeting button, so the click counts as an
///     interaction (Veteran on alert, Sixth Sense, Lookout...).
/// </summary>
public sealed class ExpertExamineButton : TownOfUsRoleButton<ExpertRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ExpertExamine", "Examine");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Expert;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<ExpertOptions>.Instance.ExamineCooldown + MapCooldown, 5f, 120f);

    public override int MaxUses => (int)OptionGroupSingleton<ExpertOptions>.Instance.MaxExaminations;

    // TownOfUsButton defaults to false: without it the "0 = no limit" option would mean no examinations at all.
    public override bool ZeroIsInfinite { get; set; } = true;

    public override LoadableAsset<Sprite> Sprite => AreAssets.ExpertButton;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) &&
               (OptionGroupSingleton<ExpertOptions>.Instance.CanReexamine ||
                !target!.HasModifier<ExpertVerdictModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Examine: Target is null");
            return;
        }

        Role.Examine(Target);
    }
}
