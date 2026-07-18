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

        if (GUILayout.Button("Bake Texture Array & Update Blocks"))
        {
            BakeStatic(db);
        }
    }

    public static void BakeStatic(BlockDatabase db)
    {
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

    private static void AddTexture(Texture2D tex, List<Texture2D> list)
    {
        if (tex != null && !list.Contains(tex))
            list.Add(tex);
    }
}