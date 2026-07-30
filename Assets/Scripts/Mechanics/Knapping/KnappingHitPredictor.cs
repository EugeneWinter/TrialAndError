using UnityEngine;
using System.Collections.Generic;

public class KnappingHitPredictor
{
    public static List<Vector3Int> Predict(KnappingStone stone, Vector3Int center, int radius, float power01)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        float stretch = 1.5f + power01 * 1.5f;

        Vector3 dir = (new Vector3(center.x - stone.Width * 0.5f, 0, center.z - stone.Depth * 0.5f)).normalized;
        if (dir.magnitude < 0.01f) dir = Vector3.forward;

        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int px = center.x + dx;
                    int py = center.y + dy;
                    int pz = center.z + dz;

                    if (px < 0 || px >= stone.Width) continue;
                    if (py < 0 || py >= stone.Height) continue;
                    if (pz < 0 || pz >= stone.Depth) continue;
                    if (!stone.Voxels[px, py, pz]) continue;

                    Vector3 offset = new Vector3(dx, dy, dz);
                    float distAlong = Vector3.Dot(offset, dir);
                    Vector3 perp = offset - dir * distAlong;
                    float alongScaled = distAlong / stretch;
                    float totalDistSq = perp.sqrMagnitude + alongScaled * alongScaled;

                    if (totalDistSq <= radius * radius)
                        result.Add(new Vector3Int(px, py, pz));
                }

        return result;
    }
}