using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct WorldLayout
{
    public const int CONTINENT_SIZE = 4096;
    public const float CONTINENT_HALF = CONTINENT_SIZE * 0.5f;
    public const float GRID_STEP = 5000f;
    public const float TRANSITION_WIDTH = 250f;
    public const float ISTHMUS_HALF_WIDTH = 60f;

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

    public struct WorldSample
    {
        public int PrimaryBiome;
        public int SecondaryBiome;
        public float Blend;
        public bool IsOcean;
        public float ContinentFactor;
        public float Temperature;
        public float Moisture;
    }

    public static WorldSample Sample(int wx, int wz, int seed)
    {
        float bestDist = float.MaxValue;
        float secondDist = float.MaxValue;
        int bestCol = 1, bestRow = 1;
        int secondCol = 1, secondRow = 1;

        for (int col = 0; col < 3; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                float2 center = ContinentCenter(col, row);
                float2 d = new float2(wx - center.x, wz - center.y);
                float dist = math.lengthsq(d);

                if (dist < bestDist)
                {
                    secondDist = bestDist; secondCol = bestCol; secondRow = bestRow;
                    bestDist = dist; bestCol = col; bestRow = row;
                }
                else if (dist < secondDist)
                {
                    secondDist = dist; secondCol = col; secondRow = row;
                }
            }
        }

        float2 primaryCenter = ContinentCenter(bestCol, bestRow);
        float distToCenter = math.sqrt(bestDist);

        float continentFactor = 1f - math.saturate((distToCenter - CONTINENT_HALF * 0.55f) / (CONTINENT_HALF * 0.45f));
        bool isIsland = IsIslandCell(bestCol, bestRow);
        bool isOcean = false;

        if (isIsland)
        {
            float islandEdge = CONTINENT_HALF * 0.82f;
            float cn = CoastNoise(wx, wz, seed);
            islandEdge += cn * CONTINENT_HALF * 0.18f;

            if (distToCenter > islandEdge)
            {
                isOcean = true;
                continentFactor = 0f;
            }
        }
        else
        {
            if (distToCenter > CONTINENT_HALF)
            {
                if (IsOnIsthmus(wx, wz, seed))
                    continentFactor = 0.25f;
                else
                {
                    isOcean = true;
                    continentFactor = 0f;
                }
            }
        }

        float blend = 0f;
        if (!isOcean && continentFactor < 0.35f)
        {
            float2 secondCenter = ContinentCenter(secondCol, secondRow);
            float distToSecond = math.distance(new float2(wx, wz), secondCenter);
            blend = 1f - math.saturate(math.abs(distToCenter - distToSecond) / TRANSITION_WIDTH);
        }

        int primaryBiome = isOcean ? Ocean : BiomeForCell(bestCol, bestRow);
        int secondaryBiome = isOcean ? Ocean : BiomeForCell(secondCol, secondRow);

        float normalizedZ = (wz + GRID_STEP * 1.5f) / (GRID_STEP * 3f);
        float normalizedX = (wx + GRID_STEP * 1.5f) / (GRID_STEP * 3f);
        float temperature = 1f - math.saturate(normalizedZ);
        float moisture = math.saturate(normalizedX);

        return new WorldSample
        {
            PrimaryBiome = primaryBiome,
            SecondaryBiome = secondaryBiome,
            Blend = blend,
            IsOcean = isOcean,
            ContinentFactor = continentFactor,
            Temperature = temperature,
            Moisture = moisture
        };
    }

    public static float2 ContinentCenter(int col, int row)
    {
        return new float2((col - 1) * GRID_STEP, (row - 1) * GRID_STEP);
    }

    private static int BiomeForCell(int col, int row)
    {
        if (col == 1 && row == 1) return Center;
        if (col == 1 && row == 2) return IceWastes;
        if (col == 0 && row == 1) return Canyons;
        if (col == 1 && row == 0) return Savanna;
        if (col == 2 && row == 1) return DeciduousForest;
        if (col == 0 && row == 2) return SnowyMountains;
        if (col == 2 && row == 2) return Taiga;
        if (col == 0 && row == 0) return Desert;
        if (col == 2 && row == 0) return Tropics;
        return Ocean;
    }

    private static bool IsIslandCell(int col, int row)
    {
        return (col == 0 || col == 2) && (row == 0 || row == 2);
    }

    private static bool IsOnIsthmus(int wx, int wz, int seed)
    {
        float2 center = ContinentCenter(1, 1);

        for (int i = 0; i < 4; i++)
        {
            int tc, tr;
            if (i == 0) { tc = 1; tr = 2; }
            else if (i == 1) { tc = 0; tr = 1; }
            else if (i == 2) { tc = 1; tr = 0; }
            else { tc = 2; tr = 1; }

            float2 target = ContinentCenter(tc, tr);
            float2 dir = math.normalizesafe(target - center);
            float2 point = new float2(wx, wz);
            float2 toPoint = point - center;

            float proj = math.dot(toPoint, dir);
            float totalDist = math.distance(center, target);

            if (proj < CONTINENT_HALF * 0.65f || proj > totalDist - CONTINENT_HALF * 0.65f)
                continue;

            float2 projPoint = center + dir * proj;
            float perpDist = math.distance(point, projPoint);

            float noiseW = noise.snoise(new float2((wx + seed + i * 3333f) * 0.008f, (wz + seed + i * 3333f) * 0.008f)) * ISTHMUS_HALF_WIDTH * 0.4f;
            float halfWidth = ISTHMUS_HALF_WIDTH + noiseW;

            if (perpDist < halfWidth)
                return true;
        }
        return false;
    }

    private static float CoastNoise(int wx, int wz, int seed)
    {
        float n1 = noise.snoise(new float2((wx + seed) * 0.004f, (wz + seed) * 0.004f));
        float n2 = noise.snoise(new float2((wx + seed + 7777) * 0.012f, (wz + seed + 7777) * 0.012f));
        return n1 * 0.7f + n2 * 0.3f;
    }
}