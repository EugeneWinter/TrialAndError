using Unity.Mathematics;

public static class WorldBiomeMath
{
    public const int Center = 0;
    public const int IceWastes = 1;
    public const int Canyons = 2;
    public const int Savanna = 3;
    public const int DeciduousForest = 4;
    public const int SnowyMountains = 5;
    public const int Taiga = 6;
    public const int Desert = 7;
    public const int Tropics = 8;
    public const int Ocean = 9;

    public const float ContinentSize = 4096f;
    public const float ContinentHalf = ContinentSize * 0.5f;
    public const float GridStep = 5000f;
    public const float TransitionWidth = 220f;
    public const float IsthmusWidth = 120f;

    public struct BiomePoint
    {
        public int biome;
        public int secondaryBiome;
        public bool isOcean;
        public float blend;
        public float continentFactor;
        public float temperature;
        public float moisture;
    }

    public static BiomePoint Sample(int worldX, int worldZ, int seed)
    {
        float bestDist = float.MaxValue;
        float secondDist = float.MaxValue;
        int bestCol = 1;
        int bestRow = 1;
        int secondCol = 1;
        int secondRow = 1;

        for (int col = 0; col < 3; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                float2 center = GetContinentCenter(col, row);
                float2 d = new float2(worldX - center.x, worldZ - center.y);
                float dist = math.lengthsq(d);

                if (dist < bestDist)
                {
                    secondDist = bestDist;
                    secondCol = bestCol;
                    secondRow = bestRow;
                    bestDist = dist;
                    bestCol = col;
                    bestRow = row;
                }
                else if (dist < secondDist)
                {
                    secondDist = dist;
                    secondCol = col;
                    secondRow = row;
                }
            }
        }

        int primaryBiome = GetBiomeForCell(bestCol, bestRow);
        int secondaryBiome = GetBiomeForCell(secondCol, secondRow);

        float2 primaryCenter = GetContinentCenter(bestCol, bestRow);
        float distToCenter = math.sqrt(bestDist);

        float continentFactor = 1f - math.saturate((distToCenter - ContinentHalf * 0.58f) / (ContinentHalf * 0.42f));
        bool isIsland = IsIsland(bestCol, bestRow);
        bool isOcean = false;

        if (isIsland)
        {
            float islandEdge = ContinentHalf * 0.85f;
            float coastNoise = CoastNoise(worldX, worldZ, seed) * ContinentHalf * 0.15f;
            islandEdge += coastNoise;

            if (distToCenter > islandEdge)
            {
                isOcean = true;
                continentFactor = 0f;
            }
        }
        else
        {
            if (distToCenter > ContinentHalf)
            {
                bool onIsthmus = IsOnIsthmus(worldX, worldZ, seed);
                if (!onIsthmus)
                {
                    isOcean = true;
                    continentFactor = 0f;
                }
                else
                {
                    continentFactor = 0.28f;
                }
            }
        }

        float blend = 0f;
        if (!isOcean)
        {
            float2 secondCenter = GetContinentCenter(secondCol, secondRow);
            float2 d2 = new float2(worldX - secondCenter.x, worldZ - secondCenter.y);
            float distToSecond = math.sqrt(math.lengthsq(d2));
            float distDiff = math.abs(distToCenter - distToSecond);
            blend = 1f - math.saturate(distDiff / TransitionWidth);
        }

        float normalizedX = (worldX + GridStep * 1.5f) / (GridStep * 3f);
        float normalizedZ = (worldZ + GridStep * 1.5f) / (GridStep * 3f);

        BiomePoint result;
        result.biome = isOcean ? Ocean : primaryBiome;
        result.secondaryBiome = isOcean ? Ocean : secondaryBiome;
        result.isOcean = isOcean;
        result.blend = blend;
        result.continentFactor = continentFactor;
        result.temperature = 1f - math.saturate(normalizedZ);
        result.moisture = math.saturate(normalizedX);
        return result;
    }

    public static bool IsWetBiome(int biome)
    {
        return biome == Center || biome == DeciduousForest || biome == Taiga || biome == Tropics;
    }

    public static bool IsHotBiome(int biome)
    {
        return biome == Savanna || biome == Desert || biome == Tropics || biome == Canyons;
    }

    public static bool IsColdBiome(int biome)
    {
        return biome == IceWastes || biome == SnowyMountains || biome == Taiga;
    }

    public static bool IsMountainBiome(int biome)
    {
        return biome == SnowyMountains;
    }

    private static float2 GetContinentCenter(int col, int row)
    {
        float x = (col - 1) * GridStep;
        float z = (row - 1) * GridStep;
        return new float2(x, z);
    }

    private static int GetBiomeForCell(int col, int row)
    {
        if (col == 0 && row == 0) return Desert;
        if (col == 0 && row == 1) return Canyons;
        if (col == 0 && row == 2) return SnowyMountains;
        if (col == 1 && row == 0) return Savanna;
        if (col == 1 && row == 1) return Center;
        if (col == 1 && row == 2) return IceWastes;
        if (col == 2 && row == 0) return Tropics;
        if (col == 2 && row == 1) return DeciduousForest;
        return Taiga;
    }

    private static bool IsIsland(int col, int row)
    {
        return (col == 0 || col == 2) && (row == 0 || row == 2);
    }

    private static bool IsOnIsthmus(int worldX, int worldZ, int seed)
    {
        float2 center = GetContinentCenter(1, 1);

        for (int i = 0; i < 4; i++)
        {
            int col = 1;
            int row = 1;

            if (i == 0) { col = 1; row = 2; }
            if (i == 1) { col = 0; row = 1; }
            if (i == 2) { col = 1; row = 0; }
            if (i == 3) { col = 2; row = 1; }

            float2 target = GetContinentCenter(col, row);
            float2 dir = math.normalize(target - center);
            float2 point = new float2(worldX, worldZ);
            float2 toPoint = point - center;

            float proj = math.dot(toPoint, dir);
            float totalDist = math.distance(center, target);

            if (proj < ContinentHalf * 0.7f || proj > totalDist - ContinentHalf * 0.7f)
                continue;

            float2 projPoint = center + dir * proj;
            float perpDist = math.distance(point, projPoint);

            float noiseWidth = IsthmusNoise(worldX, worldZ, seed, i) * IsthmusWidth * 0.3f;
            float halfWidth = IsthmusWidth * 0.5f + noiseWidth;

            if (perpDist < halfWidth)
                return true;
        }

        return false;
    }

    private static float CoastNoise(int x, int z, int seed)
    {
        float2 p1 = new float2((x + seed) * 0.005f, (z + seed) * 0.005f);
        float2 p2 = new float2((x + seed + 7777) * 0.015f, (z + seed + 7777) * 0.015f);
        float n1 = noise.snoise(p1) * 0.5f + 0.5f;
        float n2 = noise.snoise(p2) * 0.5f + 0.5f;
        return (n1 * 0.7f + n2 * 0.3f) * 2f - 1f;
    }

    private static float IsthmusNoise(int x, int z, int seed, int index)
    {
        float offset = index * 3333f;
        float2 p = new float2((x + seed + offset) * 0.01f, (z + seed + offset) * 0.01f);
        return noise.snoise(p);
    }
}