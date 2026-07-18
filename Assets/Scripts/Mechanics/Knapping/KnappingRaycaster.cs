using UnityEngine;

public struct KnappingHit
{
    public bool hit;
    public Vector3Int voxel;
    public Vector3Int normal;
    public Vector3 worldPoint;
}

public static class KnappingRaycaster
{
    public static KnappingHit Cast(KnappingStone stone, Ray ray, float maxDistance = 10f)
    {
        KnappingHit result = new KnappingHit { hit = false };
        if (stone == null) return result;

        Matrix4x4 worldToLocal = stone.transform.worldToLocalMatrix;
        Vector3 localOrigin = worldToLocal.MultiplyPoint3x4(ray.origin);
        Vector3 localDir = worldToLocal.MultiplyVector(ray.direction).normalized;

        float vs = stone.voxelSize;
        Vector3 gridOrigin = new Vector3(-stone.Width * 0.5f * vs, 0f, -stone.Depth * 0.5f * vs);

        Vector3 rel = (localOrigin - gridOrigin) / vs;
        Vector3 dir = localDir;

        int x = Mathf.FloorToInt(rel.x);
        int y = Mathf.FloorToInt(rel.y);
        int z = Mathf.FloorToInt(rel.z);

        int stepX = dir.x >= 0 ? 1 : -1;
        int stepY = dir.y >= 0 ? 1 : -1;
        int stepZ = dir.z >= 0 ? 1 : -1;

        float tDeltaX = Mathf.Abs(1f / dir.x);
        float tDeltaY = Mathf.Abs(1f / dir.y);
        float tDeltaZ = Mathf.Abs(1f / dir.z);

        float tMaxX = dir.x >= 0 ? (Mathf.Floor(rel.x) + 1 - rel.x) * tDeltaX : (rel.x - Mathf.Floor(rel.x)) * tDeltaX;
        float tMaxY = dir.y >= 0 ? (Mathf.Floor(rel.y) + 1 - rel.y) * tDeltaY : (rel.y - Mathf.Floor(rel.y)) * tDeltaY;
        float tMaxZ = dir.z >= 0 ? (Mathf.Floor(rel.z) + 1 - rel.z) * tDeltaZ : (rel.z - Mathf.Floor(rel.z)) * tDeltaZ;

        Vector3Int normal = Vector3Int.zero;
        int maxSteps = 200;

        for (int i = 0; i < maxSteps; i++)
        {
            if (x >= 0 && x < stone.Width && y >= 0 && y < stone.Height && z >= 0 && z < stone.Depth)
            {
                if (stone.Voxels[x, y, z])
                {
                    result.hit = true;
                    result.voxel = new Vector3Int(x, y, z);
                    result.normal = normal;
                    float t = Mathf.Min(tMaxX, tMaxY, tMaxZ);
                    result.worldPoint = ray.origin + ray.direction * (t * vs);
                    return result;
                }
            }

            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                x += stepX;
                tMaxX += tDeltaX;
                normal = new Vector3Int(-stepX, 0, 0);
            }
            else if (tMaxY < tMaxZ)
            {
                y += stepY;
                tMaxY += tDeltaY;
                normal = new Vector3Int(0, -stepY, 0);
            }
            else
            {
                z += stepZ;
                tMaxZ += tDeltaZ;
                normal = new Vector3Int(0, 0, -stepZ);
            }

            float minT = Mathf.Min(tMaxX, tMaxY, tMaxZ);
            if (minT * vs > maxDistance) break;
        }

        return result;
    }
}