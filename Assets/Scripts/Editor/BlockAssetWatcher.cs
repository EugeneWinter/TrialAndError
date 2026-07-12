using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

public class BlockAssetWatcher : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool shouldRebake = false;

        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath<BlockSO>(path) != null)
            {
                shouldRebake = true;
                break;
            }

            if ((path.EndsWith(".png") || path.EndsWith(".jpg")) && path.Contains("Textures/Blocks"))
            {
                shouldRebake = true;
                break;
            }
        }

        if (!shouldRebake) return;

        string[] guids = AssetDatabase.FindAssets("t:BlockDatabase");
        if (guids.Length == 0) return;

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
}