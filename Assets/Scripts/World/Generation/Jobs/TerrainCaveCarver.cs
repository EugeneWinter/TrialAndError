using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct TerrainCaveCarver
{
    public int seed;

    public bool ShouldCarve(int wx, int wy, int wz, int surfaceHeight, int biomeId)
    {
        if (wy <= 2) return false;

        int depthBelowSurface = surfaceHeight - wy;

        if (depthBelowSurface < 4) return false;

        if (ShouldCarveSurfaceEntrance(wx, wy, wz, surfaceHeight, depthBelowSurface))
            return true;

        if (depthBelowSurface < 12) return false;

        float3 cheeseP = new float3(
            (wx + seed * 3) * 0.025f,
            wy * 0.035f,
            (wz + seed * 3) * 0.025f
        );
        float cheese = noise.snoise(cheeseP) * 0.5f + 0.5f;

        float3 spagP1 = new float3(
            (wx + seed * 7) * 0.04f,
            wy * 0.03f,
            (wz + seed * 7) * 0.04f
        );
        float3 spagP2 = new float3(
            (wx + seed * 13) * 0.04f + 500f,
            wy * 0.03f + 500f,
            (wz + seed * 13) * 0.04f + 500f
        );
        float s1 = math.abs(noise.snoise(spagP1));
        float s2 = math.abs(noise.snoise(spagP2));
        float spaghetti = math.sqrt(s1 * s1 + s2 * s2);

        float depthFactor = math.saturate((depthBelowSurface - 12f) / 20f);
        if (wy < 12) depthFactor *= math.saturate((wy - 2f) / 10f);

        float combined = math.max(cheese, spaghetti * 0.65f);
        combined *= depthFactor;

        return combined > 0.78f;
    }

    private bool ShouldCarveSurfaceEntrance(int wx, int wy, int wz, int surfaceHeight, int depthBelowSurface)
    {
        if (depthBelowSurface < 3 || depthBelowSurface > 30) return false;

        float entranceChance = noise.snoise(new float2(wx * 0.0012f + seed * 0.002f, wz * 0.0012f + seed * 0.002f)) * 0.5f + 0.5f;
        if (entranceChance < 0.88f) return false;

        float2 entrancePos = new float2(
            noise.snoise(new float2(wx * 0.006f + seed * 0.003f, wz * 0.006f + seed * 0.004f)),
            noise.snoise(new float2(wx * 0.006f + seed * 0.005f + 300f, wz * 0.006f + seed * 0.005f + 300f))
        );

        float tunnelDist = math.sqrt(entrancePos.x * entrancePos.x + entrancePos.y * entrancePos.y);
        if (tunnelDist > 0.15f) return false;

        float tunnelRadius = math.lerp(2.5f, 0.8f, math.saturate(depthBelowSurface / 25f));
        float horizontalDist = math.abs(entrancePos.x) * 12f;

        return horizontalDist < tunnelRadius;
    }
}