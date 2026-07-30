using UnityEngine;

[CreateAssetMenu(fileName = "KnappingTemplate", menuName = "Game Data/Knapping Template")]
public class KnappingTemplate : ScriptableObject
{
    public ushort resultItemId;
    public ushort failItemId;
    public int failCount = 2;

    [Header("Magic Scanner (Voxelizer)")]
    public Mesh sourceMesh;
    public Texture2D sourceTexture;
    public float voxelSize = 0.06f;

    [HideInInspector] public int width, height, depth;
    [HideInInspector] public bool[] solidData;
    [HideInInspector] public Color[] colorData;

    [ContextMenu("✨ MAGIC SCAN (Превратить в воксели) ✨")]
    public void Voxelize()
    {
        if (sourceMesh == null)
        {
            Debug.LogError("Добавь Source Mesh!");
            return;
        }

        Bounds b = sourceMesh.bounds;
        width = Mathf.CeilToInt(b.size.x / voxelSize);
        height = Mathf.CeilToInt(b.size.y / voxelSize);
        depth = Mathf.CeilToInt(b.size.z / voxelSize);

        solidData = new bool[width * height * depth];
        colorData = new Color[width * height * depth];

        GameObject tempObj = new GameObject("TempCollider");
        MeshCollider collider = tempObj.AddComponent<MeshCollider>();
        collider.sharedMesh = sourceMesh;

        int voxelsFound = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float worldX = b.min.x + x * voxelSize + voxelSize * 0.5f;
                float worldZ = b.min.z + z * voxelSize + voxelSize * 0.5f;

                RaycastHit topHit, bottomHit;
                bool hitTop = collider.Raycast(new Ray(new Vector3(worldX, b.max.y + 1f, worldZ), Vector3.down), out topHit, b.size.y + 2f);
                bool hitBot = collider.Raycast(new Ray(new Vector3(worldX, b.min.y - 1f, worldZ), Vector3.up), out bottomHit, b.size.y + 2f);

                if (hitTop && hitBot)
                {
                    int topY = Mathf.FloorToInt((topHit.point.y - b.min.y) / voxelSize);
                    int botY = Mathf.FloorToInt((bottomHit.point.y - b.min.y) / voxelSize);

                    Color col = new Color(0.5f, 0.5f, 0.5f);
                    if (sourceTexture != null)
                    {
                        Vector2 uv = topHit.textureCoord;
                        col = sourceTexture.GetPixelBilinear(uv.x, uv.y);
                    }

                    for (int y = botY; y <= topY; y++)
                    {
                        if (y >= 0 && y < height)
                        {
                            int idx = x + y * width + z * width * height;
                            solidData[idx] = true;
                            colorData[idx] = col;
                            voxelsFound++;
                        }
                    }
                }
            }
        }

        DestroyImmediate(tempObj);
        Debug.Log($"[Magic Scan] Готово! Найдено {voxelsFound} вокселей. Размер: {width}x{height}x{depth}");
    }

    public bool GetVoxel(int x, int y, int z)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth) return false;
        return solidData[x + y * width + z * width * height];
    }

    public Color GetColor(int x, int y, int z)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth) return Color.white;
        return colorData[x + y * width + z * width * height];
    }
}