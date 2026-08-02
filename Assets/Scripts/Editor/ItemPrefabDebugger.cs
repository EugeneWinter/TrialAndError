using UnityEngine;
using UnityEditor;

public class ItemPrefabDebugger : EditorWindow
{
    [MenuItem("Tools/Debug Item Prefabs")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDatabase");
        if (guids.Length == 0)
        {
            Debug.LogError("[ItemDebug] ItemDatabase not found.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        ItemDatabase db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
        if (db == null || db.items == null)
        {
            Debug.LogError("[ItemDebug] ItemDatabase is null or has no items.");
            return;
        }

        Debug.Log($"[ItemDebug] ===== ITEM PREFAB DIAGNOSTICS =====");
        Debug.Log($"[ItemDebug] ItemDatabase: {path}, items: {db.items.Count}");

        foreach (var item in db.items)
        {
            if (item == null)
            {
                Debug.LogWarning("[ItemDebug] Null entry in ItemDatabase.");
                continue;
            }

            Debug.Log($"[ItemDebug] --- Item: {item.itemName} (id={item.id}) ---");

            DebugModel("heldModel", item.heldModel, item);
            DebugModel("groundModel", item.groundModel, item);

            Debug.Log($"  heldPosition: {item.heldPosition}");
            Debug.Log($"  heldRotation: {item.heldRotation}");
            Debug.Log($"  heldScale: {item.heldScale}");
            Debug.Log($"  groundScale: {item.groundScale}");
            Debug.Log($"  groundSink: {item.groundSink}");
            Debug.Log($"  iconPosition: {item.iconPosition}");
            Debug.Log($"  iconRotation: {item.iconRotation}");
            Debug.Log($"  iconScale: {item.iconScale}");
        }

        Debug.Log($"[ItemDebug] ===== END =====");
    }

    static void DebugModel(string label, GameObject prefab, ItemSO item)
    {
        if (prefab == null)
        {
            Debug.LogError($"  [{label}] NOT ASSIGNED for {item.itemName}");
            return;
        }

        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        Debug.Log($"  [{label}] prefab: {prefab.name} path: {prefabPath}");

        MeshFilter rootMF = prefab.GetComponent<MeshFilter>();
        MeshRenderer rootMR = prefab.GetComponent<MeshRenderer>();
        MeshFilter childMF = prefab.GetComponentInChildren<MeshFilter>();
        MeshRenderer childMR = prefab.GetComponentInChildren<MeshRenderer>();

        Debug.Log($"  [{label}] root has MeshFilter: {rootMF != null}");
        Debug.Log($"  [{label}] root has MeshRenderer: {rootMR != null}");
        Debug.Log($"  [{label}] children has MeshFilter: {childMF != null}");
        Debug.Log($"  [{label}] children has MeshRenderer: {childMR != null}");

        if (childMF != null)
        {
            Debug.Log($"  [{label}] mesh: {(childMF.sharedMesh != null ? childMF.sharedMesh.name : "NULL")}");
            Debug.Log($"  [{label}] mesh vertexCount: {(childMF.sharedMesh != null ? childMF.sharedMesh.vertexCount : 0)}");
            Debug.Log($"  [{label}] meshFilter on object: {childMF.gameObject.name}");
            Debug.Log($"  [{label}] meshFilter object active: {childMF.gameObject.activeSelf}");
            Debug.Log($"  [{label}] meshFilter localPosition: {childMF.transform.localPosition}");
            Debug.Log($"  [{label}] meshFilter localScale: {childMF.transform.localScale}");
            Debug.Log($"  [{label}] meshFilter lossyScale: {childMF.transform.lossyScale}");
        }
        else
        {
            Debug.LogError($"  [{label}] NO MeshFilter found anywhere in prefab {prefab.name}");
        }

        if (childMR != null)
        {
            Debug.Log($"  [{label}] renderer on object: {childMR.gameObject.name}");
            Debug.Log($"  [{label}] renderer enabled: {childMR.enabled}");
            Debug.Log($"  [{label}] renderer object active: {childMR.gameObject.activeSelf}");
            Debug.Log($"  [{label}] sharedMaterial: {(childMR.sharedMaterial != null ? childMR.sharedMaterial.name : "NULL")}");
            Debug.Log($"  [{label}] sharedMaterials count: {childMR.sharedMaterials.Length}");

            if (childMR.sharedMaterial != null)
            {
                Debug.Log($"  [{label}] material shader: {childMR.sharedMaterial.shader.name}");
                Texture mainTex = childMR.sharedMaterial.mainTexture;
                Debug.Log($"  [{label}] material mainTexture: {(mainTex != null ? mainTex.name : "NULL")}");
            }

            Bounds bounds = childMR.bounds;
            Debug.Log($"  [{label}] renderer bounds center: {bounds.center}");
            Debug.Log($"  [{label}] renderer bounds size: {bounds.size}");
        }
        else
        {
            Debug.LogError($"  [{label}] NO MeshRenderer found anywhere in prefab {prefab.name}");
        }

        int totalChildren = CountChildren(prefab.transform);
        Debug.Log($"  [{label}] total children (recursive): {totalChildren}");

        LogHierarchy(prefab.transform, label, 0);
    }

    static void LogHierarchy(Transform t, string label, int depth)
    {
        string indent = new string(' ', depth * 4);
        string components = "";

        foreach (var comp in t.GetComponents<Component>())
        {
            if (comp == null) continue;
            if (comp is Transform) continue;
            components += comp.GetType().Name + ", ";
        }

        if (components.Length > 2)
            components = components.Substring(0, components.Length - 2);

        Debug.Log($"  [{label}] {indent}{t.name} active={t.gameObject.activeSelf} pos={t.localPosition} scale={t.localScale} components=[{components}]");

        for (int i = 0; i < t.childCount; i++)
            LogHierarchy(t.GetChild(i), label, depth + 1);
    }

    static int CountChildren(Transform t)
    {
        int count = t.childCount;
        for (int i = 0; i < t.childCount; i++)
            count += CountChildren(t.GetChild(i));
        return count;
    }
}