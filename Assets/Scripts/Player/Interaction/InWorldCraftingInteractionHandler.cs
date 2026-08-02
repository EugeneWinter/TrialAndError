using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class InWorldCraftingInteractionHandler : MonoBehaviour
{
    public float craftProgress = 0f;

    private int3 craftingBlockPos;
    private InWorldRecipeSO currentRecipe;

    public bool Tick(PlayerInteractionContext context)
    {
        if (GroundItemManager.Instance == null || InWorldCraftingManager.Instance == null)
        {
            ResetState();
            return false;
        }

        if (!context.voxelHit.found || !InputManager.Instance.InteractHeld)
        {
            if (craftProgress > 0f)
            {
                Debug.Log($"[Craft] Reset: hit={context.voxelHit.found}, interactHeld={InputManager.Instance.InteractHeld}");
            }
            ResetState();
            return false;
        }

        int3 targetPos = context.voxelHit.blockPos;
        ushort[] itemsOnBlock = GroundItemManager.Instance.GetItemsOnBlock(targetPos);

        if (itemsOnBlock.Length == 0)
        {
            int3 above = targetPos;
            above.y += 1;
            ushort[] itemsAbove = GroundItemManager.Instance.GetItemsOnBlock(above);

            if (itemsAbove.Length > 0)
            {
                targetPos = above;
                itemsOnBlock = itemsAbove;
            }
        }

        if (itemsOnBlock.Length == 0)
        {
            ResetState();
            return false;
        }

        if (!targetPos.Equals(craftingBlockPos))
        {
            craftingBlockPos = targetPos;
            currentRecipe = InWorldCraftingManager.Instance.FindMatchingRecipe(itemsOnBlock);
            craftProgress = 0f;

            string itemList = "";
            foreach (ushort id in itemsOnBlock) itemList += id + " ";
            Debug.Log($"[Craft] Looking at block ({targetPos.x},{targetPos.y},{targetPos.z}), items: [{itemList}], recipe: {(currentRecipe != null ? currentRecipe.recipeName : "NONE")}");
        }

        if (currentRecipe == null)
        {
            craftProgress = 0f;
            return false;
        }

        craftProgress += Time.deltaTime;

        if (Mathf.FloorToInt(craftProgress * 4f) != Mathf.FloorToInt((craftProgress - Time.deltaTime) * 4f))
        {
            Debug.Log($"[Craft] Crafting '{currentRecipe.recipeName}': {craftProgress:F1}/{currentRecipe.craftTime:F1}s");
        }

        if (craftProgress >= currentRecipe.craftTime)
        {
            Debug.Log($"[Craft] COMPLETE: {currentRecipe.recipeName} -> item {currentRecipe.resultItemId}");
            ExecuteCraft(targetPos, currentRecipe);
            ResetState();
        }

        BlockBreakingSystem.Instance.StopBreaking();
        return true;
    }

    void ExecuteCraft(int3 pos, InWorldRecipeSO recipe)
    {
        GroundItemManager.Instance.ClearBlock(pos);

        Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.2f, pos.z + 0.5f);
        ItemSpawner.Instance.SpawnItem(spawnPos, recipe.resultItemId, recipe.resultCount);

        if (AudioManager.Instance != null)
        {
            AudioClip clip = SoundBanks.ItemCraft.GetRandom();
            if (clip != null)
                AudioManager.Instance.PlaySample3D(clip, spawnPos, 0.7f, Random.Range(0.9f, 1.1f));
        }

        if (BlockParticleSystem.Instance != null)
        {
            BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(BlockIDs.Stone);
            if (block != null)
                BlockParticleSystem.Instance.SpawnBreakParticles(spawnPos, block, BlockFace.Top);
        }
    }

    void ResetState()
    {
        craftProgress = 0f;
        currentRecipe = null;
    }
}