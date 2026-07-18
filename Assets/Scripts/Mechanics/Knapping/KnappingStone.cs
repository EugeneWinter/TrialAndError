using UnityEngine;
using System.Collections.Generic;

public class KnappingStone : MonoBehaviour
{
    public int width = 20;
    public int height = 12;
    public int depth = 28;
    public float voxelSize = 0.04f;

    public Material stoneMaterial;

    [ColorUsage(false)]
    public Color baseColor = new Color(0x6B / 255f, 0x6D / 255f, 0x70 / 255f);
    public float colorVariation = 0.04f;

    private bool[,,] voxels;
    private Color[] voxelColors;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    public bool[,,] Voxels => voxels;
    public int Width => width;
    public int Height => height;
    public int Depth => depth;

    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        mesh = new Mesh();
        mesh.name = "KnappingStoneMesh";
        meshFilter.mesh = mesh;
    }

    public void Generate(int seed)
    {
        if (stoneMaterial != null && meshRenderer != null)
            meshRenderer.material = stoneMaterial;

        System.Random rng = new System.Random(seed);
        voxels = new bool[width, height, depth];
        voxelColors = new Color[width * height * depth];

        float cx = width * 0.5f;
        float cy = height * 0.5f;
        float cz = depth * 0.5f;

        float radiusX = width * (0.35f + (float)rng.NextDouble() * 0.1f);
        float radiusY = height * (0.35f + (float)rng.NextDouble() * 0.1f);
        float radiusZ = depth * (0.35f + (float)rng.NextDouble() * 0.1f);

        float noiseOffsetX = (float)rng.NextDouble() * 1000f;
        float noiseOffsetY = (float)rng.NextDouble() * 1000f;
        float noiseOffsetZ = (float)rng.NextDouble() * 1000f;

        float bigNoiseScale = 0.15f;
        float smallNoiseScale = 0.4f;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    float dx = (x - cx) / radiusX;
                    float dy = (y - cy) / radiusY;
                    float dz = (z - cz) / radiusZ;
                    float distSq = dx * dx + dy * dy + dz * dz;

                    float bigNoise = Mathf.PerlinNoise(
                        (x + noiseOffsetX) * bigNoiseScale,
                        (z + noiseOffsetZ) * bigNoiseScale) * 0.25f;

                    float smallNoise = Mathf.PerlinNoise(
                        (x + noiseOffsetX) * smallNoiseScale + y * 0.3f,
                        (z + noiseOffsetZ) * smallNoiseScale + y * 0.3f) * 0.15f;

                    float threshold = 1.0f + bigNoise + smallNoise;

                    if (distSq < threshold)
                    {
                        voxels[x, y, z] = true;
                    }
                }

        ErodeEdges(rng, 0.35f);
        RemoveFloatingVoxels();

        for (int i = 0; i < voxelColors.Length; i++)
        {
            float r = baseColor.r + ((float)rng.NextDouble() - 0.5f) * colorVariation;
            float g = baseColor.g + ((float)rng.NextDouble() - 0.5f) * colorVariation;
            float b = baseColor.b + ((float)rng.NextDouble() - 0.5f) * colorVariation;
            voxelColors[i] = new Color(
                Mathf.Clamp01(r),
                Mathf.Clamp01(g),
                Mathf.Clamp01(b));
        }

        BuildMesh();
    }

    void FillBlock(int x, int y, int z, int w, int h, int d)
    {
        for (int dx = 0; dx < w; dx++)
            for (int dy = 0; dy < h; dy++)
                for (int dz = 0; dz < d; dz++)
                {
                    int px = x + dx;
                    int py = y + dy;
                    int pz = z + dz;

                    if (px < 0 || px >= width || py < 0 || py >= height || pz < 0 || pz >= depth) continue;

                    voxels[px, py, pz] = true;
                }
    }

    void ErodeEdges(System.Random rng, float chance)
    {
        List<Vector3Int> toRemove = new List<Vector3Int>();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    if (!voxels[x, y, z]) continue;
                    int neighbors = CountFilledNeighbors(x, y, z);
                    if (neighbors <= 3 && rng.NextDouble() < chance)
                        toRemove.Add(new Vector3Int(x, y, z));
                }

        foreach (var v in toRemove)
            voxels[v.x, v.y, v.z] = false;
    }

    int CountFilledNeighbors(int x, int y, int z)
    {
        int count = 0;
        if (x > 0 && voxels[x - 1, y, z]) count++;
        if (x < width - 1 && voxels[x + 1, y, z]) count++;
        if (y > 0 && voxels[x, y - 1, z]) count++;
        if (y < height - 1 && voxels[x, y + 1, z]) count++;
        if (z > 0 && voxels[x, y, z - 1]) count++;
        if (z < depth - 1 && voxels[x, y, z + 1]) count++;
        return count;
    }

    void RemoveFloatingVoxels()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    if (!voxels[x, y, z]) continue;
                    if (CountFilledNeighbors(x, y, z) <= 1)
                        voxels[x, y, z] = false;
                }
    }

    bool IsSurfaceVoxel(int x, int y, int z)
    {
        if (!voxels[x, y, z]) return false;
        if (x == 0 || x == width - 1 || y == height - 1 || z == 0 || z == depth - 1) return true;
        if (!voxels[x + 1, y, z]) return true;
        if (!voxels[x - 1, y, z]) return true;
        if (!voxels[x, y + 1, z]) return true;
        if (!voxels[x, y, z + 1]) return true;
        if (!voxels[x, y, z - 1]) return true;
        return false;
    }

    public void RemoveVoxel(int x, int y, int z)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth) return;
        voxels[x, y, z] = false;
        BuildMesh();
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

                    Vector3 pos = new Vector3(
                        (x - width * 0.5f) * voxelSize,
                        y * voxelSize,
                        (z - depth * 0.5f) * voxelSize);

                    int voxelIdx = x + y * width + z * width * height;
                    Color voxColor = voxelColors[voxelIdx];

                    if (y == height - 1 || !voxels[x, y + 1, z])
                        AddFace(vertices, triangles, normals, colors, pos + new Vector3(0, voxelSize, 0), Vector3.right, Vector3.forward, Vector3.up, voxColor);
                    if (y == 0 || !voxels[x, y - 1, z])
                        AddFace(vertices, triangles, normals, colors, pos, Vector3.forward, Vector3.right, Vector3.down, voxColor);
                    if (z == depth - 1 || !voxels[x, y, z + 1])
                        AddFace(vertices, triangles, normals, colors, pos + new Vector3(0, 0, voxelSize), Vector3.up, Vector3.right, Vector3.forward, voxColor);
                    if (z == 0 || !voxels[x, y, z - 1])
                        AddFace(vertices, triangles, normals, colors, pos, Vector3.right, Vector3.up, Vector3.back, voxColor);
                    if (x == width - 1 || !voxels[x + 1, y, z])
                        AddFace(vertices, triangles, normals, colors, pos + new Vector3(voxelSize, 0, 0), Vector3.forward, Vector3.up, Vector3.right, voxColor);
                    if (x == 0 || !voxels[x - 1, y, z])
                        AddFace(vertices, triangles, normals, colors, pos, Vector3.up, Vector3.forward, Vector3.left, voxColor);
                }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
    }

    void AddFace(List<Vector3> verts, List<int> tris, List<Vector3> norms, List<Color> colors,
        Vector3 origin, Vector3 dirA, Vector3 dirB, Vector3 normal, Color color)
    {
        int start = verts.Count;
        Vector3 a = dirA * voxelSize;
        Vector3 b = dirB * voxelSize;

        verts.Add(origin);
        verts.Add(origin + a);
        verts.Add(origin + a + b);
        verts.Add(origin + b);

        Vector3 cross = Vector3.Cross(a, b).normalized;
        bool flip = Vector3.Dot(cross, normal) < 0;

        if (flip)
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
            colors.Add(color);
        }
    }
}