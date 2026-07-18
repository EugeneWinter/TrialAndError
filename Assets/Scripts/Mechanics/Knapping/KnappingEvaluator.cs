using UnityEngine;

public enum KnappingResult
{
    Perfect,
    Good,
    Average,
    Poor,
    Broken
}

public class KnappingEvaluator
{
    public static KnappingResult Evaluate(KnappingStone stone, out float qualityScore)
    {
        qualityScore = 0f;
        if (stone == null) return KnappingResult.Broken;

        int totalVoxels = CountVoxels(stone);
        int minSize = 20;

        if (totalVoxels < minSize)
            return KnappingResult.Broken;

        float sharpness = CalculateSharpness(stone);
        float integrity = CalculateIntegrity(stone);
        float balance = CalculateBalance(stone);

        qualityScore = (sharpness * 0.5f + integrity * 0.3f + balance * 0.2f);

        Debug.Log($"Knapping eval: voxels={totalVoxels}, sharp={sharpness:F2}, integ={integrity:F2}, bal={balance:F2}, total={qualityScore:F2}");

        if (qualityScore >= 0.75f) return KnappingResult.Perfect;
        if (qualityScore >= 0.55f) return KnappingResult.Good;
        if (qualityScore >= 0.35f) return KnappingResult.Average;
        return KnappingResult.Poor;
    }

    static int CountVoxels(KnappingStone stone)
    {
        int count = 0;
        for (int x = 0; x < stone.Width; x++)
            for (int y = 0; y < stone.Height; y++)
                for (int z = 0; z < stone.Depth; z++)
                    if (stone.Voxels[x, y, z]) count++;
        return count;
    }

    static float CalculateSharpness(KnappingStone stone)
    {
        int thinCount = 0;
        int surfaceCount = 0;

        for (int x = 0; x < stone.Width; x++)
            for (int y = 0; y < stone.Height; y++)
                for (int z = 0; z < stone.Depth; z++)
                {
                    if (!stone.Voxels[x, y, z]) continue;
                    int neighbors = CountFilledNeighbors(stone, x, y, z);
                    if (neighbors < 6) surfaceCount++;
                    if (neighbors <= 3) thinCount++;
                }

        if (surfaceCount == 0) return 0f;
        return Mathf.Clamp01((float)thinCount / surfaceCount * 2f);
    }

    static float CalculateIntegrity(KnappingStone stone)
    {
        int weakCount = 0;
        int totalCount = 0;

        for (int x = 0; x < stone.Width; x++)
            for (int y = 0; y < stone.Height; y++)
                for (int z = 0; z < stone.Depth; z++)
                {
                    if (!stone.Voxels[x, y, z]) continue;
                    totalCount++;
                    int neighbors = CountFilledNeighbors(stone, x, y, z);
                    if (neighbors <= 1) weakCount++;
                }

        if (totalCount == 0) return 0f;
        float weakRatio = (float)weakCount / totalCount;
        return Mathf.Clamp01(1f - weakRatio * 3f);
    }

    static float CalculateBalance(KnappingStone stone)
    {
        float sumX = 0f, sumY = 0f, sumZ = 0f;
        int count = 0;

        for (int x = 0; x < stone.Width; x++)
            for (int y = 0; y < stone.Height; y++)
                for (int z = 0; z < stone.Depth; z++)
                {
                    if (!stone.Voxels[x, y, z]) continue;
                    sumX += x;
                    sumY += y;
                    sumZ += z;
                    count++;
                }

        if (count == 0) return 0f;

        float avgX = sumX / count;
        float avgZ = sumZ / count;

        float dx = Mathf.Abs(avgX - stone.Width * 0.5f) / stone.Width;
        float dz = Mathf.Abs(avgZ - stone.Depth * 0.5f) / stone.Depth;

        return Mathf.Clamp01(1f - (dx + dz));
    }

    static int CountFilledNeighbors(KnappingStone stone, int x, int y, int z)
    {
        int count = 0;
        if (x > 0 && stone.Voxels[x - 1, y, z]) count++;
        if (x < stone.Width - 1 && stone.Voxels[x + 1, y, z]) count++;
        if (y > 0 && stone.Voxels[x, y - 1, z]) count++;
        if (y < stone.Height - 1 && stone.Voxels[x, y + 1, z]) count++;
        if (z > 0 && stone.Voxels[x, y, z - 1]) count++;
        if (z < stone.Depth - 1 && stone.Voxels[x, y, z + 1]) count++;
        return count;
    }

    public static string GetResultName(KnappingResult r)
    {
        switch (r)
        {
            case KnappingResult.Perfect: return "Mastercraft Stone Blade";
            case KnappingResult.Good: return "Stone Blade";
            case KnappingResult.Average: return "Rough Blade";
            case KnappingResult.Poor: return "Crude Splinter";
            case KnappingResult.Broken: return "Stone Shards";
            default: return "Unknown";
        }
    }

    public static float GetDurabilityMultiplier(KnappingResult r)
    {
        switch (r)
        {
            case KnappingResult.Perfect: return 1.5f;
            case KnappingResult.Good: return 1.0f;
            case KnappingResult.Average: return 0.7f;
            case KnappingResult.Poor: return 0.4f;
            case KnappingResult.Broken: return 0f;
            default: return 0f;
        }
    }
}