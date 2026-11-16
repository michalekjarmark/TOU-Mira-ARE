using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Are;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     The Handyman's button (AmonRa Edition): the job drawn for this round, with its icon and name. Uses per round
///     come back at every meeting.
/// </summary>
public sealed class HandymanJobButton : TownOfUsRoleButton<HandymanRole>
{
    private HandymanJob _shownJob = HandymanJob.None;
    private HandymanJob _runningJob = HandymanJob.None;
    private Minigame? _cameras;

    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.HandymanJob", "Job");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Handyman;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<HandymanOptions>.Instance.JobCooldown + MapCooldown, 0.001f, 120f);

    public override int MaxUses => (int)OptionGroupSingleton<HandymanOptions>.Instance.UsesPerRound;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => AreAssets.HandymanServiceButton;

    // The job's sprite and name are shown from the first FixedUpdate; the countdown is the job's time.
    public override float EffectDuration
    {
        get
        {
            var opts = OptionGroupSingleton<HandymanOptions>.Instance;
            return CurrentJob switch
            {
                HandymanJob.Service => opts.ServiceDelay.Value,
                HandymanJob.Pizza => opts.PizzaDuration.Value,
                HandymanJob.Warehouse => opts.WarehouseDuration.Value,
                HandymanJob.Stream => opts.StreamDuration.Value,
                _ => 0f
            };
        }
    }

    private HandymanJob CurrentJob => EffectActive ? _runningJob : Role?.Job ?? HandymanJob.None;

    public override bool CanUse()
    {
        if (!base.CanUse() || Role == null)
        {
            return false;
        }

        switch (Role.Job)
        {
            case HandymanJob.Service:
                return HandymanSystem.AnySabotage();
            case HandymanJob.Pizza:
                return !PlayerControl.LocalPlayer.HasModifier<HandymanSprintModifier>();
            case HandymanJob.Warehouse:
            case HandymanJob.Stream:
                // A Moderator's equipment ban covers these like cameras and admin.
                return !Minigame.Instance && !HandymanSystem.AdminOpen &&
                       !ModeratorSystem.IsBanned(PlayerControl.LocalPlayer, ModeratorBanType.Equipment);
            default:
                return false;
        }
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        // Like the Spy's and the Operative's gadgets: the "tasks before using" setting applies.
        var utility = Role.Job switch
        {
            HandymanJob.Warehouse => GameUtility.Admin,
            HandymanJob.Stream => GameUtility.Cams,
            _ => (GameUtility?)null
        };
        if (utility != null && !MiscUtils.CanUseUtility(utility.Value, true))
        {
            return;
        }

        base.ClickHandler();
    }

    protected override void OnClick()
    {
        _runningJob = Role.Job;
        var opts = OptionGroupSingleton<HandymanOptions>.Instance;
        switch (_runningJob)
        {
            case HandymanJob.Service:
                // No delay: no effect either, so OnEffectEnd won't run.
                if (opts.ServiceDelay.Value <= 0f)
                {
                    _runningJob = HandymanJob.None;
                    HandymanSystem.FixSabotage(PlayerControl.LocalPlayer);
                }
                else
                {
                    OverrideName(MiraLocaleManager.Get("TownOfUsMira.Role.EngineerFixing", "Fixing"));
                }

                break;
            case HandymanJob.Pizza:
                PlayerControl.LocalPlayer.RpcAddModifier<HandymanSprintModifier>();
                break;
            case HandymanJob.Warehouse:
                HandymanSystem.OpenAdmin(opts.MoveWithAdmin.Value);
                break;
            case HandymanJob.Stream:
                _cameras = HandymanSystem.OpenCameras();
                break;
        }
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        var job = _runningJob;
        _runningJob = HandymanJob.None;
        _shownJob = HandymanJob.None;
        switch (job)
        {
            case HandymanJob.Service:
                // Meetings fix sabotages on their own, and this runs when one starts.
                if (!MeetingHud.Instance && HandymanSystem.AnySabotage())
                {
                    HandymanSystem.FixSabotage(PlayerControl.LocalPlayer);
                }

                break;
            case HandymanJob.Warehouse:
                if (HandymanSystem.AdminOpen)
                {
                    MapBehaviour.Instance.Close();
                }

                break;
            case HandymanJob.Stream:
                if (_cameras != null)
                {
                    _cameras.Close();
                }

                _cameras = null;
                break;
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (Role == null)
        {
            return;
        }

        // A new job: its icon and name on the button.
        if (!EffectActive && _shownJob != Role.Job)
        {
            _shownJob = Role.Job;
            OverrideSprite(HandymanSystem.JobSprite(Role.Job).LoadAsset());
            OverrideName(Role.Job == HandymanJob.None ? Name : HandymanSystem.JobName(Role.Job));
        }

        if (!EffectActive || MeetingHud.Instance)
        {
            return;
        }

        // Closed the admin map or the cameras before the time ran out: the job is done.
        var closed = _runningJob switch
        {
            HandymanJob.Warehouse => !HandymanSystem.AdminOpen,
            HandymanJob.Stream => _cameras == null,
            _ => false
        };
        if (closed && Timer < EffectDuration - 0.25f)
        {
            ResetCooldownAndOrEffect();
        }
    }
}
