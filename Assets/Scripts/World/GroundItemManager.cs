using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class GroundItemManager : MonoBehaviour, IGameSystem
{
    public static GroundItemManager Instance;

    public class GroundBlockData
    {
        public ushort[] items = new ushort[4];
        public int count = 0;
    }

    [SerializeField] private ItemDatabase itemDatabase;

    private Dictionary<int3, GroundBlockData> groundItems = new Dictionary<int3, GroundBlockData>();
    private Dictionary<ushort, List<Matrix4x4>> renderBatches = new Dictionary<ushort, List<Matrix4x4>>();
    private Dictionary<ushort, CachedItemRenderData> renderDataCache = new Dictionary<ushort, CachedItemRenderData>();
    private bool batchMode = false;

    private struct CachedItemRenderData
    {
        public Mesh mesh;
        public Material material;
        public bool valid;
    }

    private readonly Vector3[] slotOffsets = new Vector3[4]
    {
        new Vector3(0.25f, 0f, 0.25f),
        new Vector3(0.75f, 0f, 0.25f),
        new Vector3(0.25f, 0f, 0.75f),
        new Vector3(0.75f, 0f, 0.75f)
    };

    private static readonly Matrix4x4[] batchBuffer = new Matrix4x4[1023];

    void Awake()
    {
        Instance = this;
    }

    public void InitializeSystem() { }

    void Update()
    {
        DrawInstancedItems();
    }

    public void BeginBatch()
    {
        batchMode = true;
    }

    public void EndBatch()
    {
        batchMode = false;
        RebuildRenderBatches();
    }

    public bool TryPlaceItem(int3 blockPos, ushort itemId)
    {
        if (!groundItems.ContainsKey(blockPos))
            groundItems[blockPos] = new GroundBlockData();

        GroundBlockData data = groundItems[blockPos];
        if (data.count >= 4) return false;

        data.items[data.count] = itemId;
        data.count++;

        if (!batchMode)
            RebuildRenderBatches();

        return true;
    }

    public ushort TryTakeItem(int3 blockPos)
    {
        if (!groundItems.ContainsKey(blockPos)) return 0;

        GroundBlockData data = groundItems[blockPos];
        if (data.count == 0) return 0;

        ushort itemId = data.items[data.count - 1];
        data.items[data.count - 1] = 0;
        data.count--;

        if (data.count == 0)
            groundItems.Remove(blockPos);

        if (!batchMode)
            RebuildRenderBatches();

        return itemId;
    }

    public ushort[] GetItemsOnBlock(int3 blockPos)
    {
        if (!groundItems.ContainsKey(blockPos)) return System.Array.Empty<ushort>();

        GroundBlockData data = groundItems[blockPos];
        ushort[] result = new ushort[data.count];
        for (int i = 0; i < data.count; i++) result[i] = data.items[i];
        return result;
    }

    public void ClearBlock(int3 blockPos)
    {
        if (groundItems.Remove(blockPos))
        {
            if (!batchMode)
                RebuildRenderBatches();
        }
    }

    private ItemSO GetItemSO(ushort itemId)
    {
        if (itemDatabase != null)
            return itemDatabase.GetItem(itemId);

        if (Inventory.Instance != null && Inventory.Instance.itemDatabase != null)
            return Inventory.Instance.itemDatabase.GetItem(itemId);

        return null;
    }

    private void RebuildRenderBatches()
    {
        renderBatches.Clear();

        foreach (var kvp in groundItems)
        {
            int3 pos = kvp.Key;
            GroundBlockData data = kvp.Value;

            for (int i = 0; i < data.count; i++)
            {
                ushort itemId = data.items[i];
                if (itemId == 0) continue;

                if (!renderBatches.ContainsKey(itemId))
                    renderBatches[itemId] = new List<Matrix4x4>();

                ItemSO item = GetItemSO(itemId);

                float sink = 0.02f;
                Vector3 scale = Vector3.one * 0.4f;
                Vector3 rotOffset = Vector3.zero;

                if (item != null)
                {
                    sink = item.groundSink;
                    scale = item.groundScale;
                    rotOffset = item.groundRotationOffset;
                }

                System.Random rng = new System.Random(pos.x * 738 + pos.z * 193 + i * 571);
                float yaw = rng.Next(0, 360);

                Vector3 worldPos = new Vector3(pos.x, pos.y, pos.z) + slotOffsets[i];
                worldPos.y -= sink;

                Quaternion rot = Quaternion.Euler(rotOffset.x, yaw + rotOffset.y, rotOffset.z);
                Matrix4x4 matrix = Matrix4x4.TRS(worldPos, rot, scale);
                renderBatches[itemId].Add(matrix);
            }
        }
    }

    private CachedItemRenderData GetRenderData(ushort itemId)
    {
        if (renderDataCache.TryGetValue(itemId, out CachedItemRenderData cached))
            return cached;

        CachedItemRenderData result = new CachedItemRenderData { valid = false };

        ItemSO item = GetItemSO(itemId);
        if (item == null) return result;

        GameObject model = item.groundModel != null ? item.groundModel : item.heldModel;
        if (model == null) return result;

        MeshFilter mf = model.GetComponentInChildren<MeshFilter>();
        MeshRenderer mr = model.GetComponentInChildren<MeshRenderer>();

        if (mf != null && mr != null && mf.sharedMesh != null && mr.sharedMaterial != null)
        {
            result.mesh = mf.sharedMesh;
            result.material = new Material(mr.sharedMaterial);
            result.material.enableInstancing = true;
            result.valid = true;
        }

        renderDataCache[itemId] = result;
        return result;
    }

    private void DrawInstancedItems()
    {
        if (renderBatches.Count == 0) return;

        foreach (var kvp in renderBatches)
        {
            ushort itemId = kvp.Key;
            List<Matrix4x4> matrices = kvp.Value;

            if (matrices.Count == 0) continue;

            CachedItemRenderData rd = GetRenderData(itemId);
            if (!rd.valid) continue;

            for (int i = 0; i < matrices.Count; i += 1023)
            {
                int count = Mathf.Min(1023, matrices.Count - i);
                matrices.CopyTo(i, batchBuffer, 0, count);
                Graphics.DrawMeshInstanced(rd.mesh, 0, rd.material, batchBuffer, count);
            }
        }
    }
}