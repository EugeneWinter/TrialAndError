using UnityEngine;

public class WorldItemSpawner : MonoBehaviour
{
    public static WorldItemSpawner Instance;

    [Header("Prefabs")]
    public GameObject looseStoneModel;
    public GameObject looseStickModel;
    public GameObject looseFiberModel;

    [Header("Item IDs")]
    public ushort stoneItemId = 1001;
    public ushort stickItemId = 1003;
    public ushort fiberItemId = 1004;

    [Header("Spawn Settings")]
    public float stoneChance = 3f;
    public float stickChance = 2f;
    public float fiberChance = 1.5f;

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
                if (surfaceBlock == 0) continue;

                float roll = (float)rng.NextDouble() * 100f;

                if (roll < stoneChance)
                {
                    SpawnWorldItem(worldX, height, worldZ, stoneItemId, looseStoneModel);
                }
                else if (roll < stoneChance + stickChance)
                {
                    bool nearTree = CheckNearTree(worldX, height, worldZ);
                    if (nearTree)
                        SpawnWorldItem(worldX, height, worldZ, stickItemId, looseStickModel);
                }
                else if (roll < stoneChance + stickChance + fiberChance)
                {
                    ushort grassBlock = WorldManager.Instance.GetBlock(worldX, height - 1, worldZ);
                    if (grassBlock == 2)
                        SpawnWorldItem(worldX, height, worldZ, fiberItemId, looseFiberModel);
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
                    if (block == 4) return true;
                }
        return false;
    }

    void SpawnWorldItem(int wx, int wy, int wz, ushort itemId, GameObject model)
    {
        Vector3 pos = new Vector3(wx + 0.5f, wy, wz + 0.5f);

        GameObject obj = new GameObject($"WorldItem_{itemId}");
        obj.transform.position = pos;

        if (model != null)
        {
            GameObject visual = Instantiate(model, obj.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 0.4f;
        }

        WorldItem wi = obj.AddComponent<WorldItem>();
        wi.Setup(itemId, 1, pos);
    }

    int FindSurface(int x, int z)
    {
        for (int y = 60; y >= 0; y--)
        {
            if (WorldManager.Instance.IsBlockSolid(x, y, z))
                return y + 1;
        }
        return -1;
    }
}