using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogWindowView  : MonoBehaviour {
    [Header("General")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Name")]
    [SerializeField] private GameObject nameObject;
    [SerializeField] private TextMeshProUGUI speakerNameText;

    [Header ("Portrait")]
    [SerializeField] private GameObject portraitObject;
    [SerializeField] private Image speakerPortraitImage;
    [Header("Dialog")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    public void ShowLine(string text, string name = "", Sprite portrait = null) {
        if (string.IsNullOrEmpty(name)) {
            nameObject.SetActive(false);
        } else {
            nameObject.SetActive(true);
            speakerNameText.text = name;
        }

        if (string.IsNullOrEmpty(name)) {
            portraitObject.SetActive(false);
        } else {
            portraitObject.SetActive(true);
            speakerPortraitImage.sprite = portrait;
        }

        dialogueText.text = text;
    }

    public void ShowDialoguePanel () {
        dialoguePanel.SetActive(true);
    }

    public void HideDialogPanel() {
        dialoguePanel.SetActive(false);
        dialogueText.text = string.Empty;
    }
}
