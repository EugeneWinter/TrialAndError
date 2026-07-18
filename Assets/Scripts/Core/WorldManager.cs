using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    public GameObject chunkPrefab;
    public BlockDatabase blockDatabase;
    public int renderDistance = 6;
    public int seed = 12345;

    [Header("World Shape")]
    public int worldHeightInChunks = 8;
    public int seaLevel = 64;

    private Queue<int3> lightRecalcQueue = new Queue<int3>();
    private HashSet<int3> lightRecalcSet = new HashSet<int3>();
    public int chunksPerFrameRecalc = 1;

    private Dictionary<int3, ChunkData> chunks = new Dictionary<int3, ChunkData>();
    private Dictionary<int3, ChunkRenderer> renderers = new Dictionary<int3, ChunkRenderer>();

    void Awake() => Instance = this;

    public bool HasChunk(int3 coord)
    {
        return chunks.ContainsKey(coord);
    }

    public ChunkData GetChunkData(int3 coord)
    {
        return chunks[coord];
    }

    public void GenerateWorld()
    {
        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
                for (int y = 0; y < worldHeightInChunks; y++)
                    CreateChunkData(new int3(x, y, z));

        foreach (var coord in new List<int3>(chunks.Keys))
            CreateChunkRenderer(coord);

        if (WaterMeshBuilder.Instance != null)
            WaterMeshBuilder.Instance.BuildWaterForWorld(renderDistance, seaLevel);

        if (WorldItemSpawner.Instance != null)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
                for (int z = -renderDistance; z <= renderDistance; z++)
                    WorldItemSpawner.Instance.SpawnItemsForChunk(x, z, seed);
        }
    }

    void CreateChunkData(int3 coord)
    {
        if (chunks.ContainsKey(coord)) return;

        ChunkData data = new ChunkData();
        data.Initialize(coord);

        System.Random rng = new System.Random(coord.x * 73856093 ^ coord.y * 19349663 ^ coord.z * 83492791 ^ seed);

        for (int x = 0; x < 32; x++)
            for (int z = 0; z < 32; z++)
            {
                int worldX = coord.x * 32 + x;
                int worldZ = coord.z * 32 + z;

                float continentality = GetContinentality(worldX, worldZ);
                float erosion = GetErosion(worldX, worldZ);
                float peaks = GetPeaks(worldX, worldZ);
                float moisture = GetMoisture(worldX, worldZ);

                int surfaceHeight = CalculateSurfaceHeight(continentality, erosion, peaks);

                for (int y = 0; y < 32; y++)
                {
                    int worldY = coord.y * 32 + y;

                    ushort block = GetBlockForPosition(worldX, worldY, worldZ, surfaceHeight, moisture, rng);
                    if (block != 0)
                        data.SetBlock(x, y, z, block);
                }
            }

        GenerateTrees(data, coord, rng);
        chunks.Add(coord, data);
    }

    ushort GetBlockForPosition(int wx, int wy, int wz, int surfaceHeight, float moisture, System.Random rng)
    {
        if (wy == 0) return 9;

        if (wy > surfaceHeight)
        {
            if (wy <= seaLevel) return 6;
            return 0;
        }

        float caveDensity = GetCaveDensity(wx, wy, wz);
        bool isCave = caveDensity > 0.62f && wy > 2 && wy < surfaceHeight - 3;

        if (isCave)
        {
            if (wy <= seaLevel) return 6;
            return 0;
        }

        if (wy == surfaceHeight)
        {
            if (surfaceHeight <= seaLevel) return 7;
            if (surfaceHeight <= seaLevel + 3) return 7;
            if (moisture < 0.25f) return 7;
            return 2;
        }

        int depthFromSurface = surfaceHeight - wy;

        if (depthFromSurface <= 4)
        {
            if (surfaceHeight <= seaLevel + 3) return 7;
            return 3;
        }

        if (wy < 20)
        {
            if (rng.Next(0, 100) < 30 + (20 - wy) * 3)
                return 8;
        }

        return 1;
    }

    int CalculateSurfaceHeight(float continentality, float erosion, float peaks)
    {
        float baseHeight;

        if (continentality < 0.2f)
        {
            baseHeight = Mathf.Lerp(40f, 60f, continentality / 0.2f);
        }
        else if (continentality < 0.4f)
        {
            float t = (continentality - 0.2f) / 0.2f;
            baseHeight = Mathf.Lerp(60f, 72f, t);
        }
        else if (continentality < 0.6f)
        {
            float t = (continentality - 0.4f) / 0.2f;
            baseHeight = Mathf.Lerp(72f, 90f, t);
        }
        else if (continentality < 0.75f)
        {
            float t = (continentality - 0.6f) / 0.15f;
            baseHeight = Mathf.Lerp(90f, 120f, t);
        }
        else
        {
            float t = (continentality - 0.75f) / 0.25f;
            baseHeight = Mathf.Lerp(120f, 160f, t * t);
        }

        float erosionInfluence = (1f - erosion) * 0.2f + 0.8f;
        baseHeight *= erosionInfluence;

        if (continentality > 0.7f)
        {
            float mountainFactor = Mathf.InverseLerp(0.7f, 1f, continentality);
            float peakHeight = peaks * peaks * 90f * mountainFactor;
            baseHeight += peakHeight;
        }

        baseHeight += erosion * 2f;

        return Mathf.Clamp(Mathf.FloorToInt(baseHeight), 2, 250);
    }

    float GetCaveDensity(int x, int y, int z)
    {
        float scale1 = 0.035f;
        float scale2 = 0.06f;
        float scale3 = 0.015f;

        float cheese = Perlin3D(
            (x + seed * 3) * scale1,
            y * scale1 * 1.5f,
            (z + seed * 3) * scale1);

        float spaghetti1 = Perlin3D(
            (x + seed * 7) * scale2,
            y * scale2 * 0.8f,
            (z + seed * 7) * scale2);

        float spaghetti2 = Perlin3D(
            (x + seed * 13) * scale2 + 500f,
            y * scale2 * 0.8f + 500f,
            (z + seed * 13) * scale2 + 500f);

        float spaghettiCombined = Mathf.Sqrt(spaghetti1 * spaghetti1 + spaghetti2 * spaghetti2);

        float largeCaves = Perlin3D(
            (x + seed * 17) * scale3,
            y * scale3,
            (z + seed * 17) * scale3);

        float depthFactor = 1f;
        if (y > 100) depthFactor = Mathf.InverseLerp(140f, 100f, y);
        if (y < 10) depthFactor = Mathf.InverseLerp(2f, 10f, y);

        float combined = Mathf.Max(cheese, spaghettiCombined * 0.85f);
        combined = Mathf.Max(combined, largeCaves * 0.7f);
        combined *= depthFactor;

        return combined;
    }

    float Perlin3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ca = Mathf.PerlinNoise(z, x);
        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ac = Mathf.PerlinNoise(x, z);
        return (ab + bc + ca + ba + cb + ac) / 6f;
    }

    void GenerateTrees(ChunkData data, int3 coord, System.Random rng)
    {
        for (int x = 3; x < 29; x++)
            for (int z = 3; z < 29; z++)
            {
                int worldX = coord.x * 32 + x;
                int worldZ = coord.z * 32 + z;

                float continentality = GetContinentality(worldX, worldZ);
                float moisture = GetMoisture(worldX, worldZ);

                for (int y = 31; y >= 0; y--)
                {
                    int worldY = coord.y * 32 + y;

                    if (worldY <= seaLevel + 1) break;

                    ushort block = data.GetBlock(x, y, z);
                    if (block != 2) continue;

                    if (y >= 28) break;

                    ushort above = data.GetBlock(x, y + 1, z);
                    if (above != 0) break;

                    float treeChance = 0f;
                    if (continentality > 0.7f) treeChance = 0f;
                    else if (continentality > 0.5f) treeChance = 1f;
                    else if (continentality > 0.2f) treeChance = 4f * moisture;
                    else treeChance = 2f * moisture;

                    if (rng.Next(0, 100) < treeChance)
                    {
                        int treeY = y + 1;
                        int treeType = rng.Next(0, 3);
                        if (treeType == 0) GenerateOakTree(data, x, treeY, z, rng);
                        else if (treeType == 1) GenerateBirchTree(data, x, treeY, z, rng);
                        else GenerateSpruceTree(data, x, treeY, z, rng);
                    }

                    break;
                }
            }
    }

    void GenerateOakTree(ChunkData data, int x, int groundY, int z, System.Random rng)
    {
        int trunkHeight = 5 + rng.Next(0, 3);

        for (int i = 0; i < trunkHeight; i++)
        {
            int y = groundY + i;
            if (y >= 32) break;
            data.SetBlock(x, y, z, 4);
        }

        int branchCount = 2 + rng.Next(0, 3);
        for (int b = 0; b < branchCount; b++)
        {
            int branchY = groundY + trunkHeight - 2 - rng.Next(0, 2);
            if (branchY < 0 || branchY >= 32) continue;

            int dirX = rng.Next(-1, 2);
            int dirZ = rng.Next(-1, 2);
            if (dirX == 0 && dirZ == 0) dirX = 1;

            int branchLen = 1 + rng.Next(0, 2);
            for (int i = 1; i <= branchLen; i++)
            {
                int bx = x + dirX * i;
                int by = branchY + (i / 2);
                int bz = z + dirZ * i;

                if (bx < 0 || bx >= 32 || bz < 0 || bz >= 32 || by >= 32) break;
                if (data.GetBlock(bx, by, bz) == 0)
                    data.SetBlock(bx, by, bz, 4);
            }

            int leafX = x + dirX * branchLen;
            int leafY = branchY + (branchLen / 2) + 1;
            int leafZ = z + dirZ * branchLen;
            GenerateLeafBlob(data, leafX, leafY, leafZ, 2, rng);
        }

        int topY = groundY + trunkHeight;
        GenerateLeafBlob(data, x, topY, z, 3, rng);
    }

    void GenerateBirchTree(ChunkData data, int x, int groundY, int z, System.Random rng)
    {
        int trunkHeight = 7 + rng.Next(0, 3);

        for (int i = 0; i < trunkHeight; i++)
        {
            int y = groundY + i;
            if (y >= 32) break;
            data.SetBlock(x, y, z, 4);
        }

        int topY = groundY + trunkHeight;
        GenerateLeafBlob(data, x, topY, z, 2, rng);

        if (topY - 1 < 32)
            GenerateLeafBlob(data, x, topY - 1, z, 2, rng);
    }

    void GenerateSpruceTree(ChunkData data, int x, int groundY, int z, System.Random rng)
    {
        int trunkHeight = 8 + rng.Next(0, 4);

        for (int i = 0; i < trunkHeight; i++)
        {
            int y = groundY + i;
            if (y >= 32) break;
            data.SetBlock(x, y, z, 4);
        }

        int layers = 4;
        for (int layer = 0; layer < layers; layer++)
        {
            int layerY = groundY + trunkHeight - layer * 2 - 1;
            if (layerY < 0 || layerY >= 32) continue;

            int radius = 1 + layer;
            if (radius > 3) radius = 3;

            for (int lx = -radius; lx <= radius; lx++)
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int px = x + lx;
                    int pz = z + lz;

                    if (px < 0 || px >= 32 || pz < 0 || pz >= 32) continue;

                    int dist = Mathf.Abs(lx) + Mathf.Abs(lz);
                    if (dist > radius) continue;
                    if (dist == radius && rng.Next(0, 100) < 40) continue;

                    if (data.GetBlock(px, layerY, pz) == 0)
                        data.SetBlock(px, layerY, pz, 5);
                }
        }

        int topY = groundY + trunkHeight;
        if (topY < 32 && data.GetBlock(x, topY, z) == 0)
            data.SetBlock(x, topY, z, 5);
    }

    void GenerateLeafBlob(ChunkData data, int cx, int cy, int cz, int radius, System.Random rng)
    {
        for (int lx = -radius; lx <= radius; lx++)
            for (int ly = -radius; ly <= radius; ly++)
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int px = cx + lx;
                    int py = cy + ly;
                    int pz = cz + lz;

                    if (px < 0 || px >= 32 || py < 0 || py >= 32 || pz < 0 || pz >= 32) continue;

                    float distSq = lx * lx + ly * ly * 1.3f + lz * lz;
                    float radiusSq = radius * radius;

                    if (distSq > radiusSq) continue;

                    float edgeFactor = distSq / radiusSq;
                    if (edgeFactor > 0.7f && rng.Next(0, 100) < 40) continue;

                    if (data.GetBlock(px, py, pz) == 0)
                        data.SetBlock(px, py, pz, 5);
                }
    }

    public float GetContinentality(int x, int z)
    {
        float scale1 = 0.002f;
        float scale2 = 0.008f;
        float scale3 = 0.025f;

        float c1 = Mathf.PerlinNoise((x + seed) * scale1, (z + seed) * scale1);
        float c2 = Mathf.PerlinNoise((x + seed + 10000) * scale2, (z + seed + 10000) * scale2);
        float c3 = Mathf.PerlinNoise((x + seed + 20000) * scale3, (z + seed + 20000) * scale3);

        float combined = c1 * 0.6f + c2 * 0.3f + c3 * 0.1f;
        return Mathf.Clamp01(combined);
    }

    public float GetErosion(int x, int z)
    {
        float scale1 = 0.005f;
        float scale2 = 0.02f;

        float e1 = Mathf.PerlinNoise((x + seed + 30000) * scale1, (z + seed + 30000) * scale1);
        float e2 = Mathf.PerlinNoise((x + seed + 40000) * scale2, (z + seed + 40000) * scale2);

        return e1 * 0.7f + e2 * 0.3f;
    }

    public float GetPeaks(int x, int z)
    {
        float scale1 = 0.01f;
        float scale2 = 0.04f;

        float p1 = Mathf.PerlinNoise((x + seed + 50000) * scale1, (z + seed + 50000) * scale1);
        float p2 = Mathf.PerlinNoise((x + seed + 60000) * scale2, (z + seed + 60000) * scale2);

        float combined = p1 * 0.7f + p2 * 0.3f;
        return Mathf.Clamp01(combined);
    }

    public float GetMoisture(int x, int z)
    {
        return Mathf.PerlinNoise((x + seed + 5000) * 0.008f, (z + seed + 5000) * 0.008f);
    }

    public void SetBlock(int x, int y, int z, ushort id)
    {
        int3 coord = new int3(
            Mathf.FloorToInt((float)x / 32),
            Mathf.FloorToInt((float)y / 32),
            Mathf.FloorToInt((float)z / 32));

        if (!chunks.TryGetValue(coord, out ChunkData data)) return;

        int lx = x - coord.x * 32;
        int ly = y - coord.y * 32;
        int lz = z - coord.z * 32;

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
        int3 coord = new int3(
            Mathf.FloorToInt((float)x / 32),
            Mathf.FloorToInt((float)y / 32),
            Mathf.FloorToInt((float)z / 32));

        if (!chunks.TryGetValue(coord, out ChunkData data)) return 0;

        int lx = x - coord.x * 32;
        int ly = y - coord.y * 32;
        int lz = z - coord.z * 32;

        return data.GetBlock(lx, ly, lz);
    }

    public bool IsBlockSolid(int x, int y, int z)
    {
        ushort id = GetBlock(x, y, z);
        if (id == 0) return false;

        BlockSO block = blockDatabase.GetBlockSO(id);
        if (block != null && !block.isSolid) return false;

        return true;
    }

    public void RefreshAllChunks()
    {
        blockDatabase.Dispose();

        foreach (var kvp in renderers)
            kvp.Value.RenderMesh();
    }

    void OnDestroy()
    {
        foreach (var c in chunks.Values) c.Dispose();
        blockDatabase.Dispose();
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
}