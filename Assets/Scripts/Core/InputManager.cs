using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private GameInput input;

    public Vector2 Move => input.Player.Move.ReadValue<Vector2>();
    public Vector2 Look => input.Player.Look.ReadValue<Vector2>();
    public bool JumpPressed => input.Player.Jump.WasPressedThisFrame();
    public bool AttackPressed => input.Player.Attack.WasPressedThisFrame();
    public bool PlacePressed => input.Player.Place.WasPressedThisFrame();
    public float ScrollHotbar => input.Player.ScrollHotbar.ReadValue<float>();

    void Awake()
    {
        Instance = this;
        input = new GameInput();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
}