using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CloudChunk : MonoBehaviour
{
    private int2 coord;
    private CloudLayer layer;
    private Mesh mesh;

    private float cachedWindX;
    private float cachedWindZ;
    private Vector3 basePosition;

    public void Initialize(int2 c, CloudLayer l)
    {
        coord = c;
        layer = l;
        basePosition = new Vector3(coord.x * 32, layer.cloudHeight, coord.y * 32);
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        Regenerate();
    }

    public void UpdateWindOffset(float windX, float windZ)
    {
        transform.position = basePosition - new Vector3(windX, 0, windZ);
    }

    public void Regenerate()
    {
        cachedWindX = 0f;
        cachedWindZ = 0f;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        int startX = coord.x * 32;
        int startZ = coord.y * 32;

        for (int x = 0; x < 32; x++)
            for (int y = 0; y < layer.cloudThickness; y++)
                for (int z = 0; z < 32; z++)
                {
                    int wx = startX + x;
                    int wy = layer.cloudHeight + y;
                    int wz = startZ + z;

                    if (!layer.IsCloudAt(wx, wy, wz, cachedWindX, cachedWindZ)) continue;

                    Vector3 pos = new Vector3(x, y, z);

                    if (!layer.IsCloudAt(wx, wy + 1, wz, cachedWindX, cachedWindZ))
                        AddFace(vertices, triangles, normals, pos + new Vector3(0, 1, 0), pos + new Vector3(0, 1, 1), pos + new Vector3(1, 1, 1), pos + new Vector3(1, 1, 0), Vector3.up);
                    if (!layer.IsCloudAt(wx, wy - 1, wz, cachedWindX, cachedWindZ))
                        AddFace(vertices, triangles, normals, pos + new Vector3(0, 0, 1), pos + new Vector3(0, 0, 0), pos + new Vector3(1, 0, 0), pos + new Vector3(1, 0, 1), Vector3.down);
                    if (!layer.IsCloudAt(wx, wy, wz - 1, cachedWindX, cachedWindZ))
                        AddFace(vertices, triangles, normals, pos + new Vector3(0, 0, 0), pos + new Vector3(0, 1, 0), pos + new Vector3(1, 1, 0), pos + new Vector3(1, 0, 0), Vector3.back);
                    if (!layer.IsCloudAt(wx, wy, wz + 1, cachedWindX, cachedWindZ))
                        AddFace(vertices, triangles, normals, pos + new Vector3(1, 0, 1), pos + new Vector3(1, 1, 1), pos + new Vector3(0, 1, 1), pos + new Vector3(0, 0, 1), Vector3.forward);
                    if (!layer.IsCloudAt(wx - 1, wy, wz, cachedWindX, cachedWindZ))
                        AddFace(vertices, triangles, normals, pos + new Vector3(0, 0, 1), pos + new Vector3(0, 1, 1), pos + new Vector3(0, 1, 0), pos + new Vector3(0, 0, 0), Vector3.left);
                    if (!layer.IsCloudAt(wx + 1, wy, wz, cachedWindX, cachedWindZ))
                        AddFace(vertices, triangles, normals, pos + new Vector3(1, 0, 0), pos + new Vector3(1, 1, 0), pos + new Vector3(1, 1, 1), pos + new Vector3(1, 0, 1), Vector3.right);
                }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
    }

    void AddFace(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 normal)
    {
        int vc = vertices.Count;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);
        triangles.Add(vc); triangles.Add(vc + 1); triangles.Add(vc + 2);
        triangles.Add(vc); triangles.Add(vc + 2); triangles.Add(vc + 3);
        normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);
    }
}