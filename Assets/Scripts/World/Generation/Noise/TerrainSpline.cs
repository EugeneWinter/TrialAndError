using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct TerrainSpline
{
    public static float Evaluate(float t, float4 p0, float4 p1)
    {
        float range = p1.x - p0.x;
        if (range < 0.0001f) return p0.y;

        float localT = (t - p0.x) / range;
        localT = math.saturate(localT);

        float h00 = 2f * localT * localT * localT - 3f * localT * localT + 1f;
        float h10 = localT * localT * localT - 2f * localT * localT + localT;
        float h01 = -2f * localT * localT * localT + 3f * localT * localT;
        float h11 = localT * localT * localT - localT * localT;

        return h00 * p0.y + h10 * (p0.w * range) + h01 * p1.y + h11 * (p1.w * range);
    }

    public static float Eval2(float t, float4 a, float4 b)
    {
        if (t <= a.x) return a.y;
        if (t >= b.x) return b.y;
        return Evaluate(t, a, b);
    }

    public static float Eval3(float t, float4 a, float4 b, float4 c)
    {
        if (t <= a.x) return a.y;
        if (t >= c.x) return c.y;
        if (t < b.x) return Evaluate(t, a, b);
        return Evaluate(t, b, c);
    }

    public static float Eval4(float t, float4 a, float4 b, float4 c, float4 d)
    {
        if (t <= a.x) return a.y;
        if (t >= d.x) return d.y;
        if (t < b.x) return Evaluate(t, a, b);
        if (t < c.x) return Evaluate(t, b, c);
        return Evaluate(t, c, d);
    }

    public static float Eval5(float t, float4 a, float4 b, float4 c, float4 d, float4 e)
    {
        if (t <= a.x) return a.y;
        if (t >= e.x) return e.y;
        if (t < b.x) return Evaluate(t, a, b);
        if (t < c.x) return Evaluate(t, b, c);
        if (t < d.x) return Evaluate(t, c, d);
        return Evaluate(t, d, e);
    }

    public static float4 P(float location, float value, float derivative)
    {
        return new float4(location, value, 0, derivative);
    }
}