using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.5f;
    public float sprintSpeed = 7.0f;
    public float sneakSpeed = 2.0f;
    public float acceleration = 12.0f;
    public float airControl = 0.3f;

    [Header("Physics")]
    public float gravity = 25.0f;
    public float jumpHeight = 8.0f;
    public float3 size = new float3(0.6f, 1.8f, 0.6f);
    public float3 sneakSize = new float3(0.6f, 1.5f, 0.6f);

    [Header("Camera Sensitivity")]
    public float mouseSensitivity = 0.1f;
    public float stickSensitivity = 200f;

    [Header("Camera")]
    public Transform cameraTransform;
    public Camera playerCamera;
    public float normalCameraY = 1.6f;
    public float sneakCameraY = 1.3f;
    public float cameraLerpSpeed = 10f;

    [Header("FOV")]
    public float normalFOV = 75f;
    public float sprintFOV = 85f;
    public float fovLerpSpeed = 8f;

    [Header("Head Bob")]
    public float walkBobFrequency = 8f;
    public float walkBobAmplitude = 0.05f;
    public float sprintBobFrequency = 12f;
    public float sprintBobAmplitude = 0.08f;
    public float sneakBobFrequency = 4f;
    public float sneakBobAmplitude = 0.02f;
    public float bobLerpSpeed = 10f;

    private float pitch = 0.0f;
    private float3 velocity;
    private Vector3 currentMoveVelocity;
    private bool onGround;
    private bool isSprinting;
    private bool isSneaking;
    private float currentCameraY;
    private float bobTimer = 0f;
    private Vector3 currentBobOffset;
    private bool wasCameraGoingDown = false;
    private bool wasOnGround = false;
    private bool justLanded = false;
    private float landingCooldown = 0f;

    void Start()
    {
        currentCameraY = normalCameraY;
        if (playerCamera != null) playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        if (GameManager.Instance.state != GameState.Playing) return;
        if (KnappingGame.Instance != null && KnappingGame.Instance.JustEnded()) return;

        float dt = Time.deltaTime;
        float3 pos = transform.position;

        HandleLook(dt);
        HandleMovement(dt);
        HandleCollisions(ref pos, dt);
        UpdateCameraHeight(dt);
        UpdateFOV(dt);
        UpdateHeadBob(dt);
        HandleFootsteps(dt);

        transform.position = pos;

        if (landingCooldown > 0f) landingCooldown -= dt;

        if (!wasOnGround && onGround && landingCooldown <= 0f)
        {
            PlayFootstep();
            justLanded = true;
            bobTimer = 0f;
            landingCooldown = 0.2f;
        }
        wasOnGround = onGround;
    }

    void HandleLook(float dt)
    {
        Vector2 look = InputManager.Instance.Look;

        bool usingGamepad = Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0.1f;

        float lookX, lookY;
        if (usingGamepad)
        {
            lookX = look.x * stickSensitivity * dt;
            lookY = look.y * stickSensitivity * dt;
        }
        else
        {
            lookX = look.x * mouseSensitivity;
            lookY = look.y * mouseSensitivity;
        }

        transform.Rotate(Vector3.up * lookX);
        pitch -= lookY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    void HandleMovement(float dt)
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isSneaking;
        isSneaking = Input.GetKey(KeyCode.LeftControl);

        float currentSpeed = walkSpeed;
        if (isSprinting) currentSpeed = sprintSpeed;
        if (isSneaking) currentSpeed = sneakSpeed;

        Vector2 move = InputManager.Instance.Move;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 targetMove = (forward * move.y + right * move.x);
        if (targetMove.magnitude > 1) targetMove.Normalize();
        targetMove *= currentSpeed;

        float accelRate = onGround ? acceleration : acceleration * airControl;
        currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, targetMove, accelRate * dt);

        velocity.x = currentMoveVelocity.x;
        velocity.z = currentMoveVelocity.z;

        if (onGround && InputManager.Instance.JumpPressed && !isSneaking)
        {
            velocity.y = jumpHeight;
            onGround = false;
            PlayFootstep();
        }

        velocity.y -= gravity * dt;
        if (velocity.y < -40.0f) velocity.y = -40.0f;
    }

    void UpdateCameraHeight(float dt)
    {
        float targetY = isSneaking ? sneakCameraY : normalCameraY;
        currentCameraY = Mathf.Lerp(currentCameraY, targetY, cameraLerpSpeed * dt);

        Vector3 basePos = new Vector3(0, currentCameraY, 0);
        cameraTransform.localPosition = basePos + currentBobOffset;
        cameraTransform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    void UpdateFOV(float dt)
    {
        if (playerCamera == null) return;

        float targetFOV = isSprinting && IsMoving() ? sprintFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovLerpSpeed * dt);
    }

    void UpdateHeadBob(float dt)
    {
        Vector3 targetBob = Vector3.zero;

        if (onGround && IsMoving())
        {
            float freq = walkBobFrequency;
            float amp = walkBobAmplitude;

            if (isSprinting) { freq = sprintBobFrequency; amp = sprintBobAmplitude; }
            else if (isSneaking) { freq = sneakBobFrequency; amp = sneakBobAmplitude; }

            bobTimer += dt * freq;

            float bobY = Mathf.Sin(bobTimer) * amp;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * amp * 0.5f;

            targetBob = new Vector3(bobX, bobY, 0);
        }
        else
        {
            bobTimer = 0f;
        }

        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBob, bobLerpSpeed * dt);
    }

    void HandleFootsteps(float dt)
    {
        if (!onGround || !IsMoving())
        {
            wasCameraGoingDown = false;
            return;
        }

        bool isGoingDown = Mathf.Cos(bobTimer) < 0f;

        if (wasCameraGoingDown && !isGoingDown && !justLanded)
        {
            PlayFootstep();
        }

        justLanded = false;
        wasCameraGoingDown = isGoingDown;
    }

    void PlayFootstep()
    {
        if (AudioManager.Instance == null) return;

        int fx = Mathf.FloorToInt(transform.position.x);
        int fy = Mathf.FloorToInt(transform.position.y - 0.1f);
        int fz = Mathf.FloorToInt(transform.position.z);

        ushort blockUnder = WorldManager.Instance.GetBlock(fx, fy, fz);
        if (blockUnder == 0) return;

        SfxPreset footstep = AudioManager.Instance.GetFootstepForBlock(blockUnder);
        AudioManager.Instance.Play3D(footstep, transform.position);
    }

    bool IsMoving()
    {
        Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
        return horizontal.magnitude > 0.5f;
    }

    void HandleCollisions(ref float3 pos, float dt)
    {
        float3 currentSize = isSneaking ? sneakSize : size;

        pos.x += velocity.x * dt;
        if (CheckCollision(pos, currentSize)) { pos.x -= velocity.x * dt; velocity.x = 0; currentMoveVelocity.x = 0; }

        pos.z += velocity.z * dt;
        if (CheckCollision(pos, currentSize)) { pos.z -= velocity.z * dt; velocity.z = 0; currentMoveVelocity.z = 0; }

        pos.y += velocity.y * dt;
        onGround = false;
        if (CheckCollision(pos, currentSize))
        {
            if (velocity.y < 0) onGround = true;
            pos.y -= velocity.y * dt;
            velocity.y = 0;
        }
    }

    bool CheckCollision(float3 pos, float3 checkSize)
    {
        AABB playerBox = AABB.FromPositionSize(pos, checkSize);
        int minX = (int)math.floor(playerBox.min.x);
        int maxX = (int)math.floor(playerBox.max.x);
        int minY = (int)math.floor(playerBox.min.y);
        int maxY = (int)math.floor(playerBox.max.y);
        int minZ = (int)math.floor(playerBox.min.z);
        int maxZ = (int)math.floor(playerBox.max.z);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    AABB blockBox = new AABB(new float3(x, y, z), new float3(x + 1, y + 1, z + 1));
                    if (WorldManager.Instance.IsBlockSolid(x, y, z) && playerBox.Intersects(blockBox))
                        return true;
                }
        return false;
    }
}