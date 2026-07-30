using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Setup")]
    public Transform cameraTransform;
    public Camera playerCamera;
    public float normalCameraY = 1.6f;
    public float sneakCameraY = 1.3f;
    public float cameraLerpSpeed = 10f;

    [Header("FOV")]
    public float normalFOV = 75f;
    public float sprintFOV = 85f;
    public float waterFOV = 70f;
    public float fovLerpSpeed = 8f;

    [Header("Head Bob (Усиленный)")]
    public float walkBobFrequency = 10f;
    public float walkBobAmplitude = 0.08f;
    public float walkBobRoll = 1.5f;

    public float sprintBobFrequency = 14f;
    public float sprintBobAmplitude = 0.14f;
    public float sprintBobRoll = 2.5f;

    public float sneakBobFrequency = 5f;
    public float sneakBobAmplitude = 0.04f;
    public float sneakBobRoll = 0.5f;

    public float waterSurfaceBobSpeed = 2.0f;
    public float waterSurfaceBobAmount = 0.08f;
    public float bobLerpSpeed = 10f;

    public bool JustStepped { get; private set; }

    private PlayerController master;
    private PlayerMovement movement;
    private float pitch = 0.0f;
    private float currentCameraY;
    private float bobTimer = 0f;

    private Vector3 currentBobOffset;
    private float currentBobRoll;
    private bool wasGoingDown = false;

    public void Init(PlayerController controller)
    {
        master = controller;
        movement = GetComponent<PlayerMovement>();
        currentCameraY = normalCameraY;
        if (playerCamera != null) playerCamera.fieldOfView = normalFOV;
    }

    public void HandleLook(float dt)
    {
        Vector2 look = InputManager.Instance.Look;
        bool usingGamepad = InputManager.Instance.IsGamepadActive;
        float sens = usingGamepad ? SettingsManager.Instance.settings.stickSensitivity : SettingsManager.Instance.settings.mouseSensitivity;

        float lookX = usingGamepad ? look.x * sens * dt : look.x * sens;
        float lookY = usingGamepad ? look.y * sens * dt : look.y * sens;

        transform.Rotate(Vector3.up * lookX);
        pitch -= lookY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    public void UpdateCamera(float dt)
    {
        float targetY = master.isSneaking ? sneakCameraY : normalCameraY;
        currentCameraY = Mathf.Lerp(currentCameraY, targetY, cameraLerpSpeed * dt);

        if (master.isInWater)
        {
            bobTimer += dt * waterSurfaceBobSpeed;
            float bobY = Mathf.Sin(bobTimer) * waterSurfaceBobAmount;
            currentBobOffset = Vector3.Lerp(currentBobOffset, new Vector3(0, bobY, 0), bobLerpSpeed * dt);
            currentBobRoll = Mathf.Lerp(currentBobRoll, 0f, bobLerpSpeed * dt);
            JustStepped = false;
        }
        else
        {
            UpdateHeadBob(dt);
        }

        cameraTransform.localPosition = new Vector3(0, currentCameraY, 0) + currentBobOffset;
        cameraTransform.localEulerAngles = new Vector3(pitch, 0, currentBobRoll);

        if (playerCamera != null)
        {
            float targetFOV = normalFOV;
            if (master.isSubmerged) targetFOV = waterFOV;
            else if (master.isSprinting && movement.IsMovingHorizontally()) targetFOV = sprintFOV;

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovLerpSpeed * dt);
        }
    }

    private void UpdateHeadBob(float dt)
    {
        Vector3 targetBob = Vector3.zero;
        float targetRoll = 0f;
        JustStepped = false;

        if (master.isGrounded && movement.IsMovingHorizontally())
        {
            float freq = master.isSprinting ? sprintBobFrequency : (master.isSneaking ? sneakBobFrequency : walkBobFrequency);
            float amp = master.isSprinting ? sprintBobAmplitude : (master.isSneaking ? sneakBobAmplitude : walkBobAmplitude);
            float rollAmp = master.isSprinting ? sprintBobRoll : (master.isSneaking ? sneakBobRoll : walkBobRoll);
            float refSpeed = master.isSprinting ? movement.sprintSpeed : (master.isSneaking ? movement.sneakSpeed : movement.walkSpeed);

            float speedRatio = Mathf.Clamp01(new Vector3(master.velocity.x, 0, master.velocity.z).magnitude / refSpeed);
            bobTimer += dt * freq * speedRatio;

            float bobX = Mathf.Cos(bobTimer * 0.5f) * amp * 0.5f * speedRatio;
            float bobY = Mathf.Sin(bobTimer) * amp * speedRatio;
            targetBob = new Vector3(bobX, bobY, 0);

            targetRoll = Mathf.Cos(bobTimer * 0.5f) * rollAmp * speedRatio;

            bool isGoingDown = Mathf.Cos(bobTimer) < 0f;
            if (wasGoingDown && !isGoingDown) JustStepped = true;
            wasGoingDown = isGoingDown;
        }
        else
        {
            bobTimer = 0f;
            wasGoingDown = false;
        }

        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBob, bobLerpSpeed * dt);
        currentBobRoll = Mathf.Lerp(currentBobRoll, targetRoll, bobLerpSpeed * dt);
    }
}