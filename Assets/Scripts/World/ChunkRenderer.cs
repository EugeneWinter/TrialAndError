using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    private Mesh mesh;
    private ChunkData data;

    public void Initialize(ChunkData chunkData)
    {
        data = chunkData;
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        RenderMesh();
    }

    public void RenderMesh()
    {
        var verts = new NativeList<float3>(Allocator.TempJob);
        var tris = new NativeList<int>(Allocator.TempJob);
        var uvs = new NativeList<float3>(Allocator.TempJob);
        var norms = new NativeList<float3>(Allocator.TempJob);

        var job = new ChunkMeshJob
        {
            blocks = data.blocks,
            visualData = WorldManager.Instance.blockDatabase.GetVisualData(),
            vertices = verts,
            triangles = tris,
            uvs = uvs,
            normals = norms
        };

        job.Schedule().Complete();

        mesh.Clear();
        mesh.SetVertices(verts.AsArray().Reinterpret<Vector3>());
        mesh.SetIndices(tris.AsArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs.AsArray().Reinterpret<Vector3>());
        mesh.SetNormals(norms.AsArray().Reinterpret<Vector3>());

        verts.Dispose();
        tris.Dispose();
        uvs.Dispose();
        norms.Dispose();
    }
}