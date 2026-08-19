using Unity.Mathematics;

public static class TreeGenerator
{
    public static void GenerateTreesForColumn(ChunkColumn column, int seed, int seaLevel)
    {
        int2 colPos = column.Position;
        uint hashSeed = (uint)(colPos.x * 73856093 ^ colPos.y * 19349669 ^ seed * 83492791);
        Random rng = new Random(hashSeed | 1);

        for (int x = 4; x < 28; x++)
        {
            for (int z = 4; z < 28; z++)
            {
                int wx = colPos.x * 32 + x;
                int wz = colPos.y * 32 + z;

                int surfaceY = column.Heightmap[x + z * 32];
                if (surfaceY <= seaLevel + 1 || surfaceY >= 470) continue;

                ushort surfaceBlock = column.GetBlock(x, surfaceY, z);
                if (surfaceBlock != BlockIDs.Grass) continue;
                if (column.GetBlock(x, surfaceY + 1, z) != BlockIDs.Air) continue;

                float densityNoise = noise.snoise(new float2(wx * 0.003f + seed * 0.07f, wz * 0.003f + seed * 0.07f)) * 0.5f + 0.5f;
                float forestMask = noise.snoise(new float2(wx * 0.0008f + seed * 0.03f, wz * 0.0008f + seed * 0.03f)) * 0.5f + 0.5f;

                float chance = densityNoise * forestMask * 6f;
                if (rng.NextFloat(0f, 100f) >= chance) continue;

                float spacingCheck = noise.snoise(new float2(wx * 0.06f, wz * 0.06f)) * 0.5f + 0.5f;
                if (spacingCheck < 0.25f) continue;

                int groundY = surfaceY + 1;
                float typeRoll = rng.NextFloat();

                if (typeRoll < 0.35f)
                    GenerateFancyOak(column, x, groundY, z, ref rng);
                else if (typeRoll < 0.55f)
                    GenerateSmallOak(column, x, groundY, z, ref rng);
                else if (typeRoll < 0.75f)
                    GenerateForkingBirch(column, x, groundY, z, ref rng);
                else
                    GenerateTallSpruce(column, x, groundY, z, ref rng);
            }
        }
    }

    private static void GenerateFancyOak(ChunkColumn col, int x, int groundY, int z, ref Random rng)
    {
        int trunkHeight = 8 + rng.NextInt(0, 5);

        for (int i = 0; i < trunkHeight; i++)
            SetIfAir(col, x, groundY + i, z, BlockIDs.Log);

        int branchStart = groundY + trunkHeight / 2 + rng.NextInt(0, 2);
        int branchCount = 3 + rng.NextInt(0, 3);

        for (int b = 0; b < branchCount; b++)
        {
            int branchY = branchStart + rng.NextInt(0, trunkHeight / 2);
            if (branchY >= groundY + trunkHeight) branchY = groundY + trunkHeight - 1;

            int dirX = rng.NextInt(-1, 2);
            int dirZ = rng.NextInt(-1, 2);
            if (dirX == 0 && dirZ == 0) dirX = (b % 2 == 0) ? 1 : -1;

            int branchLen = 2 + rng.NextInt(0, 3);
            int bx = x, bz = z, by = branchY;

            for (int i = 1; i <= branchLen; i++)
            {
                bx = x + dirX * i;
                bz = z + dirZ * i;
                by = branchY + (i + 1) / 2;

                if (bx < 1 || bx >= 31 || bz < 1 || bz >= 31) break;
                SetIfAir(col, bx, by, bz, BlockIDs.Log);
            }

            int crownRadius = 2 + rng.NextInt(0, 2);
            PlaceSphereLeaves(col, bx, by + 1, bz, crownRadius, ref rng);
        }

        PlaceSphereLeaves(col, x, groundY + trunkHeight, z, 3, ref rng);
    }

    private static void GenerateSmallOak(ChunkColumn col, int x, int groundY, int z, ref Random rng)
    {
        int trunkHeight = 4 + rng.NextInt(0, 3);

        for (int i = 0; i < trunkHeight; i++)
            SetIfAir(col, x, groundY + i, z, BlockIDs.Log);

        int topY = groundY + trunkHeight;

        for (int ly = -1; ly <= 1; ly++)
        {
            int radius = (ly == 0) ? 2 : 1;
            for (int lx = -radius; lx <= radius; lx++)
                for (int lz = -radius; lz <= radius; lz++)
                {
                    if (math.abs(lx) == radius && math.abs(lz) == radius && rng.NextInt(0, 100) < 50) continue;
                    int px = x + lx;
                    int pz = z + lz;
                    if (px < 0 || px >= 32 || pz < 0 || pz >= 32) continue;
                    SetIfAir(col, px, topY + ly, pz, BlockIDs.Leaf);
                }
        }

        SetIfAir(col, x, topY + 2, z, BlockIDs.Leaf);
        if (rng.NextInt(0, 100) < 60) SetIfAir(col, x + 1, topY + 2, z, BlockIDs.Leaf);
        if (rng.NextInt(0, 100) < 60) SetIfAir(col, x - 1, topY + 2, z, BlockIDs.Leaf);
        if (rng.NextInt(0, 100) < 60) SetIfAir(col, x, topY + 2, z + 1, BlockIDs.Leaf);
        if (rng.NextInt(0, 100) < 60) SetIfAir(col, x, topY + 2, z - 1, BlockIDs.Leaf);
    }

