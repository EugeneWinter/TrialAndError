using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct GrassOverlayBuilder
{
    public ChunkBlockAccess access;
    public ChunkLighting lighting;

    public const ushort GRASS_ID = 2;
    public const float GRASS_OVERHANG = 0.25f;
    public const float GRASS_OUTWARD = 0.02f;

    public void AddOverlays(int x, int y, int z, ref GrassOverlayMeshBuffers buffers)
    {
        float lightHere = lighting.GetLightFloat(x, y, z);
        float lightAbove = lighting.GetLightFloat(x, y + 1, z);
        float sampledLight = math.max(lightHere, lightAbove);
        float aoTop = sampledLight;
        float aoBottom = sampledLight * 0.85f;

        ushort north = access.GetBlock(x, y, z - 1);
        ushort south = access.GetBlock(x, y, z + 1);
        ushort west = access.GetBlock(x - 1, y, z);
        ushort east = access.GetBlock(x + 1, y, z);

        if (!access.IsOpaque(north))
            AddOverlayQuad(x, y, z, 0, aoTop, aoBottom, ref buffers);
        if (!access.IsOpaque(south))
            AddOverlayQuad(x, y, z, 1, aoTop, aoBottom, ref buffers);
        if (!access.IsOpaque(west))
            AddOverlayQuad(x, y, z, 2, aoTop, aoBottom, ref buffers);
        if (!access.IsOpaque(east))
            AddOverlayQuad(x, y, z, 3, aoTop, aoBottom, ref buffers);
    }

    void AddOverlayQuad(int x, int y, int z, int side, float aoTop, float aoBottom, ref GrassOverlayMeshBuffers buffers)
    {
        float3 p = new float3(x, y, z);
        float overhang = GRASS_OVERHANG;
        float outward = GRASS_OUTWARD;

        float3 v1, v2, v3, v4;
        float3 normal;

        if (side == 0)
        {
            float zEdge = -outward;
            v1 = p + new float3(0, 1, zEdge);
            v2 = p + new float3(1, 1, zEdge);
            v3 = p + new float3(1, -overhang, zEdge);
            v4 = p + new float3(0, -overhang, zEdge);
            normal = new float3(0, 0, -1);
        }
        else if (side == 1)
        {
            float zEdge = 1f + outward;
            v1 = p + new float3(1, 1, zEdge);
            v2 = p + new float3(0, 1, zEdge);
            v3 = p + new float3(0, -overhang, zEdge);
            v4 = p + new float3(1, -overhang, zEdge);
            normal = new float3(0, 0, 1);
        }
        else if (side == 2)
        {
            float xEdge = -outward;
            v1 = p + new float3(xEdge, 1, 1);
            v2 = p + new float3(xEdge, 1, 0);
            v3 = p + new float3(xEdge, -overhang, 0);
            v4 = p + new float3(xEdge, -overhang, 1);
            normal = new float3(-1, 0, 0);
        }
        else
        {
            float xEdge = 1f + outward;
            v1 = p + new float3(xEdge, 1, 0);
            v2 = p + new float3(xEdge, 1, 1);
            v3 = p + new float3(xEdge, -overhang, 1);
            v4 = p + new float3(xEdge, -overhang, 0);
            normal = new float3(1, 0, 0);
        }

        int vc = buffers.vertices.Length;
        buffers.vertices.Add(v1);
        buffers.vertices.Add(v2);
        buffers.vertices.Add(v3);
        buffers.vertices.Add(v4);

        buffers.triangles.Add(vc);
        buffers.triangles.Add(vc + 1);
        buffers.triangles.Add(vc + 2);
        buffers.triangles.Add(vc);
        buffers.triangles.Add(vc + 2);
        buffers.triangles.Add(vc + 3);

        buffers.uvs.Add(new float2(0, 1));
        buffers.uvs.Add(new float2(1, 1));
        buffers.uvs.Add(new float2(1, 0));
        buffers.uvs.Add(new float2(0, 0));

        buffers.normals.Add(normal);
        buffers.normals.Add(normal);
        buffers.normals.Add(normal);
        buffers.normals.Add(normal);

        buffers.vertexColors.Add(new float4(aoTop, 0, 0, 1));
        buffers.vertexColors.Add(new float4(aoTop, 0, 0, 1));
        buffers.vertexColors.Add(new float4(aoBottom, 0, 1, 1));
        buffers.vertexColors.Add(new float4(aoBottom, 0, 1, 1));
    }
}