using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    private PreviewRenderUtility previewRender;
    private GameObject previewObject;
    private ItemSO item;
    private Vector2 dragDelta;
    private bool previewDirty = true;
    private Texture2D cachedPreview;

    void OnEnable()
    {
        item = (ItemSO)target;
    }

    void OnDisable()
    {
        CleanupPreview();
    }

    void CleanupPreview()
    {
        if (previewObject != null)
            DestroyImmediate(previewObject);

        if (previewRender != null)
        {
            previewRender.Cleanup();
            previewRender = null;
        }

        if (cachedPreview != null)
        {
            DestroyImmediate(cachedPreview);
            cachedPreview = null;
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
            previewDirty = true;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Icon Preview", EditorStyles.boldLabel);

        GameObject model = item.groundModel != null ? item.groundModel : item.heldModel;

        if (model == null)
        {
            EditorGUILayout.HelpBox("No model assigned. Set Ground Model or Held Model to see preview.", MessageType.Info);
            return;
        }

        if (previewDirty || cachedPreview == null)
        {
            RenderPreview(model);
            previewDirty = false;
        }

        if (cachedPreview != null)
        {
            Rect previewRect = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false));
            previewRect.x = (EditorGUIUtility.currentViewWidth - 128) * 0.5f;

            EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f));
            GUI.DrawTexture(previewRect, cachedPreview, ScaleMode.ScaleToFit, true);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Refresh Preview"))
            previewDirty = true;
    }

    void RenderPreview(GameObject modelPrefab)
    {
        CleanupPreview();

        previewRender = new PreviewRenderUtility();

        previewRender.camera.clearFlags = CameraClearFlags.SolidColor;
        previewRender.camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        previewRender.camera.orthographic = true;
        previewRender.camera.orthographicSize = 0.8f;
        previewRender.camera.nearClipPlane = 0.01f;
        previewRender.camera.farClipPlane = 100f;
        previewRender.camera.transform.position = new Vector3(0, 0, -2);
        previewRender.camera.transform.LookAt(Vector3.zero);

        previewRender.lights[0].transform.position = new Vector3(1, 3, -2);
        previewRender.lights[0].transform.LookAt(Vector3.zero);
        previewRender.lights[0].intensity = 3f;
        previewRender.lights[0].color = Color.white;

        if (previewRender.lights.Length > 1)
        {
            previewRender.lights[1].transform.position = new Vector3(-2, 1, 1);
            previewRender.lights[1].transform.LookAt(Vector3.zero);
            previewRender.lights[1].intensity = 1.5f;
            previewRender.lights[1].color = new Color(0.8f, 0.85f, 1f);
            previewRender.lights[1].enabled = true;
        }

        previewRender.ambientColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        previewObject = Instantiate(modelPrefab);
        previewObject.transform.position = item.iconPosition;
        previewObject.transform.eulerAngles = item.iconRotation;
        previewObject.transform.localScale = Vector3.one * item.iconScale;
        previewObject.hideFlags = HideFlags.HideAndDontSave;

        SetLayerRecursively(previewObject, previewRender.camera.gameObject.layer);

        previewRender.AddSingleGO(previewObject);

        previewRender.BeginPreview(new Rect(0, 0, 128, 128), GUIStyle.none);
        previewRender.camera.Render();
        Texture resultTex = previewRender.EndPreview();

        cachedPreview = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        cachedPreview.filterMode = FilterMode.Point;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = resultTex as RenderTexture;
        cachedPreview.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
        cachedPreview.Apply();
        RenderTexture.active = prev;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}