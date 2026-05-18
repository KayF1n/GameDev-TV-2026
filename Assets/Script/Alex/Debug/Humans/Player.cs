using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Human {
    

    private InputSystem_Actions.PlayerActions player;
    private InputAction _interactAction;
    [SerializeField] PlayerDialogSystem dialogSystem;

    private void Start() {
        player = InputManager.Instance.InputActions.Player;

        _interactAction = player.Interact;
        _interactAction.performed += OnInteract;
    }

    private void OnDestroy() {
        _interactAction.performed -= OnInteract;
    }

    

    private void OnInteract(InputAction.CallbackContext ctx) {
        HandleInteraction();
    }

    private void HandleInteraction() {
        if (DialogueManager.Instance.IsDialoguePlaying) return;

        DialogueNPC[] npcs = dialogSystem.FindDialogueNPC();
        if (npcs.Length == 0) return;

        DialogueNPC npc = npcs[0];
        npc.OnDialogueBegin(); // NPC підписується на свої події
        DialogueManager.Instance.EnterDialogueMode(npc.BeginDialogue(), this, npc);
    }
}