using UnityEngine;
using System.Collections.Generic;

public class KnappingStone : MonoBehaviour
{
    public Material stoneMaterial;
    public Color highlightColor = new Color(1f, 0.3f, 0.2f, 1f);

    private bool[,,] voxels;
    private Color[,,] voxelColors;
    private float voxelSize;
    private int width, height, depth;

    private HashSet<Vector3Int> highlightedVoxels = new HashSet<Vector3Int>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    public bool[,,] Voxels => voxels;
    public int Width => width;
    public int Height => height;
    public int Depth => depth;
    public float VoxelSize => voxelSize;

    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "KnappingStoneMesh";
        meshFilter.mesh = mesh;
    }

    public void GenerateFromTemplate(KnappingTemplate template)
    {
        if (stoneMaterial != null)
            meshRenderer.material = stoneMaterial;

        width = template.width;
        height = template.height;
        depth = template.depth;
        voxelSize = template.voxelSize;

        voxels = new bool[width, height, depth];
        voxelColors = new Color[width, height, depth];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    voxels[x, y, z] = template.GetVoxel(x, y, z);
                    voxelColors[x, y, z] = template.GetColor(x, y, z);
                }

        BuildMesh();
    }

    public void RemoveVoxel(int x, int y, int z)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth) return;
        if (!voxels[x, y, z]) return;

        voxels[x, y, z] = false;
        BuildMesh();
    }

    public void HighlightVoxels(List<Vector3Int> voxelList)
    {
        highlightedVoxels.Clear();
        foreach (var v in voxelList)
            highlightedVoxels.Add(v);

        BuildMesh();
    }

    public void ClearHighlight()
    {
        if (highlightedVoxels.Count == 0) return;
        highlightedVoxels.Clear();
        BuildMesh();
    }

    public Vector3 VoxelMinToLocalSpace(int x, int y, int z)
    {
        return new Vector3(
            (x - width * 0.5f) * voxelSize,
            (y - height * 0.5f) * voxelSize,
            (z - depth * 0.5f) * voxelSize
        );
    }

    public Vector3 VoxelCenterToLocalSpace(int x, int y, int z)
    {
        return VoxelMinToLocalSpace(x, y, z) + Vector3.one * (voxelSize * 0.5f);
    }

    public Vector3 VoxelToLocalSpace(Vector3Int voxel)
    {
        return VoxelCenterToLocalSpace(voxel.x, voxel.y, voxel.z);
    }

    public void BuildMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Color> colors = new List<Color>();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    if (!voxels[x, y, z]) continue;

                    Vector3 pos = VoxelMinToLocalSpace(x, y, z);
                    bool isHighlighted = highlightedVoxels.Contains(new Vector3Int(x, y, z));
                    Color col = isHighlighted ? highlightColor : voxelColors[x, y, z];

                    if (y == height - 1 || !voxels[x, y + 1, z])
                        AddFace(vertices, triangles, normals, colors, pos + Vector3.up * voxelSize, Vector3.right * voxelSize, Vector3.forward * voxelSize, Vector3.up, col);

                    if (y == 0 || !voxels[x, y - 1, z])
                        AddFace(vertices, triangles, normals, colors, pos, Vector3.forward * voxelSize, Vector3.right * voxelSize, Vector3.down, col);

                    if (z == depth - 1 || !voxels[x, y, z + 1])
                        AddFace(vertices, triangles, normals, colors, pos + Vector3.forward * voxelSize, Vector3.up * voxelSize, Vector3.right * voxelSize, Vector3.forward, col);

                    if (z == 0 || !voxels[x, y, z - 1])
                        AddFace(vertices, triangles, normals, colors, pos, Vector3.right * voxelSize, Vector3.up * voxelSize, Vector3.back, col);

                    if (x == width - 1 || !voxels[x + 1, y, z])
                        AddFace(vertices, triangles, normals, colors, pos + Vector3.right * voxelSize, Vector3.forward * voxelSize, Vector3.up * voxelSize, Vector3.right, col);

                    if (x == 0 || !voxels[x - 1, y, z])
                        AddFace(vertices, triangles, normals, colors, pos, Vector3.up * voxelSize, Vector3.forward * voxelSize, Vector3.left, col);
                }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();
    }

    void AddFace(
        List<Vector3> verts,
        List<int> tris,
        List<Vector3> norms,
        List<Color> cols,
        Vector3 origin,
        Vector3 dirA,
        Vector3 dirB,
        Vector3 normal,
        Color color)
    {
        int start = verts.Count;

        verts.Add(origin);
        verts.Add(origin + dirA);
        verts.Add(origin + dirA + dirB);
        verts.Add(origin + dirB);

        Vector3 cross = Vector3.Cross(dirA, dirB).normalized;

        if (Vector3.Dot(cross, normal) < 0)
        {
            tris.Add(start);
            tris.Add(start + 2);
            tris.Add(start + 1);
            tris.Add(start);
            tris.Add(start + 3);
            tris.Add(start + 2);
        }
        else
        {
            tris.Add(start);
            tris.Add(start + 1);
            tris.Add(start + 2);
            tris.Add(start);
            tris.Add(start + 2);
            tris.Add(start + 3);
        }

        for (int i = 0; i < 4; i++)
        {
            norms.Add(normal);
            cols.Add(color);
        }
    }
    public int RemoveDisconnected()
    {
        int totalVoxels = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                    if (voxels[x, y, z]) totalVoxels++;

        if (totalVoxels <= 1) return 0;

        bool[,,] visited = new bool[width, height, depth];
        List<List<Vector3Int>> islands = new List<List<Vector3Int>>();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    if (!voxels[x, y, z] || visited[x, y, z]) continue;

                    List<Vector3Int> island = new List<Vector3Int>();
                    Queue<Vector3Int> queue = new Queue<Vector3Int>();

                    queue.Enqueue(new Vector3Int(x, y, z));
                    visited[x, y, z] = true;

                    while (queue.Count > 0)
                    {
                        Vector3Int current = queue.Dequeue();
                        island.Add(current);

                        TryEnqueue(queue, visited, current.x + 1, current.y, current.z);
                        TryEnqueue(queue, visited, current.x - 1, current.y, current.z);
                        TryEnqueue(queue, visited, current.x, current.y + 1, current.z);
                        TryEnqueue(queue, visited, current.x, current.y - 1, current.z);
                        TryEnqueue(queue, visited, current.x, current.y, current.z + 1);
                        TryEnqueue(queue, visited, current.x, current.y, current.z - 1);
                    }

                    islands.Add(island);
                }

        if (islands.Count <= 1) return 0;

        int largestIndex = 0;
        for (int i = 1; i < islands.Count; i++)
        {
            if (islands[i].Count > islands[largestIndex].Count)
                largestIndex = i;
        }

        int removed = 0;

        for (int i = 0; i < islands.Count; i++)
        {
            if (i == largestIndex) continue;

            foreach (Vector3Int v in islands[i])
            {
                voxels[v.x, v.y, v.z] = false;
                removed++;
            }
        }

        if (removed > 0)
            BuildMesh();

        return removed;
    }

    void TryEnqueue(Queue<Vector3Int> queue, bool[,,] visited, int x, int y, int z)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth) return;
        if (!voxels[x, y, z] || visited[x, y, z]) return;

        visited[x, y, z] = true;
        queue.Enqueue(new Vector3Int(x, y, z));
    }
}