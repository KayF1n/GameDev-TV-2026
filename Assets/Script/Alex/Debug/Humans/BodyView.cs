using DG.Tweening;
using UnityEngine;

public class BodyView : MonoBehaviour {
    [SerializeField] private MeshRenderer bodyRenderer;

    private Material material;

    [SerializeField] GameObject visualCue;

    public Color CurrentColor => material.color;

    private void Awake() {
        if (bodyRenderer == null)
            bodyRenderer = GetComponent<MeshRenderer>();

        material = bodyRenderer.material;
    }

    public void SetColor(Color color, float duration = 0.3f) {
        material.DOColor(color, duration);
    }

    public void SetAlpha(float alphaAmount = 0, float duration = 0.3f) {
        material.DOFade(alphaAmount, duration);
    }

    public void ToggleVisualCue(bool enabled) {
        if (!visualCue) return;
        visualCue.SetActive(enabled);
    }
}