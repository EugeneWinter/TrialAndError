using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item")]
public class ItemSO : ScriptableObject
{
    public ushort id;
    public string itemName;
    public int maxStack = 64;

    [Header("Type")]
    public ItemType itemType = ItemType.Material;

    [Header("3D Representation - Held")]
    public GameObject heldModel;
    public Vector3 heldPosition = new Vector3(0.3f, -0.3f, 0.5f);
    public Vector3 heldRotation = Vector3.zero;
    public Vector3 heldScale = Vector3.one;

    [Header("3D Representation - Ground")]
    public GameObject groundModel;
    public Vector3 groundScale = Vector3.one * 0.4f;
    public Vector3 groundRotationOffset = Vector3.zero;
    public float groundSink = 0.02f;

    [Header("3D Representation - Icon")]
    public Vector3 iconPosition = Vector3.zero;
    public Vector3 iconRotation = new Vector3(30f, 45f, 0f);
    public float iconScale = 1f;

    [Header("Tool Properties")]
    public ToolType toolType = ToolType.None;
    public int toolTier = 0;
    public float miningSpeedMultiplier = 1f;
    public float durability = 100f;
    public BlockCategory[] effectiveOn;
}