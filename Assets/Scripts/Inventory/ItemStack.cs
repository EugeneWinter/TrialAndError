[System.Serializable]
public struct ItemStack
{
    public ushort id;
    public int count;

    public bool IsEmpty => id == 0 || count <= 0;

    public static ItemStack Empty => new ItemStack { id = 0, count = 0 };
}