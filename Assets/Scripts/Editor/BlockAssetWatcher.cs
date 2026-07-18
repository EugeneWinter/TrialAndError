using UnityEditor;
using UnityEngine;

public class BlockAssetWatcher : AssetPostprocessor
{
    private static bool isBaking = false;

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (isBaking) return;

        bool shouldRebake = false;

        foreach (string path in importedAssets)
        {
            if (path.Contains("BakedTextureArray")) continue;
            if (path.Contains("BlockDatabase")) continue;

            if (path.EndsWith(".png") && path.Contains("Textures/Blocks"))
            {
                shouldRebake = true;
                break;
            }
        }

        if (!shouldRebake) return;

        string[] guids = AssetDatabase.FindAssets("t:BlockDatabase");
        if (guids.Length == 0) return;

        isBaking = true;

        try
        {
            foreach (string guid in guids)
            {
                string dbPath = AssetDatabase.GUIDToAssetPath(guid);
                BlockDatabase db = AssetDatabase.LoadAssetAtPath<BlockDatabase>(dbPath);

                if (db != null && db.blocks != null && db.blocks.Count > 0)
                {
                    BlockDatabaseEditor.BakeStatic(db);
                    Debug.Log($"[Auto] BlockDatabase rebaked: {dbPath}");
                }
            }
        }
        finally
        {
            isBaking = false;
        }
    }
}