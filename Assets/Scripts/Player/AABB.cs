using Unity.Mathematics;

public struct AABB
{
    public float3 min;
    public float3 max;

    public AABB(float3 min, float3 max)
    {
        this.min = min;
        this.max = max;
    }

    public static AABB FromPositionSize(float3 pos, float3 size)
    {
        return new AABB(
            pos - new float3(size.x / 2f, 0, size.z / 2f),
            pos + new float3(size.x / 2f, size.y, size.z / 2f)
        );
    }

    public bool Intersects(AABB other)
    {
        return (min.x < other.max.x && max.x > other.min.x) &&
               (min.y < other.max.y && max.y > other.min.y) &&
               (min.z < other.max.z && max.z > other.min.z);
    }
}