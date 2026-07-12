using Unity.Collections;
using Unity.Mathematics;

public struct ChunkData
{
    public const int SIZE = 32;
    public NativeArray<ushort> blocks;
    public int3 position;

    public void Initialize(int3 pos)
    {
        position = pos;
        blocks = new NativeArray<ushort>(SIZE * SIZE * SIZE, Allocator.Persistent);
    }

    public void Dispose()
    {
        if (blocks.IsCreated) blocks.Dispose();
    }

    public ushort GetBlock(int x, int y, int z)
    {
        if (x < 0 || x >= SIZE || y < 0 || y >= SIZE || z < 0 || z >= SIZE) return 0;
        return blocks[x + y * SIZE + z * SIZE * SIZE];
    }

    public void SetBlock(int x, int y, int z, ushort id)
    {
        if (x < 0 || x >= SIZE || y < 0 || y >= SIZE || z < 0 || z >= SIZE) return;
        blocks[x + y * SIZE + z * SIZE * SIZE] = id;
    }
}