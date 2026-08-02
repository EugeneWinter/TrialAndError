using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(VoxelInteraction))]
public class PlayerInteractionController : MonoBehaviour
{
    public Transform cameraTransform;
    public LayerMask interactableMask;

    public VoxelInteraction voxelInteraction;
    public InWorldCraftingInteractionHandler inWorldCraftingHandler;
    public KnappingInteractionHandler knappingHandler;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (voxelInteraction == null)
            voxelInteraction = GetComponent<VoxelInteraction>();

        if (inWorldCraftingHandler == null)
            inWorldCraftingHandler = GetComponent<InWorldCraftingInteractionHandler>();

        if (knappingHandler == null)
            knappingHandler = GetComponent<KnappingInteractionHandler>();

        if (cameraTransform == null && voxelInteraction != null && voxelInteraction.playerCamera != null)
            cameraTransform = voxelInteraction.playerCamera;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.state != GameState.Playing) return;
        if (voxelInteraction == null) return;
        if (cameraTransform == null) return;

        RaycastResult voxelHit = voxelInteraction.PerformRaycast();

        UpdateBlockHighlight(voxelHit);

        PlayerInteractionContext context = PlayerInteractionContext.Create(
            playerController,
            voxelInteraction,
            cameraTransform,
            voxelHit,
            interactableMask
        );

        if (inWorldCraftingHandler != null && inWorldCraftingHandler.Tick(context))
            return;

        if (knappingHandler != null && knappingHandler.Tick(context))
            return;

        voxelInteraction.HandleStandardInteractions(context);
    }

    void UpdateBlockHighlight(RaycastResult hit)
    {
        if (BlockHighlight.Instance == null) return;

        if (hit.found)
            BlockHighlight.Instance.Show(hit.blockPos);
        else
            BlockHighlight.Instance.Hide();
    }
}