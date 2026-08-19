using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BlockDatabase))]
public class BlockDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BlockDatabase db = (BlockDatabase)target;

        if (GUILayout.Button("Bake Texture Array & Update Blocks", GUILayout.Height(30)))
        {
            BakeStatic(db);
        }
    }

    public static void BakeStatic(BlockDatabase db)
    {
        GeneratePaletteTextures(db);

        List<Texture2D> uniqueTextures = new List<Texture2D>();

        foreach (var block in db.blocks)
        {
            if (block == null) continue;
            block.AutoFill();

            AddTexture(block.texTop, uniqueTextures);
            AddTexture(block.texBottom, uniqueTextures);
            AddTexture(block.texNorth, uniqueTextures);
            AddTexture(block.texSouth, uniqueTextures);
            AddTexture(block.texEast, uniqueTextures);
            AddTexture(block.texWest, uniqueTextures);
        }

        if (uniqueTextures.Count == 0) return;

        int size = uniqueTextures[0].width;
        Texture2DArray texArray = new Texture2DArray(size, size, uniqueTextures.Count, TextureFormat.RGBA32, false);
        texArray.filterMode = FilterMode.Point;
        texArray.wrapMode = TextureWrapMode.Repeat;
        texArray.name = "BakedTextureArray";

        for (int i = 0; i < uniqueTextures.Count; i++)
        {
            texArray.SetPixels(uniqueTextures[i].GetPixels(), i);
        }
        texArray.Apply();

        string path = "Assets/Data/BakedTextureArray.asset";
        Texture2DArray existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(texArray, existing);
            db.textureArray = existing;
        }
        else
        {
            AssetDatabase.CreateAsset(texArray, path);
            db.textureArray = texArray;
        }

        foreach (var block in db.blocks)
        {
            if (block == null) continue;
            block.indexTop = uniqueTextures.IndexOf(block.texTop);
            block.indexBottom = uniqueTextures.IndexOf(block.texBottom);
            block.indexNorth = uniqueTextures.IndexOf(block.texNorth);
            block.indexSouth = uniqueTextures.IndexOf(block.texSouth);
            block.indexEast = uniqueTextures.IndexOf(block.texEast);
            block.indexWest = uniqueTextures.IndexOf(block.texWest);
            EditorUtility.SetDirty(block);
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        if (Application.isPlaying && WorldManager.Instance != null)
            WorldManager.Instance.RefreshAllChunks();
    }

    private static void GeneratePaletteTextures(BlockDatabase db)
    {
        string genFolder = "Assets/Art/Textures/Generated";
        if (!System.IO.Directory.Exists(genFolder))
            System.IO.Directory.CreateDirectory(genFolder);

        bool needRefresh = false;

        foreach (var block in db.blocks)
        {
            if (block == null) continue;

            if (block.usePalette && !block.useMultiFacePalette && block.masterTexture != null)
            {
                needRefresh |= GenerateSingleFace(block, genFolder);
            }
            else if (block.useMultiFacePalette)
            {
                needRefresh |= GenerateMultiFace(block, genFolder);
            }
        }

        if (needRefresh)
        {
            AssetDatabase.Refresh();
            ApplyGeneratedTextures(db, genFolder);
            AssetDatabase.SaveAssets();
        }
    }

    private static bool GenerateSingleFace(BlockSO block, string genFolder)
    {
        if (block.palette == null || block.palette.Length == 0) return false;
        EnsureReadable(block.masterTexture);
        string path = $"{genFolder}/{block.blockName}_Gen.png";
        SaveSwappedTexture(block.masterTexture, block.palette, path);
        return true;
    }

    private static bool GenerateMultiFace(BlockSO block, string genFolder)
    {
        bool changed = false;

        if (block.masterTop != null && block.paletteTop != null && block.paletteTop.Length > 0)
        {
            EnsureReadable(block.masterTop);
            string topPath = $"{genFolder}/{block.blockName}_Top_Gen.png";
            SaveSwappedTexture(block.masterTop, block.paletteTop, topPath);
            changed = true;
        }

        if (block.masterSide != null && block.paletteSide != null && block.paletteSide.Length > 0)
        {
            EnsureReadable(block.masterSide);
            string sidePath = $"{genFolder}/{block.blockName}_Side_Gen.png";
            SaveSwappedTexture(block.masterSide, block.paletteSide, sidePath);
            changed = true;
        }

        if (block.masterBottom != null && block.paletteBottom != null && block.paletteBottom.Length > 0)
        {
            EnsureReadable(block.masterBottom);
            string bottomPath = $"{genFolder}/{block.blockName}_Bottom_Gen.png";
            SaveSwappedTexture(block.masterBottom, block.paletteBottom, bottomPath);
            changed = true;
        }

        return changed;
    }

    private static void SaveSwappedTexture(Texture2D master, Color[] palette, string path)
    {
        int paletteSize = palette.Length;
        Texture2D result = new Texture2D(master.width, master.height, TextureFormat.RGBA32, false);
        result.filterMode = FilterMode.Point;

        Color[] pixels = master.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.01f)
            {
                pixels[i] = new Color(0f, 0f, 0f, 0f);
                continue;
            }

            float brightness = pixels[i].r;
            int index = Mathf.FloorToInt(brightness * paletteSize);
            index = Mathf.Clamp(index, 0, paletteSize - 1);

            Color swapped = palette[index];
            swapped.a = pixels[i].a;
            pixels[i] = swapped;
        }

        result.SetPixels(pixels);
        result.Apply();
        System.IO.File.WriteAllBytes(path, result.EncodeToPNG());
    }

    private static void ApplyGeneratedTextures(BlockDatabase db, string genFolder)
    {
        foreach (var block in db.blocks)
        {
            if (block == null) continue;

            if (block.usePalette && !block.useMultiFacePalette && block.masterTexture != null)
            {
                string path = $"{genFolder}/{block.blockName}_Gen.png";
                ConfigureImportedTexture(path, block.isTransparent);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    block.texTop = tex;
                    block.texBottom = tex;
                    block.texNorth = tex;
                    block.texSouth = tex;
                    block.texEast = tex;
                    block.texWest = tex;
                    EditorUtility.SetDirty(block);
                }
            }
            else if (block.useMultiFacePalette)
            {
                if (block.masterTop != null)
                {
                    string topPath = $"{genFolder}/{block.blockName}_Top_Gen.png";
                    ConfigureImportedTexture(topPath, block.isTransparent);
                    Texture2D topTex = AssetDatabase.LoadAssetAtPath<Texture2D>(topPath);
                    if (topTex != null) block.texTop = topTex;
                }

                if (block.masterSide != null)
                {
                    string sidePath = $"{genFolder}/{block.blockName}_Side_Gen.png";
                    ConfigureImportedTexture(sidePath, block.isTransparent);
                    Texture2D sideTex = AssetDatabase.LoadAssetAtPath<Texture2D>(sidePath);
                    if (sideTex != null)
                    {
                        block.texNorth = sideTex;
                        block.texSouth = sideTex;
                        block.texEast = sideTex;
                        block.texWest = sideTex;
                    }
                }

                if (block.masterBottom != null)
                {
                    string bottomPath = $"{genFolder}/{block.blockName}_Bottom_Gen.png";
                    ConfigureImportedTexture(bottomPath, block.isTransparent);
                    Texture2D bottomTex = AssetDatabase.LoadAssetAtPath<Texture2D>(bottomPath);
                    if (bottomTex != null) block.texBottom = bottomTex;
                }

                EditorUtility.SetDirty(block);
            }
        }
    }

    private static void ConfigureImportedTexture(string path, bool hasAlpha)
    {
        TextureImporter texImp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (texImp == null) return;

        bool changed = false;
        if (texImp.textureType != TextureImporterType.Default) { texImp.textureType = TextureImporterType.Default; changed = true; }
        if (texImp.filterMode != FilterMode.Point) { texImp.filterMode = FilterMode.Point; changed = true; }
        if (texImp.textureCompression != TextureImporterCompression.Uncompressed) { texImp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (!texImp.isReadable) { texImp.isReadable = true; changed = true; }
        if (texImp.mipmapEnabled) { texImp.mipmapEnabled = false; changed = true; }
        if (texImp.alphaIsTransparency != hasAlpha) { texImp.alphaIsTransparency = hasAlpha; changed = true; }
        if (changed) texImp.SaveAndReimport();
    }

    private static void EnsureReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }

    private static void AddTexture(Texture2D tex, List<Texture2D> list)
    {
        if (tex != null && !list.Contains(tex))
            list.Add(tex);
    }
}