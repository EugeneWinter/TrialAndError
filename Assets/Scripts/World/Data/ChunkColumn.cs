using System;
using Unity.Collections;
using Unity.Mathematics;

public class ChunkColumn : IDisposable
{
    public const int SIZE_XZ = 32;
    public const int HEIGHT = 512;
    public const int SLICE = SIZE_XZ * SIZE_XZ;
    public const int BLOCKS_PER_COLUMN = SIZE_XZ * HEIGHT * SIZE_XZ;

    public readonly int2 Position;
    public NativeArray<ushort> Blocks;
    public NativeArray<int> Heightmap;
    public NativeArray<byte> Biomes;

    public bool IsDataGenerated;
    public bool IsMeshGenerated;

    public ChunkColumn(int2 pos)
    {
        Position = pos;
        Blocks = new NativeArray<ushort>(BLOCKS_PER_COLUMN, Allocator.Persistent);
        Heightmap = new NativeArray<int>(SLICE, Allocator.Persistent);
        Biomes = new NativeArray<byte>(SLICE, Allocator.Persistent);
    }

    public static int BlockIndex(int x, int y, int z)
    {
        return x + z * SIZE_XZ + y * SLICE;
    }

    public ushort GetBlock(int lx, int ly, int lz)
    {
        if (lx < 0 || lx >= SIZE_XZ || lz < 0 || lz >= SIZE_XZ || ly < 0 || ly >= HEIGHT)
            return 0;
        return Blocks[BlockIndex(lx, ly, lz)];
    }

    public void SetBlock(int lx, int ly, int lz, ushort id)
    {
        if (lx < 0 || lx >= SIZE_XZ || lz < 0 || lz >= SIZE_XZ || ly < 0 || ly >= HEIGHT)
            return;
        Blocks[BlockIndex(lx, ly, lz)] = id;
    }

    public void Dispose()
    {
        if (Blocks.IsCreated) Blocks.Dispose();
        if (Heightmap.IsCreated) Heightmap.Dispose();
        if (Biomes.IsCreated) Biomes.Dispose();
    }
}