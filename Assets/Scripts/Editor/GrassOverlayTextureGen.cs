using UnityEngine;
using UnityEditor;
using System.IO;

public class GrassOverlayTextureGen
{
    [MenuItem("Tools/Generate Grass Overlay Texture")]
    public static void Generate()
    {
        string sourcePath = "Assets/Art/Textures/Blocks/Grass_Side.png";
        string outputPath = "Assets/Art/Textures/Blocks/Grass_Overlay.png";

        Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
        if (source == null)
        {
            Debug.LogError($"Source texture not found at {sourcePath}");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(source);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool wasReadable = importer.isReadable;
        if (!wasReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        int w = source.width;
        int h = source.height;

        Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
        result.filterMode = FilterMode.Point;
        result.wrapMode = TextureWrapMode.Repeat;

        Color[] sourcePixels = source.GetPixels();
        Color[] outputPixels = new Color[w * h];

        bool[] isGreen = new bool[w * h];
        for (int i = 0; i < w * h; i++)
        {
            isGreen[i] = IsGrassGreen(sourcePixels[i]);
        }

        System.Random rng = new System.Random(42);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int idx = y * w + x;
                Color src = sourcePixels[idx];

                if (isGreen[idx])
                {
                    outputPixels[idx] = new Color(src.r, src.g, src.b, 1f);
                }
                else
                {
                    int distanceToGrass = FindDistanceUpToGreen(x, y, w, h, isGreen);

                    if (distanceToGrass < 0 || distanceToGrass > 6)
                    {
                        outputPixels[idx] = Color.clear;
                    }
                    else
                    {
                        float fringeChance = 1f - (distanceToGrass / 7f);
                        fringeChance *= fringeChance;

                        float columnNoise = Mathf.PerlinNoise(x * 0.7f, 0f);
                        fringeChance *= (0.4f + columnNoise * 0.9f);

                        if (rng.NextDouble() < fringeChance)
                        {
                            Color grassAbove = FindNearestGreenColor(x, y, w, h, sourcePixels, isGreen);
                            outputPixels[idx] = new Color(grassAbove.r, grassAbove.g, grassAbove.b, 1f);
                        }
                        else
                        {
                            outputPixels[idx] = Color.clear;
                        }
                    }
                }
            }
        }

        result.SetPixels(outputPixels);
        result.Apply();

        byte[] pngData = result.EncodeToPNG();
        File.WriteAllBytes(outputPath, pngData);

        if (!wasReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        TextureImporter newImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (newImporter != null)
        {
            newImporter.textureType = TextureImporterType.Default;
            newImporter.filterMode = FilterMode.Point;
            newImporter.wrapMode = TextureWrapMode.Repeat;
            newImporter.alphaIsTransparency = true;
            newImporter.mipmapEnabled = false;
            newImporter.SaveAndReimport();
        }

        Debug.Log($"Grass overlay texture generated at {outputPath}");
    }

    static bool IsGrassGreen(Color c)
    {
        if (c.a < 0.1f) return false;
        if (c.g < 0.25f) return false;
        if (c.g <= c.r) return false;
        if (c.g <= c.b) return false;
        if (c.r + c.b > c.g * 1.6f) return false;
        return true;
    }

    static int FindDistanceUpToGreen(int x, int y, int w, int h, bool[] isGreen)
    {
        for (int dy = 1; dy <= 8; dy++)
        {
            int checkY = y + dy;
            if (checkY >= h) return -1;
            if (isGreen[checkY * w + x]) return dy;
        }
        return -1;
    }

    static Color FindNearestGreenColor(int x, int y, int w, int h, Color[] pixels, bool[] isGreen)
    {
        for (int dy = 1; dy <= 8; dy++)
        {
            int checkY = y + dy;
            if (checkY >= h) break;
            int idx = checkY * w + x;
            if (isGreen[idx]) return pixels[idx];
        }
        return new Color(0.4f, 0.6f, 0.3f, 1f);
    }
}