using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private GameInput input;

    public Vector2 Move => input.Player.Move.ReadValue<Vector2>();
    public Vector2 Look => input.Player.Look.ReadValue<Vector2>();
    public bool JumpPressed => input.Player.Jump.WasPressedThisFrame();

    public bool AttackHeld => input.Player.Attack.IsPressed();
    public bool AttackPressed => input.Player.Attack.WasPressedThisFrame();
    public bool AttackReleased => input.Player.Attack.WasReleasedThisFrame();

    public bool PlaceHeld => input.Player.Place.IsPressed();
    public bool PlacePressed => input.Player.Place.WasPressedThisFrame();
    public bool PlaceReleased => input.Player.Place.WasReleasedThisFrame();

    public float ScrollHotbar => input.Player.ScrollHotbar.ReadValue<float>();
    public bool JumpHeld => input.Player.Jump.IsPressed();
    public bool SprintHeld => input.Player.Sprint.IsPressed();
    public bool SneakHeld => input.Player.Sneak.IsPressed();
    public bool InteractPressed => input.Player.Interact.WasPressedThisFrame();
    public bool InteractHeld => input.Player.Interact.IsPressed();
    public bool CancelPressed => input.Player.Cancel.WasPressedThisFrame();

    public bool KnappingStrikePressed => input.Player.KnappingStrike.WasPressedThisFrame();
    public bool KnappingStrikeReleased => input.Player.KnappingStrike.WasReleasedThisFrame();
    public bool KnappingStrikeHeld => input.Player.KnappingStrike.IsPressed();

    public float KnappingAngle => input.Player.KnappingAngle.ReadValue<float>();
    public bool KnappingConfirmPressed => input.Player.KnappingConfirm.WasPressedThisFrame();

    public bool IsGamepadActive { get; private set; }

    void Awake()
    {
        Instance = this;
        input = new GameInput();
        InputSystem.onActionChange += OnActionChange;
    }

    void OnDestroy()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        if (obj is InputAction action && action.activeControl != null)
            IsGamepadActive = action.activeControl.device is Gamepad;
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
}