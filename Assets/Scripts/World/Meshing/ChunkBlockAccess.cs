using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkBlockAccess
{
    [ReadOnly] public NativeArray<ushort> blocks;
    [ReadOnly] public NativeArray<BlockDatabase.BlockVisualData> visualData;

    [ReadOnly] public NativeArray<ushort> neighborXNeg;
    [ReadOnly] public NativeArray<ushort> neighborXPos;
    [ReadOnly] public NativeArray<ushort> neighborYNeg;
    [ReadOnly] public NativeArray<ushort> neighborYPos;
    [ReadOnly] public NativeArray<ushort> neighborZNeg;
    [ReadOnly] public NativeArray<ushort> neighborZPos;

    public bool hasNeighborXNeg;
    public bool hasNeighborXPos;
    public bool hasNeighborYNeg;
    public bool hasNeighborYPos;
    public bool hasNeighborZNeg;
    public bool hasNeighborZPos;

    public ushort GetBlock(int x, int y, int z)
    {
        if (x >= 0 && x < 32 && y >= 0 && y < 32 && z >= 0 && z < 32)
            return blocks[x + y * 32 + z * 32 * 32];

        return GetNeighborBlock(x, y, z);
    }

    public ushort GetBlockSafe(int x, int y, int z)
    {
        if (x >= 0 && x < 32 && y >= 0 && y < 32 && z >= 0 && z < 32)
            return blocks[x + y * 32 + z * 32 * 32];

        return GetNeighborBlock(x, y, z);
    }

    ushort GetNeighborBlock(int x, int y, int z)
    {
        if (x < 0)
        {
            if (!hasNeighborXNeg) return 0;
            if (y < 0 || y >= 32 || z < 0 || z >= 32) return 0;
            return neighborXNeg[y + z * 32];
        }
        if (x >= 32)
        {
            if (!hasNeighborXPos) return 0;
            if (y < 0 || y >= 32 || z < 0 || z >= 32) return 0;
            return neighborXPos[y + z * 32];
        }
        if (y < 0)
        {
            if (!hasNeighborYNeg) return 0;
            if (x < 0 || x >= 32 || z < 0 || z >= 32) return 0;
            return neighborYNeg[x + z * 32];
        }
        if (y >= 32)
        {
            if (!hasNeighborYPos) return 0;
            if (x < 0 || x >= 32 || z < 0 || z >= 32) return 0;
            return neighborYPos[x + z * 32];
        }
        if (z < 0)
        {
            if (!hasNeighborZNeg) return 0;
            if (x < 0 || x >= 32 || y < 0 || y >= 32) return 0;
            return neighborZNeg[x + y * 32];
        }
        if (z >= 32)
        {
            if (!hasNeighborZPos) return 0;
            if (x < 0 || x >= 32 || y < 0 || y >= 32) return 0;
            return neighborZPos[x + y * 32];
        }

        return 0;
    }

    public bool IsSolid(int x, int y, int z)
    {
        ushort b = GetBlockSafe(x, y, z);
        if (b == 0) return false;
        if (b >= visualData.Length) return false;
        if (visualData[b].isTransparent) return false;
        if (visualData[b].isCustomModel) return false;
        return true;
    }

    public bool IsOpaque(ushort blockId)
    {
        if (blockId == 0) return false;
        if (blockId == 6) return false;
        if (blockId >= visualData.Length) return false;
        if (visualData[blockId].isTransparent) return false;
        if (visualData[blockId].isCustomModel) return false;
        return true;
    }

    public bool IsTransparent(ushort blockId)
    {
        if (blockId >= visualData.Length) return false;
        return visualData[blockId].isTransparent;
    }
}