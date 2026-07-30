using UnityEngine;

public class KnappingCameraController : MonoBehaviour
{
    [Header("Rotation")]
    public float mouseRotateSpeed = 1.5f;
    public float stickRotateSpeed = 60f;
    public float damping = 12f;

    [Header("Cursor")]
    public float cursorSpeed = 600f;

    [HideInInspector] public Vector3 virtualCursorScreenPos;

    private KnappingSession session;
    private Transform pivot;
    private Vector3 stoneRotation;
    private Vector2 rotationVelocity;
    private bool initialized = false;

    public void Begin(Transform stonePivot)
    {
        session = KnappingSession.Instance;
        pivot = stonePivot;
        stoneRotation = new Vector3(-25f, 45f, 15f);
        rotationVelocity = Vector2.zero;
        pivot.localEulerAngles = stoneRotation;
        virtualCursorScreenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        initialized = true;
    }

    public void Tick()
    {
        if (!initialized || pivot == null) return;

        float dt = Time.deltaTime;
        bool useGamepad = InputManager.Instance.IsGamepadActive;

        if (useGamepad)
        {
            Vector2 rightStick = InputManager.Instance.Look;
            if (rightStick.magnitude > 0.15f)
            {
                rotationVelocity.x = rightStick.x * stickRotateSpeed * dt;
                rotationVelocity.y = -rightStick.y * stickRotateSpeed * dt;
            }

            Vector2 leftStick = InputManager.Instance.Move;
            virtualCursorScreenPos.x += leftStick.x * cursorSpeed * dt;
            virtualCursorScreenPos.y += leftStick.y * cursorSpeed * dt;
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                Vector2 delta = InputManager.Instance.Look;
                rotationVelocity.x = delta.x * mouseRotateSpeed;
                rotationVelocity.y = -delta.y * mouseRotateSpeed;
            }

            virtualCursorScreenPos = Input.mousePosition;
        }

        virtualCursorScreenPos.x = Mathf.Clamp(virtualCursorScreenPos.x, 0, Screen.width);
        virtualCursorScreenPos.y = Mathf.Clamp(virtualCursorScreenPos.y, 0, Screen.height);

        stoneRotation.y += rotationVelocity.x;
        stoneRotation.x += rotationVelocity.y;
        stoneRotation.x = Mathf.Clamp(stoneRotation.x, -80f, 80f);

        rotationVelocity = Vector2.Lerp(rotationVelocity, Vector2.zero, damping * dt);

        pivot.localEulerAngles = stoneRotation;
    }

    public Ray GetCursorRay()
    {
        if (session == null || session.knappingCamera == null)
            return new Ray(Vector3.zero, Vector3.forward);

        return session.knappingCamera.ScreenPointToRay(virtualCursorScreenPos);
    }
}