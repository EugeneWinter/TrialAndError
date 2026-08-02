using UnityEngine;

public struct PlayerInteractionContext
{
    public PlayerController player;
    public VoxelInteraction voxelInteraction;
    public Transform cameraTransform;
    public RaycastResult voxelHit;
    public LayerMask interactableMask;
    public ItemStack selectedStack;
    public float maxReach;

    public ushort SelectedId => selectedStack.id;
    public int SelectedCount => selectedStack.count;
    public bool HasSelection => !selectedStack.IsEmpty;

    public Vector3 CameraPosition => cameraTransform != null ? cameraTransform.position : Vector3.zero;
    public Vector3 CameraForward => cameraTransform != null ? cameraTransform.forward : Vector3.forward;
    public Ray CameraRay => new Ray(CameraPosition, CameraForward);

    public static PlayerInteractionContext Create(
        PlayerController player,
        VoxelInteraction voxelInteraction,
        Transform cameraTransform,
        RaycastResult voxelHit,
        LayerMask interactableMask)
    {
        ItemStack selected = ItemStack.Empty;

        if (Inventory.Instance != null &&
            Inventory.Instance.slots != null &&
            Inventory.Instance.slots.Length > 0)
        {
            selected = Inventory.Instance.slots[Inventory.Instance.selectedSlot];
        }

        return new PlayerInteractionContext
        {
            player = player,
            voxelInteraction = voxelInteraction,
            cameraTransform = cameraTransform,
            voxelHit = voxelHit,
            interactableMask = interactableMask,
            selectedStack = selected,
            maxReach = voxelInteraction != null ? voxelInteraction.maxReach : 0f
        };
    }
}