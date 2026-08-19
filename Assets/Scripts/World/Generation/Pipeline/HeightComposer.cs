using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct HeightComposer
{
    public int SeaLevel;

    public int Compose(TerrainFields f, NoiseLib noise, int wx, int wz)
    {
        if (f.IsOcean)
            return ComposeOcean(f, noise, wx, wz);

        if (f.PrimaryBiome == WorldLayout.Center)
            return ComposeCenter(f, noise, wx, wz);

        float baseHeight = GetBiomeBaseHeight(f.PrimaryBiome);
        if (f.BiomeBlend > 0.01f && f.SecondaryBiome != f.PrimaryBiome && f.SecondaryBiome != WorldLayout.Ocean)
            baseHeight = math.lerp(baseHeight, GetBiomeBaseHeight(f.SecondaryBiome), f.BiomeBlend * 0.5f);

        float mountainMask = math.smoothstep(0.45f, 0.1f, f.ErosionFolded);
        float relief = f.RidgesFolded * GetMountainAmplitude(f.PrimaryBiome) * mountainMask;
        float hillMask = math.smoothstep(0.25f, 0.55f, f.ErosionFolded);
        relief += GetHillRelief(f) * hillMask;
        relief += GetSpecialRelief(f);

        float coastBlend = math.smoothstep(0.0f, 0.3f, f.ContinentFactor);
        return (int)math.clamp(baseHeight + relief * coastBlend, 5, 500);
    }

    private int ComposeCenter(TerrainFields f, NoiseLib noise, int wx, int wz)
    {
        float2 p = new float2(wx, wz);
        float2 warped = noise.Warp(p, 0.0007f, 30f);

        float ef = f.ErosionFolded;
        float rf = f.RidgesFolded;
        float region = f.Region;

        float baseOffset = TerrainSpline.Eval4(f.Continentalness,
            TerrainSpline.P(-0.5f, -8f, 0f),
            TerrainSpline.P(-0.1f, 2f, 0f),
            TerrainSpline.P(0.3f, 10f, 0f),
            TerrainSpline.P(0.8f, 16f, 0f)
        );

        float terrainHeight = ComposeCenterErosionSpline(ef, rf, region, noise, wx, wz);

        float asymmetric = terrainHeight >= 0f ? terrainHeight * 3.5f : terrainHeight * 0.8f;

        float roughness = 0f;
        if (ef < 0.4f)
        {
            float roughMask = math.smoothstep(0.4f, 0.15f, ef);
            float rough3d = noise.Fbm(warped, 0.012f, 3, 2.0f, 0.35f);
            roughness = rough3d * 4f * roughMask;
        }

        float detail = noise.Fbm(p, 0.025f, 2, 2.0f, 0.3f) * 1.5f;

        float coastBlend = math.smoothstep(0.0f, 0.35f, f.ContinentFactor);
        float total = SeaLevel + baseOffset + (asymmetric + roughness + detail) * coastBlend;

        return (int)math.clamp(total, 5, 500);
    }

    private float ComposeCenterErosionSpline(float ef, float rf, float region, NoiseLib noise, int wx, int wz)
    {
        float mountainZone = ComposeCenterMountainZone(rf, noise, wx, wz);
        float foothillZone = ComposeCenterFoothillZone(rf, region);
        float plainZone = ComposeCenterPlainZone(rf, region, noise, wx, wz);
        float flatZone = ComposeCenterFlatZone(rf);

        return TerrainSpline.Eval5(ef,
            TerrainSpline.P(0.0f, mountainZone, 0f),
            TerrainSpline.P(0.15f, foothillZone, 0f),
            TerrainSpline.P(0.35f, plainZone, 0f),
            TerrainSpline.P(0.6f, flatZone, 0f),
            TerrainSpline.P(1.0f, flatZone * 0.5f, 0f)
        );
    }

    private float ComposeCenterMountainZone(float rf, NoiseLib noise, int wx, int wz)
    {
        float2 p = new float2(wx, wz);

        float baseRidge = noise.Ridge(p, 0.0004f, 4);

        float2 shiftedP = p + new float2(170f, 0f);
        float slopeX = baseRidge - noise.Ridge(shiftedP, 0.0004f, 4);
        float2 shiftedP2 = p + new float2(0f, 170f);
        float slopeZ = baseRidge - noise.Ridge(shiftedP2, 0.0004f, 4);

        float detailShifted = noise.Fbm(
            p + new float2(math.abs(slopeX) * 2000f, math.abs(slopeZ) * 2000f),
            0.002f, 3, 2.0f, 0.4f
        );

        float weathering = math.abs(noise.Fbm(p, 0.003f, 2, 2.5f, 0.5f));
        float weatherMask = 1f - math.saturate(math.smoothstep(0.02f, 0.5f, weathering));

        float combined = (baseRidge + detailShifted * 0.5f * weatherMask);

        return TerrainSpline.Eval4(rf,
            TerrainSpline.P(0.0f, -3f, 0f),
            TerrainSpline.P(0.08f, 8f, 80f),
            TerrainSpline.P(0.2f, 18f, 20f),
            TerrainSpline.P(0.5f, combined * 35f, 0f)
        );
    }

    private float ComposeCenterFoothillZone(float rf, float region)
    {
        float rollingHills = TerrainSpline.Eval3(rf,
            TerrainSpline.P(0.0f, -2f, 0f),
            TerrainSpline.P(0.12f, 4f, 30f),
            TerrainSpline.P(0.35f, 10f, 0f)
        );

        float gentleHills = TerrainSpline.Eval3(rf,
            TerrainSpline.P(0.0f, -1.5f, 0f),
            TerrainSpline.P(0.1f, 2f, 15f),
            TerrainSpline.P(0.3f, 6f, 0f)
        );

        float t = math.smoothstep(-0.2f, 0.2f, region);
        return math.lerp(rollingHills, gentleHills, t);
    }

    private float ComposeCenterPlainZone(float rf, float region, NoiseLib noise, int wx, int wz)
    {
        float valley = TerrainSpline.Eval4(rf,
            TerrainSpline.P(0.0f, -4f, 0f),
            TerrainSpline.P(0.03f, -2f, 10f),
            TerrainSpline.P(0.1f, 1f, 0f),
            TerrainSpline.P(0.25f, 2.5f, 0f)
        );

        float outcropNoise = math.abs(noise.Fbm(new float2(wx, wz) + 800f, 0.007f, 3, 2.0f, 0.4f));
        float outcrop = math.smoothstep(0.55f, 0.7f, outcropNoise) * 6f;

        if (region > 0.3f)
        {
            float lakeMask = math.smoothstep(0.15f, 0.02f, rf);
            valley -= lakeMask * 5f;
        }

        return valley + outcrop;
    }

    private float ComposeCenterFlatZone(float rf)
    {
        return TerrainSpline.Eval3(rf,
            TerrainSpline.P(0.0f, -3f, 0f),
            TerrainSpline.P(0.04f, -0.5f, 8f),
            TerrainSpline.P(0.12f, 1f, 0f)
        );
    }

    private int ComposeOcean(TerrainFields f, NoiseLib noise, int wx, int wz)
    {
        float2 p = new float2(wx, wz);
        float broad = noise.Fbm(p, 0.0025f, 3, 2.0f, 0.42f) * 0.5f + 0.5f;
        float detail = noise.Fbm(p, 0.008f, 2, 2.0f, 0.35f) * 0.5f + 0.5f;
        float reef = math.max(0f, 1f - 6f * math.abs(noise.Fbm(p, 0.005f, 2)));
        float depth = SeaLevel - 22f + broad * 14f + detail * 3f + reef * 5f;
        return (int)math.clamp(depth, 5, SeaLevel - 3);
    }

    private float GetBiomeBaseHeight(int biome)
    {
        if (biome == WorldLayout.Center) return SeaLevel + 8;
        if (biome == WorldLayout.Canyons) return SeaLevel + 45;
        if (biome == WorldLayout.DeciduousForest) return SeaLevel + 15;
        if (biome == WorldLayout.IceWastes) return SeaLevel + 5;
        if (biome == WorldLayout.Savanna) return SeaLevel + 5;
        if (biome == WorldLayout.SnowyMountains) return SeaLevel + 35;
        if (biome == WorldLayout.Taiga) return SeaLevel + 4;
        if (biome == WorldLayout.Desert) return SeaLevel + 8;
        if (biome == WorldLayout.Tropics) return SeaLevel + 12;
        return SeaLevel;
    }

    private float GetMountainAmplitude(int biome)
    {
        if (biome == WorldLayout.SnowyMountains) return 350f;
        if (biome == WorldLayout.Tropics) return 80f;
        if (biome == WorldLayout.DeciduousForest) return 60f;
        if (biome == WorldLayout.Canyons) return 30f;
        if (biome == WorldLayout.Taiga) return 20f;
        return 10f;
    }

    private float GetHillRelief(TerrainFields f)
    {
        int biome = f.PrimaryBiome;
        if (biome == WorldLayout.DeciduousForest) return TriangleWave(f.Region * 5f) * 25f + f.Erosion * 10f;
        if (biome == WorldLayout.Savanna) return 4f + math.smoothstep(0.82f, 0.95f, f.RidgesFolded) * 40f;
        if (biome == WorldLayout.Taiga) return TriangleWave(f.Region * 3f) * 12f;
        if (biome == WorldLayout.Tropics) return TriangleWave(f.Region * 4f) * 20f;
        if (biome == WorldLayout.IceWastes) return f.Erosion * 8f;
        if (biome == WorldLayout.Desert)
        {
            float dunes = math.sin(f.Region * 8f) * 12f + math.sin(f.Region * 13f) * 6f;
            return dunes * math.smoothstep(0.3f, 0.7f, f.ErosionFolded);
        }
        return 0f;
    }

    private float GetSpecialRelief(TerrainFields f)
    {
        if (f.PrimaryBiome == WorldLayout.Canyons)
            return -math.smoothstep(0.12f, 0.0f, f.RidgesFolded) * 65f;
        if (f.PrimaryBiome == WorldLayout.Tropics)
            return -math.smoothstep(0.88f, 0.95f, f.ErosionFolded) * 15f;
        return 0f;
    }

    private float TriangleWave(float x)
    {
        return math.abs(x - math.floor(x + 0.5f)) * 2f;
    }
}