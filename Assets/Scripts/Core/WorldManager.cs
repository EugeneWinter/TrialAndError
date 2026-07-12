using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    public GameObject chunkPrefab;
    public BlockDatabase blockDatabase;
    public int renderDistance = 4;
    public int seed = 12345;

    private Dictionary<int3, ChunkData> chunks = new Dictionary<int3, ChunkData>();
    private Dictionary<int3, ChunkRenderer> renderers = new Dictionary<int3, ChunkRenderer>();

    void Awake() => Instance = this;

    public void GenerateWorld()
    {
        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
                CreateChunk(new int3(x, 0, z));
    }

    void CreateChunk(int3 coord)
    {
        if (chunks.ContainsKey(coord)) return;

        ChunkData data = new ChunkData();
        data.Initialize(coord);

        System.Random rng = new System.Random(coord.x * 73856093 ^ coord.z * 19349663 ^ seed);

        for (int x = 0; x < 32; x++)
            for (int z = 0; z < 32; z++)
            {
                int worldX = coord.x * 32 + x;
                int worldZ = coord.z * 32 + z;
                int height = GetHeight(worldX, worldZ);

                for (int y = 0; y < 32; y++)
                {
                    int worldY = coord.y * 32 + y;
                    if (worldY == height - 1) data.SetBlock(x, y, z, 2);
                    else if (worldY >= height - 4 && worldY < height - 1) data.SetBlock(x, y, z, 3);
                    else if (worldY < height - 4) data.SetBlock(x, y, z, 1);
                }
            }

        for (int x = 3; x < 29; x++)
            for (int z = 3; z < 29; z++)
            {
                if (rng.Next(0, 100) < 3)
                {
                    int worldX = coord.x * 32 + x;
                    int worldZ = coord.z * 32 + z;
                    int height = GetHeight(worldX, worldZ);
                    int localGroundY = height - coord.y * 32;

                    if (localGroundY > 0 && localGroundY < 24)
                    {
                        GenerateTree(data, x, localGroundY, z, rng);
                    }
                }
            }

        chunks.Add(coord, data);

        GameObject obj = Instantiate(chunkPrefab, new Vector3(coord.x * 32, coord.y * 32, coord.z * 32), Quaternion.identity);
        obj.name = $"Chunk_{coord.x}_{coord.y}_{coord.z}";
        ChunkRenderer r = obj.GetComponent<ChunkRenderer>();
        r.Initialize(data);
        renderers.Add(coord, r);
    }

    void GenerateTree(ChunkData data, int x, int groundY, int z, System.Random rng)
    {
        int totalHeight = 6 + rng.Next(0, 5);
        int trunkHeight = totalHeight - 2;

        int currentX = x;
        int currentZ = z;
        int lastBendY = 0;

        for (int i = 0; i < trunkHeight; i++)
        {
            int y = groundY + i;
            if (y >= 32) break;

            int thickness = 1;
            if (i < trunkHeight * 0.3f) thickness = 2;

            for (int dx = 0; dx < thickness; dx++)
                for (int dz = 0; dz < thickness; dz++)
                {
                    int px = currentX + dx;
                    int pz = currentZ + dz;
                    if (px >= 0 && px < 32 && pz >= 0 && pz < 32)
                        data.SetBlock(px, y, pz, 4);
                }

            if (i > 2 && i - lastBendY >= 3 && rng.Next(0, 100) < 40)
            {
                int bendDir = rng.Next(0, 4);
                if (bendDir == 0) currentX++;
                else if (bendDir == 1) currentX--;
                else if (bendDir == 2) currentZ++;
                else currentZ--;
                lastBendY = i;
            }

            if (i > trunkHeight / 2 && rng.Next(0, 100) < 25)
            {
                GenerateBranch(data, currentX, y, currentZ, rng);
            }
        }

        int topY = groundY + trunkHeight;
        GenerateCanopy(data, currentX, topY, currentZ, rng);
    }

    void GenerateBranch(ChunkData data, int startX, int y, int startZ, System.Random rng)
    {
        int length = 2 + rng.Next(0, 3);
        int dirX = rng.Next(-1, 2);
        int dirZ = rng.Next(-1, 2);

        if (dirX == 0 && dirZ == 0) dirX = 1;

        int bx = startX;
        int bz = startZ;

        for (int i = 0; i < length; i++)
        {
            bx += dirX;
            bz += dirZ;
            int by = y + (i / 2);

            if (bx < 0 || bx >= 32 || bz < 0 || bz >= 32 || by >= 32) break;

            if (data.GetBlock(bx, by, bz) == 0)
                data.SetBlock(bx, by, bz, 4);
        }

        GenerateLeafCluster(data, bx, y + (length / 2), bz, 1, rng);
    }

    void GenerateCanopy(ChunkData data, int centerX, int centerY, int centerZ, System.Random rng)
    {
        int radius = 2 + rng.Next(0, 2);
        int heightRange = 2;

        for (int lx = -radius; lx <= radius; lx++)
            for (int ly = -1; ly <= heightRange; ly++)
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int px = centerX + lx;
                    int py = centerY + ly;
                    int pz = centerZ + lz;

                    if (px < 0 || px >= 32 || py < 0 || py >= 32 || pz < 0 || pz >= 32) continue;

                    float distance = Mathf.Sqrt(lx * lx + ly * ly * 1.5f + lz * lz);
                    float threshold = radius + 0.3f;

                    if (distance > threshold) continue;

                    if (rng.Next(0, 100) < 20 && distance > radius * 0.6f) continue;

                    if (data.GetBlock(px, py, pz) == 0)
                        data.SetBlock(px, py, pz, 5);
                }
    }

    void GenerateLeafCluster(ChunkData data, int cx, int cy, int cz, int radius, System.Random rng)
    {
        for (int lx = -radius; lx <= radius; lx++)
            for (int ly = -radius; ly <= radius; ly++)
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int px = cx + lx;
                    int py = cy + ly;
                    int pz = cz + lz;

                    if (px < 0 || px >= 32 || py < 0 || py >= 32 || pz < 0 || pz >= 32) continue;

                    int dist = Mathf.Abs(lx) + Mathf.Abs(ly) + Mathf.Abs(lz);
                    if (dist > radius + 1) continue;

                    if (rng.Next(0, 100) < 25) continue;

                    if (data.GetBlock(px, py, pz) == 0)
                        data.SetBlock(px, py, pz, 5);
                }
    }

    int GetHeight(int x, int z)
    {
        float scale = 0.05f;
        float n = Mathf.PerlinNoise((x + seed) * scale, (z + seed) * scale);
        return Mathf.FloorToInt(n * 16) + 4;
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
        renderers[coord].RenderMesh();
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
        return GetBlock(x, y, z) != 0;
    }

    public void RefreshAllChunks()
    {
        blockDatabase.Dispose();

        foreach (var kvp in renderers)
        {
            kvp.Value.RenderMesh();
        }

        Debug.Log($"Refreshed {renderers.Count} chunks");
    }

    void OnDestroy()
    {
        foreach (var c in chunks.Values) c.Dispose();
        blockDatabase.Dispose();
    }
}