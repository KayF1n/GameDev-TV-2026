using Ink.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

/// <summary>
/// Drives an Ink story and routes input.
/// All choice UI is delegated to <see cref="ChoiceSelector"/>.
/// </summary>

[RequireComponent(typeof(ChoiceSelector))]
public class DialogueManager : Singleton<DialogueManager> {
    [Header("UI")]
    
    [SerializeField] private ChoiceSelector choiceSelector;
    [SerializeField] private DialogWindowView dialogWindow;
    

    public bool IsDialoguePlaying { get; private set; }

    private Story _story;
    private bool _waitingForInput;
    private InputSystem_Actions.UIActions _uiMap;

    public event Action OnDialogueBegin;
    public event Action OnDialogueEnd;

    [SerializeField] private TextAsset inkJsonAsset;

    private Dictionary<string, Human> speakers = new();

    protected override void Awake() {
        base.Awake();
        choiceSelector ??= GetComponent<ChoiceSelector>();
        if (inkJsonAsset == null) {
            Debug.LogWarning("[DialogueManager] inkJsonAsset is null — aborting.");
            return;
        }
        _story = new Story(inkJsonAsset.text);
        _story.BindExternalFunction("changeHappiness", (string name, int delta) => {
            ChangeHappiness(name, delta);
        });

        _story.BindExternalFunction("addFriend", (string name) => {
            if (npc == null || player == null) return;
            if (name == npc.Name) {
                npc.AddFriend(player);
                player.AddFriend(npc);
            }
        });
    }

    private void Start() {
        ExitDialogueMode();
        _uiMap = InputManager.Instance.InputActions.UI;
        _uiMap.Submit.performed += HandleSubmit;
        _uiMap.Navigate.performed += HandleNavigation;
        choiceSelector.OnChoiceSelected += OnChoiceSelected;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        _uiMap.Submit.performed -= HandleSubmit;
        _uiMap.Navigate.performed -= HandleNavigation;
        choiceSelector.OnChoiceSelected -= OnChoiceSelected;
    }

    private Player player;
    private DialogueNPC npc;
    private string currentSpeaker;

    // ── Public entry point ────────────────────────────────────────────────
    public void EnterDialogueMode(DialogueNodeSO dialogueNodeSO, Player player, DialogueNPC npc) {
        this.player = player;
        this.npc = npc;

        if (IsDialoguePlaying) {
            Debug.LogWarning("[DialogueManager] Dialogue already playing — ignoring.");
            return;
        }
        if (dialogueNodeSO == null) {
            Debug.LogWarning("[DialogueManager] dialogueNodeSO is null — aborting.");
            return;
        }

        _story.ChoosePathString(dialogueNodeSO.InkKnotName);
        // Inject values directly into Ink variables
        _story.variablesState["player_name"] = player.Name;
        _story.variablesState["player_happiness"] = player.Happiness;
        _story.variablesState["npc_name"] = npc.Name;
        _story.variablesState["npc_happiness"] = npc.Happiness;
        _story.variablesState["player_friends_count"] = player.GetFriendCount();

        CacheSpeakerSprite(player);
        CacheSpeakerSprite(npc);

        IsDialoguePlaying = true;
        dialogWindow.ShowDialoguePanel();
        ContinueStory();
        OnDialogueBegin?.Invoke();
    }

    private void CacheSpeakerSprite(Human human) {
        if (human != null && human.personDataSO != null) {
            speakers[human.Name] = human;
        }
    }
    private void ChangeHappiness(string name, int delta) {
        Human toChange = null;
        if (player.Name.Equals(name)) toChange = player;
        if (npc.Name.Equals(name)) toChange = npc;
        if (toChange != null) toChange.ChangeHappiness(delta);
    }

    // ── Input handlers ────────────────────────────────────────────────────
    private void HandleSubmit(InputAction.CallbackContext ctx) {
        if (!IsDialoguePlaying) return;

        if (choiceSelector.IsShowingChoices)
            choiceSelector.ConfirmSelection();
        else if (_waitingForInput) {
            _waitingForInput = false;
            ContinueStory();
        }
    }

    private void HandleNavigation(InputAction.CallbackContext ctx) {
        if (!IsDialoguePlaying || !choiceSelector.IsShowingChoices) return;

        float x = ctx.ReadValue<Vector2>().x;
        if (x > 0.5f) choiceSelector.Navigate(+1);
        else if (x < -0.5f) choiceSelector.Navigate(-1);
    }

    // ── Story loop ────────────────────────────────────────────────────────
    private void ContinueStory() {
        while (_story.canContinue) {
            string line = _story.Continue().Trim();
            string speakerName = "";
            Sprite speakerPortrait = null;

            ProcessEventTags(_story.currentTags);

            if (string.IsNullOrWhiteSpace(line)) continue;

            foreach (var tag in _story.currentTags) {
                int colon = tag.IndexOf(':');

                string key = tag[..colon].Trim();
                string value = tag[(colon + 1)..].Trim();

                if (key.Equals("speaker")) {
                    speakerName = value;

                    if (speakers.TryGetValue(speakerName, out Human human)) {
                        if (human.personDataSO)
                        speakerPortrait = human.personDataSO.Portrait;
                    }
                }
            }

            dialogWindow.ShowLine(line, speakerName, speakerPortrait);

            if (_story.currentChoices.Count > 0)
                choiceSelector.Show(_story.currentChoices);
            else
                _waitingForInput = true;

            return;
        }

        ExitDialogueMode();
    }

    private void ProcessEventTags(List<string> tags) {
        foreach (string tag in tags) {
            int colon = tag.IndexOf(':');
            if (colon < 0) continue;

            string key = tag[..colon].Trim();
            string value = tag[(colon + 1)..].Trim();

            switch (key) {
                case "anim": DialogueEvents.FireAnimationTag(value); break;
                case "sfx": DialogueEvents.FireSfxTag(value); break;
            }
        }
    }

    private void OnChoiceSelected(Choice choice) {
        _story.ChooseChoiceIndex(choice.index);
        ContinueStory();
    }

    private void ExitDialogueMode() {
        IsDialoguePlaying = false;

        _waitingForInput = false;
        dialogWindow.HideDialogPanel();
        choiceSelector.Hide();
        OnDialogueEnd?.Invoke();
    }
}


public static class DialogueEvents {
    public static event Action<string> OnDialogueBegin;
    public static event Action<string> OnDialogueEnd;

    public static event Action<string> OnAnimationTag;
    public static event Action<string> OnSfxTag;
    public static event Action<string> OnSpeakerChanged;
    
    public static void FireAnimationTag(string tag) => OnAnimationTag?.Invoke(tag);
    public static void FireSfxTag(string tag) => OnSfxTag?.Invoke(tag);
    public static void FireSpeakerChanged(string speaker) => OnSpeakerChanged?.Invoke(speaker);
}
