using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LegacyFontReplacer : EditorWindow
{
    private Font newFont;

    [MenuItem("Tools/Legacy Font Replacer")]
    public static void ShowWindow()
    {
        GetWindow<LegacyFontReplacer>("Legacy Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Legacy Fonts", EditorStyles.boldLabel);

        newFont = (Font)EditorGUILayout.ObjectField(
            "New Font (.ttf/.otf)",
            newFont,
            typeof(Font),
            false
        );

        EditorGUILayout.Space();

        GUI.enabled = newFont != null;

        if (GUILayout.Button("List Legacy Texts In Open Scenes"))
            ListLegacyTextsInOpenScenes();

        if (GUILayout.Button("Replace In Open Scenes"))
            ReplaceInOpenScenes();

        if (GUILayout.Button("Replace In All Project Scenes"))
            ReplaceInAllProjectScenes();

        if (GUILayout.Button("Replace In All Prefabs"))
            ReplaceInAllPrefabs();

        GUI.enabled = true;
    }

    private void ListLegacyTextsInOpenScenes()
    {
        int uiCount = 0;
        int meshCount = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                var uiTexts = root.GetComponentsInChildren<Text>(true);
                foreach (var t in uiTexts)
                {
                    Debug.Log($"[UI.Text] Scene: {scene.name} | {GetHierarchyPath(t.transform)} | Font: {(t.font ? t.font.name : "NULL")}", t.gameObject);
                    uiCount++;
                }

                var meshTexts = root.GetComponentsInChildren<TextMesh>(true);
                foreach (var t in meshTexts)
                {
                    Debug.Log($"[TextMesh] Scene: {scene.name} | {GetHierarchyPath(t.transform)} | Font: {(t.font ? t.font.name : "NULL")}", t.gameObject);
                    meshCount++;
                }
            }
        }

        Debug.Log($"[LegacyFontReplacer] Found {uiCount} UI.Text and {meshCount} TextMesh objects in open scenes.");
    }

    private void ReplaceInOpenScenes()
    {
        int changed = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            bool dirty = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                var uiTexts = root.GetComponentsInChildren<Text>(true);
                foreach (var t in uiTexts)
                {
                    if (t.font == newFont) continue;
                    Undo.RecordObject(t, "Replace Legacy UI Font");
                    t.font = newFont;
                    EditorUtility.SetDirty(t);
                    changed++;
                    dirty = true;
                }

                var meshTexts = root.GetComponentsInChildren<TextMesh>(true);
                foreach (var t in meshTexts)
                {
                    if (t.font == newFont) continue;
                    Undo.RecordObject(t, "Replace TextMesh Font");
                    t.font = newFont;
                    EditorUtility.SetDirty(t);
                    changed++;
                    dirty = true;
                }
            }

            if (dirty)
                EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"[LegacyFontReplacer] Replaced fonts on {changed} legacy text objects in open scenes.");
    }

    private void ReplaceInAllProjectScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

        int totalChanged = 0;
        int changedScenes = 0;

        var setup = EditorSceneManager.GetSceneManagerSetup();

        try
        {
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                int changedInScene = 0;

                foreach (var root in scene.GetRootGameObjects())
                {
                    var uiTexts = root.GetComponentsInChildren<Text>(true);
                    foreach (var t in uiTexts)
                    {
                        if (t.font == newFont) continue;
                        t.font = newFont;
                        EditorUtility.SetDirty(t);
                        changedInScene++;
                    }

                    var meshTexts = root.GetComponentsInChildren<TextMesh>(true);
                    foreach (var t in meshTexts)
                    {
                        if (t.font == newFont) continue;
                        t.font = newFont;
                        EditorUtility.SetDirty(t);
                        changedInScene++;
                    }
                }

                if (changedInScene > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    totalChanged += changedInScene;
                    changedScenes++;
                    Debug.Log($"[LegacyFontReplacer] Scene updated: {scene.name} ({changedInScene} texts)");
                }
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LegacyFontReplacer] Updated {totalChanged} legacy text components across {changedScenes} scenes.");
    }

    private void ReplaceInAllPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        int totalChanged = 0;
        int changedPrefabs = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            bool changed = false;
            int changedInPrefab = 0;

            var uiTexts = root.GetComponentsInChildren<Text>(true);
            foreach (var t in uiTexts)
            {
                if (t.font == newFont) continue;
                t.font = newFont;
                EditorUtility.SetDirty(t);
                changed = true;
                changedInPrefab++;
            }

            var meshTexts = root.GetComponentsInChildren<TextMesh>(true);
            foreach (var t in meshTexts)
            {
                if (t.font == newFont) continue;
                t.font = newFont;
                EditorUtility.SetDirty(t);
                changed = true;
                changedInPrefab++;
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                totalChanged += changedInPrefab;
                changedPrefabs++;
                Debug.Log($"[LegacyFontReplacer] Prefab updated: {root.name} ({changedInPrefab} texts)");
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LegacyFontReplacer] Updated {totalChanged} legacy text components across {changedPrefabs} prefabs.");
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}