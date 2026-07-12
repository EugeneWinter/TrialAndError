using UnityEngine;
using Unity.Mathematics;
using System.Linq;

public class BlockBreakingSystem : MonoBehaviour
{
    public static BlockBreakingSystem Instance;

    public GameObject breakOverlayPrefab;
    public Texture2D[] crackTextures;

    private int3 currentBlockPos;
    private bool isBreaking = false;
    private float breakProgress = 0f;
    private float totalBreakTime = 1f;

    private GameObject overlayInstance;
    private MeshRenderer overlayRenderer;
    private Material overlayMaterial;

    void Awake() => Instance = this;

    void Start()
    {
        overlayInstance = Instantiate(breakOverlayPrefab);
        overlayInstance.SetActive(false);
        overlayRenderer = overlayInstance.GetComponent<MeshRenderer>();
        overlayMaterial = overlayRenderer.material;
    }

    public void StartBreaking(int3 blockPos)
    {
        if (GameManager.Instance.state != GameState.Playing) { StopBreaking(); return; }
        if (isBreaking && currentBlockPos.Equals(blockPos)) return;

        currentBlockPos = blockPos;
        breakProgress = 0f;

        ushort blockId = WorldManager.Instance.GetBlock(blockPos.x, blockPos.y, blockPos.z);
        BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(blockId);

        if (block == null || !block.breakable)
        {
            StopBreaking();
            return;
        }

        ItemSO tool = Inventory.Instance.GetSelectedTool();

        if (!CanBreak(block, tool))
        {
            StopBreaking();
            return;
        }

        totalBreakTime = CalculateBreakTime(block, tool);
        isBreaking = true;

        overlayInstance.transform.position = new Vector3(blockPos.x + 0.5f, blockPos.y + 0.5f, blockPos.z + 0.5f);
        overlayInstance.SetActive(true);
    }

    private bool CanBreak(BlockSO block, ItemSO tool)
    {
        if (block.requiredTool == ToolType.None) return true;
        if (tool == null) return block.requiredTier == 0;
        if (tool.toolType != block.requiredTool) return block.requiredTier == 0;
        return tool.toolTier >= block.requiredTier;
    }

    private float CalculateBreakTime(BlockSO block, ItemSO tool)
    {
        float baseTime = block.hardness;

        if (tool == null) return baseTime * 3f;

        bool effective = tool.effectiveOn != null && tool.effectiveOn.Contains(block.category);
        bool correctType = tool.toolType == block.requiredTool;

        if (effective && correctType)
            return baseTime / tool.miningSpeedMultiplier;

        return baseTime * 5f;
    }

    public void ContinueBreaking()
    {
        if (GameManager.Instance.state != GameState.Playing) { StopBreaking(); return; }
        if (!isBreaking) return;

        breakProgress += Time.deltaTime;
        float ratio = breakProgress / totalBreakTime;

        int crackStage = Mathf.FloorToInt(ratio * crackTextures.Length);
        crackStage = Mathf.Clamp(crackStage, 0, crackTextures.Length - 1);
        overlayMaterial.mainTexture = crackTextures[crackStage];

        if (breakProgress >= totalBreakTime)
        {
            CompleteBreaking();
        }
    }

    public void StopBreaking()
    {
        isBreaking = false;
        breakProgress = 0f;
        if (overlayInstance != null) overlayInstance.SetActive(false);
    }

    private void CompleteBreaking()
    {
        Vector3 blockCenter = new Vector3(currentBlockPos.x + 0.5f, currentBlockPos.y + 0.5f, currentBlockPos.z + 0.5f);

        if (AudioManager.Instance != null)
            AudioManager.Instance.Play3D(AudioManager.Instance.blockBreak, blockCenter);

        ushort blockId = WorldManager.Instance.GetBlock(currentBlockPos.x, currentBlockPos.y, currentBlockPos.z);
        BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(blockId);

        WorldManager.Instance.SetBlock(currentBlockPos.x, currentBlockPos.y, currentBlockPos.z, 0);

        if (block != null && block.dropId != 0)
        {
            ItemSpawner.Instance.SpawnItem(
                new Vector3(currentBlockPos.x + 0.5f, currentBlockPos.y + 0.5f, currentBlockPos.z + 0.5f),
                block.dropId);
        }

        StopBreaking();
    }
}