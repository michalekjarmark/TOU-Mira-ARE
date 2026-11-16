using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

/// <summary>
///     Chairman's "Board Meeting" button (AmonRa Edition): from the Director title on, calls a meeting from anywhere,
///     once per game. Not during a sabotage (like the meeting button).
/// </summary>
public sealed class ChairmanMeetingButton : TownOfUsRoleButton<ChairmanRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.ChairmanMeeting", "Board Meeting");
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Chairman;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => AreAssets.ChairmanMeetingButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is ChairmanRole chairman &&
               ChairmanSystem.HasPerk(chairman, ChairmanRank.Director);
    }

    public override bool CanUse()
    {
        if (!base.CanUse() || Role is not { MeetingUsed: false } || ShipStatus.Instance == null)
        {
            return false;
        }

        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
        return system is { AnyActive: false };
    }

    protected override void OnClick()
    {
        Role.MeetingUsed = true;
        ChairmanRole.RpcChairmanMeeting(PlayerControl.LocalPlayer);
    }
}
