using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class WaterMeshBuilder : MonoBehaviour
{
    public static WaterMeshBuilder Instance;
    public Material waterMaterial;

    private Dictionary<int2, GameObject> waterChunks = new Dictionary<int2, GameObject>();

    void Awake() { Instance = this; }

    public void BuildWaterForWorld(int renderDistance, int seaLevel)
    {
        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
                RebuildWaterChunkColumn(x, z);
    }

    public void RebuildWaterChunkColumn(int chunkX, int chunkZ)
    {
        int2 key = new int2(chunkX, chunkZ);
        if (waterChunks.TryGetValue(key, out GameObject existing))
        {
            Destroy(existing);
            waterChunks.Remove(key);
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        int startX = chunkX * 32;
        int startZ = chunkZ * 32;

        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                int wx = startX + x;
                int wz = startZ + z;

                for (int y = 510; y >= 0; y--)
                {
                    ushort block = WorldManager.Instance.GetBlock(wx, y, wz);
                    if (block != BlockIDs.Water) continue;

                    ushort above = WorldManager.Instance.GetBlock(wx, y + 1, wz);
                    if (above == BlockIDs.Water) continue;

                    float waterOffset = -0.1f;
                    float wy = y + 1f + waterOffset;
                    int vc = vertices.Count;

                    vertices.Add(new Vector3(x, wy, z));
                    vertices.Add(new Vector3(x, wy, z + 1));
                    vertices.Add(new Vector3(x + 1, wy, z + 1));
                    vertices.Add(new Vector3(x + 1, wy, z));

                    triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 2);
                    triangles.Add(vc); triangles.Add(vc + 2); triangles.Add(vc + 3);

                    normals.Add(Vector3.up); normals.Add(Vector3.up);
                    normals.Add(Vector3.up); normals.Add(Vector3.up);
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
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
        mf.mesh = mesh;

        mr.material = waterMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        waterChunks[key] = obj;
    }
}