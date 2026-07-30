using UnityEngine;
using System.Collections.Generic;

public class KnappingPreview : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private int width;
    private int height;
    private int depth;
    private float voxelSize;

    public bool Setup(KnappingTemplate template, Material ghostMaterial, float voxelSizeOverride)
    {
        if (template == null)
        {
            Debug.LogError("[KnappingPreview] Template is null");
            return false;
        }

        if (ghostMaterial == null)
        {
            Debug.LogError("[KnappingPreview] Ghost Material is null");
            return false;
        }

        if (template.solidData == null || template.solidData.Length == 0)
        {
            Debug.LogError($"[KnappingPreview] Template '{template.name}' has no voxel data. Run MAGIC SCAN");
            return false;
        }

        if (template.width <= 0 || template.height <= 0 || template.depth <= 0)
        {
            Debug.LogError($"[KnappingPreview] Template '{template.name}' has invalid size: {template.width}x{template.height}x{template.depth}");
            return false;
        }

        width = template.width;
        height = template.height;
        depth = template.depth;
        voxelSize = voxelSizeOverride;

        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.sharedMaterial = ghostMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "KnappingPreviewMesh";
        }
        else
        {
            mesh.Clear();
        }

        meshFilter.sharedMesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    if (!template.GetVoxel(x, y, z)) continue;

                    Vector3 pos = new Vector3(
                        (x - width * 0.5f) * voxelSize,
                        (y - height * 0.5f) * voxelSize,
                        (z - depth * 0.5f) * voxelSize);

                    if (y == height - 1 || !template.GetVoxel(x, y + 1, z))
                        AddFace(vertices, triangles, normals, pos + Vector3.up * voxelSize, Vector3.right * voxelSize, Vector3.forward * voxelSize, Vector3.up);

                    if (y == 0 || !template.GetVoxel(x, y - 1, z))
                        AddFace(vertices, triangles, normals, pos, Vector3.forward * voxelSize, Vector3.right * voxelSize, Vector3.down);

                    if (z == depth - 1 || !template.GetVoxel(x, y, z + 1))
                        AddFace(vertices, triangles, normals, pos + Vector3.forward * voxelSize, Vector3.up * voxelSize, Vector3.right * voxelSize, Vector3.forward);

                    if (z == 0 || !template.GetVoxel(x, y, z - 1))
                        AddFace(vertices, triangles, normals, pos, Vector3.right * voxelSize, Vector3.up * voxelSize, Vector3.back);

                    if (x == width - 1 || !template.GetVoxel(x + 1, y, z))
                        AddFace(vertices, triangles, normals, pos + Vector3.right * voxelSize, Vector3.forward * voxelSize, Vector3.up * voxelSize, Vector3.right);

                    if (x == 0 || !template.GetVoxel(x - 1, y, z))
                        AddFace(vertices, triangles, normals, pos, Vector3.up * voxelSize, Vector3.forward * voxelSize, Vector3.left);
                }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mesh.RecalculateBounds();

        return true;
    }

    void AddFace(List<Vector3> verts, List<int> tris, List<Vector3> norms, Vector3 origin, Vector3 dirA, Vector3 dirB, Vector3 normal)
    {
        int vc = verts.Count;

        verts.Add(origin);
        verts.Add(origin + dirA);
        verts.Add(origin + dirA + dirB);
        verts.Add(origin + dirB);

        tris.Add(vc);
        tris.Add(vc + 1);
        tris.Add(vc + 2);
        tris.Add(vc);
        tris.Add(vc + 2);
        tris.Add(vc + 3);

        norms.Add(normal);
        norms.Add(normal);
        norms.Add(normal);
        norms.Add(normal);
    }
}