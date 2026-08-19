using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct TerrainFields
{
    public float Continentalness;
    public float Erosion;
    public float ErosionFolded;
    public float Ridges;
    public float RidgesFolded;
    public float Region;
    public float Temperature;
    public float Moisture;
    public float ContinentFactor;
    public int PrimaryBiome;
    public int SecondaryBiome;
    public float BiomeBlend;
    public bool IsOcean;
}

[BurstCompile]
public struct FieldSampler
{
    public NoiseLib Noise;
    public int Seed;

    public TerrainFields Sample(int wx, int wz)
    {
        WorldLayout.WorldSample world = WorldLayout.Sample(wx, wz, Seed);

        float2 p = new float2(wx, wz);

        float rawCont = Noise.Fbm(p, 0.00015f, 5, 2.0f, 0.42f);
        float contSpline = TerrainSpline.Eval3(math.abs(rawCont),
            TerrainSpline.P(0f, 0f, 0f),
            TerrainSpline.P(0.35f, 0.5f, 1f),
            TerrainSpline.P(0.5f, 0.7f, 1f)
        );
        float continentalness = math.lerp(-0.8f, 0.7f, world.ContinentFactor) + contSpline * 0.25f;
        if (world.IsOcean) continentalness = math.min(continentalness, -0.4f);

        float rawErosion = Noise.Fbm(p + 4000f, 0.00028f, 4, 2.0f, 0.42f);
        float erosion = TerrainSpline.Eval4(rawErosion,
            TerrainSpline.P(-0.35f, -0.4f, 0.6f),
            TerrainSpline.P(-0.14f, -0.1f, 0.4f),
            TerrainSpline.P(0.14f, 0.1f, 0.4f),
            TerrainSpline.P(0.35f, 0.4f, 0.6f)
        );
        erosion = BiomeErosionBias(world.PrimaryBiome, erosion);

        float rawRidge = Noise.Ridge(p + 8000f, 0.00045f, 5);
        float ridges = BiomeRidgeBias(world.PrimaryBiome, rawRidge);

        float region = Noise.Fbm(p + 12000f, 0.00035f, 3, 2.0f, 0.4f);
        region += BiomeRegionBias(world.PrimaryBiome);

        return new TerrainFields
        {
            Continentalness = continentalness,
            Erosion = erosion,
            ErosionFolded = math.abs(erosion),
            Ridges = ridges,
            RidgesFolded = math.abs(ridges),
            Region = region,
            Temperature = world.Temperature,
            Moisture = world.Moisture,
            ContinentFactor = world.ContinentFactor,
            PrimaryBiome = world.PrimaryBiome,
            SecondaryBiome = world.SecondaryBiome,
            BiomeBlend = world.Blend,
            IsOcean = world.IsOcean
        };
    }

    private float BiomeErosionBias(int biome, float val)
    {
        if (biome == WorldLayout.Center) return val * 0.9f;
        if (biome == WorldLayout.Canyons) return val * 1.2f + 0.15f;
        if (biome == WorldLayout.SnowyMountains) return val * 0.5f - 0.3f;
        if (biome == WorldLayout.DeciduousForest) return val * 0.9f;
        if (biome == WorldLayout.Savanna) return val * 0.7f + 0.1f;
        if (biome == WorldLayout.IceWastes) return val * 0.5f + 0.2f;
        if (biome == WorldLayout.Taiga) return val * 0.8f + 0.05f;
        if (biome == WorldLayout.Desert) return val * 0.6f + 0.2f;
        if (biome == WorldLayout.Tropics) return val * 0.9f - 0.1f;
        return val;
    }

    private float BiomeRidgeBias(int biome, float val)
    {
        if (biome == WorldLayout.SnowyMountains) return val * 1.4f;
        if (biome == WorldLayout.Canyons) return val * 0.5f;
        if (biome == WorldLayout.Center) return val * 0.7f;
        if (biome == WorldLayout.Savanna) return val * 0.3f;
        if (biome == WorldLayout.IceWastes) return val * 0.2f;
        if (biome == WorldLayout.Desert) return val * 0.4f;
        if (biome == WorldLayout.Tropics) return val * 0.7f;
        if (biome == WorldLayout.DeciduousForest) return val * 0.8f;
        if (biome == WorldLayout.Taiga) return val * 0.5f;
        return val;
    }

    private float BiomeRegionBias(int biome)
    {
        if (biome == WorldLayout.Canyons) return -0.3f;
        if (biome == WorldLayout.Desert) return 0.4f;
        if (biome == WorldLayout.DeciduousForest) return 0.2f;
        if (biome == WorldLayout.Tropics) return 0.15f;
        return 0f;
    }
}