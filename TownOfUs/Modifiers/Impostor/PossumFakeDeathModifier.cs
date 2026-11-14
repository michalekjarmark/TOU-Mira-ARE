using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Are;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

/// <summary>
///     ARE: the Possum is playing dead (on every client). Invisible like a Swooper (impostors and the dead who know
///     see a faint outline), not targetable, counted as not alive, no abilities, consoles or reports. The fake body
///     comes and goes with this modifier (<see cref="PossumSystem" />). No timer: the Possum's button ends it.
/// </summary>
public sealed class PossumFakeDeathModifier : DisabledModifier, IVisualAppearance
{
    public override string ModifierName => "Playing Dead";
    public override bool HideOnUi => true;
    public override bool AutoStart => false;
    public override float Duration => 9999f;
    public override bool CanBeInteractedWith => false;
    public override bool IsConsideredAlive => false;
    public override bool CanUseAbilities => false;
    public override bool CanReport => false;

    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        var local = PlayerControl.LocalPlayer;
        var seesOutline = local.IsImpostorAligned() ||
                          (local.DiedOtherRound() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow);
        return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PlayerName = string.Empty,
            PetId = "pet_EmptyPet",
            RendererColor = seesOutline ? new Color(0f, 0f, 0f, 0.15f) : Color.clear,
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear
        };
    }

    public override void OnActivate()
    {
        PossumSystem.SpawnBody(Player);
        Player.RawSetAppearance(this);
        Player.cosmetics.ToggleNameVisible(false);

        if (Player.AmOwner)
        {
            CustomButtonSingleton<PossumPlayDeadButton>.Instance.OverrideName(
                MiraLocaleManager.Get("TownOfUsMira.Role.PossumStandUp", "Stand Up"));
        }
    }

    public override void OnDeactivate()
    {
        PossumSystem.RemoveBody(Player);
        Player.ResetAppearance();
        Player.cosmetics.ToggleNameVisible(true);
        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }

        if (Player.AmOwner)
        {
            CustomButtonSingleton<PossumPlayDeadButton>.Instance.OverrideName(
                MiraLocaleManager.Get("TownOfUsMira.Role.PossumPlayDead", "Play Dead"));
        }
    }

    public override void OnMeetingStart()
    {
        RemoveSelf();
    }
}
