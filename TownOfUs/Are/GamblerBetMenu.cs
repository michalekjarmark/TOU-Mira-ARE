using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace TownOfUs.Are;

/// <summary>
///     Bet chips of the Gambler shown on the meeting screen (local player only): one next to every player that can be
///     bet on and one next to Skip Vote (= "no one is ejected"). Built like <see cref="Modules.MeetingMenu" /> (clone of
///     the vote area's cancel button), but it also covers the skip button and keeps the chosen chip visible after betting.
/// </summary>
public sealed class GamblerBetMenu
{
    /// <summary>Player id of <see cref="MeetingHud.SkipVoteButton" /> (set in MeetingHudGetVotesPatch).</summary>
    public const byte SkipId = 253;

    // Same spot as the Doomsayer / Vigilante guess button (the Gambler can't have those).
    private static readonly Vector3 PlayerChipPosition = new(-0.95f, 0.03f, -3f);
    private const float SkipChipGap = 0.35f;

    private static readonly Color IdleColor = Color.white;
    private static readonly Color HoverColor = new(1f, 0.85f, 0.3f);
    private static readonly Color ChosenColor = Color.green;

    private readonly Action<byte> _onBet;
    private readonly Dictionary<byte, GameObject> _chips = [];
    private readonly List<TextMeshPro> _labels = [];

    public GamblerBetMenu(Action<byte> onBet)
    {
        _onBet = onBet;
    }

    public void Show(MeetingHud meeting, Func<PlayerVoteArea, bool> canBetOn, bool allowNoEjection)
    {
        Hide();
        if (meeting == null || meeting.playerStates == null || meeting.playerStates.Length == 0)
        {
            return;
        }

        var template = meeting.playerStates[0].Buttons.transform.Find("CancelButton");
        if (template == null)
        {
            Warning("GamblerBetMenu: no cancel button template found, bet chips not shown");
            return;
        }

        foreach (var area in meeting.playerStates)
        {
            if (area != null && canBetOn(area))
            {
                CreateChip(meeting, template.gameObject, area.transform, PlayerChipPosition, area.PlayerId);
            }
        }

        // A child of the skip button, so it shows and hides together with it (hidden during the discussion time,
        // or always when skipping is disabled in the TOU options).
        var skip = meeting.SkipVoteButton;
        if (allowNoEjection && skip != null)
        {
            CreateChip(meeting, template.gameObject, skip.transform, SkipChipPosition(skip), SkipId);
        }
    }

    /// <summary>
    ///     Text under every chip (the remaining betting time), empty to clear it.
    /// </summary>
    public void SetLabel(string text)
    {
        foreach (var label in _labels)
        {
            if (label)
            {
                label.text = text;
            }
        }
    }

    /// <summary>
    ///     Removes every chip except the chosen one, which turns green and stops reacting to clicks.
    /// </summary>
    public void Lock(byte chosenId)
    {
        foreach (var (id, chip) in _chips.ToArray())
        {
            if (!chip)
            {
                continue;
            }

            if (id != chosenId)
            {
                chip.Destroy();
                _chips.Remove(id);
                continue;
            }

            var renderer = chip.GetComponent<SpriteRenderer>();
            renderer.color = ChosenColor;
            var button = chip.GetComponent<PassiveButton>();
            button.OverrideOnClickListeners(() => { });
            button.OverrideOnMouseOverListeners(() => renderer.color = ChosenColor);
            button.OverrideOnMouseOutListeners(() => renderer.color = ChosenColor);
        }
    }

    public void Hide()
    {
        foreach (var chip in _chips.Values)
        {
            if (chip)
            {
                chip.Destroy();
            }
        }

        _chips.Clear();
        _labels.Clear();
    }

    private void CreateChip(MeetingHud meeting, GameObject template, Transform parent, Vector3 localPosition, byte id)
    {
        var chip = UObject.Instantiate(template, parent);
        chip.name = $"GamblerBet{id}";
        chip.transform.localPosition = localPosition;
        chip.SetActive(true);

        var renderer = chip.GetComponent<SpriteRenderer>();
        renderer.sprite = AreAssets.GamblerBetButton.LoadAsset();
        renderer.color = IdleColor;

        var button = chip.GetComponent<PassiveButton>();
        button.OverrideOnClickListeners(() => _onBet(id));
        button.OverrideOnMouseOverListeners(() => renderer.color = HoverColor);
        button.OverrideOnMouseOutListeners(() => renderer.color = IdleColor);

        var collider = chip.GetComponent<BoxCollider2D>();
        collider.size = renderer.sprite.bounds.size;
        collider.offset = Vector2.zero;

        // The cancel button's "X" glyph.
        if (chip.transform.childCount > 0)
        {
            chip.transform.GetChild(0).gameObject.DeepDestroy();
        }

        // Countdown label under the chip, made like the MeetingMenu ability labels.
        if (meeting.MeetingAbilityButton != null && meeting.MeetingAbilityButton.buttonLabelText != null)
        {
            var labelObject = UObject.Instantiate(meeting.MeetingAbilityButton.buttonLabelText.gameObject, chip.transform);
            labelObject.transform.localPosition = new Vector3(0f, -0.36f, 0f);
            var label = labelObject.GetComponent<TextMeshPro>();
            label.color = Color.white;
            label.text = string.Empty;
            label.fontSize = 2.2f;
            label.fontSizeMax = 2.2f;
            label.fontSizeMin = 2.2f;
            label.m_enableWordWrapping = false;
            _labels.Add(label);
        }

        _chips[id] = chip;
    }

    // Left of the Skip Vote button, whatever its size is in this layout. Uses the sprite sizes, not Renderer.bounds,
    // because the button can still be inactive when the meeting starts.
    private static Vector3 SkipChipPosition(PlayerVoteArea skip)
    {
        var minX = float.MaxValue;
        foreach (var r in skip.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (r.sprite == null || (skip.Buttons != null && r.transform.IsChildOf(skip.Buttons.transform)))
            {
                continue;
            }

            var worldMin = r.transform.TransformPoint(r.sprite.bounds.min);
            minX = Mathf.Min(minX, skip.transform.InverseTransformPoint(worldMin).x);
        }

        return minX < float.MaxValue
            ? new Vector3(minX - SkipChipGap, 0f, -3f)
            : new Vector3(-1.1f, 0f, -3f);
    }
}
