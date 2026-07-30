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
    private int3 currentHitNormal;
    private float digTimer = 0f;
    private float digInterval = 0.35f;

    void Awake() => Instance = this;

    void Start()
    {
        overlayInstance = Instantiate(breakOverlayPrefab);
        overlayInstance.SetActive(false);
        overlayRenderer = overlayInstance.GetComponent<MeshRenderer>();
        overlayMaterial = overlayRenderer.material;
    }

    public void StartBreaking(int3 blockPos, int3 hitNormal)
    {
        if (GameManager.Instance.state != GameState.Playing) { StopBreaking(); return; }
        if (isBreaking && currentBlockPos.Equals(blockPos))
        {
            currentHitNormal = hitNormal;
            return;
        }

        currentBlockPos = blockPos;
        currentHitNormal = hitNormal;
        breakProgress = 0f;
        digTimer = 0f;
        digInterval = 0.35f;

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
        if (tool == null) return false;
        if (tool.toolType != block.requiredTool) return false;
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

        digTimer += Time.deltaTime;
        if (digTimer >= digInterval)
        {
            PlayDigStep();
            digTimer = 0f;
            digInterval = Mathf.Lerp(0.35f, 0.2f, ratio);
        }

        int crackStage = Mathf.FloorToInt(ratio * crackTextures.Length);
        crackStage = Mathf.Clamp(crackStage, 0, crackTextures.Length - 1);
        overlayMaterial.mainTexture = crackTextures[crackStage];

        if (breakProgress >= totalBreakTime)
        {
            CompleteBreaking();
        }
    }

    void PlayDigStep()
    {
        if (AudioManager.Instance == null) return;

        ushort id = WorldManager.Instance.GetBlock(currentBlockPos.x, currentBlockPos.y, currentBlockPos.z);
        if (id == 0) return;

        float progress = breakProgress / totalBreakTime;
        Vector3 p = new Vector3(currentBlockPos.x + 0.5f, currentBlockPos.y + 0.5f, currentBlockPos.z + 0.5f);

        AudioManager.Instance.PlayDigHit(id, p, progress);

        if (BlockParticleSystem.Instance != null)
        {
            BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(id);
            if (block != null)
            {
                Vector3Int normal = new Vector3Int(currentHitNormal.x, currentHitNormal.y, currentHitNormal.z);
                BlockFace face = BlockColorSampler.FaceFromNormal(normal);

                Vector3 hitPoint = p + new Vector3(normal.x, normal.y, normal.z) * 0.5f;
                BlockParticleSystem.Instance.SpawnDigParticles(hitPoint, block, face);
            }
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

        ushort blockId = WorldManager.Instance.GetBlock(currentBlockPos.x, currentBlockPos.y, currentBlockPos.z);
        BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(blockId);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBlockBreak(blockId, blockCenter);
        }

        if (BlockParticleSystem.Instance != null && block != null)
        {
            Vector3Int normal = new Vector3Int(currentHitNormal.x, currentHitNormal.y, currentHitNormal.z);
            BlockFace face = BlockColorSampler.FaceFromNormal(normal);
            BlockParticleSystem.Instance.SpawnBreakParticles(blockCenter, block, face);
        }

        WorldManager.Instance.SetBlock(currentBlockPos.x, currentBlockPos.y, currentBlockPos.z, 0);

        if (block != null && block.dropId != 0)
        {
            ItemSpawner.Instance.SpawnItem(blockCenter, block.dropId);
        }

        StopBreaking();
    }
}