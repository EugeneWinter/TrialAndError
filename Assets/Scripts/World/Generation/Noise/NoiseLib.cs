using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct NoiseLib
{
    public int Seed;

    public float GetNoise2D(float2 p, float scale)
    {
        return noise.snoise(p * scale + Seed * 0.01f);
    }

    public float Fbm(float2 p, float scale, int octaves, float lacunarity = 2.0f, float persistence = 0.5f)
    {
        float sum = 0;
        float amp = 1;
        float freq = scale;
        float maxAmp = 0;

        for (int i = 0; i < octaves; i++)
        {
            sum += noise.snoise(p * freq + (Seed + i * 1234)) * amp;
            maxAmp += amp;
            amp *= persistence;
            freq *= lacunarity;
        }

        return sum / maxAmp;
    }

    public float Ridge(float2 p, float scale, int octaves)
    {
        float sum = 0;
        float amp = 1;
        float freq = scale;
        float maxAmp = 0;

        for (int i = 0; i < octaves; i++)
        {
            float n = noise.snoise(p * freq + (Seed + i * 5678));
            n = 1.0f - math.abs(n);
            n = n * n;
            sum += n * amp;
            maxAmp += amp;
            amp *= 0.5f;
            freq *= 2.0f;
        }

        return sum / maxAmp;
    }

    public float Fold(float value)
    {
        return math.abs(value);
    }

    public float2 Warp(float2 p, float scale, float strength)
    {
        float2 offset = new float2(
            noise.snoise(p * scale + Seed * 0.1f),
            noise.snoise(p * scale + (Seed + 100) * 0.1f)
        );
        return p + offset * strength;
    }
}