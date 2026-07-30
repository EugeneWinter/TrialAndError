using UnityEngine;

public class PlayerWater : MonoBehaviour
{
    [Header("Swimming")]
    public float swimSpeed = 3.0f;
    public float swimUpSpeed = 4.0f;
    public float swimDownSpeed = 3.0f;
    public float waterDrag = 6.0f;
    public float waterGravity = 4.0f;
    public float waterBreachThreshold = 0.25f;
    public float waterBreachPeakAboveSurface = 0.5f;
    public float waterBreachRearmDepth = 0.45f;

    private PlayerController master;
    private bool waterBreachConsumed = false;

    public void Init(PlayerController controller)
    {
        master = controller;
    }

    public void CheckWaterState()
    {
        float surfaceY = FindWaterSurfaceY(transform.position);

        if (surfaceY == float.MinValue)
        {
            master.isInWater = false;
            master.isSubmerged = false;
            return;
        }

        float feetY = transform.position.y;
        float headY = transform.position.y + GetComponent<PlayerCamera>().normalCameraY;

        master.isInWater = feetY < surfaceY;
        master.isSubmerged = headY < surfaceY;

        if (master.isInWater)
        {
            float depthBelowSurface = surfaceY - feetY;
            if (depthBelowSurface > waterBreachRearmDepth)
                waterBreachConsumed = false;
        }
    }

    public void HandleSwimming(float dt)
    {
        master.isSprinting = false;
        master.isSneaking = InputManager.Instance.SneakHeld;

        Vector2 move = InputManager.Instance.Move;
        Transform camTrans = GetComponent<PlayerCamera>().cameraTransform;

        float currentSwimSpeed = swimSpeed;
        if (InputManager.Instance.SprintHeld) currentSwimSpeed = swimSpeed * 1.5f;
        else if (master.isSneaking) currentSwimSpeed = swimSpeed * 0.75f;

        Vector3 moveDir = camTrans.forward * move.y + camTrans.right * move.x;
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        Vector3 targetVelocity = moveDir * currentSwimSpeed;

        float surfaceY = FindWaterSurfaceY(transform.position);
        float feetY = transform.position.y;
        float depthBelowSurface = surfaceY == float.MinValue ? 999f : surfaceY - feetY;

        if (InputManager.Instance.JumpHeld)
        {
            bool canBreach = !waterBreachConsumed && surfaceY != float.MinValue &&
                             depthBelowSurface <= waterBreachThreshold && master.velocity.y >= 0f;

            if (canBreach)
            {
                float targetPeakY = surfaceY + waterBreachPeakAboveSurface;
                float deltaHeight = Mathf.Max(0.05f, targetPeakY - feetY);
                master.velocity.y = Mathf.Sqrt(2f * GetComponent<PlayerMovement>().gravity * deltaHeight);
                waterBreachConsumed = true;
                master.currentMoveVelocity = new Vector3(master.velocity.x, 0, master.velocity.z);
                return;
            }

            if (waterBreachConsumed && depthBelowSurface < waterBreachRearmDepth)
                targetVelocity.y = -waterGravity * 0.5f;
            else
                targetVelocity.y = swimUpSpeed;
        }
        else if (master.isSneaking)
        {
            targetVelocity.y = -swimDownSpeed;
        }
        else
        {
            targetVelocity.y = -waterGravity * 0.5f;
        }

        float lerpRate = waterDrag * dt;
        master.velocity = Vector3.Lerp(master.velocity, targetVelocity, lerpRate);
        master.currentMoveVelocity = new Vector3(master.velocity.x, 0, master.velocity.z);
    }

    private float FindWaterSurfaceY(Vector3 pos)
    {
        int px = Mathf.FloorToInt(pos.x);
        int pz = Mathf.FloorToInt(pos.z);
        int startY = Mathf.FloorToInt(pos.y) + 4;

        for (int y = startY; y >= Mathf.Max(0, startY - 12); y--)
        {
            ushort block = WorldManager.Instance.GetBlock(px, y, pz);
            ushort above = WorldManager.Instance.GetBlock(px, y + 1, pz);

            if (block == BlockIDs.Water && above != BlockIDs.Water)
                return y + 1f;
        }
        return float.MinValue;
    }
}