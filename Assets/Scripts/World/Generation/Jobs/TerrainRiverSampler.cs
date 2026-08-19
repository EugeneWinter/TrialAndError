using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct TerrainRiverSampler
{
    public NoiseLib Noise;
    public int SeaLevel;

    public struct RiverSample
    {
        public bool HasWater;
        public int GroundHeight;
        public int WaterSurfaceHeight;
        public float RiverWidth;
    }

    public RiverSample Sample(int wx, int wz, TerrainFields f, int baseHeight)
    {
        RiverSample result;
        result.HasWater = false;
        result.GroundHeight = baseHeight;
        result.WaterSurfaceHeight = baseHeight;
        result.RiverWidth = 0f;

        if (f.IsOcean) return result;
        if (f.ContinentFactor < 0.25f) return result;
        if (baseHeight <= SeaLevel + 3) return result;
        if (f.PrimaryBiome == WorldLayout.Desert) return result;
        if (f.PrimaryBiome == WorldLayout.Canyons) return result;

        float2 p = new float2(wx, wz);

        float2 warpOffset = new float2(
            Noise.Fbm(p + 5000f, 0.0005f, 2, 2.0f, 0.5f),
            Noise.Fbm(p + 6000f, 0.0005f, 2, 2.0f, 0.5f)
        ) * 40f;
        float2 warped = p + warpOffset;

        float river1 = math.abs(Noise.Fbm(warped, 0.0007f, 3, 2.0f, 0.45f));
        float river2 = math.abs(Noise.Fbm(warped + 3000f, 0.0012f, 3, 2.0f, 0.45f));
        float riverVal = math.min(river1, river2 * 0.85f + 0.02f);

        float widthNoise = Noise.Fbm(p + 9000f, 0.001f, 2) * 0.5f + 0.5f;
        float riverThreshold = math.lerp(0.025f, 0.045f, widthNoise);

        if (riverVal > riverThreshold) return result;

        float riverStrength = 1f - math.saturate(riverVal / riverThreshold);
        riverStrength = riverStrength * riverStrength;

        float moistureBonus = math.smoothstep(0.3f, 0.7f, f.Moisture);
        riverStrength *= math.lerp(0.6f, 1.0f, moistureBonus);

        if (riverStrength < 0.15f) return result;

        int carveDepth = 2 + (int)(riverStrength * 3f);
        int waterSurface = baseHeight - 1;
        int ground = waterSurface - carveDepth;

        if (ground < SeaLevel) ground = SeaLevel;
        if (ground >= baseHeight - 1) return result;
        if (waterSurface <= ground) return result;

        result.HasWater = true;
        result.GroundHeight = ground;
        result.WaterSurfaceHeight = waterSurface;
        result.RiverWidth = riverStrength;

        return result;
    }
}