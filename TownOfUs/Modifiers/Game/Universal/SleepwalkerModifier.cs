using MiraAPI.GameOptions;
using TownOfUs.Are;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modifiers.Game.Universal;

/// <summary>
///     ARE universal modifier (negative): after standing still for a while the player starts to walk on their own,
///     slower than normal, in a random direction that turns now and then and after hitting a wall. Any movement
///     input wakes them up. The movement is driven from <see cref="Patches.Modifiers.SleepwalkerMovementPatches"/> on
///     the player's own client (movement is client-authoritative, so others see it as normal walking).
/// </summary>
public sealed class SleepwalkerModifier : UniversalGameModifier, IWikiDiscoverable
{
    private const float WallSpeedFraction = 0.35f; // moving slower than this part of the wanted speed = a wall
    private const float WallTime = 0.15f;          // ...for this long

    private float _idleTime;
    private float _walkTime;
    private float _turnTimer;
    private float _wallTimer;
    private float _lastSpeed;
    private Vector2 _direction = Vector2.right;

    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Sleepwalker,
        TmpSpriteUtils.CreateSpriteAsset(AreAssets.SleepwalkerIcon.LoadAsset(),
            "TouMira.Modifier.Universal.Sleepwalker", 1.45f));

    public override string IdPart => "Sleepwalker";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}", "Sleepwalker");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");
    public override LoadableAsset<Sprite>? ModifierIcon => AreAssets.SleepwalkerIcon;
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public bool Walking { get; private set; }

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription").Replace("<time>", IdleText());
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription").Replace("<time>", IdleText()) +
               MiscUtils.AppendOptionsText(GetType());
    }

    private static string IdleText()
    {
        return Math.Round(OptionGroupSingleton<SleepwalkerOptions>.Instance.IdleTime, 1)
            .ToString(TownOfUsPlugin.Culture);
    }

    public override int GetAssignmentChance()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.SleepwalkerChance;
    }

    public override int GetAmountPerGame()
    {
        return OptionGroupSingleton<UniversalModifierOptions>.Instance.SleepwalkerAmount;
    }

    /// <summary>
    ///     Resets the idle timer and stops sleepwalking (input, task, meeting, vent...).
    /// </summary>
    public void Wake()
    {
        if (Walking)
        {
            Message($"Sleepwalker: {Player?.Data?.PlayerName} woke up after {_walkTime:0.0} s");
        }

        Walking = false;
        _idleTime = 0f;
        _walkTime = 0f;
        _wallTimer = 0f;
        _lastSpeed = 0f;
    }

    /// <summary>
    ///     One physics step on the owner's client.
    /// </summary>
    /// <param name="input">Velocity vanilla wants from the player's input (zero without input).</param>
    /// <param name="actualBefore">Body velocity before this step, after the collisions of the last one.</param>
    /// <param name="speed">The player's current walking speed.</param>
    /// <param name="dt">Physics time step.</param>
    /// <returns>The velocity to use.</returns>
    public Vector2 Step(Vector2 input, Vector2 actualBefore, float speed, float dt)
    {
        if (input.sqrMagnitude > 0.0001f)
        {
            Wake();
            return input;
        }

        var options = OptionGroupSingleton<SleepwalkerOptions>.Instance;
        if (!Walking)
        {
            _idleTime += dt;
            if (_idleTime < options.IdleTime)
            {
                return input;
            }

            Walking = true;
            _walkTime = 0f;
            _direction = Rotate(Vector2.right, Random.Range(0f, 360f));
            _turnTimer = Random.Range(2f, 5f);
            Message($"Sleepwalker: {Player?.Data?.PlayerName} started sleepwalking");
        }

        _walkTime += dt;
        if (options.WalkDuration > 0f && _walkTime >= options.WalkDuration)
        {
            Wake();
            return input;
        }

        if (_lastSpeed > 0.01f && actualBefore.magnitude < _lastSpeed * WallSpeedFraction)
        {
            _wallTimer += dt;
        }
        else
        {
            _wallTimer = 0f;
        }

        if (_wallTimer >= WallTime)
        {
            _direction = Rotate(_direction, Random.Range(100f, 260f));
            _wallTimer = 0f;
            _turnTimer = Random.Range(2f, 5f);
        }

        _turnTimer -= dt;
        if (_turnTimer <= 0f)
        {
            _direction = Rotate(_direction, Random.Range(25f, 70f) * (Random.value < 0.5f ? -1f : 1f));
            _turnTimer = Random.Range(2f, 5f);
        }

        var velocity = _direction * (speed * options.WalkSpeed);
        _lastSpeed = velocity.magnitude;
        return velocity;
    }

    private static Vector2 Rotate(Vector2 vector, float degrees)
    {
        var rad = degrees * Mathf.Deg2Rad;
        var cos = Mathf.Cos(rad);
        var sin = Mathf.Sin(rad);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
    }
}
