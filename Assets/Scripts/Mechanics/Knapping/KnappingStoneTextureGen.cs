using UnityEngine;

public static class KnappingStoneTextureGen
{
    public static Texture2D Generate(int seed, int size = 128)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, true);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        System.Random rng = new System.Random(seed);
        float noiseOffsetX = (float)rng.NextDouble() * 1000f;
        float noiseOffsetY = (float)rng.NextDouble() * 1000f;

        Color baseColor = new Color(0.55f, 0.52f, 0.48f);
        Color darkColor = new Color(0.35f, 0.33f, 0.30f);
        Color lightColor = new Color(0.72f, 0.68f, 0.62f);

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float nx = (x + noiseOffsetX) * 0.05f;
                float ny = (y + noiseOffsetY) * 0.05f;

                float bigNoise = Mathf.PerlinNoise(nx, ny);
                float midNoise = Mathf.PerlinNoise(nx * 3f, ny * 3f) * 0.5f;
                float fineNoise = Mathf.PerlinNoise(nx * 12f, ny * 12f) * 0.2f;

                float value = bigNoise * 0.6f + midNoise + fineNoise;
                value = Mathf.Clamp01(value);

                Color pixel;
                if (value < 0.4f)
                    pixel = Color.Lerp(darkColor, baseColor, value / 0.4f);
                else
                    pixel = Color.Lerp(baseColor, lightColor, (value - 0.4f) / 0.6f);

                if (rng.NextDouble() < 0.03)
                    pixel *= 0.7f;

                pixels[y * size + x] = pixel;
            }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}