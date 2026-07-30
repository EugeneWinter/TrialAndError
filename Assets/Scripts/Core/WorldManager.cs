using UnityEngine;
using Unity.Mathematics;
using System.Collections;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    public GameObject chunkPrefab;
    public BlockDatabase blockDatabase;
    public int renderDistance = 6;
    public int seed = 12345;

    public int worldHeightInChunks = 8;
    public int seaLevel = 64;

    public int chunksPerFrameRecalc = 1;
    public int chunksPerFrameGeneration = 4;

    private Dictionary<int3, ChunkData> chunks = new Dictionary<int3, ChunkData>();
    private Dictionary<int3, ChunkRenderer> renderers = new Dictionary<int3, ChunkRenderer>();

    private Queue<int3> lightRecalcQueue = new Queue<int3>();
    private HashSet<int3> lightRecalcSet = new HashSet<int3>();

    private TerrainGenerator terrain;
    private TreeGenerator trees;

    public bool IsWorldReady { get; private set; }

    void Awake()
    {
        Instance = this;
        terrain = new TerrainGenerator(seed, seaLevel);
        trees = new TreeGenerator(seaLevel);
    }

    public void GenerateWorld()
    {
        StartCoroutine(GenerateWorldAsync());
    }

    IEnumerator GenerateWorldAsync()
    {
        IsWorldReady = false;

        List<int3> allCoords = new List<int3>();
        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
                for (int y = 0; y < worldHeightInChunks; y++)
                    allCoords.Add(new int3(x, y, z));

        int totalChunks = allCoords.Count;
        int generated = 0;

        for (int i = 0; i < allCoords.Count; i++)
        {
            CreateChunkData(allCoords[i]);
            generated++;

            if (generated % chunksPerFrameGeneration == 0)
            {
                if (LoadingScreenUI.Instance != null)
                {
                    float progress = 0.1f + 0.4f * ((float)generated / totalChunks);
                    LoadingScreenUI.Instance.SetStatus($"Generating terrain... {generated}/{totalChunks}", progress);
                }
                yield return null;
            }
        }

        int rendered = 0;
        List<int3> coordList = new List<int3>(chunks.Keys);

        for (int i = 0; i < coordList.Count; i++)
        {
            CreateChunkRenderer(coordList[i]);
            rendered++;

            if (rendered % chunksPerFrameGeneration == 0)
            {
                if (LoadingScreenUI.Instance != null)
                {
                    float progress = 0.5f + 0.3f * ((float)rendered / coordList.Count);
                    LoadingScreenUI.Instance.SetStatus($"Building meshes... {rendered}/{coordList.Count}", progress);
                }
                yield return null;
            }
        }

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.SetStatus("Building water...", 0.82f);
        yield return null;

        if (WaterMeshBuilder.Instance != null)
            WaterMeshBuilder.Instance.BuildWaterForWorld(renderDistance, seaLevel);

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.SetStatus("Spawning items...", 0.88f);
        yield return null;

        if (WorldItemSpawner.Instance != null)
        {
            if (GroundItemManager.Instance != null)
                GroundItemManager.Instance.BeginBatch();

            int spawnedColumns = 0;
            int totalColumns = (renderDistance * 2 + 1) * (renderDistance * 2 + 1);

            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    WorldItemSpawner.Instance.SpawnItemsForChunk(x, z, seed);
                    spawnedColumns++;
                }

                if (spawnedColumns % 20 == 0)
                {
                    if (LoadingScreenUI.Instance != null)
                    {
                        float progress = 0.88f + 0.1f * ((float)spawnedColumns / totalColumns);
                        LoadingScreenUI.Instance.SetStatus($"Spawning items... {spawnedColumns}/{totalColumns}", progress);
                    }
                    yield return null;
                }
            }

            if (GroundItemManager.Instance != null)
                GroundItemManager.Instance.EndBatch();
        }

        IsWorldReady = true;
    }

    void CreateChunkData(int3 coord)
    {
        if (chunks.ContainsKey(coord)) return;

        ChunkData data = new ChunkData();
        data.Initialize(coord);

        System.Random rng = new System.Random(coord.x * 73856093 ^ coord.y * 19349663 ^ coord.z * 83492791 ^ seed);

        terrain.PopulateChunk(ref data, coord, rng);
        trees.GenerateTrees(ref data, coord, rng, terrain);

        chunks.Add(coord, data);
    }

    void CreateChunkRenderer(int3 coord)
    {
        if (renderers.ContainsKey(coord)) return;
        if (!chunks.ContainsKey(coord)) return;

        ChunkData data = chunks[coord];
        GameObject obj = Instantiate(chunkPrefab, new Vector3(coord.x * 32, coord.y * 32, coord.z * 32), Quaternion.identity);
        obj.name = $"Chunk_{coord.x}_{coord.y}_{coord.z}";
        ChunkRenderer r = obj.GetComponent<ChunkRenderer>();
        r.Initialize(data);
        renderers.Add(coord, r);
    }

    public void SetBlock(int x, int y, int z, ushort id)
    {
        int3 coord = new int3(x >> 5, y >> 5, z >> 5);
        if (!chunks.TryGetValue(coord, out ChunkData data)) return;

        int lx = x - (coord.x << 5);
        int ly = y - (coord.y << 5);
        int lz = z - (coord.z << 5);

        data.SetBlock(lx, ly, lz, id);
        chunks[coord] = data;
        renderers[coord].RenderMesh();

        if (lx == 0) RefreshNeighbor(coord + new int3(-1, 0, 0));
        if (lx == 31) RefreshNeighbor(coord + new int3(1, 0, 0));
        if (ly == 0) RefreshNeighbor(coord + new int3(0, -1, 0));
        if (ly == 31) RefreshNeighbor(coord + new int3(0, 1, 0));
        if (lz == 0) RefreshNeighbor(coord + new int3(0, 0, -1));
        if (lz == 31) RefreshNeighbor(coord + new int3(0, 0, 1));
    }

    void RefreshNeighbor(int3 neighborCoord)
    {
        if (renderers.TryGetValue(neighborCoord, out ChunkRenderer r))
            r.RenderMesh();
    }

    public ushort GetBlock(int x, int y, int z)
    {
        int3 coord = new int3(x >> 5, y >> 5, z >> 5);
        if (!chunks.TryGetValue(coord, out ChunkData data)) return BlockIDs.Air;

        int lx = x - (coord.x << 5);
        int ly = y - (coord.y << 5);
        int lz = z - (coord.z << 5);

        return data.GetBlock(lx, ly, lz);
    }

    public bool IsBlockSolid(int x, int y, int z)
    {
        ushort id = GetBlock(x, y, z);
        if (id == BlockIDs.Air) return false;

        var visualData = blockDatabase.GetVisualData();
        if (id < visualData.Length)
            return visualData[id].isSolid;

        return true;
    }

    public bool HasChunk(int3 coord) => chunks.ContainsKey(coord);
    public ChunkData GetChunkData(int3 coord) => chunks[coord];

    public float GetContinentality(int x, int z) => terrain.GetContinentality(x, z);
    public float GetErosion(int x, int z) => terrain.GetErosion(x, z);
    public float GetPeaks(int x, int z) => terrain.GetPeaks(x, z);
    public float GetMoisture(int x, int z) => terrain.GetMoisture(x, z);

    public void RecalculateLightingForAllChunks()
    {
        foreach (var coord in renderers.Keys)
        {
            if (!lightRecalcSet.Contains(coord))
            {
                lightRecalcQueue.Enqueue(coord);
                lightRecalcSet.Add(coord);
            }
        }
    }

    public void RefreshAllChunks()
    {
        blockDatabase.Dispose();
        foreach (var kvp in renderers)
            kvp.Value.RenderMesh();
    }

    void Update()
    {
        int processed = 0;
        while (lightRecalcQueue.Count > 0 && processed < chunksPerFrameRecalc)
        {
            int3 coord = lightRecalcQueue.Dequeue();
            lightRecalcSet.Remove(coord);

            if (renderers.TryGetValue(coord, out ChunkRenderer r))
                r.RenderMesh();

            processed++;
        }
    }

    void OnDestroy()
    {
        foreach (var c in chunks.Values) c.Dispose();
        blockDatabase.Dispose();
    }
}