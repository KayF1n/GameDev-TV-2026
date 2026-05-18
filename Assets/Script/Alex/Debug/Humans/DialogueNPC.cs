using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogueNPC : Human {
    [SerializeField] private DialogueNodeSO dialogueData;
    [SerializeField] private BodyView bodyView;
    [SerializeField] private Color lonelyColor, happyColor, questionColor;
    [SerializeField] public bool CanSpeak = true;

    private DialogueTrigger _dTrigger;

    private void Awake() {
        _dTrigger = GetComponentInChildren<DialogueTrigger>();
        if (_dTrigger) _dTrigger.OnDialoguePossible += HandlePossibleDialog;
        bodyView.ToggleVisualCue(false);
    }

    private void HandlePossibleDialog(bool isPossible) {
        if (CanSpeak) bodyView.ToggleVisualCue(isPossible);
    }

    public DialogueNodeSO BeginDialogue() => dialogueData;

    // Called by DialogueManager subscriber (set up in Player.HandleInteraction)
    public void OnDialogueBegin() {
        bodyView.ToggleVisualCue(false);
        CanSpeak = false;
        // Subscribe to events only for the duration of THIS dialogue
        DialogueEvents.OnAnimationTag += HandleAnimationTag;
        DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
    }

    private void OnDialogueEnd() {
        CanSpeak = false; // stays false after talking — change if you want repeat dialogue
        DialogueEvents.OnAnimationTag -= HandleAnimationTag;
        DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
    }

    // Handles only tags that start with this NPC's name
    private void HandleAnimationTag(string tag) {
        int colon = tag.IndexOf(':');
        if (colon < 0) return;

        string owner = tag[..colon];
        string action = tag[(colon + 1)..];

        Color target = action switch {
            "question" => questionColor,
            "color_fade" => lonelyColor,
            "color_green" => happyColor,
            _ => bodyView.CurrentColor
        };
        bodyView.SetColor(target);
    }
}