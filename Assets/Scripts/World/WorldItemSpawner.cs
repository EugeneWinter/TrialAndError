using UnityEngine;
using Unity.Mathematics;

public class WorldItemSpawner : MonoBehaviour
{
    public static WorldItemSpawner Instance;

    [Header("Item IDs")]
    public ushort stoneItemId = 1001;
    public ushort stickItemId = 1003;
    public ushort fiberItemId = 1004;

    [Header("Spawn Settings")]
    public float stoneChance = 8f;
    public float stickChance = 5f;
    public float fiberChance = 4f;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnItemsForChunk(int chunkX, int chunkZ, int seed)
    {
        System.Random rng = new System.Random(chunkX * 48611 ^ chunkZ * 96293 ^ seed);

        for (int x = 0; x < 32; x++)
            for (int z = 0; z < 32; z++)
            {
                int worldX = chunkX * 32 + x;
                int worldZ = chunkZ * 32 + z;
                int height = FindSurface(worldX, worldZ);

                if (height <= 0) continue;

                ushort surfaceBlock = WorldManager.Instance.GetBlock(worldX, height - 1, worldZ);
                if (surfaceBlock == BlockIDs.Air || surfaceBlock == BlockIDs.Water) continue;

                float roll = (float)rng.NextDouble() * 100f;
                int3 surfacePos = new int3(worldX, height, worldZ);

                if (roll < stoneChance)
                {
                    int amount = rng.Next(1, 4);
                    for (int i = 0; i < amount; i++)
                        GroundItemManager.Instance.TryPlaceItem(surfacePos, stoneItemId);
                }
                else if (roll < stoneChance + stickChance)
                {
                    if (CheckNearTree(worldX, height, worldZ))
                        GroundItemManager.Instance.TryPlaceItem(surfacePos, stickItemId);
                }
                else if (roll < stoneChance + stickChance + fiberChance)
                {
                    if (surfaceBlock == BlockIDs.Grass)
                        GroundItemManager.Instance.TryPlaceItem(surfacePos, fiberItemId);
                }
            }
    }

    bool CheckNearTree(int wx, int wy, int wz)
    {
        for (int dx = -3; dx <= 3; dx++)
            for (int dz = -3; dz <= 3; dz++)
                for (int dy = 0; dy < 6; dy++)
                {
                    ushort block = WorldManager.Instance.GetBlock(wx + dx, wy + dy, wz + dz);
                    if (block == BlockIDs.Log) return true;
                }
        return false;
    }

    int FindSurface(int x, int z)
    {
        for (int y = WorldManager.Instance.worldHeightInChunks * 32 - 1; y >= 0; y--)
        {
            if (WorldManager.Instance.IsBlockSolid(x, y, z))
                return y + 1;
        }
        return -1;
    }
}