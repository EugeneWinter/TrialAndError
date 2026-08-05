using Unity.Collections;
using Unity.Mathematics;

public struct StandardMeshBuffers
{
    public NativeList<float3> vertices;
    public NativeList<int> triangles;
    public NativeList<float3> uvs;
    public NativeList<float3> normals;
    public NativeList<float4> vertexColors;

    public void AddQuad(float3 v1, float3 v2, float3 v3, float3 v4, float3 normal, int texLayer,
        float ao1, float ao2, float ao3, float ao4)
    {
        int vc = vertices.Length;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);

        bool flip = (ao1 + ao3) < (ao2 + ao4);

        if (flip)
        {
            triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 3);
            triangles.Add(vc + 1); triangles.Add(vc + 2); triangles.Add(vc + 3);
        }
        else
        {
            triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 2);
            triangles.Add(vc); triangles.Add(vc + 2); triangles.Add(vc + 3);
        }

        uvs.Add(new float3(0, 0, texLayer));
        uvs.Add(new float3(0, 1, texLayer));
        uvs.Add(new float3(1, 1, texLayer));
        uvs.Add(new float3(1, 0, texLayer));

        normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);

        vertexColors.Add(new float4(ao1, ao1, ao1, 1f));
        vertexColors.Add(new float4(ao2, ao2, ao2, 1f));
        vertexColors.Add(new float4(ao3, ao3, ao3, 1f));
        vertexColors.Add(new float4(ao4, ao4, ao4, 1f));
    }
}

public struct LeafMeshBuffers
{
    public NativeList<float3> vertices;
    public NativeList<int> triangles;
    public NativeList<float3> uvs;
    public NativeList<float3> normals;
    public NativeList<float4> vertexColors;

    public void AddQuadWithAO(float3 v1, float3 v2, float3 v3, float3 v4, float3 normal, int texLayer,
        float ao1, float ao2, float ao3, float ao4, float leafDensity)
    {
        int vc = vertices.Length;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);

        bool flip = (ao1 + ao3) < (ao2 + ao4);

        if (flip)
        {
            triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 3);
            triangles.Add(vc + 1); triangles.Add(vc + 2); triangles.Add(vc + 3);
        }
        else
        {
            triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 2);
            triangles.Add(vc); triangles.Add(vc + 2); triangles.Add(vc + 3);
        }

        uvs.Add(new float3(0, 0, texLayer));
        uvs.Add(new float3(0, 1, texLayer));
        uvs.Add(new float3(1, 1, texLayer));
        uvs.Add(new float3(1, 0, texLayer));

        normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);

        vertexColors.Add(new float4(ao1, leafDensity, 0, 1f));
        vertexColors.Add(new float4(ao2, leafDensity, 0, 1f));
        vertexColors.Add(new float4(ao3, leafDensity, 0, 1f));
        vertexColors.Add(new float4(ao4, leafDensity, 0, 1f));
    }

    public void AddSimpleQuad(float3 v1, float3 v2, float3 v3, float3 v4, float3 normal, int texLayer,
        float ao, float leafDensity)
    {
        int vc = vertices.Length;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);

        triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 2);
        triangles.Add(vc); triangles.Add(vc + 2); triangles.Add(vc + 3);

        uvs.Add(new float3(0, 0, texLayer));
        uvs.Add(new float3(0, 1, texLayer));
        uvs.Add(new float3(1, 1, texLayer));
        uvs.Add(new float3(1, 0, texLayer));

        normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);

        vertexColors.Add(new float4(ao, leafDensity, 0, 1f));
        vertexColors.Add(new float4(ao, leafDensity, 0, 1f));
        vertexColors.Add(new float4(ao, leafDensity, 0, 1f));
        vertexColors.Add(new float4(ao, leafDensity, 0, 1f));
    }
}

public struct GrassOverlayMeshBuffers
{
    public NativeList<float3> vertices;
    public NativeList<int> triangles;
    public NativeList<float2> uvs;
    public NativeList<float3> normals;
    public NativeList<float4> vertexColors;
}