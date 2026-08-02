using UnityEngine;
using Unity.Mathematics;

public class KnappingInteractionHandler : MonoBehaviour
{
    public ushort knappingRequiredItemId = 1001;
    public int knappingRequiredCount = 2;

    public bool Tick(PlayerInteractionContext context)
    {
        if (KnappingSession.Instance == null) return false;
        if (!InputManager.Instance.InteractPressed) return false;
        if (!context.HasSelection) return false;
        if (context.SelectedId != knappingRequiredItemId) return false;
        if (context.SelectedCount < knappingRequiredCount) return false;

        if (HasBlockingWorldInteraction(context))
            return false;

        BlockBreakingSystem.Instance.StopBreaking();
        KnappingSession.Instance.StartSession();
        return true;
    }

    bool HasBlockingWorldInteraction(PlayerInteractionContext context)
    {
        if (HasPhysicsInteractableAhead(context))
            return true;

        if (GroundItemManager.Instance == null)
            return false;

        if (context.voxelHit.found)
        {
            ushort[] itemsOnHitBlock = GroundItemManager.Instance.GetItemsOnBlock(context.voxelHit.blockPos);
            if (itemsOnHitBlock.Length > 0)
                return true;

            int3 above = context.voxelHit.blockPos + new int3(0, 1, 0);
            ushort[] itemsAbove = GroundItemManager.Instance.GetItemsOnBlock(above);
            if (itemsAbove.Length > 0)
                return true;
        }

        return false;
    }

    bool HasPhysicsInteractableAhead(PlayerInteractionContext context)
    {
        if (context.interactableMask.value == 0) return false;
        return Physics.Raycast(context.CameraRay, context.maxReach, context.interactableMask, QueryTriggerInteraction.Ignore);
    }
}