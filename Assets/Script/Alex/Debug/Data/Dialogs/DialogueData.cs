using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Dialogue Node")]
public class DialogueNodeSO : ScriptableObject {
    [Header("Ink Configuration")]
    [Tooltip("knot or stitch in the Ink file to start from.")]
    [SerializeField] private string inkKnotName;
    public string InkKnotName => inkKnotName;
}
