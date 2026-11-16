using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Are;
using TownOfUs.Options.Modifiers.Universal;

namespace TownOfUs.Modifiers;

/// <summary>
///     "Going offline" (ARE, <see cref="Game.Universal.StreamerModifier" />), added on every client: hidden from the
///     cameras (and the admin map, with that option) until it runs out or a meeting starts. The hiding is in
///     <see cref="StreamerSystem" />.
/// </summary>
public sealed class StreamerOfflineModifier : TimedModifier
{
    public override string ModifierName => "Offline";
    public override bool AutoStart => true;
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<StreamerOptions>.Instance.OfflineDuration.Value;

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        RemoveSelf();
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        RemoveSelf();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        if (Player != null)
        {
            StreamerSystem.Restore(Player);
        }
    }
}
