using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Workaholic's "Take Task" button (AmonRa Edition). Usable after all own tasks are done, while the Workaholic holds
///     no taken task and someone has a task to give. The cooldown starts when a taken task is finished (set in
///     <see cref="WorkaholicSystem.OnCopyCompleted" />); when nothing was taken, the use comes back and there's no cooldown.
/// </summary>
public sealed class WorkaholicTakeButton : TownOfUsRoleButton<WorkaholicRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.WorkaholicTake", "Take Task");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Workaholic;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<WorkaholicOptions>.Instance.TakeCooldown, 0f, 120f);
    public override float InitialCooldown => 0f;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => (int)OptionGroupSingleton<WorkaholicOptions>.Instance.MaxTakenTasks;
    public override LoadableAsset<Sprite> Sprite => AreAssets.WorkaholicButton;

    public override bool CanUse()
    {
        var local = PlayerControl.LocalPlayer;
        return base.CanUse() && !WorkaholicSystem.IsBusy && WorkaholicSystem.OwnTasksDone(local) &&
               WorkaholicSystem.HasCandidate;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        var local = PlayerControl.LocalPlayer;
        if (local && !local.HasDied())
        {
            WorkaholicSystem.LocalTick(local);
        }
    }

    protected override void OnClick()
    {
        WorkaholicSystem.StartTake(PlayerControl.LocalPlayer);
    }
}
