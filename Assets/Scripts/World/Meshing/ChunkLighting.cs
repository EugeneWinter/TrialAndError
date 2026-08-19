using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkLighting
{
    [ReadOnly] public NativeArray<byte> lightMap;
    public int chunkY;

    public float GetLightFloat(int x, int y, int z)
    {
        if (x < 0 || x >= 32 || y < 0 || y >= 32 || z < 0 || z >= 32)
            return 0.9f;

        int idx = x + y * 32 + z * 32 * 32;
        if (idx < 0 || idx >= lightMap.Length)
            return 0.9f;

        float light = lightMap[idx] / 15f;
        return math.max(light, 0.05f);
    }

    public static float CalcAO(bool side1, bool side2, bool corner)
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
}