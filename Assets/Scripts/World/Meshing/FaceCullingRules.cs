using Unity.Burst;

[BurstCompile]
public static class FaceCullingRules
{
    public static bool ShouldDrawFace(ushort block, ushort neighbor, ChunkBlockAccess access)
    {
        if (block == BlockIDs.Air) return false;
        if (neighbor == BlockIDs.Air) return true;
        if (neighbor == BlockIDs.Water) return block != BlockIDs.Water;

        if (access.IsTransparent(neighbor) && !access.IsTransparent(block)) return true;
        if (access.IsTransparent(block) && access.IsTransparent(neighbor)) return false;
        if (!access.IsOpaque(neighbor)) return true;

        return false;
    }
}