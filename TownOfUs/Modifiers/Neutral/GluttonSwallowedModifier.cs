using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Are;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Neutral;

/// <summary>
///     ARE: a player swallowed by a Glutton (on every client, added by the host's confirmation in
///     <see cref="GluttonSystem" />). After the swallow animation the player is inside the Glutton: invisible, not
///     targetable, counted as not alive for abilities, without abilities, consoles, the map or reports, and moved with
///     the Glutton on every client (movement and network sync are off until they come out). The host digests them
///     (a kill without a body) when the next meeting is called or after the digestion time.
/// </summary>
public sealed class GluttonSwallowedModifier(byte gluttonId) : DisabledModifier, IVisualAppearance
{
    public override string ModifierName => "Swallowed";
    public override bool HideOnUi => true;
    public override bool AutoStart => false;
    public override float Duration => 9999f;
    public override bool CanBeInteractedWith => false;
    public override bool IsConsideredAlive => false;
    public override bool CanUseAbilities => false;
    public override bool CanUseConsoles => false;
    public override bool CanOpenMap => false;
    public override bool CanReport => false;

    public byte GluttonId { get; } = gluttonId;

    public PlayerControl? Glutton => MiscUtils.PlayerById(GluttonId);

    /// <summary>Seconds since the swallow (each client counts on its own; only the host acts on it).</summary>
    public float SwallowedFor { get; private set; }

    /// <summary>The swallow animation is over: the player is hidden inside the Glutton.</summary>
    public bool Inside { get; private set; }

    /// <summary>The host sent the digestion kill (so it isn't sent twice).</summary>
    public bool DigestSent { get; set; }

    /// <summary>The modifier is gone (MiraAPI removes it at the end of the frame), so the animation stops.</summary>
    public bool Removed { get; private set; }

    private ScreenFlash? _stomach;

    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        if (!Inside)
        {
            return Player.GetDefaultModifiedAppearance();
        }

        return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PlayerName = string.Empty,
            PetId = "pet_EmptyPet",
            RendererColor = Color.clear,
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear
        };
    }

    public override void OnActivate()
    {
        base.OnActivate();
        KillAnimation.SetMovement(Player, false);

        if (Player.AmOwner)
        {
            if (Minigame.Instance)
            {
                Minigame.Instance.Close();
            }

            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }

            var opts = OptionGroupSingleton<GluttonOptions>.Instance;
            var key = opts.DigestTime > 0f
                ? "TownOfUsMira.Role.GluttonSwallowedTimed"
                : "TownOfUsMira.Role.GluttonSwallowedNotice";
            var text = MiraLocaleManager.Get(key)
                .Replace("<time>", Math.Round(opts.DigestTime).ToString(TownOfUsPlugin.Culture));
            if (opts.ReleaseOnDeath)
            {
                text += " " + MiraLocaleManager.Get("TownOfUsMira.Role.GluttonSwallowedHope");
            }

            var notif = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Glutton.ToTextColor()}{text}</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: AreAssets.GluttonIcon.LoadAsset());
            notif.AdjustNotification();
            notif.alphaTimer = 10f;
        }

        Coroutines.Start(GluttonSystem.CoSwallowAnimation(this));
    }

    /// <summary>End of the swallow animation (every client): the player disappears inside the Glutton.</summary>
    public void EnterStomach()
    {
        if (Inside || Removed || Player == null || Player.HasDied())
        {
            return;
        }

        Inside = true;
        Player.RawSetAppearance(this);
        Player.cosmetics.ToggleNameVisible(false);
        FollowGlutton();

        if (Player.AmOwner)
        {
            _stomach = new ScreenFlash();
            _stomach.SetColour(OptionGroupSingleton<GluttonOptions>.Instance.SwallowedSeeOutside
                ? new Color(0.28f, 0.05f, 0.04f, 0.55f)
                : new Color(0.02f, 0f, 0f, 1f));
            _stomach.SetActive(true);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        SwallowedFor += Time.fixedDeltaTime;
        if (Inside)
        {
            FollowGlutton();
        }

        GluttonSystem.CheckDigestTimer(this);
    }

    public override void Update()
    {
        base.Update();
        if (Inside)
        {
            FollowGlutton();
        }
    }

    // Every client moves the hidden player with the Glutton (its own sync is off), so a body or a release lands there.
    private void FollowGlutton()
    {
        var glutton = Glutton;
        if (glutton == null || Player == null)
        {
            return;
        }

        var pos = glutton.transform.position;
        Player.transform.position = new Vector3(pos.x, pos.y, pos.z + 0.001f);
        Player.MyPhysics.body.position = glutton.MyPhysics.body.position;
    }

    public override void OnDeactivate()
    {
        Removed = true;
        _stomach?.Dispose();
        _stomach = null;

        if (Player == null)
        {
            return;
        }

        GluttonSystem.RestoreShape(Player);
        if (Inside)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(!HudManagerPatches.CamouflageCommsEnabled);
        }

        KillAnimation.SetMovement(Player, true);
        if (!Player.HasDied())
        {
            // Out of the Glutton, where it is now (one snap on every client keeps the movement sync in step).
            var pos = Player.transform.position;
            Player.MyPhysics.ResetMoveState();
            Player.NetTransform.SnapTo(pos);
        }
    }
}