    private static void GenerateForkingBirch(ChunkColumn col, int x, int groundY, int z, ref Random rng)
    {
        int mainHeight = 6 + rng.NextInt(0, 3);
        int forkHeight = mainHeight / 2 + rng.NextInt(0, 2);

        for (int i = 0; i < forkHeight; i++)
            SetIfAir(col, x, groundY + i, z, BlockIDs.Log);

        int fork1X = rng.NextInt(0, 2) == 0 ? -1 : 1;
        int fork2X = -fork1X;
        int fork1Z = rng.NextInt(-1, 2);
        int fork2Z = rng.NextInt(-1, 2);

        int fork1Len = mainHeight - forkHeight;
        int fork2Len = fork1Len + rng.NextInt(-1, 2);

        int f1x = x, f1z = z;
        for (int i = 0; i < fork1Len; i++)
        {
            int fy = groundY + forkHeight + i;
            if (i == 0 || i == fork1Len / 2) { f1x += fork1X; f1z += fork1Z; }
            if (f1x < 1 || f1x >= 31 || f1z < 1 || f1z >= 31) break;
            SetIfAir(col, f1x, fy, f1z, BlockIDs.Log);
        }
        PlaceSphereLeaves(col, f1x, groundY + forkHeight + fork1Len, f1z, 2, ref rng);

        int f2x = x, f2z = z;
        for (int i = 0; i < fork2Len; i++)
        {
            int fy = groundY + forkHeight + i;
            if (i == 0 || i == fork2Len / 2) { f2x += fork2X; f2z += fork2Z; }
            if (f2x < 1 || f2x >= 31 || f2z < 1 || f2z >= 31) break;
            SetIfAir(col, f2x, fy, f2z, BlockIDs.Log);
        }
        PlaceSphereLeaves(col, f2x, groundY + forkHeight + fork2Len, f2z, 2, ref rng);
    }

    private static void GenerateTallSpruce(ChunkColumn col, int x, int groundY, int z, ref Random rng)
    {
        int trunkHeight = 12 + rng.NextInt(0, 8);

        for (int i = 0; i < trunkHeight; i++)
            SetIfAir(col, x, groundY + i, z, BlockIDs.Log);

        int crownStart = groundY + 3 + rng.NextInt(0, 2);
        int topY = groundY + trunkHeight;

        for (int y = crownStart; y <= topY; y++)
        {
            float progress = (float)(y - crownStart) / (float)(topY - crownStart);
            int radius;

            if (progress > 0.85f)
                radius = 0;
            else if (progress > 0.65f)
                radius = 1;
            else if (progress > 0.3f)
                radius = 2 + (int)((1f - progress) * 1.5f);
            else
                radius = 1 + rng.NextInt(0, 2);

            if ((y - crownStart) % 2 == 0 && radius > 1)
                radius--;

            for (int lx = -radius; lx <= radius; lx++)
            {
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int px = x + lx;
                    int pz = z + lz;
                    if (px < 0 || px >= 32 || pz < 0 || pz >= 32) continue;

                    int dist = math.abs(lx) + math.abs(lz);
                    if (dist > radius) continue;
                    if (dist == radius && rng.NextInt(0, 100) < 35) continue;

                    SetIfAir(col, px, y, pz, BlockIDs.Leaf);
                }
            }
        }

        SetIfAir(col, x, topY + 1, z, BlockIDs.Leaf);
        SetIfAir(col, x, topY + 2, z, BlockIDs.Leaf);
    }

    private static void PlaceSphereLeaves(ChunkColumn col, int cx, int cy, int cz, int radius, ref Random rng)
    {
        float radiusSq = (radius + 0.5f) * (radius + 0.5f);

        for (int lx = -radius; lx <= radius; lx++)
        {
            for (int ly = -radius; ly <= radius; ly++)
            {
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int px = cx + lx;
                    int py = cy + ly;
                    int pz = cz + lz;

                    if (px < 0 || px >= 32 || py < 0 || py >= 508 || pz < 0 || pz >= 32) continue;

                    float distSq = lx * lx + ly * ly * 1.2f + lz * lz;
                    if (distSq > radiusSq) continue;

                    float edgeFactor = distSq / radiusSq;
                    if (edgeFactor > 0.6f && rng.NextInt(0, 100) < (int)(edgeFactor * 60f)) continue;

                    SetIfAir(col, px, py, pz, BlockIDs.Leaf);
                }
            }
        }
    }

    private static void SetIfAir(ChunkColumn col, int x, int y, int z, ushort id)
    {
        if (x < 0 || x >= 32 || z < 0 || z >= 32 || y < 0 || y >= 508) return;
        if (col.GetBlock(x, y, z) == BlockIDs.Air)
            col.SetBlock(x, y, z, id);
    }
}