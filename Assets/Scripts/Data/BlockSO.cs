using UnityEngine;

[CreateAssetMenu(fileName = "New Block", menuName = "Game Data/Block")]
public class BlockSO : ScriptableObject
{
    public ushort id;
    public string blockName;

    [Header("Gameplay")]
    public float hardness = 1.0f;
    public bool breakable = true;
    public ushort dropId = 0;
    public int dropCount = 1;

    [Header("Mining Requirements")]
    public BlockCategory category = BlockCategory.None;
    public ToolType requiredTool = ToolType.None;
    public int requiredTier = 0;

    [Header("Visual Type")]
    public bool isSolid = true;
    public bool isTransparent = false;
    public bool isCustomModel = false;

    [Header("Custom Model (If isCustomModel = true)")]
    public GameObject customModelPrefab;

    [Header("Textures (For normal blocks)")]
    public Texture2D texTop;
    public Texture2D texBottom;
    public Texture2D texNorth;
    public Texture2D texSouth;
    public Texture2D texEast;
    public Texture2D texWest;

    [HideInInspector] public int indexTop;
    [HideInInspector] public int indexBottom;
    [HideInInspector] public int indexNorth;
    [HideInInspector] public int indexSouth;
    [HideInInspector] public int indexEast;
    [HideInInspector] public int indexWest;

    public void AutoFill()
    {
        Texture2D fallback = texTop ?? texNorth ?? texSouth ?? texEast ?? texWest ?? texBottom;
        if (fallback != null)
        {
            Texture2D sides = texNorth ?? texSouth ?? texEast ?? texWest ?? fallback;
            if (texTop == null) texTop = fallback;
            if (texBottom == null) texBottom = texTop;
            if (texNorth == null) texNorth = sides;
            if (texSouth == null) texSouth = texNorth;
            if (texEast == null) texEast = texNorth;
            if (texWest == null) texWest = texNorth;
        }

        if (dropId == 0) dropId = id;
    }
}