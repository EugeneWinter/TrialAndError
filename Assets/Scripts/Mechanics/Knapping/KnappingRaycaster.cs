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
        Vector3 localDir = worldToLocal.MultiplyVector(ray.direction);

        if (localDir.sqrMagnitude < 0.000001f)
            return result;

        localDir.Normalize();

        float vs = stone.VoxelSize;

        Vector3 gridMin = new Vector3(
            -stone.Width * 0.5f * vs,
            -stone.Height * 0.5f * vs,
            -stone.Depth * 0.5f * vs);

        Vector3 gridMax = gridMin + new Vector3(
            stone.Width * vs,
            stone.Height * vs,
            stone.Depth * vs);

        if (!TryIntersectAABB(localOrigin, localDir, gridMin, gridMax, out float tEnter, out float tExit, out Vector3Int enterNormal))
            return result;

        if (tEnter > maxDistance)
            return result;

        float startT = Mathf.Max(0f, tEnter + 0.0001f);
        if (startT > maxDistance)
            return result;

        Vector3 startPoint = localOrigin + localDir * startT;

        float relX = (startPoint.x - gridMin.x) / vs;
        float relY = (startPoint.y - gridMin.y) / vs;
        float relZ = (startPoint.z - gridMin.z) / vs;

        int x = Mathf.FloorToInt(relX);
        int y = Mathf.FloorToInt(relY);
        int z = Mathf.FloorToInt(relZ);

        x = Mathf.Clamp(x, 0, stone.Width - 1);
        y = Mathf.Clamp(y, 0, stone.Height - 1);
        z = Mathf.Clamp(z, 0, stone.Depth - 1);

        int stepX = localDir.x >= 0 ? 1 : -1;
        int stepY = localDir.y >= 0 ? 1 : -1;
        int stepZ = localDir.z >= 0 ? 1 : -1;

        float tDeltaX = localDir.x != 0f ? Mathf.Abs(vs / localDir.x) : float.MaxValue;
        float tDeltaY = localDir.y != 0f ? Mathf.Abs(vs / localDir.y) : float.MaxValue;
        float tDeltaZ = localDir.z != 0f ? Mathf.Abs(vs / localDir.z) : float.MaxValue;

        float cellMinX = gridMin.x + x * vs;
        float cellMinY = gridMin.y + y * vs;
        float cellMinZ = gridMin.z + z * vs;

        float tMaxX = localDir.x >= 0f
            ? ((cellMinX + vs) - startPoint.x) / localDir.x
            : (cellMinX - startPoint.x) / localDir.x;

        float tMaxY = localDir.y >= 0f
            ? ((cellMinY + vs) - startPoint.y) / localDir.y
            : (cellMinY - startPoint.y) / localDir.y;

        float tMaxZ = localDir.z >= 0f
            ? ((cellMinZ + vs) - startPoint.z) / localDir.z
            : (cellMinZ - startPoint.z) / localDir.z;

        if (localDir.x == 0f) tMaxX = float.MaxValue;
        if (localDir.y == 0f) tMaxY = float.MaxValue;
        if (localDir.z == 0f) tMaxZ = float.MaxValue;

        Vector3Int normal = enterNormal;
        float baseTravel = startT;

        for (int i = 0; i < 512; i++)
        {
            if (x >= 0 && x < stone.Width &&
                y >= 0 && y < stone.Height &&
                z >= 0 && z < stone.Depth)
            {
                if (stone.Voxels[x, y, z])
                {
                    result.hit = true;
                    result.voxel = new Vector3Int(x, y, z);
                    result.normal = normal;

                    Vector3 voxelCenter = stone.VoxelCenterToLocalSpace(x, y, z);
                    Vector3 faceOffset = new Vector3(normal.x, normal.y, normal.z) * (stone.VoxelSize * 0.5f);
                    result.worldPoint = stone.transform.TransformPoint(voxelCenter + faceOffset);

                    return result;
                }
            }

            float nextStepT;

            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                nextStepT = tMaxX;
                x += stepX;
                tMaxX += tDeltaX;
                normal = new Vector3Int(-stepX, 0, 0);
            }
            else if (tMaxY < tMaxZ)
            {
                nextStepT = tMaxY;
                y += stepY;
                tMaxY += tDeltaY;
                normal = new Vector3Int(0, -stepY, 0);
            }
            else
            {
                nextStepT = tMaxZ;
                z += stepZ;
                tMaxZ += tDeltaZ;
                normal = new Vector3Int(0, 0, -stepZ);
            }

            if (baseTravel + nextStepT > maxDistance)
                break;
        }

        return result;
    }

    static bool TryIntersectAABB(
        Vector3 origin,
        Vector3 dir,
        Vector3 min,
        Vector3 max,
        out float tEnter,
        out float tExit,
        out Vector3Int enterNormal)
    {
        tEnter = float.NegativeInfinity;
        tExit = float.PositiveInfinity;
        enterNormal = Vector3Int.zero;

        if (!IntersectAxis(origin.x, dir.x, min.x, max.x, new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), ref tEnter, ref tExit, ref enterNormal))
            return false;

        if (!IntersectAxis(origin.y, dir.y, min.y, max.y, new Vector3Int(0, -1, 0), new Vector3Int(0, 1, 0), ref tEnter, ref tExit, ref enterNormal))
            return false;

        if (!IntersectAxis(origin.z, dir.z, min.z, max.z, new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1), ref tEnter, ref tExit, ref enterNormal))
            return false;

        return tExit >= Mathf.Max(tEnter, 0f);
    }

    static bool IntersectAxis(
        float origin,
        float dir,
        float min,
        float max,
        Vector3Int minFaceNormal,
        Vector3Int maxFaceNormal,
        ref float tEnter,
        ref float tExit,
        ref Vector3Int enterNormal)
    {
        if (Mathf.Abs(dir) < 0.000001f)
            return origin >= min && origin <= max;

        float t1 = (min - origin) / dir;
        float t2 = (max - origin) / dir;

        Vector3Int axisEnterNormal = minFaceNormal;

        if (t1 > t2)
        {
            float temp = t1;
            t1 = t2;
            t2 = temp;
            axisEnterNormal = maxFaceNormal;
        }

        if (t1 > tEnter)
        {
            tEnter = t1;
            enterNormal = axisEnterNormal;
        }

        if (t2 < tExit)
            tExit = t2;

        return tEnter <= tExit;
    }
}