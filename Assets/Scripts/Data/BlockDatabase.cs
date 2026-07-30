using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BlockDatabase", menuName = "Game Data/Block Database")]
public class BlockDatabase : ScriptableObject
{
    public Texture2DArray textureArray;
    public List<BlockSO> blocks;

    public struct BlockVisualData
    {
        public int top;
        public int bottom;
        public int north;
        public int south;
        public int east;
        public int west;
        public bool isTransparent;
        public bool isSolid;
        public bool isCustomModel; // <-- днаюбкемн
    }

    private NativeArray<BlockVisualData> _visualData;
    private Dictionary<ushort, BlockSO> _lookup;
    private bool _isInitialized;

    public NativeArray<BlockVisualData> GetVisualData()
    {
        if (!_isInitialized || !_visualData.IsCreated) Initialize();
        return _visualData;
    }

    public BlockSO GetBlockSO(ushort id)
    {
        if (!_isInitialized || _lookup == null) Initialize();
        return _lookup.TryGetValue(id, out var block) ? block : null;
    }

    private void Initialize()
    {
        if (_visualData.IsCreated) _visualData.Dispose();

        int maxId = 0;
        foreach (var b in blocks) if (b != null && b.id > maxId) maxId = b.id;

        _visualData = new NativeArray<BlockVisualData>(maxId + 1, Allocator.Persistent);
        _lookup = new Dictionary<ushort, BlockSO>();

        foreach (var b in blocks)
        {
            if (b == null) continue;
            _visualData[b.id] = new BlockVisualData
            {
                top = b.indexTop,
                bottom = b.indexBottom,
                north = b.indexNorth,
                south = b.indexSouth,
                east = b.indexEast,
                west = b.indexWest,
                isTransparent = b.isTransparent,
                isSolid = b.isSolid,
                isCustomModel = b.isCustomModel
            };
            _lookup[b.id] = b;
        }
        _isInitialized = true;
    }

    public void Dispose()
    {
        if (_visualData.IsCreated) _visualData.Dispose();
        _isInitialized = false;
        _lookup = null;
    }
}