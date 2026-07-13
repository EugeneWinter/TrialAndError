using UnityEngine;

public enum BlockFace
{
    Top, Bottom, North, South, East, West
}

public static class BlockColorSampler
{
    public static Color SampleRandomFromFace(BlockSO block, BlockFace face)
    {
        if (block == null) return Color.gray;

        Texture2D tex = GetTextureForFace(block, face);
        if (tex == null) return Color.gray;

        int x = Random.Range(0, tex.width);
        int y = Random.Range(0, tex.height);

        try
        {
            return tex.GetPixel(x, y);
        }
        catch (UnityException)
        {
            return Color.gray;
        }
    }

    public static BlockFace FaceFromNormal(Vector3Int normal)
    {
        if (normal.y > 0) return BlockFace.Top;
        if (normal.y < 0) return BlockFace.Bottom;
        if (normal.x > 0) return BlockFace.East;
        if (normal.x < 0) return BlockFace.West;
        if (normal.z > 0) return BlockFace.North;
        return BlockFace.South;
    }

    static Texture2D GetTextureForFace(BlockSO block, BlockFace face)
    {
        return face switch
        {
            BlockFace.Top => block.texTop,
            BlockFace.Bottom => block.texBottom,
            BlockFace.North => block.texNorth,
            BlockFace.South => block.texSouth,
            BlockFace.East => block.texEast,
            BlockFace.West => block.texWest,
            _ => block.texTop
        };
    }
}