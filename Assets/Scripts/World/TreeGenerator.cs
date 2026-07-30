using UnityEngine;
using Unity.Mathematics;

public class TreeGenerator
{
    private int seaLevel;

    public TreeGenerator(int seaLevel)
    {
        this.seaLevel = seaLevel;
    }

    public void GenerateTrees(ref ChunkData data, int3 coord, System.Random rng, TerrainGenerator terrain)
    {
        for (int x = 3; x < 29; x++)
            for (int z = 3; z < 29; z++)
            {
                int worldX = coord.x * 32 + x;
                int worldZ = coord.z * 32 + z;

                float continentality = terrain.GetContinentality(worldX, worldZ);
                float moisture = terrain.GetMoisture(worldX, worldZ);

                for (int y = 31; y >= 0; y--)
                {
                    int worldY = coord.y * 32 + y;
                    if (worldY <= seaLevel + 1) break;

                    if (data.GetBlock(x, y, z) != BlockIDs.Grass) continue;
                    if (y >= 28) break;
                    if (data.GetBlock(x, y + 1, z) != BlockIDs.Air) break;

                    float treeChance = 0f;
                    if (continentality > 0.7f) treeChance = 0f;
                    else if (continentality > 0.5f) treeChance = 1f;
                    else if (continentality > 0.2f) treeChance = 4f * moisture;
                    else treeChance = 2f * moisture;

                    if (rng.Next(0, 100) < treeChance)
                    {
                        int treeType = rng.Next(0, 3);
                        if (treeType == 0) GenerateOak(data, x, y + 1, z, rng);
                        else if (treeType == 1) GenerateBirch(data, x, y + 1, z, rng);
                        else GenerateSpruce(data, x, y + 1, z, rng);
                    }
                    break;
                }
            }
    }

    void GenerateOak(ChunkData data, int x, int groundY, int z, System.Random rng)
    {
        int trunkHeight = 5 + rng.Next(0, 3);

        for (int i = 0; i < trunkHeight; i++)
        {
            int y = groundY + i;
            if (y >= 32) break;
            data.SetBlock(x, y, z, BlockIDs.Log);
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
                if (data.GetBlock(bx, by, bz) == BlockIDs.Air)
                    data.SetBlock(bx, by, bz, BlockIDs.Log);
            }

            PlaceLeafBlob(data, x + dirX * branchLen, branchY + (branchLen / 2) + 1, z + dirZ * branchLen, 2, rng);
        }

        PlaceLeafBlob(data, x, groundY + trunkHeight, z, 3, rng);
    }

    void GenerateBirch(ChunkData data, int x, int groundY, int z, System.Random rng)
    {
        int trunkHeight = 7 + rng.Next(0, 3);

        for (int i = 0; i < trunkHeight; i++)
        {
            int y = groundY + i;
            if (y >= 32) break;
            data.SetBlock(x, y, z, BlockIDs.Log);
        }

        int topY = groundY + trunkHeight;
        PlaceLeafBlob(data, x, topY, z, 2, rng);
        if (topY - 1 < 32)
            PlaceLeafBlob(data, x, topY - 1, z, 2, rng);
    }

    void GenerateSpruce(ChunkData data, int x, int groundY, int z, System.Random rng)
    {
        int trunkHeight = 8 + rng.Next(0, 4);

        for (int i = 0; i < trunkHeight; i++)
        {
            int y = groundY + i;
            if (y >= 32) break;
            data.SetBlock(x, y, z, BlockIDs.Log);
        }

        for (int layer = 0; layer < 4; layer++)
        {
            int layerY = groundY + trunkHeight - layer * 2 - 1;
            if (layerY < 0 || layerY >= 32) continue;

            int radius = Mathf.Min(1 + layer, 3);

            for (int lx = -radius; lx <= radius; lx++)
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int px = x + lx;
                    int pz = z + lz;
                    if (px < 0 || px >= 32 || pz < 0 || pz >= 32) continue;

                    int dist = Mathf.Abs(lx) + Mathf.Abs(lz);
                    if (dist > radius) continue;
                    if (dist == radius && rng.Next(0, 100) < 40) continue;

                    if (data.GetBlock(px, layerY, pz) == BlockIDs.Air)
                        data.SetBlock(px, layerY, pz, BlockIDs.Leaf);
                }
        }

        int topY = groundY + trunkHeight;
        if (topY < 32 && data.GetBlock(x, topY, z) == BlockIDs.Air)
            data.SetBlock(x, topY, z, BlockIDs.Leaf);
    }

    void PlaceLeafBlob(ChunkData data, int cx, int cy, int cz, int radius, System.Random rng)
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
                    if (distSq / radiusSq > 0.7f && rng.Next(0, 100) < 40) continue;

                    if (data.GetBlock(px, py, pz) == BlockIDs.Air)
                        data.SetBlock(px, py, pz, BlockIDs.Leaf);
                }
    }
}