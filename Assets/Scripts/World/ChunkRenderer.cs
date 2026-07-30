using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    private Mesh mesh;
    private ChunkData data;
    private Transform modelsRoot;
    private List<GameObject> spawnedModels = new List<GameObject>();

    public void Initialize(ChunkData chunkData)
    {
        data = chunkData;
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        modelsRoot = new GameObject("ModelsRoot").transform;
        modelsRoot.SetParent(transform);
        modelsRoot.localPosition = Vector3.zero;

        RenderMesh();
    }

    public void RenderMesh()
    {
        Vector3 sunDir = Vector3.down;
        if (WorldManager.Instance != null)
        {
            Light dirLight = GameObject.FindObjectOfType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
                sunDir = dirLight.transform.forward;
        }

        byte[] lightMap = LightEngine.CalculateLight(data, WorldManager.Instance, sunDir);

        var verts = new NativeList<float3>(Allocator.TempJob);
        var tris = new NativeList<int>(Allocator.TempJob);
        var uvs = new NativeList<float3>(Allocator.TempJob);
        var norms = new NativeList<float3>(Allocator.TempJob);
        var colors = new NativeList<float4>(Allocator.TempJob);

        NativeArray<byte> nativeLightMap = new NativeArray<byte>(lightMap, Allocator.TempJob);

        WorldManager world = WorldManager.Instance;
        int3 coord = data.position;

        bool hasXN = world.HasChunk(coord + new int3(-1, 0, 0));
        bool hasXP = world.HasChunk(coord + new int3(1, 0, 0));
        bool hasYN = world.HasChunk(coord + new int3(0, -1, 0));
        bool hasYP = world.HasChunk(coord + new int3(0, 1, 0));
        bool hasZN = world.HasChunk(coord + new int3(0, 0, -1));
        bool hasZP = world.HasChunk(coord + new int3(0, 0, 1));

        NativeArray<ushort> sliceXN = hasXN ? world.GetChunkData(coord + new int3(-1, 0, 0)).ExtractBorderSlice(0, 1) : new NativeArray<ushort>(0, Allocator.TempJob);
        NativeArray<ushort> sliceXP = hasXP ? world.GetChunkData(coord + new int3(1, 0, 0)).ExtractBorderSlice(0, 0) : new NativeArray<ushort>(0, Allocator.TempJob);
        NativeArray<ushort> sliceYN = hasYN ? world.GetChunkData(coord + new int3(0, -1, 0)).ExtractBorderSlice(1, 1) : new NativeArray<ushort>(0, Allocator.TempJob);
        NativeArray<ushort> sliceYP = hasYP ? world.GetChunkData(coord + new int3(0, 1, 0)).ExtractBorderSlice(1, 0) : new NativeArray<ushort>(0, Allocator.TempJob);
        NativeArray<ushort> sliceZN = hasZN ? world.GetChunkData(coord + new int3(0, 0, -1)).ExtractBorderSlice(2, 1) : new NativeArray<ushort>(0, Allocator.TempJob);
        NativeArray<ushort> sliceZP = hasZP ? world.GetChunkData(coord + new int3(0, 0, 1)).ExtractBorderSlice(2, 0) : new NativeArray<ushort>(0, Allocator.TempJob);

        var job = new ChunkMeshJob
        {
            blocks = data.blocks,
            visualData = world.blockDatabase.GetVisualData(),
            lightMap = nativeLightMap,
            vertices = verts,
            triangles = tris,
            uvs = uvs,
            normals = norms,
            vertexColors = colors,
            neighborXNeg = sliceXN,
            neighborXPos = sliceXP,
            neighborYNeg = sliceYN,
            neighborYPos = sliceYP,
            neighborZNeg = sliceZN,
            neighborZPos = sliceZP,
            hasNeighborXNeg = hasXN,
            hasNeighborXPos = hasXP,
            hasNeighborYNeg = hasYN,
            hasNeighborYPos = hasYP,
            hasNeighborZNeg = hasZN,
            hasNeighborZPos = hasZP
        };

        job.Schedule().Complete();

        mesh.Clear();
        mesh.SetVertices(verts.AsArray().Reinterpret<Vector3>());
        mesh.SetIndices(tris.AsArray(), MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs.AsArray().Reinterpret<Vector3>());
        mesh.SetNormals(norms.AsArray().Reinterpret<Vector3>());
        mesh.SetColors(colors.AsArray().Reinterpret<Color>());

        verts.Dispose();
        tris.Dispose();
        uvs.Dispose();
        norms.Dispose();
        colors.Dispose();
        nativeLightMap.Dispose();
        sliceXN.Dispose(); sliceXP.Dispose(); sliceYN.Dispose(); sliceYP.Dispose(); sliceZN.Dispose(); sliceZP.Dispose();

        SpawnCustomModels(world);
    }

    void SpawnCustomModels(WorldManager world)
    {
        // Удаляем старые модели
        foreach (var model in spawnedModels) Destroy(model);
        spawnedModels.Clear();

        var visualData = world.blockDatabase.GetVisualData();

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int z = 0; z < 32; z++)
                {
                    ushort blockId = data.GetBlock(x, y, z);
                    if (blockId == 0 || blockId >= visualData.Length) continue;

                    if (visualData[blockId].isCustomModel)
                    {
                        BlockSO blockSO = world.blockDatabase.GetBlockSO(blockId);
                        if (blockSO != null && blockSO.customModelPrefab != null)
                        {
                            GameObject obj = Instantiate(blockSO.customModelPrefab, modelsRoot);
                            obj.transform.localPosition = new Vector3(x + 0.5f, y, z + 0.5f);

                            System.Random rng = new System.Random(data.position.x * 73856 + x * 19349 + z * 83492);
                            obj.transform.localRotation = Quaternion.Euler(0, rng.Next(0, 360), 0);

                            obj.transform.localScale = Vector3.one * 0.4f;

                            spawnedModels.Add(obj);
                        }
                    }
                }
            }
        }
    }
}