using UnityEngine;
using Unity.Mathematics;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    public BlockDatabase blockDatabase;
    public GameObject chunkPrefab;
    public int renderDistance = 8;
    public int seed = 12345;
    public int seaLevel = 64;
    public int worldHeightInChunks = 16;
    public int columnsPerFrame = 1;
    public int startupRadius = 3;

    public bool IsWorldReady { get; private set; }

    private Dictionary<int2, ChunkColumn> columns = new Dictionary<int2, ChunkColumn>();
    private Dictionary<int3, ChunkRenderer> renderers = new Dictionary<int3, ChunkRenderer>();
    private Transform playerTransform;

    private List<int2> streamingQueue = new List<int2>();
    private Queue<int3> meshQueue = new Queue<int3>();
    private int2 lastPlayerColumn = new int2(int.MaxValue, int.MaxValue);

    void Awake()
    {
        Instance = this;
    }

    public void GenerateWorld()
    {
        StartCoroutine(GenerateWorldAsync());
    }

    private IEnumerator GenerateWorldAsync()
    {
        IsWorldReady = false;
        int radius = Mathf.Min(startupRadius, renderDistance);

        List<int2> startCols = new List<int2>();
        for (int x = -radius; x <= radius; x++)
            for (int z = -radius; z <= radius; z++)
                if (x * x + z * z <= radius * radius)
                    startCols.Add(new int2(x, z));

        int total = startCols.Count;
        int done = 0;

        foreach (var pos in startCols)
        {
            GenerateColumnData(pos);
            done++;
            if (done % 4 == 0)
            {
                if (LoadingScreenUI.Instance != null)
                    LoadingScreenUI.Instance.SetStatus($"Generating terrain... {done}/{total}",
                        0.1f + 0.5f * ((float)done / total));
                yield return null;
            }
        }

        done = 0;
        foreach (var pos in startCols)
        {
            CreateColumnRenderers(pos);
            done++;
            if (done % 2 == 0)
            {
                if (LoadingScreenUI.Instance != null)
                    LoadingScreenUI.Instance.SetStatus($"Building meshes... {done}/{total}",
                        0.6f + 0.3f * ((float)done / total));
                yield return null;
            }
        }

        if (WaterMeshBuilder.Instance != null)
        {
            foreach (var pos in startCols)
                WaterMeshBuilder.Instance.RebuildWaterChunkColumn(pos.x, pos.y);
        }

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.SetStatus("Ready!", 1f);

        IsWorldReady = true;
    }

    private void GenerateColumnData(int2 pos)
    {
        if (columns.ContainsKey(pos)) return;

        ChunkColumn col = new ChunkColumn(pos);
        columns.Add(pos, col);

        var job = new TerrainGenerationJob
        {
            ColumnPos = pos,
            Seed = seed,
            SeaLevel = seaLevel,
            Blocks = col.Blocks,
            Heightmap = col.Heightmap
        };

        job.Schedule().Complete();
        TreeGenerator.GenerateTreesForColumn(col, seed, seaLevel);
        col.IsDataGenerated = true;
    }

    private void CreateColumnRenderers(int2 pos)
    {
        if (!columns.TryGetValue(pos, out ChunkColumn col)) return;

        int minChunkY = 512;
        int maxChunkY = 0;

        for (int i = 0; i < col.Heightmap.Length; i++)
        {
            int h = col.Heightmap[i];
            int seaH = math.max(h, seaLevel);
            if (seaH < minChunkY) minChunkY = seaH;
            if (seaH > maxChunkY) maxChunkY = seaH;
        }

        int minY = math.max(0, (minChunkY - 32) / 32);
        int maxY = math.min(worldHeightInChunks - 1, (maxChunkY + 32) / 32);

        for (int y = minY; y <= maxY; y++)
        {
            int3 chunkPos = new int3(pos.x, y, pos.y);
            if (renderers.ContainsKey(chunkPos)) continue;

            GameObject obj = Instantiate(chunkPrefab,
                new Vector3(pos.x * 32, y * 32, pos.y * 32),
                Quaternion.identity, transform);
            obj.name = $"Chunk_{pos.x}_{y}_{pos.y}";
            ChunkRenderer r = obj.GetComponent<ChunkRenderer>();
            renderers.Add(chunkPos, r);
            r.Initialize(col);
        }
    }

    public void SetBlock(int x, int y, int z, ushort id)
    {
        int2 colPos = new int2(x >> 5, z >> 5);
        if (columns.TryGetValue(colPos, out ChunkColumn col))
        {
            col.SetBlock(x & 31, y, z & 31, id);
            RefreshChunkAndNeighbors(x, y, z);
            if (WaterMeshBuilder.Instance != null)
                WaterMeshBuilder.Instance.RebuildWaterChunkColumn(colPos.x, colPos.y);
        }
    }

    public ushort GetBlock(int x, int y, int z)
    {
        int2 colPos = new int2(x >> 5, z >> 5);
        if (columns.TryGetValue(colPos, out ChunkColumn col))
            return col.GetBlock(x & 31, y, z & 31);
        return 0;
    }

    public bool IsBlockSolid(int x, int y, int z)
    {
        ushort id = GetBlock(x, y, z);
        if (id == BlockIDs.Air || id == BlockIDs.Water) return false;
        var vd = blockDatabase.GetVisualData();
        if (id < vd.Length) return vd[id].isSolid;
        return true;
    }

    public bool HasChunk(int3 coord) => renderers.ContainsKey(coord);
    public bool HasRenderer(int3 coord) => renderers.ContainsKey(coord);

    public ChunkColumn GetColumn(int2 pos)
    {
        columns.TryGetValue(pos, out var col);
        return col;
    }

    public IEnumerable<int2> GetAllColumnCoords() => columns.Keys;
    public void RecalculateLightingForAllChunks() { }

    public void RefreshAllChunks()
    {
        blockDatabase.Dispose();
        foreach (var r in renderers.Values)
            r.RenderMesh();
    }

    private void RefreshChunkAndNeighbors(int x, int y, int z)
    {
        int3 coord = new int3(x >> 5, y >> 5, z >> 5);
        RefreshRenderer(coord);
        if ((x & 31) == 0) RefreshRenderer(coord + new int3(-1, 0, 0));
        if ((x & 31) == 31) RefreshRenderer(coord + new int3(1, 0, 0));
        if ((y & 31) == 0) RefreshRenderer(coord + new int3(0, -1, 0));
        if ((y & 31) == 31) RefreshRenderer(coord + new int3(0, 1, 0));
        if ((z & 31) == 0) RefreshRenderer(coord + new int3(0, 0, -1));
        if ((z & 31) == 31) RefreshRenderer(coord + new int3(0, 0, 1));
    }

    private void RefreshRenderer(int3 coord)
    {
        if (renderers.TryGetValue(coord, out ChunkRenderer r)) r.RenderMesh();
    }

    void Update()
    {
        if (!IsWorldReady) return;

        if (playerTransform == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        UpdateStreamingQueue();
        ProcessStreamingQueue();
    }

    private void UpdateStreamingQueue()
    {
        int pCX = (int)math.floor(playerTransform.position.x / 32f);
        int pCZ = (int)math.floor(playerTransform.position.z / 32f);
        int2 currentCol = new int2(pCX, pCZ);

        if (currentCol.x == lastPlayerColumn.x && currentCol.y == lastPlayerColumn.y)
            return;

        lastPlayerColumn = currentCol;
        streamingQueue.Clear();

        HashSet<int2> needed = new HashSet<int2>();

        for (int x = pCX - renderDistance; x <= pCX + renderDistance; x++)
        {
            for (int z = pCZ - renderDistance; z <= pCZ + renderDistance; z++)
            {
                int dx = x - pCX;
                int dz = z - pCZ;
                if (dx * dx + dz * dz > renderDistance * renderDistance) continue;

                int2 pos = new int2(x, z);
                needed.Add(pos);

                if (!columns.ContainsKey(pos))
                    streamingQueue.Add(pos);
            }
        }

        float px = playerTransform.position.x / 32f;
        float pz = playerTransform.position.z / 32f;
        streamingQueue.Sort((a, b) =>
        {
            float da = (a.x - px) * (a.x - px) + (a.y - pz) * (a.y - pz);
            float db = (b.x - px) * (b.x - px) + (b.y - pz) * (b.y - pz);
            return da.CompareTo(db);
        });

        List<int2> toRemove = new List<int2>();
        foreach (var pos in columns.Keys)
        {
            if (!needed.Contains(pos))
                toRemove.Add(pos);
        }

        int removeLimit = 4;
        for (int i = 0; i < toRemove.Count && i < removeLimit; i++)
            UnloadColumn(toRemove[i]);
    }

    private void ProcessStreamingQueue()
    {
        int loaded = 0;
        while (streamingQueue.Count > 0 && loaded < columnsPerFrame)
        {
            int2 pos = streamingQueue[0];
            streamingQueue.RemoveAt(0);
            if (columns.ContainsKey(pos)) continue;

            GenerateColumnData(pos);
            CreateColumnRenderers(pos);

            if (WaterMeshBuilder.Instance != null)
                WaterMeshBuilder.Instance.RebuildWaterChunkColumn(pos.x, pos.y);

            loaded++;
        }
    }

    private void UnloadColumn(int2 pos)
    {
        for (int y = 0; y < worldHeightInChunks; y++)
        {
            int3 chunkPos = new int3(pos.x, y, pos.y);
            if (renderers.TryGetValue(chunkPos, out ChunkRenderer r))
            {
                Destroy(r.gameObject);
                renderers.Remove(chunkPos);
            }
        }

        if (columns.TryGetValue(pos, out ChunkColumn col))
        {
            col.Dispose();
            columns.Remove(pos);
        }
    }

    void OnDestroy()
    {
        foreach (var col in columns.Values) col.Dispose();
    }
}