using UnityEngine;
using Unity.Mathematics;

public class VoxelInteraction : MonoBehaviour
{
    public float maxReach = 5.0f;
    public Transform playerCamera;
    public PlayerController playerController;

    void Update()
    {
        if (GameManager.Instance.state != GameState.Playing) return;

        RaycastResult hit = PerformRaycast();

        if (!hit.found)
        {
            BlockBreakingSystem.Instance.StopBreaking();
            return;
        }

        HandleBreaking(hit);
        HandlePlacing(hit);
    }

    void HandleBreaking(RaycastResult hit)
    {
        if (Input.GetMouseButton(0))
        {
            BlockBreakingSystem.Instance.StartBreaking(hit.blockPos);
            BlockBreakingSystem.Instance.ContinueBreaking();
        }
        else
        {
            BlockBreakingSystem.Instance.StopBreaking();
        }
    }

    void HandlePlacing(RaycastResult hit)
    {
        if (!InputManager.Instance.PlacePressed) return;

        ushort selectedBlock = Inventory.Instance.GetSelectedBlockID();
        if (selectedBlock == 0) return;

        int3 placePos = hit.blockPos + hit.normal;

        AABB blockBox = new AABB(
            new float3(placePos.x, placePos.y, placePos.z),
            new float3(placePos.x + 1, placePos.y + 1, placePos.z + 1));
        AABB playerBox = AABB.FromPositionSize(transform.position, playerController.size);

        if (!playerBox.Intersects(blockBox))
        {
            WorldManager.Instance.SetBlock(placePos.x, placePos.y, placePos.z, selectedBlock);

            if (AudioManager.Instance != null)
            {
                Vector3 blockCenter = new Vector3(placePos.x + 0.5f, placePos.y + 0.5f, placePos.z + 0.5f);
                AudioManager.Instance.PlayFootstep(FootstepAction.Drop, selectedBlock, blockCenter, 1.0f);
            }

            Inventory.Instance.RemoveSelected();
        }
    }

    RaycastResult PerformRaycast()
    {
        float3 origin = playerCamera.position;
        float3 direction = playerCamera.forward;

        int3 blockPos = (int3)math.floor(origin);
        int3 step = new int3(
            direction.x >= 0 ? 1 : -1,
            direction.y >= 0 ? 1 : -1,
            direction.z >= 0 ? 1 : -1);

        float3 tDelta = math.abs(1.0f / direction);
        float3 tMax = new float3(
            direction.x >= 0 ? (math.floor(origin.x) + 1 - origin.x) * tDelta.x : (origin.x - math.floor(origin.x)) * tDelta.x,
            direction.y >= 0 ? (math.floor(origin.y) + 1 - origin.y) * tDelta.y : (origin.y - math.floor(origin.y)) * tDelta.y,
            direction.z >= 0 ? (math.floor(origin.z) + 1 - origin.z) * tDelta.z : (origin.z - math.floor(origin.z)) * tDelta.z);

        int3 hitNormal = int3.zero;

        for (int i = 0; i < 100; i++)
        {
            if (WorldManager.Instance.IsBlockSolid(blockPos.x, blockPos.y, blockPos.z))
                return new RaycastResult { found = true, blockPos = blockPos, normal = hitNormal };

            if (tMax.x < tMax.y)
            {
                if (tMax.x < tMax.z) { blockPos.x += step.x; tMax.x += tDelta.x; hitNormal = new int3(-step.x, 0, 0); }
                else { blockPos.z += step.z; tMax.z += tDelta.z; hitNormal = new int3(0, 0, -step.z); }
            }
            else
            {
                if (tMax.y < tMax.z) { blockPos.y += step.y; tMax.y += tDelta.y; hitNormal = new int3(0, -step.y, 0); }
                else { blockPos.z += step.z; tMax.z += tDelta.z; hitNormal = new int3(0, 0, -step.z); }
            }

            if (math.distance(origin, (float3)blockPos) > maxReach) break;
        }

        return new RaycastResult { found = false };
    }
}