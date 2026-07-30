using UnityEngine;
using Unity.Mathematics;

public class PlayerMovement : MonoBehaviour
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

    private PlayerController master;

    private float lastForwardTapTime = -1f;
    private float doubleTapWindow = 0.3f;
    private bool doubleTapSprinting = false;
    private bool wasPressingForward = false;

    public void Init(PlayerController controller)
    {
        master = controller;
    }

    public void HandleMovement(float dt)
    {
        Vector2 move = InputManager.Instance.Move;
        master.isSneaking = InputManager.Instance.SneakHeld;

        bool forwardNow = move.y > 0.5f;

        if (forwardNow && !wasPressingForward)
        {
            if (Time.time - lastForwardTapTime <= doubleTapWindow)
                doubleTapSprinting = true;
            lastForwardTapTime = Time.time;
        }

        if (!forwardNow && doubleTapSprinting)
            doubleTapSprinting = false;

        wasPressingForward = forwardNow;

        master.isSprinting = (InputManager.Instance.SprintHeld || doubleTapSprinting) && !master.isSneaking;

        float currentSpeed = walkSpeed;
        if (master.isSprinting) currentSpeed = sprintSpeed;
        if (master.isSneaking) currentSpeed = sneakSpeed;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 targetMove = (forward * move.y + right * move.x);
        if (targetMove.magnitude > 1) targetMove.Normalize();
        targetMove *= currentSpeed;

        float accelRate = master.isGrounded ? acceleration : acceleration * airControl;
        master.currentMoveVelocity = Vector3.Lerp(master.currentMoveVelocity, targetMove, accelRate * dt);

        master.velocity.x = master.currentMoveVelocity.x;
        master.velocity.z = master.currentMoveVelocity.z;

        if (master.isGrounded && InputManager.Instance.JumpPressed && !master.isSneaking)
        {
            master.velocity.y = jumpHeight;
            master.isGrounded = false;
        }

        master.velocity.y -= gravity * dt;
        if (master.velocity.y < -40.0f) master.velocity.y = -40.0f;
    }

    public void HandleCollisions(float dt)
    {
        if (!master.isGrounded && master.velocity.y < 0f && !master.isInWater)
            master.fallVelocity = master.velocity.y;

        float3 pos = transform.position;
        float3 currentSize = master.CurrentSize;

        pos.x += master.velocity.x * dt;
        if (CheckCollision(pos, currentSize)) { pos.x -= master.velocity.x * dt; master.velocity.x = 0; master.currentMoveVelocity.x = 0; }

        pos.z += master.velocity.z * dt;
        if (CheckCollision(pos, currentSize)) { pos.z -= master.velocity.z * dt; master.velocity.z = 0; master.currentMoveVelocity.z = 0; }

        pos.y += master.velocity.y * dt;
        master.isGrounded = false;
        if (CheckCollision(pos, currentSize))
        {
            if (master.velocity.y < 0) master.isGrounded = true;
            pos.y -= master.velocity.y * dt;
            master.velocity.y = 0;
        }

        transform.position = pos;
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

    public bool IsMovingHorizontally()
    {
        return new Vector3(master.velocity.x, 0, master.velocity.z).magnitude > 0.05f;
    }
}