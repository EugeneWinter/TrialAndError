using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class VoxelInteraction : MonoBehaviour
{
    public float maxReach = 5.0f;
    public Transform playerCamera;
    public PlayerController playerController;

    public void HandleStandardInteractions(PlayerInteractionContext context)
    {
        RaycastResult hit = context.voxelHit;

        if (InputManager.Instance.AttackPressed)
        {
            if (TryPickupGroundItem(hit))
                return;
        }

        if (!hit.found)
        {
            BlockBreakingSystem.Instance.StopBreaking();
            return;
        }

        if (InputManager.Instance.PlacePressed)
        {
            if (TryPlaceGroundItem(hit))
                return;
        }

        HandleBreaking(hit);
        HandlePlacing(hit);
    }

    bool TryPickupGroundItem(RaycastResult hit)
    {
        if (GroundItemManager.Instance == null) return false;
        if (ItemSpawner.Instance == null) return false;

        float3 origin = playerCamera.position;
        float3 dir = playerCamera.forward;

        for (int i = 0; i < Mathf.CeilToInt(maxReach * 4f); i++)
        {
            float t = i * 0.25f;
            if (t > maxReach) break;

            float3 point = origin + dir * t;
            int3 checkPos = (int3)math.floor(point);

            ushort blockAtPos = WorldManager.Instance.GetBlock(checkPos.x, checkPos.y, checkPos.z);
            if (blockAtPos != BlockIDs.Air && blockAtPos != BlockIDs.Water)
                return false;

            ushort[] items = GroundItemManager.Instance.GetItemsOnBlock(checkPos);
            if (items.Length > 0)
            {
                ushort pickedItem = GroundItemManager.Instance.TryTakeItem(checkPos);
                if (pickedItem == 0) return false;

                Vector3 spawnPos = new Vector3(checkPos.x + 0.5f, checkPos.y + 0.3f, checkPos.z + 0.5f);
                ItemSpawner.Instance.SpawnItem(spawnPos, pickedItem, 1);

                PlayGroundItemSound(spawnPos, pickedItem, true);

                BlockBreakingSystem.Instance.StopBreaking();
                return true;
            }
        }

        return false;
    }

    bool TryPlaceGroundItem(RaycastResult hit)
    {
        if (!hit.found) return false;
        if (GroundItemManager.Instance == null) return false;

        ushort selectedItem = Inventory.Instance.slots[Inventory.Instance.selectedSlot].id;
        if (selectedItem == 0) return false;
        if (selectedItem < 1000) return false;

        int3 placePos = hit.blockPos + new int3(0, 1, 0);

        if (hit.normal.y > 0)
            placePos = hit.blockPos + hit.normal;

        if (!WorldManager.Instance.IsBlockSolid(placePos.x, placePos.y - 1, placePos.z))
            return false;

        ushort blockAtPlace = WorldManager.Instance.GetBlock(placePos.x, placePos.y, placePos.z);
        if (blockAtPlace != BlockIDs.Air && blockAtPlace != BlockIDs.Water)
            return false;

        if (GroundItemManager.Instance.TryPlaceItem(placePos, selectedItem))
        {
            Inventory.Instance.RemoveSelected(1);

            Vector3 soundPos = new Vector3(placePos.x + 0.5f, placePos.y + 0.5f, placePos.z + 0.5f);
            PlayGroundItemSound(soundPos, selectedItem, false);

            return true;
        }

        return false;
    }

    void HandleBreaking(RaycastResult hit)
    {
        if (InputManager.Instance.AttackHeld)
        {
            BlockBreakingSystem.Instance.StartBreaking(hit.blockPos, hit.normal);
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
        if (selectedBlock >= 1000) return;

        int3 placePos = hit.blockPos + hit.normal;

        int3 groundCheckPos = placePos;
        if (GroundItemManager.Instance != null)
        {
            ushort[] itemsHere = GroundItemManager.Instance.GetItemsOnBlock(groundCheckPos);
            if (itemsHere.Length > 0) return;
        }

        AABB blockBox = new AABB(
            new float3(placePos.x, placePos.y, placePos.z),
            new float3(placePos.x + 1, placePos.y + 1, placePos.z + 1));
        AABB playerBox = AABB.FromPositionSize(transform.position, playerController.size);

        if (!playerBox.Intersects(blockBox))
        {
            WorldManager.Instance.SetBlock(placePos.x, placePos.y, placePos.z, selectedBlock);

            if (BlockParticleSystem.Instance != null)
            {
                BlockSO placedBlock = WorldManager.Instance.blockDatabase.GetBlockSO(selectedBlock);
                if (placedBlock != null)
                {
                    Vector3 placeCenter = new Vector3(placePos.x + 0.5f, placePos.y + 0.5f, placePos.z + 0.5f);
                    BlockParticleSystem.Instance.SpawnPlaceParticles(placeCenter, placedBlock);
                }
            }

            if (AudioManager.Instance != null)
            {
                Vector3 blockCenter = new Vector3(placePos.x + 0.5f, placePos.y + 0.5f, placePos.z + 0.5f);
                AudioManager.Instance.PlayFootstep(FootstepAction.Drop, selectedBlock, blockCenter, 1.0f);
            }

            Inventory.Instance.RemoveSelected();
        }
    }

    public RaycastResult PerformRaycast()
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
            ushort blockId = WorldManager.Instance.GetBlock(blockPos.x, blockPos.y, blockPos.z);

            if (blockId != BlockIDs.Air && blockId != BlockIDs.Water)
                return new RaycastResult { found = true, blockPos = blockPos, normal = hitNormal };

            if (tMax.x < tMax.y)
            {
                if (tMax.x < tMax.z)
                {
                    blockPos.x += step.x;
                    tMax.x += tDelta.x;
                    hitNormal = new int3(-step.x, 0, 0);
                }
                else
                {
                    blockPos.z += step.z;
                    tMax.z += tDelta.z;
                    hitNormal = new int3(0, 0, -step.z);
                }
            }
            else
            {
                if (tMax.y < tMax.z)
                {
                    blockPos.y += step.y;
                    tMax.y += tDelta.y;
                    hitNormal = new int3(0, -step.y, 0);
                }
                else
                {
                    blockPos.z += step.z;
                    tMax.z += tDelta.z;
                    hitNormal = new int3(0, 0, -step.z);
                }
            }

            if (math.distance(origin, (float3)blockPos) > maxReach) break;
        }

        return new RaycastResult { found = false };
    }

    void PlayGroundItemSound(Vector3 position, ushort itemId, bool isPickup)
    {
        if (AudioManager.Instance == null) return;

        FootstepMaterial mat = GetSoundMaterialForItem(itemId);

        float pitch = isPickup
            ? Random.Range(1.1f, 1.3f)
            : Random.Range(0.85f, 1.0f);

        float volume = isPickup ? 0.5f : 0.6f;

        AudioManager.Instance.PlayFootstep(
            isPickup ? FootstepAction.Jump : FootstepAction.Drop,
            GetSoundBlockId(mat),
            position,
            volume
        );
    }

    FootstepMaterial GetSoundMaterialForItem(ushort itemId)
    {
        switch (itemId)
        {
            case 1001: return FootstepMaterial.Stone;
            case 1002: return FootstepMaterial.Stone;
            case 1003: return FootstepMaterial.Wood;
            case 1004: return FootstepMaterial.Grass;
            default: return FootstepMaterial.Stone;
        }
    }

    ushort GetSoundBlockId(FootstepMaterial mat)
    {
        switch (mat)
        {
            case FootstepMaterial.Stone: return BlockIDs.Stone;
            case FootstepMaterial.Wood: return BlockIDs.Log;
            case FootstepMaterial.Grass: return BlockIDs.Grass;
            case FootstepMaterial.Dirt: return BlockIDs.Dirt;
            default: return BlockIDs.Stone;
        }
    }
}