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

    [Header("Swimming")]
    public float swimSpeed = 3.0f;
    public float swimUpSpeed = 4.0f;
    public float swimDownSpeed = 3.0f;
    public float waterDrag = 6.0f;
    public float waterGravity = 4.0f;
    public float waterSurfaceBobSpeed = 2.0f;
    public float waterSurfaceBobAmount = 0.08f;
    public float waterBreachThreshold = 0.25f;
    public float waterBreachPeakAboveSurface = 0.5f;
    public float waterBreachRearmDepth = 0.45f;
    private bool waterBreachConsumed = false;

    [Header("Physics")]
    public float gravity = 25.0f;
    public float jumpHeight = 8.0f;
    public float3 size = new float3(0.6f, 1.8f, 0.6f);
    public float3 sneakSize = new float3(0.6f, 1.5f, 0.6f);

    [Header("Camera")]
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
    private bool isInWater;
    private bool isSubmerged;
    private float currentCameraY;
    private float bobTimer = 0f;
    private Vector3 currentBobOffset;
    private bool wasCameraGoingDown = false;
    private bool wasOnGround = false;
    private bool justLanded = false;
    private float landingCooldown = 0f;
    private float lastFallVelocity = 0f;

    private float lastForwardTapTime = -1f;
    private float doubleTapWindow = 0.3f;
    private bool doubleTapSprinting = false;
    private bool wasPressingForward = false;

    private bool wasInWater = false;

    public bool IsInWater => isInWater;
    public bool IsSubmerged => isSubmerged;

    struct FootBlocks
    {
        public ushort primary;
        public ushort secondary;
        public float blend;
    }

    void Start()
    {
        currentCameraY = normalCameraY;
        if (playerCamera != null) playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        if (GameManager.Instance.state != GameState.Playing) return;

        float dt = Time.deltaTime;
        float3 pos = transform.position;

        CheckWaterState(pos);
        HandleLook(dt);

        if (isInWater)
            HandleSwimming(dt);
        else
            HandleMovement(dt);

        if (!onGround && velocity.y < 0f && !isInWater)
            lastFallVelocity = velocity.y;

        HandleCollisions(ref pos, dt);
        UpdateCameraHeight(dt);
        UpdateFOV(dt);

        if (!isInWater)
        {
            UpdateHeadBob(dt);
            HandleFootsteps(dt);
        }
        else
        {
            UpdateSwimBob(dt);
        }

        transform.position = pos;

        if (landingCooldown > 0f) landingCooldown -= dt;

        if (!wasOnGround && onGround && landingCooldown <= 0f && !isInWater)
        {
            PlayLanding();
            justLanded = true;
            bobTimer = 0f;
            landingCooldown = 0.2f;
        }
        wasOnGround = onGround;

        if (!wasInWater && isInWater)
            OnEnterWater();
        if (wasInWater && !isInWater)
            OnExitWater();
        wasInWater = isInWater;

        if (PlayerVoice.Instance != null)
            PlayerVoice.Instance.OnSprint(dt, isSprinting && IsMoving() && !isInWater);
    }

    void CheckWaterState(float3 pos)
    {
        float surfaceY = FindWaterSurfaceY(pos);

        if (surfaceY == float.MinValue)
        {
            isInWater = false;
            isSubmerged = false;
            return;
        }

        float feetY = pos.y;
        float headY = pos.y + normalCameraY;

        isInWater = feetY < surfaceY;
        isSubmerged = headY < surfaceY;

        if (isInWater)
        {
            float depthBelowSurface = surfaceY - feetY;
            if (depthBelowSurface > waterBreachRearmDepth)
                waterBreachConsumed = false;
        }
    }

    float FindWaterSurfaceY(float3 pos)
    {
        int px = Mathf.FloorToInt(pos.x);
        int pz = Mathf.FloorToInt(pos.z);
        int startY = Mathf.FloorToInt(pos.y) + 4;

        for (int y = startY; y >= Mathf.Max(0, startY - 12); y--)
        {
            ushort block = WorldManager.Instance.GetBlock(px, y, pz);
            ushort above = WorldManager.Instance.GetBlock(px, y + 1, pz);

            if (block == 6 && above != 6)
                return y + 1f;
        }

        return float.MinValue;
    }

    void HandleSwimming(float dt)
    {
        isSprinting = false;
        doubleTapSprinting = false;

        Vector2 move = InputManager.Instance.Move;
        isSneaking = InputManager.Instance.SneakHeld;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        float currentSwimSpeed = swimSpeed;
        if (InputManager.Instance.SprintHeld)
            currentSwimSpeed = swimSpeed * 1.5f;
        else if (isSneaking)
            currentSwimSpeed = swimSpeed * 0.75f;

        Vector3 moveDir = forward * move.y + right * move.x;
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        Vector3 targetVelocity = moveDir * currentSwimSpeed;

        bool jumpHeld = InputManager.Instance.JumpHeld;
        bool sneakHeld = InputManager.Instance.SneakHeld;

        float surfaceY = FindWaterSurfaceY(transform.position);
        float feetY = transform.position.y;
        float depthBelowSurface = surfaceY == float.MinValue ? 999f : surfaceY - feetY;

        if (jumpHeld)
        {
            bool canBreach =
                !waterBreachConsumed &&
                surfaceY != float.MinValue &&
                depthBelowSurface <= waterBreachThreshold &&
                velocity.y >= 0f;

            if (canBreach)
            {
                float targetPeakY = surfaceY + waterBreachPeakAboveSurface;
                float deltaHeight = Mathf.Max(0.05f, targetPeakY - feetY);
                velocity.y = Mathf.Sqrt(2f * gravity * deltaHeight);
                waterBreachConsumed = true;
                currentMoveVelocity = new Vector3(velocity.x, 0, velocity.z);
                return;
            }

            if (waterBreachConsumed && depthBelowSurface < waterBreachRearmDepth)
            {
                targetVelocity.y = -waterGravity * 0.5f;
            }
            else
            {
                targetVelocity.y = swimUpSpeed;
            }
        }
        else if (sneakHeld)
        {
            targetVelocity.y = -swimDownSpeed;
        }
        else
        {
            targetVelocity.y = -waterGravity * 0.5f;
        }

        float lerpRate = waterDrag * dt;
        velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, lerpRate);
        velocity.y = Mathf.Lerp(velocity.y, targetVelocity.y, lerpRate);
        velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, lerpRate);

        currentMoveVelocity = new Vector3(velocity.x, 0, velocity.z);
    }

    float FindWaterSurface()
    {
        int px = Mathf.FloorToInt(transform.position.x);
        int pz = Mathf.FloorToInt(transform.position.z);
        int startY = Mathf.FloorToInt(transform.position.y) + 3;

        for (int y = startY; y >= startY - 10; y--)
        {
            ushort block = WorldManager.Instance.GetBlock(px, y, pz);
            ushort blockAbove = WorldManager.Instance.GetBlock(px, y + 1, pz);

            if (block == 6 && blockAbove != 6)
                return y + 1f;
        }

        return transform.position.y;
    }

    void HandleMovement(float dt)
    {
        Vector2 move = InputManager.Instance.Move;

        isSneaking = InputManager.Instance.SneakHeld;

        bool forwardNow = move.y > 0.5f;

        if (forwardNow && !wasPressingForward)
        {
            float timeSinceLastTap = Time.time - lastForwardTapTime;
            if (timeSinceLastTap <= doubleTapWindow)
                doubleTapSprinting = true;
            lastForwardTapTime = Time.time;
        }

        if (!forwardNow && doubleTapSprinting)
            doubleTapSprinting = false;

        wasPressingForward = forwardNow;

        isSprinting = (InputManager.Instance.SprintHeld || doubleTapSprinting) && !isSneaking;

        float currentSpeed = walkSpeed;
        if (isSprinting) currentSpeed = sprintSpeed;
        if (isSneaking) currentSpeed = sneakSpeed;

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
            PlayJump();
        }

        velocity.y -= gravity * dt;
        if (velocity.y < -40.0f) velocity.y = -40.0f;
    }

    void OnEnterWater()
    {
        velocity.y *= 0.3f;

        if (AudioManager.Instance != null)
        {
            AudioClip clip = SoundBanks.ItemDrop.GetRandom();
            if (clip != null)
                AudioManager.Instance.PlaySample3D(clip, transform.position, 0.7f, 0.7f, 1f, 30f, 0.8f);
        }
    }

    void OnExitWater()
    {
        if (AudioManager.Instance != null)
        {
            AudioClip clip = SoundBanks.ItemPickup.GetRandom();
            if (clip != null)
                AudioManager.Instance.PlaySample3D(clip, transform.position, 0.5f, 1.2f, 1f, 20f, 0.8f);
        }
    }

    void UpdateSwimBob(float dt)
    {
        bobTimer += dt * waterSurfaceBobSpeed;
        float bobY = Mathf.Sin(bobTimer) * waterSurfaceBobAmount;
        currentBobOffset = Vector3.Lerp(currentBobOffset, new Vector3(0, bobY, 0), bobLerpSpeed * dt);
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

        float targetFOV = normalFOV;
        if (isSubmerged) targetFOV = waterFOV;
        else if (isSprinting && IsMoving()) targetFOV = sprintFOV;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovLerpSpeed * dt);
    }

    void HandleLook(float dt)
    {
        Vector2 look = InputManager.Instance.Look;
        bool usingGamepad = InputManager.Instance.IsGamepadActive;

        float sens = usingGamepad
            ? SettingsManager.Instance.settings.stickSensitivity
            : SettingsManager.Instance.settings.mouseSensitivity;

        float lookX, lookY;
        if (usingGamepad)
        {
            lookX = look.x * sens * dt;
            lookY = look.y * sens * dt;
        }
        else
        {
            lookX = look.x * sens;
            lookY = look.y * sens;
        }

        transform.Rotate(Vector3.up * lookX);
        pitch -= lookY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    void UpdateHeadBob(float dt)
    {
        Vector3 targetBob = Vector3.zero;

        if (onGround && IsMoving())
        {
            float freq = walkBobFrequency;
            float amp = walkBobAmplitude;
            float refSpeed = walkSpeed;

            if (isSprinting) { freq = sprintBobFrequency; amp = sprintBobAmplitude; refSpeed = sprintSpeed; }
            else if (isSneaking) { freq = sneakBobFrequency; amp = sneakBobAmplitude; refSpeed = sneakSpeed; }

            Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
            float speedRatio = Mathf.Clamp01(horizontal.magnitude / refSpeed);

            bobTimer += dt * freq * speedRatio;

            float bobY = Mathf.Sin(bobTimer) * amp * speedRatio;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * amp * 0.5f * speedRatio;

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
            PlayFootstep();

        justLanded = false;
        wasCameraGoingDown = isGoingDown;
    }

    void PlayFootstep()
    {
        if (AudioManager.Instance == null) return;

        FootBlocks blocks = FindBlocksUnderFeet();
        if (blocks.primary == 0) return;

        FootstepAction action = FootstepAction.Walk;
        if (isSprinting) action = FootstepAction.Run;
        else if (isSneaking) action = FootstepAction.Sneak;

        float stepVel = isSprinting ? 1.4f : (isSneaking ? 0.5f : 1.0f);

        AudioManager.Instance.PlayFootstepBlended(
            action, blocks.primary, blocks.secondary, blocks.blend,
            transform.position, stepVel);
    }

    void PlayJump()
    {
        if (AudioManager.Instance == null) return;

        FootBlocks blocks = FindBlocksUnderFeet();
        if (blocks.primary == 0) return;

        AudioManager.Instance.PlayFootstep(FootstepAction.Jump, blocks.primary, transform.position, 1.2f);

        if (PlayerVoice.Instance != null)
            PlayerVoice.Instance.OnJump();
    }

    void PlayLanding()
    {
        if (AudioManager.Instance == null) return;

        FootBlocks blocks = FindBlocksUnderFeet();
        if (blocks.primary == 0) return;

        float landingVel = Mathf.Clamp(Mathf.Abs(lastFallVelocity) / 10f + 1.0f, 1.0f, 1.6f);
        AudioManager.Instance.PlayFootstep(FootstepAction.Drop, blocks.primary, transform.position, landingVel);

        if (PlayerVoice.Instance != null)
            PlayerVoice.Instance.OnLanding(lastFallVelocity);

        lastFallVelocity = 0f;
    }

    FootBlocks FindBlocksUnderFeet()
    {
        FootBlocks result = new FootBlocks();

        float yBelow = transform.position.y - 0.1f;
        int fy = Mathf.FloorToInt(yBelow);

        float x = transform.position.x;
        float z = transform.position.z;

        int cx = Mathf.FloorToInt(x);
        int cz = Mathf.FloorToInt(z);

        ushort centerBlock = WorldManager.Instance.GetBlock(cx, fy, cz);

        float fracX = x - cx;
        float fracZ = z - cz;

        float distToEdgeX = Mathf.Min(fracX, 1f - fracX);
        float distToEdgeZ = Mathf.Min(fracZ, 1f - fracZ);
        float distToEdge = Mathf.Min(distToEdgeX, distToEdgeZ);

        float edgeThreshold = 0.35f;
        float blendFactor = 0f;
        int dx = 0, dz = 0;

        if (distToEdge < edgeThreshold)
        {
            blendFactor = 1f - (distToEdge / edgeThreshold);

            if (distToEdgeX < distToEdgeZ)
                dx = fracX < 0.5f ? -1 : 1;
            else
                dz = fracZ < 0.5f ? -1 : 1;
        }

        ushort neighborBlock = (dx != 0 || dz != 0)
            ? WorldManager.Instance.GetBlock(cx + dx, fy, cz + dz)
            : (ushort)0;

        if (centerBlock != 0 && centerBlock != 6)
        {
            result.primary = centerBlock;
            result.secondary = (neighborBlock != 6) ? neighborBlock : (ushort)0;
            result.blend = blendFactor;
        }
        else if (neighborBlock != 0 && neighborBlock != 6)
        {
            result.primary = neighborBlock;
            result.secondary = 0;
            result.blend = 0f;
        }

        return result;
    }

    bool IsMoving()
    {
        Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
        return horizontal.magnitude > 0.05f;
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