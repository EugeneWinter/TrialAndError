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
    private GameObject renderRoot;
    private GameObject renderBlock;
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
    }

    public Sprite GetIcon(ushort blockId)
    {
        if (iconCache.TryGetValue(blockId, out Sprite cached))
            return cached;

        BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(blockId);
        if (block == null) return null;

        Sprite icon = RenderBlockIcon(block);
        iconCache[blockId] = icon;
        return icon;
    }

    Sprite RenderBlockIcon(BlockSO block)
    {
        if (renderBlock != null) Destroy(renderBlock);

        renderBlock = BlockPreviewFactory.CreateMiniBlock(block, WorldManager.Instance.blockDatabase.textureArray);
        renderBlock.transform.SetParent(renderRoot.transform);
        renderBlock.transform.localPosition = Vector3.zero;
        renderBlock.transform.localEulerAngles = blockRotation;
        renderBlock.transform.localScale = Vector3.one;

        SetLayerRecursively(renderBlock, LayerMask.NameToLayer("HeldItem"));

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

        Destroy(renderBlock);
        renderBlock = null;

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, iconResolution, iconResolution), new Vector2(0.5f, 0.5f), 100f);
        return sprite;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer < 0) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}