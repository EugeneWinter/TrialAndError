using UnityEngine;

public enum ItemType { Material, Tool }
public enum ToolType { None, Fist, Axe, Pickaxe, Shovel, Knife, Hammer }
public enum BlockCategory { None, Wood, Stone, Dirt, Ore, Plant }

[CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item")]
public class ItemSO : ScriptableObject
{
    public ushort id;
    public string itemName;
    public Texture2D icon;
    public int maxStack = 64;

    [Header("Type")]
    public ItemType itemType = ItemType.Material;

    [Header("3D Representation")]
    public GameObject heldModel;
    public Vector3 heldPosition = new Vector3(0.3f, -0.3f, 0.5f);
    public Vector3 heldRotation = Vector3.zero;
    public Vector3 heldScale = Vector3.one;

    [Header("Tool Properties")]
    public ToolType toolType = ToolType.None;
    public int toolTier = 0;
    public float miningSpeedMultiplier = 1f;
    public float durability = 100f;
    public BlockCategory[] effectiveOn;
}