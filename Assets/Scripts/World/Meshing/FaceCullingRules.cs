using Unity.Burst;

[BurstCompile]
public static class FaceCullingRules
{
    public static bool ShouldDrawFace(ushort current, ushort neighbor, ChunkBlockAccess access)
    {
        if (neighbor == 0) return true;
        if (neighbor == 6) return true;
        if (neighbor >= access.visualData.Length) return true;
        if (access.visualData[neighbor].isCustomModel) return true;

        if (access.IsOpaque(neighbor)) return false;

        bool currentTransparent = access.IsTransparent(current);
        bool neighborTransparent = access.IsTransparent(neighbor);

        if (currentTransparent && neighborTransparent && current == neighbor) return false;

        return true;
    }
}