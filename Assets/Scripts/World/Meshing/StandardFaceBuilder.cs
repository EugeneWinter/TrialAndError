using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct StandardFaceBuilder
{
    public ChunkBlockAccess access;
    public ChunkLighting lighting;

    public void AddFaceTop(int x, int y, int z, int texLayer, ref StandardMeshBuffers standard, ref LeafMeshBuffers leaf, bool isLeaf, float leafDensity)
    {
        int py = y + 1;

        bool o00 = access.IsSolid(x - 1, py, z - 1);
        bool o10 = access.IsSolid(x + 1, py, z - 1);
        bool o01 = access.IsSolid(x - 1, py, z + 1);
        bool o11 = access.IsSolid(x + 1, py, z + 1);
        bool sN = access.IsSolid(x, py, z - 1);
        bool sS = access.IsSolid(x, py, z + 1);
        bool sW = access.IsSolid(x - 1, py, z);
        bool sE = access.IsSolid(x + 1, py, z);

        float light = lighting.GetLightFloat(x, py, z);

        float ao00 = ChunkLighting.CalcAO(sW, sN, o00) * light;
        float ao10 = ChunkLighting.CalcAO(sE, sN, o10) * light;
        float ao11 = ChunkLighting.CalcAO(sE, sS, o11) * light;
        float ao01 = ChunkLighting.CalcAO(sW, sS, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 1, 0);
        float3 v2 = p + new float3(0, 1, 1);
        float3 v3 = p + new float3(1, 1, 1);
        float3 v4 = p + new float3(1, 1, 0);

        if (isLeaf)
            leaf.AddQuadWithAO(v1, v2, v3, v4, new float3(0, 1, 0), texLayer, ao00, ao01, ao11, ao10, leafDensity);
        else
            standard.AddQuad(v1, v2, v3, v4, new float3(0, 1, 0), texLayer, ao00, ao01, ao11, ao10);
    }

    public void AddFaceBottom(int x, int y, int z, int texLayer, ref StandardMeshBuffers standard, ref LeafMeshBuffers leaf, bool isLeaf, float leafDensity)
    {
        int py = y - 1;

        bool o00 = access.IsSolid(x - 1, py, z - 1);
        bool o10 = access.IsSolid(x + 1, py, z - 1);
        bool o01 = access.IsSolid(x - 1, py, z + 1);
        bool o11 = access.IsSolid(x + 1, py, z + 1);
        bool sN = access.IsSolid(x, py, z - 1);
        bool sS = access.IsSolid(x, py, z + 1);
        bool sW = access.IsSolid(x - 1, py, z);
        bool sE = access.IsSolid(x + 1, py, z);

        float light = lighting.GetLightFloat(x, py, z);

        float ao00 = ChunkLighting.CalcAO(sW, sN, o00) * light;
        float ao10 = ChunkLighting.CalcAO(sE, sN, o10) * light;
        float ao11 = ChunkLighting.CalcAO(sE, sS, o11) * light;
        float ao01 = ChunkLighting.CalcAO(sW, sS, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 0, 1);
        float3 v2 = p + new float3(0, 0, 0);
        float3 v3 = p + new float3(1, 0, 0);
        float3 v4 = p + new float3(1, 0, 1);

        if (isLeaf)
            leaf.AddQuadWithAO(v1, v2, v3, v4, new float3(0, -1, 0), texLayer, ao01, ao00, ao10, ao11, leafDensity);
        else
            standard.AddQuad(v1, v2, v3, v4, new float3(0, -1, 0), texLayer, ao01, ao00, ao10, ao11);
    }

    public void AddFaceNorth(int x, int y, int z, int texLayer, ref StandardMeshBuffers standard, ref LeafMeshBuffers leaf, bool isLeaf, float leafDensity)
    {
        int pz = z - 1;

        bool o00 = access.IsSolid(x - 1, y - 1, pz);
        bool o10 = access.IsSolid(x + 1, y - 1, pz);
        bool o01 = access.IsSolid(x - 1, y + 1, pz);
        bool o11 = access.IsSolid(x + 1, y + 1, pz);
        bool sD = access.IsSolid(x, y - 1, pz);
        bool sU = access.IsSolid(x, y + 1, pz);
        bool sW = access.IsSolid(x - 1, y, pz);
        bool sE = access.IsSolid(x + 1, y, pz);

        float light = lighting.GetLightFloat(x, y, pz);

        float ao00 = ChunkLighting.CalcAO(sW, sD, o00) * light;
        float ao10 = ChunkLighting.CalcAO(sE, sD, o10) * light;
        float ao11 = ChunkLighting.CalcAO(sE, sU, o11) * light;
        float ao01 = ChunkLighting.CalcAO(sW, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 0, 0);
        float3 v2 = p + new float3(0, 1, 0);
        float3 v3 = p + new float3(1, 1, 0);
        float3 v4 = p + new float3(1, 0, 0);

        if (isLeaf)
            leaf.AddQuadWithAO(v1, v2, v3, v4, new float3(0, 0, -1), texLayer, ao00, ao01, ao11, ao10, leafDensity);
        else
            standard.AddQuad(v1, v2, v3, v4, new float3(0, 0, -1), texLayer, ao00, ao01, ao11, ao10);
    }

    public void AddFaceSouth(int x, int y, int z, int texLayer, ref StandardMeshBuffers standard, ref LeafMeshBuffers leaf, bool isLeaf, float leafDensity)
    {
        int pz = z + 1;

        bool o00 = access.IsSolid(x - 1, y - 1, pz);
        bool o10 = access.IsSolid(x + 1, y - 1, pz);
        bool o01 = access.IsSolid(x - 1, y + 1, pz);
        bool o11 = access.IsSolid(x + 1, y + 1, pz);
        bool sD = access.IsSolid(x, y - 1, pz);
        bool sU = access.IsSolid(x, y + 1, pz);
        bool sW = access.IsSolid(x - 1, y, pz);
        bool sE = access.IsSolid(x + 1, y, pz);

        float light = lighting.GetLightFloat(x, y, pz);

        float ao00 = ChunkLighting.CalcAO(sW, sD, o00) * light;
        float ao10 = ChunkLighting.CalcAO(sE, sD, o10) * light;
        float ao11 = ChunkLighting.CalcAO(sE, sU, o11) * light;
        float ao01 = ChunkLighting.CalcAO(sW, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(1, 0, 1);
        float3 v2 = p + new float3(1, 1, 1);
        float3 v3 = p + new float3(0, 1, 1);
        float3 v4 = p + new float3(0, 0, 1);

        if (isLeaf)
            leaf.AddQuadWithAO(v1, v2, v3, v4, new float3(0, 0, 1), texLayer, ao10, ao11, ao01, ao00, leafDensity);
        else
            standard.AddQuad(v1, v2, v3, v4, new float3(0, 0, 1), texLayer, ao10, ao11, ao01, ao00);
    }

    public void AddFaceWest(int x, int y, int z, int texLayer, ref StandardMeshBuffers standard, ref LeafMeshBuffers leaf, bool isLeaf, float leafDensity)
    {
        int px = x - 1;

        bool o00 = access.IsSolid(px, y - 1, z - 1);
        bool o10 = access.IsSolid(px, y - 1, z + 1);
        bool o01 = access.IsSolid(px, y + 1, z - 1);
        bool o11 = access.IsSolid(px, y + 1, z + 1);
        bool sD = access.IsSolid(px, y - 1, z);
        bool sU = access.IsSolid(px, y + 1, z);
        bool sN = access.IsSolid(px, y, z - 1);
        bool sS = access.IsSolid(px, y, z + 1);

        float light = lighting.GetLightFloat(px, y, z);

        float ao00 = ChunkLighting.CalcAO(sN, sD, o00) * light;
        float ao10 = ChunkLighting.CalcAO(sS, sD, o10) * light;
        float ao11 = ChunkLighting.CalcAO(sS, sU, o11) * light;
        float ao01 = ChunkLighting.CalcAO(sN, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 0, 1);
        float3 v2 = p + new float3(0, 1, 1);
        float3 v3 = p + new float3(0, 1, 0);
        float3 v4 = p + new float3(0, 0, 0);

        if (isLeaf)
            leaf.AddQuadWithAO(v1, v2, v3, v4, new float3(-1, 0, 0), texLayer, ao10, ao11, ao01, ao00, leafDensity);
        else
            standard.AddQuad(v1, v2, v3, v4, new float3(-1, 0, 0), texLayer, ao10, ao11, ao01, ao00);
    }

    public void AddFaceEast(int x, int y, int z, int texLayer, ref StandardMeshBuffers standard, ref LeafMeshBuffers leaf, bool isLeaf, float leafDensity)
    {
        int px = x + 1;

        bool o00 = access.IsSolid(px, y - 1, z - 1);
        bool o10 = access.IsSolid(px, y - 1, z + 1);
        bool o01 = access.IsSolid(px, y + 1, z - 1);
        bool o11 = access.IsSolid(px, y + 1, z + 1);
        bool sD = access.IsSolid(px, y - 1, z);
        bool sU = access.IsSolid(px, y + 1, z);
        bool sN = access.IsSolid(px, y, z - 1);
        bool sS = access.IsSolid(px, y, z + 1);

        float light = lighting.GetLightFloat(px, y, z);

        float ao00 = ChunkLighting.CalcAO(sN, sD, o00) * light;
        float ao10 = ChunkLighting.CalcAO(sS, sD, o10) * light;
        float ao11 = ChunkLighting.CalcAO(sS, sU, o11) * light;
        float ao01 = ChunkLighting.CalcAO(sN, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(1, 0, 0);
        float3 v2 = p + new float3(1, 1, 0);
        float3 v3 = p + new float3(1, 1, 1);
        float3 v4 = p + new float3(1, 0, 1);

        if (isLeaf)
            leaf.AddQuadWithAO(v1, v2, v3, v4, new float3(1, 0, 0), texLayer, ao00, ao01, ao11, ao10, leafDensity);
        else
            standard.AddQuad(v1, v2, v3, v4, new float3(1, 0, 0), texLayer, ao00, ao01, ao11, ao10);
    }
}