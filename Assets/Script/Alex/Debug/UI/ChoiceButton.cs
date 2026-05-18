using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Self-contained choice button with visual selection state.
/// Supports both mouse click and keyboard/gamepad navigation.
/// </summary>
public class ChoiceButton : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Selection Visual")]
    [Tooltip("Any child GameObject used as a highlight — outline, arrow, glow, etc.")]
    [SerializeField] private GameObject selectionIndicator;

    private Choice _choice;
    private System.Action<Choice> _onSelected;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake() {
        button ??= GetComponent<Button>();
        label ??= GetComponentInChildren<TextMeshProUGUI>();
        button.onClick.AddListener(OnClick);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Initialise with Ink choice data before pulling from pool.</summary>
    public void Setup(Choice choice, System.Action<Choice> onSelected) {
        _choice = choice;
        _onSelected = onSelected;
        label.text = choice.text;
        SetSelected(false);
        gameObject.SetActive(true);
    }

    /// <summary>Reset all state before returning to pool.</summary>
    public void Clear() {
        _choice = null;
        _onSelected = null;
        label.text = string.Empty;
        SetSelected(false);
        gameObject.SetActive(false);
    }

    /// <summary>Toggle visual highlight for keyboard / gamepad selection.</summary>
    public void SetSelected(bool selected) {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);

        // Drive Button's built-in colour transition so it mirrors a hovered state
        if (selected) button.OnSelect(null);
        else button.OnDeselect(null);
    }

    /// <summary>Programmatically confirm this button (called by DialogueManager on Submit).</summary>
    public void Confirm() => _onSelected?.Invoke(_choice);

    // ── Internal ──────────────────────────────────────────────────────────────
    private void OnClick() => _onSelected?.Invoke(_choice);
}
