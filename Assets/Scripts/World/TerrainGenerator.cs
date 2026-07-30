using UnityEngine;

public class TerrainGenerator
{
    private int seed;
    private int seaLevel;

    public TerrainGenerator(int seed, int seaLevel)
    {
        this.seed = seed;
        this.seaLevel = seaLevel;
    }

    public void PopulateChunk(ref ChunkData data, Unity.Mathematics.int3 coord, System.Random rng)
    {
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
                    if (block != BlockIDs.Air)
                        data.SetBlock(x, y, z, block);
                }
            }
    }

    ushort GetBlockForPosition(int wx, int wy, int wz, int surfaceHeight, float moisture, System.Random rng)
    {
        if (wy == 0) return BlockIDs.Bedrock;

        if (wy > surfaceHeight)
        {
            if (wy <= seaLevel) return BlockIDs.Water;
            return BlockIDs.Air;
        }

        float caveDensity = GetCaveDensity(wx, wy, wz);
        bool isCave = caveDensity > 0.62f && wy > 2 && wy < surfaceHeight - 3;

        if (isCave)
        {
            if (wy <= seaLevel) return BlockIDs.Water;
            return BlockIDs.Air;
        }

        if (wy == surfaceHeight)
        {
            if (surfaceHeight <= seaLevel + 3) return BlockIDs.Sand;
            if (moisture < 0.25f) return BlockIDs.Sand;
            return BlockIDs.Grass;
        }

        int depthFromSurface = surfaceHeight - wy;

        if (depthFromSurface <= 4)
        {
            if (surfaceHeight <= seaLevel + 3) return BlockIDs.Sand;
            return BlockIDs.Dirt;
        }

        if (wy < 20)
        {
            if (rng.Next(0, 100) < 30 + (20 - wy) * 3)
                return BlockIDs.Deepstone;
        }

        return BlockIDs.Stone;
    }

    int CalculateSurfaceHeight(float continentality, float erosion, float peaks)
    {
        float baseHeight;

        if (continentality < 0.2f)
            baseHeight = Mathf.Lerp(40f, 60f, continentality / 0.2f);
        else if (continentality < 0.4f)
            baseHeight = Mathf.Lerp(60f, 72f, (continentality - 0.2f) / 0.2f);
        else if (continentality < 0.6f)
            baseHeight = Mathf.Lerp(72f, 90f, (continentality - 0.4f) / 0.2f);
        else if (continentality < 0.75f)
            baseHeight = Mathf.Lerp(90f, 120f, (continentality - 0.6f) / 0.15f);
        else
        {
            float t = (continentality - 0.75f) / 0.25f;
            baseHeight = Mathf.Lerp(120f, 160f, t * t);
        }

        baseHeight *= (1f - erosion) * 0.2f + 0.8f;

        if (continentality > 0.7f)
        {
            float mountainFactor = Mathf.InverseLerp(0.7f, 1f, continentality);
            baseHeight += peaks * peaks * 90f * mountainFactor;
        }

        baseHeight += erosion * 2f;
        return Mathf.Clamp(Mathf.FloorToInt(baseHeight), 2, 250);
    }

    float GetCaveDensity(int x, int y, int z)
    {
        float cheese = Perlin3D((x + seed * 3) * 0.035f, y * 0.035f * 1.5f, (z + seed * 3) * 0.035f);
        float spaghetti1 = Perlin3D((x + seed * 7) * 0.06f, y * 0.06f * 0.8f, (z + seed * 7) * 0.06f);
        float spaghetti2 = Perlin3D((x + seed * 13) * 0.06f + 500f, y * 0.06f * 0.8f + 500f, (z + seed * 13) * 0.06f + 500f);
        float spaghettiCombined = Mathf.Sqrt(spaghetti1 * spaghetti1 + spaghetti2 * spaghetti2);
        float largeCaves = Perlin3D((x + seed * 17) * 0.015f, y * 0.015f, (z + seed * 17) * 0.015f);

        float depthFactor = 1f;
        if (y > 100) depthFactor = Mathf.InverseLerp(140f, 100f, y);
        if (y < 10) depthFactor = Mathf.InverseLerp(2f, 10f, y);

        float combined = Mathf.Max(cheese, spaghettiCombined * 0.85f);
        combined = Mathf.Max(combined, largeCaves * 0.7f);
        return combined * depthFactor;
    }

    public float GetContinentality(int x, int z)
    {
        float c1 = Mathf.PerlinNoise((x + seed) * 0.002f, (z + seed) * 0.002f);
        float c2 = Mathf.PerlinNoise((x + seed + 10000) * 0.008f, (z + seed + 10000) * 0.008f);
        float c3 = Mathf.PerlinNoise((x + seed + 20000) * 0.025f, (z + seed + 20000) * 0.025f);
        return Mathf.Clamp01(c1 * 0.6f + c2 * 0.3f + c3 * 0.1f);
    }

    public float GetErosion(int x, int z)
    {
        float e1 = Mathf.PerlinNoise((x + seed + 30000) * 0.005f, (z + seed + 30000) * 0.005f);
        float e2 = Mathf.PerlinNoise((x + seed + 40000) * 0.02f, (z + seed + 40000) * 0.02f);
        return e1 * 0.7f + e2 * 0.3f;
    }

    public float GetPeaks(int x, int z)
    {
        float p1 = Mathf.PerlinNoise((x + seed + 50000) * 0.01f, (z + seed + 50000) * 0.01f);
        float p2 = Mathf.PerlinNoise((x + seed + 60000) * 0.04f, (z + seed + 60000) * 0.04f);
        return Mathf.Clamp01(p1 * 0.7f + p2 * 0.3f);
    }

    public float GetMoisture(int x, int z)
    {
        return Mathf.PerlinNoise((x + seed + 5000) * 0.008f, (z + seed + 5000) * 0.008f);
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
}