using UnityEngine;

public static class BlockPreviewFactory
{
    private static Material sharedMaterial;

    public static GameObject CreateMiniBlock(BlockSO block, Texture2DArray textureArray)
    {
        GameObject cube = new GameObject("MiniBlock");

        MeshFilter mf = cube.AddComponent<MeshFilter>();
        MeshRenderer mr = cube.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        BuildMesh(mesh, block);
        mf.mesh = mesh;

        if (sharedMaterial == null || sharedMaterial.GetTexture("_TexArray") == null)
        {
            Shader shader = Shader.Find("Custom/BlockShader");
            if (shader == null)
            {
                Debug.LogError("BlockPreviewFactory: shader 'Custom/BlockShader' not found!");
                return cube;
            }

            sharedMaterial = new Material(shader);
            sharedMaterial.SetTexture("_TexArray", textureArray);
        }

        mr.sharedMaterial = sharedMaterial;

        return cube;
    }

    static void BuildMesh(Mesh mesh, BlockSO block)
    {
        Vector3[] vertices = new Vector3[24];
        Vector3[] uvs3 = new Vector3[24];
        Vector3[] normals = new Vector3[24];
        int[] triangles = new int[36];

        int vi = 0, ti = 0;

        AddFace(vertices, uvs3, normals, triangles, ref vi, ref ti,
            new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f),
            Vector3.up, block.indexTop);

        AddFace(vertices, uvs3, normals, triangles, ref vi, ref ti,
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            Vector3.down, block.indexBottom);

        AddFace(vertices, uvs3, normals, triangles, ref vi, ref ti,
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            Vector3.back, block.indexNorth);

        AddFace(vertices, uvs3, normals, triangles, ref vi, ref ti,
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
            Vector3.forward, block.indexSouth);

        AddFace(vertices, uvs3, normals, triangles, ref vi, ref ti,
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
            Vector3.left, block.indexWest);

        AddFace(vertices, uvs3, normals, triangles, ref vi, ref ti,
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            Vector3.right, block.indexEast);

        mesh.vertices = vertices;
        mesh.SetUVs(0, uvs3);
        mesh.normals = normals;
        mesh.triangles = triangles;
    }

    static void AddFace(Vector3[] vertices, Vector3[] uvs3, Vector3[] normals, int[] triangles,
        ref int vi, ref int ti,
        Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 normal, int texLayer)
    {
        int startV = vi;
        vertices[vi++] = v1;
        vertices[vi++] = v2;
        vertices[vi++] = v3;
        vertices[vi++] = v4;

        uvs3[startV] = new Vector3(0, 0, texLayer);
        uvs3[startV + 1] = new Vector3(0, 1, texLayer);
        uvs3[startV + 2] = new Vector3(1, 1, texLayer);
        uvs3[startV + 3] = new Vector3(1, 0, texLayer);

        for (int i = 0; i < 4; i++) normals[startV + i] = normal;

        triangles[ti++] = startV;
        triangles[ti++] = startV + 1;
        triangles[ti++] = startV + 2;
        triangles[ti++] = startV;
        triangles[ti++] = startV + 2;
        triangles[ti++] = startV + 3;
    }
}