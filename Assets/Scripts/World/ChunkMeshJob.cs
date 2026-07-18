using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkMeshJob : IJob
{
    [ReadOnly] public NativeArray<ushort> blocks;
    [ReadOnly] public NativeArray<BlockDatabase.BlockVisualData> visualData;
    [ReadOnly] public NativeArray<byte> lightMap;

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

    public NativeList<float3> vertices;
    public NativeList<int> triangles;
    public NativeList<float3> uvs;
    public NativeList<float3> normals;
    public NativeList<float4> vertexColors;

    public void Execute()
    {
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                for (int z = 0; z < 32; z++)
                {
                    ushort block = GetBlock(x, y, z);
                    if (block == 0) continue;
                    if (block == 6) continue;
                    if (block >= visualData.Length) continue;

                    var v = visualData[block];

                    if (ShouldDrawFace(block, GetBlock(x, y + 1, z)))
                        AddFaceTop(x, y, z, v.top);
                    if (ShouldDrawFace(block, GetBlock(x, y - 1, z)))
                        AddFaceBottom(x, y, z, v.bottom);
                    if (ShouldDrawFace(block, GetBlock(x, y, z - 1)))
                        AddFaceNorth(x, y, z, v.north);
                    if (ShouldDrawFace(block, GetBlock(x, y, z + 1)))
                        AddFaceSouth(x, y, z, v.south);
                    if (ShouldDrawFace(block, GetBlock(x - 1, y, z)))
                        AddFaceWest(x, y, z, v.west);
                    if (ShouldDrawFace(block, GetBlock(x + 1, y, z)))
                        AddFaceEast(x, y, z, v.east);
                }
    }

    void AddFaceTop(int x, int y, int z, int texLayer)
    {
        int py = y + 1;

        bool o00 = IsSolid(x - 1, py, z - 1);
        bool o10 = IsSolid(x + 1, py, z - 1);
        bool o01 = IsSolid(x - 1, py, z + 1);
        bool o11 = IsSolid(x + 1, py, z + 1);
        bool sN = IsSolid(x, py, z - 1);
        bool sS = IsSolid(x, py, z + 1);
        bool sW = IsSolid(x - 1, py, z);
        bool sE = IsSolid(x + 1, py, z);

        float light = GetLightFloat(x, py, z);

        float ao00 = CalcAO(sW, sN, o00) * light;
        float ao10 = CalcAO(sE, sN, o10) * light;
        float ao11 = CalcAO(sE, sS, o11) * light;
        float ao01 = CalcAO(sW, sS, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 1, 0);
        float3 v2 = p + new float3(0, 1, 1);
        float3 v3 = p + new float3(1, 1, 1);
        float3 v4 = p + new float3(1, 1, 0);

        AddQuadWithAO(v1, v2, v3, v4, new float3(0, 1, 0), texLayer, ao00, ao01, ao11, ao10);
    }

    void AddFaceBottom(int x, int y, int z, int texLayer)
    {
        int py = y - 1;

        bool o00 = IsSolid(x - 1, py, z - 1);
        bool o10 = IsSolid(x + 1, py, z - 1);
        bool o01 = IsSolid(x - 1, py, z + 1);
        bool o11 = IsSolid(x + 1, py, z + 1);
        bool sN = IsSolid(x, py, z - 1);
        bool sS = IsSolid(x, py, z + 1);
        bool sW = IsSolid(x - 1, py, z);
        bool sE = IsSolid(x + 1, py, z);

        float light = GetLightFloat(x, py, z);

        float ao00 = CalcAO(sW, sN, o00) * light;
        float ao10 = CalcAO(sE, sN, o10) * light;
        float ao11 = CalcAO(sE, sS, o11) * light;
        float ao01 = CalcAO(sW, sS, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 0, 1);
        float3 v2 = p + new float3(0, 0, 0);
        float3 v3 = p + new float3(1, 0, 0);
        float3 v4 = p + new float3(1, 0, 1);

        AddQuadWithAO(v1, v2, v3, v4, new float3(0, -1, 0), texLayer, ao01, ao00, ao10, ao11);
    }

    void AddFaceNorth(int x, int y, int z, int texLayer)
    {
        int pz = z - 1;

        bool o00 = IsSolid(x - 1, y - 1, pz);
        bool o10 = IsSolid(x + 1, y - 1, pz);
        bool o01 = IsSolid(x - 1, y + 1, pz);
        bool o11 = IsSolid(x + 1, y + 1, pz);
        bool sD = IsSolid(x, y - 1, pz);
        bool sU = IsSolid(x, y + 1, pz);
        bool sW = IsSolid(x - 1, y, pz);
        bool sE = IsSolid(x + 1, y, pz);

        float light = GetLightFloat(x, y, pz);

        float ao00 = CalcAO(sW, sD, o00) * light;
        float ao10 = CalcAO(sE, sD, o10) * light;
        float ao11 = CalcAO(sE, sU, o11) * light;
        float ao01 = CalcAO(sW, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 0, 0);
        float3 v2 = p + new float3(0, 1, 0);
        float3 v3 = p + new float3(1, 1, 0);
        float3 v4 = p + new float3(1, 0, 0);

        AddQuadWithAO(v1, v2, v3, v4, new float3(0, 0, -1), texLayer, ao00, ao01, ao11, ao10);
    }

    void AddFaceSouth(int x, int y, int z, int texLayer)
    {
        int pz = z + 1;

        bool o00 = IsSolid(x - 1, y - 1, pz);
        bool o10 = IsSolid(x + 1, y - 1, pz);
        bool o01 = IsSolid(x - 1, y + 1, pz);
        bool o11 = IsSolid(x + 1, y + 1, pz);
        bool sD = IsSolid(x, y - 1, pz);
        bool sU = IsSolid(x, y + 1, pz);
        bool sW = IsSolid(x - 1, y, pz);
        bool sE = IsSolid(x + 1, y, pz);

        float light = GetLightFloat(x, y, pz);

        float ao00 = CalcAO(sW, sD, o00) * light;
        float ao10 = CalcAO(sE, sD, o10) * light;
        float ao11 = CalcAO(sE, sU, o11) * light;
        float ao01 = CalcAO(sW, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(1, 0, 1);
        float3 v2 = p + new float3(1, 1, 1);
        float3 v3 = p + new float3(0, 1, 1);
        float3 v4 = p + new float3(0, 0, 1);

        AddQuadWithAO(v1, v2, v3, v4, new float3(0, 0, 1), texLayer, ao10, ao11, ao01, ao00);
    }

    void AddFaceWest(int x, int y, int z, int texLayer)
    {
        int px = x - 1;

        bool o00 = IsSolid(px, y - 1, z - 1);
        bool o10 = IsSolid(px, y - 1, z + 1);
        bool o01 = IsSolid(px, y + 1, z - 1);
        bool o11 = IsSolid(px, y + 1, z + 1);
        bool sD = IsSolid(px, y - 1, z);
        bool sU = IsSolid(px, y + 1, z);
        bool sN = IsSolid(px, y, z - 1);
        bool sS = IsSolid(px, y, z + 1);

        float light = GetLightFloat(px, y, z);

        float ao00 = CalcAO(sN, sD, o00) * light;
        float ao10 = CalcAO(sS, sD, o10) * light;
        float ao11 = CalcAO(sS, sU, o11) * light;
        float ao01 = CalcAO(sN, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(0, 0, 1);
        float3 v2 = p + new float3(0, 1, 1);
        float3 v3 = p + new float3(0, 1, 0);
        float3 v4 = p + new float3(0, 0, 0);

        AddQuadWithAO(v1, v2, v3, v4, new float3(-1, 0, 0), texLayer, ao10, ao11, ao01, ao00);
    }

    void AddFaceEast(int x, int y, int z, int texLayer)
    {
        int px = x + 1;

        bool o00 = IsSolid(px, y - 1, z - 1);
        bool o10 = IsSolid(px, y - 1, z + 1);
        bool o01 = IsSolid(px, y + 1, z - 1);
        bool o11 = IsSolid(px, y + 1, z + 1);
        bool sD = IsSolid(px, y - 1, z);
        bool sU = IsSolid(px, y + 1, z);
        bool sN = IsSolid(px, y, z - 1);
        bool sS = IsSolid(px, y, z + 1);

        float light = GetLightFloat(px, y, z);

        float ao00 = CalcAO(sN, sD, o00) * light;
        float ao10 = CalcAO(sS, sD, o10) * light;
        float ao11 = CalcAO(sS, sU, o11) * light;
        float ao01 = CalcAO(sN, sU, o01) * light;

        float3 p = new float3(x, y, z);
        float3 v1 = p + new float3(1, 0, 0);
        float3 v2 = p + new float3(1, 1, 0);
        float3 v3 = p + new float3(1, 1, 1);
        float3 v4 = p + new float3(1, 0, 1);

        AddQuadWithAO(v1, v2, v3, v4, new float3(1, 0, 0), texLayer, ao00, ao01, ao11, ao10);
    }

    float CalcAO(bool side1, bool side2, bool corner)
    {
        if (side1 && side2) return 0.4f;

        int blockedCount = 0;
        if (side1) blockedCount++;
        if (side2) blockedCount++;
        if (corner) blockedCount++;

        if (blockedCount == 0) return 1.0f;
        if (blockedCount == 1) return 0.75f;
        if (blockedCount == 2) return 0.55f;
        return 0.4f;
    }

    bool IsSolid(int x, int y, int z)
    {
        ushort b = GetBlockSafe(x, y, z);
        if (b == 0) return false;
        if (b >= visualData.Length) return false;
        if (visualData[b].isTransparent) return false;
        return true;
    }

    ushort GetBlockSafe(int x, int y, int z)
    {
        if (x >= 0 && x < 32 && y >= 0 && y < 32 && z >= 0 && z < 32)
            return blocks[x + y * 32 + z * 32 * 32];

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

    float GetLightFloat(int x, int y, int z)
    {
        if (x < 0 || x >= 32 || y < 0 || y >= 32 || z < 0 || z >= 32)
            return 0.9f;

        int idx = x + y * 32 + z * 32 * 32;
        float light = lightMap[idx] / 15f;
        return math.max(light, 0.05f);
    }

    ushort GetBlock(int x, int y, int z)
    {
        if (x >= 0 && x < 32 && y >= 0 && y < 32 && z >= 0 && z < 32)
            return blocks[x + y * 32 + z * 32 * 32];

        if (x < 0 && hasNeighborXNeg)
        {
            if (y < 0 || y >= 32 || z < 0 || z >= 32) return 0;
            return neighborXNeg[y + z * 32];
        }
        if (x >= 32 && hasNeighborXPos)
        {
            if (y < 0 || y >= 32 || z < 0 || z >= 32) return 0;
            return neighborXPos[y + z * 32];
        }
        if (y < 0 && hasNeighborYNeg)
        {
            if (x < 0 || x >= 32 || z < 0 || z >= 32) return 0;
            return neighborYNeg[x + z * 32];
        }
        if (y >= 32 && hasNeighborYPos)
        {
            if (x < 0 || x >= 32 || z < 0 || z >= 32) return 0;
            return neighborYPos[x + z * 32];
        }
        if (z < 0 && hasNeighborZNeg)
        {
            if (x < 0 || x >= 32 || y < 0 || y >= 32) return 0;
            return neighborZNeg[x + y * 32];
        }
        if (z >= 32 && hasNeighborZPos)
        {
            if (x < 0 || x >= 32 || y < 0 || y >= 32) return 0;
            return neighborZPos[x + y * 32];
        }

        return 0;
    }

    void AddQuadWithAO(float3 v1, float3 v2, float3 v3, float3 v4, float3 normal, int texLayer,
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

    bool ShouldDrawFace(ushort current, ushort neighbor)
    {
        if (neighbor == 0) return true;
        if (neighbor == 6) return true;
        if (neighbor >= visualData.Length) return true;

        bool currentTransparent = IsTransparent(current);
        bool neighborTransparent = IsTransparent(neighbor);

        if (!currentTransparent && !neighborTransparent) return false;
        if (currentTransparent && neighborTransparent && current == neighbor) return false;

        return true;
    }

    bool IsTransparent(ushort blockId)
    {
        if (blockId >= visualData.Length) return false;
        return visualData[blockId].isTransparent;
    }
}