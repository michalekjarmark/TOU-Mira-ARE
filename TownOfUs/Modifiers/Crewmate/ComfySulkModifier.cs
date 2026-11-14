using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

/// <summary>
///     ARE: the Comfy's sulk. While it lasts, kills and abilities on the Comfy fail like on the Cleric's barrier (the
///     checks are in <c>ComfyEvents</c>), and the Comfy can't do tasks or report bodies (<see cref="ComfySystem" />).
///     Added on every client by the Comfy's client (<c>RpcAddModifier</c>), so everyone's kill attempts see it.
/// </summary>
public sealed class ComfySulkModifier : BaseShieldModifier
{
    public override string ModifierName => MiraLocaleManager.Get("TownOfUsMira.Role.ComfySulk", "Sulk");
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.ComfyIcon;
    public override string ShieldDescription => MiraLocaleManager.Get("TownOfUsMira.Role.ComfySulkDescription");
    public override float Duration => Math.Max(OptionGroupSingleton<ComfyOptions>.Instance.SulkDuration, 0.1f);
    public override bool AutoStart => true;

    public override void OnDeath(DeathReason reason)
    {
        RemoveSelf();
    }

    public override void OnMeetingStart()
    {
        RemoveSelf();
    }
}
