using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Comfy's "Offload Task" button (AmonRa Edition): gives the task that is locked right now (the one that annoyed
///     the Comfy) to the closest player. A normal player interaction, so the Veteran, a shield etc. react as usual.
/// </summary>
public sealed class ComfyOffloadButton : TownOfUsRoleButton<ComfyRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ComfyOffload", "Offload Task");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Comfy;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ComfyOptions>.Instance.OffloadCooldown, 0f, 120f);
    public override float InitialCooldown => 0f;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => (int)OptionGroupSingleton<ComfyOptions>.Instance.MaxOffloads;
    public override LoadableAsset<Sprite> Sprite => AreAssets.ComfyButton;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    // Nothing locked to give: the button stays grey and the players around get no outline.
    public override bool CanUse()
    {
        if (!ComfySystem.HasTaskToOffload(PlayerControl.LocalPlayer))
        {
            ResetTarget();
            return false;
        }

        return base.CanUse();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        ComfySystem.LocalTick();
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error($"{Name}: Target is null");
            return;
        }

        ComfySystem.StartOffload(PlayerControl.LocalPlayer, Target);
    }
}
