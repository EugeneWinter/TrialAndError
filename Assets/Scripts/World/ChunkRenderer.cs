using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public Material leafMaterial;
    public Material grassOverlayMaterial;

    private Mesh mesh;
    private ChunkColumn columnData;
    private int chunkY;
    private bool isGenerating;

    public void Initialize(ChunkColumn column)
    {
        columnData = column;
        chunkY = Mathf.FloorToInt(transform.position.y / 32f);

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        RenderMesh();
    }

    public void RenderMesh()
    {
        if (isGenerating || columnData == null) return;
        StartCoroutine(RenderRoutine());
    }

    private System.Collections.IEnumerator RenderRoutine()
    {
        isGenerating = true;

        byte[] lightMap = ComputeSimpleLight();

        StandardMeshBuffers standardBuffers = CreateStandardBuffers();
        LeafMeshBuffers leafBuffers = CreateLeafBuffers();
        GrassOverlayMeshBuffers grassBuffers = CreateGrassBuffers();
        NativeArray<byte> nativeLightMap = new NativeArray<byte>(lightMap, Allocator.TempJob);

        WorldManager world = WorldManager.Instance;
        NativeArray<BlockDatabase.BlockVisualData> visualData = world.blockDatabase.GetVisualData();

        int2 colPos = columnData.Position;
        NativeArray<ushort> nXNeg = default;
        NativeArray<ushort> nXPos = default;
        NativeArray<ushort> nZNeg = default;
        NativeArray<ushort> nZPos = default;
        bool hasXN = false, hasXP = false, hasZN = false, hasZP = false;

        try
        {
            nXNeg = GetNeighborBlocks(new int2(colPos.x - 1, colPos.y), out hasXN);
            nXPos = GetNeighborBlocks(new int2(colPos.x + 1, colPos.y), out hasXP);
            nZNeg = GetNeighborBlocks(new int2(colPos.x, colPos.y - 1), out hasZN);
            nZPos = GetNeighborBlocks(new int2(colPos.x, colPos.y + 1), out hasZP);

            var job = new ChunkMeshJob
            {
                blocks = columnData.Blocks,
                visualData = visualData,
                lightMap = nativeLightMap,
                chunkY = chunkY,
                neighborXNeg = nXNeg,
                hasNeighborXNeg = hasXN,
                neighborXPos = nXPos,
                hasNeighborXPos = hasXP,
                neighborZNeg = nZNeg,
                hasNeighborZNeg = hasZN,
                neighborZPos = nZPos,
                hasNeighborZPos = hasZP,
                standardBuffers = standardBuffers,
                leafBuffers = leafBuffers,
                grassOverlayBuffers = grassBuffers
            };

            JobHandle handle = job.Schedule();
            while (!handle.IsCompleted) yield return null;
            handle.Complete();

            UploadMeshData(standardBuffers, leafBuffers, grassBuffers);
        }
        finally
        {
            DisposeBuffers(standardBuffers, leafBuffers, grassBuffers);
            if (nativeLightMap.IsCreated) nativeLightMap.Dispose();
            if (nXNeg.IsCreated) nXNeg.Dispose();
            if (nXPos.IsCreated) nXPos.Dispose();
            if (nZNeg.IsCreated) nZNeg.Dispose();
            if (nZPos.IsCreated) nZPos.Dispose();
        }

        isGenerating = false;
    }

    private NativeArray<ushort> GetNeighborBlocks(int2 neighborPos, out bool exists)
    {
        ChunkColumn neighbor = WorldManager.Instance.GetColumn(neighborPos);
        if (neighbor != null && neighbor.IsDataGenerated)
        {
            exists = true;
            NativeArray<ushort> copy = new NativeArray<ushort>(neighbor.Blocks.Length, Allocator.TempJob);
            NativeArray<ushort>.Copy(neighbor.Blocks, copy);
            return copy;
        }
        exists = false;
        return new NativeArray<ushort>(0, Allocator.TempJob);
    }

    private byte[] ComputeSimpleLight()
    {
        byte[] light = new byte[32 * 32 * 32];
        int baseY = chunkY * 32;

        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                int surfaceY = columnData.Heightmap[x + z * 32];
                for (int y = 31; y >= 0; y--)
                {
                    int globalY = baseY + y;
                    int idx = x + y * 32 + z * 32 * 32;
                    light[idx] = (byte)(globalY > surfaceY ? 15 : 4);
                }
            }
        }
        return light;
    }

    private void UploadMeshData(StandardMeshBuffers standard, LeafMeshBuffers leaf, GrassOverlayMeshBuffers grass)
    {
        mesh.Clear();

        int totalVerts = standard.vertices.Length + leaf.vertices.Length + grass.vertices.Length;
        if (totalVerts == 0) return;

        Vector3[] allVerts = new Vector3[totalVerts];
        Vector3[] allUvs = new Vector3[totalVerts];
        Vector3[] allNorms = new Vector3[totalVerts];
        Color[] allColors = new Color[totalVerts];

        for (int i = 0; i < standard.vertices.Length; i++)
        {
            allVerts[i] = standard.vertices[i];
            allUvs[i] = standard.uvs[i];
            allNorms[i] = standard.normals[i];
            allColors[i] = new Color(standard.vertexColors[i].x, standard.vertexColors[i].y, standard.vertexColors[i].z, standard.vertexColors[i].w);
        }

        int leafOff = standard.vertices.Length;
        for (int i = 0; i < leaf.vertices.Length; i++)
        {
            allVerts[leafOff + i] = leaf.vertices[i];
            allUvs[leafOff + i] = leaf.uvs[i];
            allNorms[leafOff + i] = leaf.normals[i];
            allColors[leafOff + i] = new Color(leaf.vertexColors[i].x, leaf.vertexColors[i].y, leaf.vertexColors[i].z, leaf.vertexColors[i].w);
        }

        int grassOff = standard.vertices.Length + leaf.vertices.Length;
        for (int i = 0; i < grass.vertices.Length; i++)
        {
            allVerts[grassOff + i] = grass.vertices[i];
            allUvs[grassOff + i] = new Vector3(grass.uvs[i].x, grass.uvs[i].y, 0);
            allNorms[grassOff + i] = grass.normals[i];
            allColors[grassOff + i] = new Color(grass.vertexColors[i].x, grass.vertexColors[i].y, grass.vertexColors[i].z, grass.vertexColors[i].w);
        }

        mesh.SetVertices(allVerts);
        mesh.SetUVs(0, new List<Vector3>(allUvs));
        mesh.SetNormals(allNorms);
        mesh.SetColors(allColors);

        bool hasLeaves = leaf.triangles.Length > 0;
        bool hasGrass = grass.triangles.Length > 0;

        int subMeshCount = 1 + (hasLeaves ? 1 : 0) + (hasGrass ? 1 : 0);
        mesh.subMeshCount = subMeshCount;
        int sub = 0;

        int[] mainTris = new int[standard.triangles.Length];
        for (int i = 0; i < standard.triangles.Length; i++) mainTris[i] = standard.triangles[i];
        mesh.SetTriangles(mainTris, sub++);

        if (hasLeaves)
        {
            int[] leafTris = new int[leaf.triangles.Length];
            for (int i = 0; i < leaf.triangles.Length; i++) leafTris[i] = leaf.triangles[i] + leafOff;
            mesh.SetTriangles(leafTris, sub++);
        }

        if (hasGrass)
        {
            int[] grassTris = new int[grass.triangles.Length];
            for (int i = 0; i < grass.triangles.Length; i++) grassTris[i] = grass.triangles[i] + grassOff;
            mesh.SetTriangles(grassTris, sub);
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material mainMat = mr.sharedMaterials.Length > 0 ? mr.sharedMaterials[0] : null;
        List<Material> mats = new List<Material> { mainMat };
        if (hasLeaves) mats.Add(leafMaterial);
        if (hasGrass) mats.Add(grassOverlayMaterial);
        mr.sharedMaterials = mats.ToArray();

        mesh.RecalculateBounds();
    }

    private StandardMeshBuffers CreateStandardBuffers() => new StandardMeshBuffers
    {
        vertices = new NativeList<float3>(Allocator.TempJob),
        triangles = new NativeList<int>(Allocator.TempJob),
        uvs = new NativeList<float3>(Allocator.TempJob),
        normals = new NativeList<float3>(Allocator.TempJob),
        vertexColors = new NativeList<float4>(Allocator.TempJob)
    };

    private LeafMeshBuffers CreateLeafBuffers() => new LeafMeshBuffers
    {
        vertices = new NativeList<float3>(Allocator.TempJob),
        triangles = new NativeList<int>(Allocator.TempJob),
        uvs = new NativeList<float3>(Allocator.TempJob),
        normals = new NativeList<float3>(Allocator.TempJob),
        vertexColors = new NativeList<float4>(Allocator.TempJob)
    };

    private GrassOverlayMeshBuffers CreateGrassBuffers() => new GrassOverlayMeshBuffers
    {
        vertices = new NativeList<float3>(Allocator.TempJob),
        triangles = new NativeList<int>(Allocator.TempJob),
        uvs = new NativeList<float2>(Allocator.TempJob),
        normals = new NativeList<float3>(Allocator.TempJob),
        vertexColors = new NativeList<float4>(Allocator.TempJob)
    };

    private void DisposeBuffers(StandardMeshBuffers s, LeafMeshBuffers l, GrassOverlayMeshBuffers g)
    {
        if (s.vertices.IsCreated) s.vertices.Dispose();
        if (s.triangles.IsCreated) s.triangles.Dispose();
        if (s.uvs.IsCreated) s.uvs.Dispose();
        if (s.normals.IsCreated) s.normals.Dispose();
        if (s.vertexColors.IsCreated) s.vertexColors.Dispose();
        if (l.vertices.IsCreated) l.vertices.Dispose();
        if (l.triangles.IsCreated) l.triangles.Dispose();
        if (l.uvs.IsCreated) l.uvs.Dispose();
        if (l.normals.IsCreated) l.normals.Dispose();
        if (l.vertexColors.IsCreated) l.vertexColors.Dispose();
        if (g.vertices.IsCreated) g.vertices.Dispose();
        if (g.triangles.IsCreated) g.triangles.Dispose();
        if (g.uvs.IsCreated) g.uvs.Dispose();
        if (g.normals.IsCreated) g.normals.Dispose();
        if (g.vertexColors.IsCreated) g.vertexColors.Dispose();
    }
}