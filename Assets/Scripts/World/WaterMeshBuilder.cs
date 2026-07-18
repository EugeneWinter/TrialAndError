using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class WaterMeshBuilder : MonoBehaviour
{
    public static WaterMeshBuilder Instance;

    public Material waterMaterial;

    private Dictionary<int2, GameObject> waterChunks = new Dictionary<int2, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void BuildWaterForWorld(int renderDistance, int seaLevel)
    {
        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
                BuildWaterChunk(x, z, seaLevel);
    }

    void BuildWaterChunk(int chunkX, int chunkZ, int seaLevel)
    {
        int2 key = new int2(chunkX, chunkZ);
        if (waterChunks.ContainsKey(key)) return;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        int startX = chunkX * 32;
        int startZ = chunkZ * 32;

        for (int x = 0; x < 32; x++)
            for (int z = 0; z < 32; z++)
            {
                int wx = startX + x;
                int wz = startZ + z;

                ushort blockAtSea = WorldManager.Instance.GetBlock(wx, seaLevel, wz);
                ushort blockAboveSea = WorldManager.Instance.GetBlock(wx, seaLevel + 1, wz);

                if (blockAtSea == 6 && blockAboveSea == 0)
                {
                    AddTopFace(vertices, triangles, normals, x, seaLevel, z);
                }
                else if (blockAtSea == 0)
                {
                    for (int y = seaLevel; y >= seaLevel - 3; y--)
                    {
                        ushort below = WorldManager.Instance.GetBlock(wx, y, wz);
                        if (below == 6)
                        {
                            ushort above = WorldManager.Instance.GetBlock(wx, y + 1, wz);
                            if (above == 0)
                            {
                                AddTopFace(vertices, triangles, normals, x, y, z);
                                break;
                            }
                        }
                    }
                }
            }

        if (vertices.Count == 0) return;

        GameObject obj = new GameObject($"Water_{chunkX}_{chunkZ}");
        obj.transform.SetParent(transform);
        obj.transform.position = new Vector3(startX, 0, startZ);

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mf.mesh = mesh;

        mr.material = waterMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        waterChunks[key] = obj;
    }

    void AddTopFace(List<Vector3> verts, List<int> tris, List<Vector3> norms, int x, int y, int z)
    {
        float waterOffset = -0.1f;
        float wy = y + 1f + waterOffset;

        int vc = verts.Count;

        verts.Add(new Vector3(x, wy, z));
        verts.Add(new Vector3(x, wy, z + 1));
        verts.Add(new Vector3(x + 1, wy, z + 1));
        verts.Add(new Vector3(x + 1, wy, z));

        tris.Add(vc);
        tris.Add(vc + 1);
        tris.Add(vc + 2);
        tris.Add(vc);
        tris.Add(vc + 2);
        tris.Add(vc + 3);

        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
        norms.Add(Vector3.up);
    }
}