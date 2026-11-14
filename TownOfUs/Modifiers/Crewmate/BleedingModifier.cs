using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

/// <summary>
///     ARE: the killer of a <see cref="Game.Crewmate.BleederModifier"/> player. Added on every client by
///     <see cref="Events.Modifiers.BleederEvents"/>, so every client draws the same dark red footprints (no player
///     color, also for the killer himself) while the timer runs. Invisible killers bleed too; vents stop the prints
///     and, with the option on, end the trail.
/// </summary>
public sealed class BleedingModifier : TimedModifier
{
    private const float PrintSpacing = 0.45f; // world units walked between two prints
    private const float PrintScale = 0.42f;   // like the Investigator's prints at size 4
    private static readonly Color BloodColor = new(0.58f, 0.03f, 0.05f, 0.95f);
    private static readonly List<GameObject> Prints = [];

    private Vector3 _lastPos;
    private bool _hasLastPos;

    public override string ModifierName => "Bleeding";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<BleederOptions>.Instance.TrailDuration;

    public override void OnActivate()
    {
        _hasLastPos = false;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Player == null || Player.Data == null || Player.Data.IsDead || !ShipStatus.Instance || MeetingHud.Instance)
        {
            return;
        }

        if (Player.inVent || Player.walkingToVent)
        {
            if (OptionGroupSingleton<BleederOptions>.Instance.VentEndsTrail)
            {
                RemoveSelf();
            }

            _hasLastPos = false;
            return;
        }

        var pos = Player.transform.position;
        if (_hasLastPos && Vector2.Distance(_lastPos, pos) < PrintSpacing)
        {
            return;
        }

        if (_hasLastPos)
        {
            SpawnPrint(pos);
        }

        _lastPos = pos;
        _hasLastPos = true;
    }

    public override void OnDeath(DeathReason reason)
    {
        RemoveSelf();
    }

    public override void OnMeetingStart()
    {
        RemoveSelf();
    }

    /// <summary>
    ///     Removes every print at once (meeting start), so no old trail is left on the map after the meeting.
    /// </summary>
    public static void ClearPrints()
    {
        foreach (var print in Prints)
        {
            if (print != null)
            {
                print.DestroyImmediate();
            }
        }

        Prints.Clear();
    }

    private void SpawnPrint(Vector3 pos)
    {
        var velocity = Player.MyPhysics.Velocity;
        var angle = velocity.sqrMagnitude > 0.0001f
            ? Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg
            : Mathf.Atan2(pos.y - _lastPos.y, pos.x - _lastPos.x) * Mathf.Rad2Deg;

        var print = new GameObject("BleederPrint")
        {
            transform =
            {
                parent = ShipStatus.Instance.transform,
                position = new Vector3(pos.x, pos.y - 0.3f * Player.GetAppearance().Size.y, pos.z + 0.001f),
                rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward)
            }
        };

        if (ModCompatibility.IsSubmerged())
        {
            print.AddSubmergedComponent("ElevatorMover");
        }

        var sprite = print.AddComponent<SpriteRenderer>();
        sprite.sprite = TouAssets.FootprintSprite.LoadAsset();
        sprite.color = BloodColor;
        print.layer = LayerMask.NameToLayer("Players");
        print.transform.localScale *= new Vector2(1.2f, 1f) * PrintScale;

        Prints.Add(print);
        Coroutines.Start(CoFade(print, sprite, OptionGroupSingleton<BleederOptions>.Instance.PrintDuration));
    }

    private static IEnumerator CoFade(GameObject print, SpriteRenderer sprite, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        const float fadeTime = 1.5f;
        for (var t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            if (print == null || sprite == null)
            {
                Prints.Remove(print!);
                yield break;
            }

            sprite.color = new Color(BloodColor.r, BloodColor.g, BloodColor.b, BloodColor.a * (1f - t / fadeTime));
            yield return null;
        }

        Prints.Remove(print);
        if (print != null)
        {
            print.DestroyImmediate();
        }
    }
}
