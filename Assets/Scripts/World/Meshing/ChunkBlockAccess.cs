using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkBlockAccess
{
    [ReadOnly] public NativeArray<ushort> blocks;
    [ReadOnly] public NativeArray<BlockDatabase.BlockVisualData> visualData;
    public int chunkY;

    [ReadOnly] public NativeArray<ushort> neighborXNeg;
    [ReadOnly] public NativeArray<ushort> neighborXPos;
    [ReadOnly] public NativeArray<ushort> neighborZNeg;
    [ReadOnly] public NativeArray<ushort> neighborZPos;
    public bool hasNeighborXNeg;
    public bool hasNeighborXPos;
    public bool hasNeighborZNeg;
    public bool hasNeighborZPos;

    private int Idx(int x, int globalY, int z)
    {
        return x + z * 32 + globalY * 1024;
    }

    public ushort GetBlock(int x, int y, int z)
    {
        int globalY = chunkY * 32 + y;

        if (globalY < 0 || globalY >= 512) return 0;

        if (x >= 0 && x < 32 && z >= 0 && z < 32)
            return blocks[Idx(x, globalY, z)];

        if (x < 0 && hasNeighborXNeg)
            return GetNeighborBlock(neighborXNeg, 31, globalY, z);
        if (x >= 32 && hasNeighborXPos)
            return GetNeighborBlock(neighborXPos, 0, globalY, z);
        if (z < 0 && hasNeighborZNeg)
            return GetNeighborBlock(neighborZNeg, x, globalY, 31);
        if (z >= 32 && hasNeighborZPos)
            return GetNeighborBlock(neighborZPos, x, globalY, 0);

        return 0;
    }

    private ushort GetNeighborBlock(NativeArray<ushort> nb, int lx, int globalY, int lz)
    {
        if (lx < 0 || lx >= 32 || lz < 0 || lz >= 32 || globalY < 0 || globalY >= 512)
            return 0;
        return nb[Idx(lx, globalY, lz)];
    }

    public ushort GetBlockSafe(int x, int y, int z)
    {
        return GetBlock(x, y, z);
    }

    public bool IsSolid(int x, int y, int z)
    {
        ushort id = GetBlock(x, y, z);
        if (id == 0 || id == BlockIDs.Water) return false;
        if (id < visualData.Length) return visualData[id].isSolid;
        return true;
    }

    public bool IsOpaque(int x, int y, int z)
    {
        ushort id = GetBlock(x, y, z);
        if (id == 0) return false;
        if (id < visualData.Length) return !visualData[id].isTransparent;
        return true;
    }

    public bool IsOpaque(ushort id)
    {
        if (id == 0) return false;
        if (id < visualData.Length) return !visualData[id].isTransparent;
        return true;
    }

    public bool IsTransparent(int x, int y, int z)
    {
        ushort id = GetBlock(x, y, z);
        if (id == 0) return true;
        if (id < visualData.Length) return visualData[id].isTransparent;
        return false;
    }

    public bool IsTransparent(ushort id)
    {
        if (id == 0) return true;
        if (id < visualData.Length) return visualData[id].isTransparent;
        return false;
    }

    public bool IsSolid(ushort id)
    {
        if (id == 0 || id == BlockIDs.Water) return false;
        if (id < visualData.Length) return visualData[id].isSolid;
        return true;
    }
}