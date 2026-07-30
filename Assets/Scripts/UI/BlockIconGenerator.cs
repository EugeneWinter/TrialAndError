using UnityEngine;
using System.Collections.Generic;

public class BlockIconGenerator : MonoBehaviour
{
    public static BlockIconGenerator Instance;

    [Header("Icon Settings")]
    public int iconResolution = 128;
    public float iconCameraDistance = 2f;
    public float iconZoom = 0.8f;
    public Vector3 blockRotation = new Vector3(30f, 45f, 0f);

    private Camera iconCamera;
    private Light iconLight;
    private GameObject renderRoot;
    private Dictionary<ushort, Sprite> iconCache = new Dictionary<ushort, Sprite>();

    void Awake()
    {
        Instance = this;
        SetupIconRenderer();
    }

    void SetupIconRenderer()
    {
        renderRoot = new GameObject("IconRenderRoot");
        renderRoot.transform.SetParent(transform);
        renderRoot.transform.position = new Vector3(0, -1000f, 0);

        GameObject camObj = new GameObject("IconCamera");
        camObj.transform.SetParent(renderRoot.transform);
        camObj.transform.localPosition = new Vector3(0, 0, -iconCameraDistance);
        camObj.transform.LookAt(renderRoot.transform.position);

        iconCamera = camObj.AddComponent<Camera>();
        iconCamera.clearFlags = CameraClearFlags.SolidColor;
        iconCamera.backgroundColor = new Color(0, 0, 0, 0);
        iconCamera.orthographic = true;
        iconCamera.orthographicSize = iconZoom;
        iconCamera.cullingMask = 1 << LayerMask.NameToLayer("HeldItem");
        iconCamera.enabled = false;

        GameObject lightObj = new GameObject("IconLight");
        lightObj.transform.SetParent(renderRoot.transform);
        lightObj.transform.localPosition = new Vector3(1, 2, -1);
        lightObj.transform.LookAt(renderRoot.transform.position);
        iconLight = lightObj.AddComponent<Light>();
        iconLight.type = LightType.Directional;
        iconLight.intensity = 1.5f;
        iconLight.cullingMask = 1 << LayerMask.NameToLayer("HeldItem");
    }

    public void ClearCache()
    {
        iconCache.Clear();
    }

    public Sprite GetIcon(ushort id)
    {
        if (iconCache.TryGetValue(id, out Sprite cached))
            return cached;

        Sprite icon = null;

        if (Inventory.Instance != null && Inventory.Instance.itemDatabase != null)
        {
            ItemSO item = Inventory.Instance.itemDatabase.GetItem(id);
            if (item != null)
            {
                GameObject model = item.groundModel != null ? item.groundModel : item.heldModel;
                if (model != null)
                    icon = RenderModelIcon(model, item.iconPosition, item.iconRotation, item.iconScale);
            }
        }

        if (icon == null && WorldManager.Instance != null)
        {
            BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(id);
            if (block != null)
                icon = RenderBlockIcon(block);
        }

        iconCache[id] = icon;
        return icon;
    }

    public Sprite RenderModelIcon(GameObject modelPrefab, Vector3 position, Vector3 rotation, float scale)
    {
        GameObject obj = Instantiate(modelPrefab, renderRoot.transform);
        obj.transform.localPosition = position;
        obj.transform.localEulerAngles = rotation;
        obj.transform.localScale = Vector3.one * scale;

        SetLayerRecursively(obj, LayerMask.NameToLayer("HeldItem"));

        Sprite icon = CaptureIcon();

        DestroyImmediate(obj);
        return icon;
    }

    Sprite RenderBlockIcon(BlockSO block)
    {
        GameObject obj = BlockPreviewFactory.CreateMiniBlock(block, WorldManager.Instance.blockDatabase.textureArray);
        obj.transform.SetParent(renderRoot.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = blockRotation;
        obj.transform.localScale = Vector3.one;

        SetLayerRecursively(obj, LayerMask.NameToLayer("HeldItem"));

        Sprite icon = CaptureIcon();

        DestroyImmediate(obj);
        return icon;
    }

    Sprite CaptureIcon()
    {
        RenderTexture rt = new RenderTexture(iconResolution, iconResolution, 16, RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Point;
        rt.Create();

        iconCamera.targetTexture = rt;
        iconCamera.Render();

        Texture2D tex = new Texture2D(iconResolution, iconResolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, iconResolution, iconResolution), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        iconCamera.targetTexture = null;
        rt.Release();

        return Sprite.Create(tex, new Rect(0, 0, iconResolution, iconResolution), new Vector2(0.5f, 0.5f), 100f);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer < 0) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}