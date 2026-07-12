using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkMeshJob : IJob
{
    [ReadOnly] public NativeArray<ushort> blocks;
    [ReadOnly] public NativeArray<BlockDatabase.BlockVisualData> visualData;

    public NativeList<float3> vertices;
    public NativeList<int> triangles;
    public NativeList<float3> uvs;
    public NativeList<float3> normals;

    public void Execute()
    {
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                for (int z = 0; z < 32; z++)
                {
                    ushort block = GetBlock(x, y, z);
                    if (block == 0) continue;

                    float3 pos = new float3(x, y, z);
                    var v = visualData[block];

                    if (GetBlock(x, y + 1, z) == 0)
                        AddFace(pos + new float3(0, 1, 0), pos + new float3(0, 1, 1), pos + new float3(1, 1, 1), pos + new float3(1, 1, 0), new float3(0, 1, 0), v.top);
                    if (GetBlock(x, y - 1, z) == 0)
                        AddFace(pos + new float3(0, 0, 1), pos + new float3(0, 0, 0), pos + new float3(1, 0, 0), pos + new float3(1, 0, 1), new float3(0, -1, 0), v.bottom);
                    if (GetBlock(x, y, z - 1) == 0)
                        AddFace(pos + new float3(0, 0, 0), pos + new float3(0, 1, 0), pos + new float3(1, 1, 0), pos + new float3(1, 0, 0), new float3(0, 0, -1), v.north);
                    if (GetBlock(x, y, z + 1) == 0)
                        AddFace(pos + new float3(1, 0, 1), pos + new float3(1, 1, 1), pos + new float3(0, 1, 1), pos + new float3(0, 0, 1), new float3(0, 0, 1), v.south);
                    if (GetBlock(x - 1, y, z) == 0)
                        AddFace(pos + new float3(0, 0, 1), pos + new float3(0, 1, 1), pos + new float3(0, 1, 0), pos + new float3(0, 0, 0), new float3(-1, 0, 0), v.west);
                    if (GetBlock(x + 1, y, z) == 0)
                        AddFace(pos + new float3(1, 0, 0), pos + new float3(1, 1, 0), pos + new float3(1, 1, 1), pos + new float3(1, 0, 1), new float3(1, 0, 0), v.east);
                }
    }

    ushort GetBlock(int x, int y, int z)
    {
        if (x < 0 || x >= 32 || y < 0 || y >= 32 || z < 0 || z >= 32) return 0;
        return blocks[x + y * 32 + z * 32 * 32];
    }

    void AddFace(float3 v1, float3 v2, float3 v3, float3 v4, float3 normal, int texLayer)
    {
        int vc = vertices.Length;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);
        triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 2);
        triangles.Add(vc); triangles.Add(vc + 2); triangles.Add(vc + 3);
        uvs.Add(new float3(0, 0, texLayer)); uvs.Add(new float3(0, 1, texLayer));
        uvs.Add(new float3(1, 1, texLayer)); uvs.Add(new float3(1, 0, texLayer));
        normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);
    }
}