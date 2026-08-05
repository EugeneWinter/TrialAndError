using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct LeafMeshBuilder
{
    public ChunkBlockAccess access;
    public ChunkLighting lighting;

    public const ushort LEAF_ID = 5;

    public float CalculateLeafDensity(int cx, int cy, int cz)
    {
        int leafCount = 0;
        int totalChecked = 0;
        int radius = 2;

        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                for (int dz = -radius; dz <= radius; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    totalChecked++;

                    ushort neighbor = access.GetBlockSafe(cx + dx, cy + dy, cz + dz);
                    if (neighbor == LEAF_ID) leafCount++;
                }

        float ratio = (float)leafCount / totalChecked;
        return math.saturate(ratio * 1.8f);
    }

    public void AddCrossQuads(int x, int y, int z, int texLayer, float leafDensity, ref LeafMeshBuffers leaf)
    {
        float light = lighting.GetLightFloat(x, y, z);
        float baseAO = 0.85f * light;

        float3 p = new float3(x, y, z);

        float3 c000 = p + new float3(0, 0, 0);
        float3 c100 = p + new float3(1, 0, 0);
        float3 c010 = p + new float3(0, 1, 0);
        float3 c110 = p + new float3(1, 1, 0);
        float3 c001 = p + new float3(0, 0, 1);
        float3 c101 = p + new float3(1, 0, 1);
        float3 c011 = p + new float3(0, 1, 1);
        float3 c111 = p + new float3(1, 1, 1);

        float3 diagNormal1 = math.normalize(new float3(1, 0, -1));
        leaf.AddSimpleQuad(c000, c010, c111, c101, diagNormal1, texLayer, baseAO, leafDensity);

        float3 diagNormal2 = math.normalize(new float3(1, 0, 1));
        leaf.AddSimpleQuad(c100, c110, c011, c001, diagNormal2, texLayer, baseAO, leafDensity);
    }
}