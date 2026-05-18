using Ink.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ChoiceSelector : MonoBehaviour {
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private ChoiceButton choicePrefab;

    [Header("Settings")]
    [SerializeField] private int poolMaxSize = 8;

    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>Fired when the player confirms a choice (Submit input or mouse click).</summary>
    public event Action<Choice> OnChoiceSelected;

    // ── State ─────────────────────────────────────────────────────────────────
    private readonly List<ChoiceButton> _activeButtons = new();
    private ObjectPool<ChoiceButton> _pool;
    private int _selectedIndex = -1;

    public bool IsShowingChoices => _activeButtons.Count > 0;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake() {
        _pool = new ObjectPool<ChoiceButton>(
            createFunc: CreateButton,
            actionOnGet: btn => btn.gameObject.SetActive(true),
            actionOnRelease: btn => {
                btn.Clear();
                btn.gameObject.SetActive(false); 
            },
            actionOnDestroy: btn => Destroy(btn.gameObject),
            collectionCheck: false,
            defaultCapacity: 4,
            maxSize: poolMaxSize
        );
    }

    private void OnDestroy() => _pool.Dispose();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Spawn buttons for every choice and select the first one.</summary>
    public void Show(List<Choice> choices) {
        Hide(); 

        foreach (Choice choice in choices) {
            ChoiceButton btn = _pool.Get();
            btn.transform.SetParent(choiceContainer, false);
            btn.transform.SetAsLastSibling();

            btn.Setup(choice, HandleButtonClick);
            _activeButtons.Add(btn);
        }

        if (_activeButtons.Count > 0)
            SetSelectedIndex(0);
    }

    /// <summary>Return all active buttons to the pool.</summary>
    public void Hide() {
        foreach (ChoiceButton btn in _activeButtons)
            _pool.Release(btn);

        _activeButtons.Clear();
        _selectedIndex = -1;
    }

    /// <summary>
    /// Navigate by <paramref name="delta"/> steps (+1 / -1) with wrap-around.
    /// Call this from DialogueManager's HandleNavigation.
    /// </summary>
    public void Navigate(int delta) {
        if (_activeButtons.Count == 0) return;
        SetSelectedIndex(_selectedIndex + delta);
    }

    /// <summary>Confirm the currently selected choice.</summary>
    public void ConfirmSelection() {
        if (_selectedIndex < 0 || _selectedIndex >= _activeButtons.Count) return;
        _activeButtons[_selectedIndex].Confirm();
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    private void SetSelectedIndex(int index) {
        // Wrap around
        index = ((index % _activeButtons.Count) + _activeButtons.Count) % _activeButtons.Count;

        if (_selectedIndex >= 0 && _selectedIndex < _activeButtons.Count)
            _activeButtons[_selectedIndex].SetSelected(false);

        _selectedIndex = index;
        _activeButtons[_selectedIndex].SetSelected(true);
    }

    private void HandleButtonClick(Choice choice) {
        Hide();
        OnChoiceSelected?.Invoke(choice);
    }

    private ChoiceButton CreateButton() {
        ChoiceButton btn = Instantiate(choicePrefab, choiceContainer);
        btn.gameObject.SetActive(false);
        return btn;
    }
}
