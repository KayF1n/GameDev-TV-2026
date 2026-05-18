public class InputManager : Singleton<InputManager> {
    public InputSystem_Actions InputActions { get; private set; }

    protected override void Awake() {
        base.Awake();
        InputActions = new InputSystem_Actions();
    }

    private void OnEnable() {
        InputActions?.Enable();
    }

    private void OnDisable() {
        InputActions?.Disable();
    }
}
