using UnityEngine;
using Unity.Mathematics;

public class BlockHighlight : MonoBehaviour
{
    public static BlockHighlight Instance;

    [Header("Visual")]
    public Material highlightMaterial;

    private GameObject highlightObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private int3 currentBlockPos;
    private bool isActive;

    void Awake()
    {
        Instance = this;
        CreateHighlightObject();
        Hide();
    }

    void CreateHighlightObject()
    {
        highlightObject = new GameObject("BlockHighlight");
        highlightObject.transform.SetParent(transform);

        meshFilter = highlightObject.AddComponent<MeshFilter>();
        meshRenderer = highlightObject.AddComponent<MeshRenderer>();

        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        if (highlightMaterial != null)
            meshRenderer.material = highlightMaterial;

        mesh = BuildCubeMesh();
        meshFilter.mesh = mesh;
    }

    public void Show(int3 blockPos)
    {
        if (isActive && blockPos.Equals(currentBlockPos))
            return;

        currentBlockPos = blockPos;
        isActive = true;

        highlightObject.transform.position = new Vector3(
            blockPos.x + 0.5f,
            blockPos.y + 0.5f,
            blockPos.z + 0.5f
        );

        highlightObject.SetActive(true);
    }

    public void Hide()
    {
        if (!isActive) return;

        isActive = false;
        highlightObject.SetActive(false);
    }

    Mesh BuildCubeMesh()
    {
        float s = 0.5005f;

        Vector3[] vertices = new Vector3[24];
        Vector2[] uvs = new Vector2[24];
        Vector3[] normals = new Vector3[24];
        int[] triangles = new int[36];

        int vi = 0;
        int ti = 0;

        AddFace(vertices, uvs, normals, triangles, ref vi, ref ti,
            new Vector3(-s, s, -s), new Vector3(-s, s, s),
            new Vector3(s, s, s), new Vector3(s, s, -s),
            Vector3.up);

        AddFace(vertices, uvs, normals, triangles, ref vi, ref ti,
            new Vector3(-s, -s, s), new Vector3(-s, -s, -s),
            new Vector3(s, -s, -s), new Vector3(s, -s, s),
            Vector3.down);

        AddFace(vertices, uvs, normals, triangles, ref vi, ref ti,
            new Vector3(-s, -s, -s), new Vector3(-s, s, -s),
            new Vector3(s, s, -s), new Vector3(s, -s, -s),
            Vector3.back);

        AddFace(vertices, uvs, normals, triangles, ref vi, ref ti,
            new Vector3(s, -s, s), new Vector3(s, s, s),
            new Vector3(-s, s, s), new Vector3(-s, -s, s),
            Vector3.forward);

        AddFace(vertices, uvs, normals, triangles, ref vi, ref ti,
            new Vector3(-s, -s, s), new Vector3(-s, s, s),
            new Vector3(-s, s, -s), new Vector3(-s, -s, -s),
            Vector3.left);

        AddFace(vertices, uvs, normals, triangles, ref vi, ref ti,
            new Vector3(s, -s, -s), new Vector3(s, s, -s),
            new Vector3(s, s, s), new Vector3(s, -s, s),
            Vector3.right);

        Mesh m = new Mesh();
        m.name = "BlockHighlightMesh";
        m.vertices = vertices;
        m.uv = uvs;
        m.normals = normals;
        m.triangles = triangles;
        return m;
    }

    void AddFace(Vector3[] verts, Vector2[] uvs, Vector3[] norms, int[] tris,
        ref int vi, ref int ti,
        Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
        Vector3 normal)
    {
        verts[vi] = v0;
        verts[vi + 1] = v1;
        verts[vi + 2] = v2;
        verts[vi + 3] = v3;

        uvs[vi] = new Vector2(0, 0);
        uvs[vi + 1] = new Vector2(0, 1);
        uvs[vi + 2] = new Vector2(1, 1);
        uvs[vi + 3] = new Vector2(1, 0);

        norms[vi] = normal;
        norms[vi + 1] = normal;
        norms[vi + 2] = normal;
        norms[vi + 3] = normal;

        tris[ti] = vi;
        tris[ti + 1] = vi + 1;
        tris[ti + 2] = vi + 2;
        tris[ti + 3] = vi;
        tris[ti + 4] = vi + 2;
        tris[ti + 5] = vi + 3;

        vi += 4;
        ti += 6;
    }
}