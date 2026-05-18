using UnityEngine;

public class WorldUIElement : MonoBehaviour {
    private Camera mainCamera;

    private void Start() {
        mainCamera = Camera.main;
    }

    private void LateUpdate() {
        if (mainCamera == null) return;

        Vector3 direction = mainCamera.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}