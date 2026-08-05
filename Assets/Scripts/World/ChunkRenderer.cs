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
        Light dirLight = GetWorldDirectionalLight();
        if (dirLight != null)
            sunDir = dirLight.transform.forward;

        byte[] lightMap = LightEngine.CalculateLight(data, WorldManager.Instance, sunDir);

        StandardMeshBuffers standardBuffers = new StandardMeshBuffers
        {
            vertices = new NativeList<float3>(Allocator.TempJob),
            triangles = new NativeList<int>(Allocator.TempJob),
            uvs = new NativeList<float3>(Allocator.TempJob),
            normals = new NativeList<float3>(Allocator.TempJob),
            vertexColors = new NativeList<float4>(Allocator.TempJob)
        };

        LeafMeshBuffers leafBuffers = new LeafMeshBuffers
        {
            vertices = new NativeList<float3>(Allocator.TempJob),
            triangles = new NativeList<int>(Allocator.TempJob),
            uvs = new NativeList<float3>(Allocator.TempJob),
            normals = new NativeList<float3>(Allocator.TempJob),
            vertexColors = new NativeList<float4>(Allocator.TempJob)
        };

        GrassOverlayMeshBuffers grassBuffers = new GrassOverlayMeshBuffers
        {
            vertices = new NativeList<float3>(Allocator.TempJob),
            triangles = new NativeList<int>(Allocator.TempJob),
            uvs = new NativeList<float2>(Allocator.TempJob),
            normals = new NativeList<float3>(Allocator.TempJob),
            vertexColors = new NativeList<float4>(Allocator.TempJob)
        };

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
            standardBuffers = standardBuffers,
            leafBuffers = leafBuffers,
            grassOverlayBuffers = grassBuffers,
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

        UploadMeshData(standardBuffers, leafBuffers, grassBuffers);

        DisposeBuffers(standardBuffers, leafBuffers, grassBuffers);

        nativeLightMap.Dispose();
        sliceXN.Dispose();
        sliceXP.Dispose();
        sliceYN.Dispose();
        sliceYP.Dispose();
        sliceZN.Dispose();
        sliceZP.Dispose();

        SpawnCustomModels(world);
    }

    Light GetWorldDirectionalLight()
    {
        if (AtmosphereController.Instance != null && AtmosphereController.Instance.directionalLight != null)
            return AtmosphereController.Instance.directionalLight;

        if (CelestialCycle.Instance != null && CelestialCycle.Instance.directionalLight != null)
            return CelestialCycle.Instance.directionalLight;

        Light[] lights = GameObject.FindObjectsOfType<Light>();
        for (int i = 0; i < lights.Length; i++)
        {
            Light l = lights[i];
            if (l == null) continue;
            if (!l.isActiveAndEnabled) continue;
            if (l.type != LightType.Directional) continue;
            if (l.shadows == LightShadows.None) continue;
            return l;
        }

        for (int i = 0; i < lights.Length; i++)
        {
            Light l = lights[i];
            if (l == null) continue;
            if (!l.isActiveAndEnabled) continue;
            if (l.type != LightType.Directional) continue;
            return l;
        }

        return null;
    }

    void UploadMeshData(StandardMeshBuffers standard, LeafMeshBuffers leaf, GrassOverlayMeshBuffers grass)
    {
        mesh.Clear();

        int totalVerts = standard.vertices.Length + leaf.vertices.Length + grass.vertices.Length;
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

        int leafOffset = standard.vertices.Length;
        for (int i = 0; i < leaf.vertices.Length; i++)
        {
            allVerts[leafOffset + i] = leaf.vertices[i];
            allUvs[leafOffset + i] = leaf.uvs[i];
            allNorms[leafOffset + i] = leaf.normals[i];
            allColors[leafOffset + i] = new Color(leaf.vertexColors[i].x, leaf.vertexColors[i].y, leaf.vertexColors[i].z, leaf.vertexColors[i].w);
        }

        int grassOffset = standard.vertices.Length + leaf.vertices.Length;
        for (int i = 0; i < grass.vertices.Length; i++)
        {
            allVerts[grassOffset + i] = grass.vertices[i];
            allUvs[grassOffset + i] = new Vector3(grass.uvs[i].x, grass.uvs[i].y, 0);
            allNorms[grassOffset + i] = grass.normals[i];
            allColors[grassOffset + i] = new Color(grass.vertexColors[i].x, grass.vertexColors[i].y, grass.vertexColors[i].z, grass.vertexColors[i].w);
        }

        mesh.SetVertices(allVerts);
        mesh.SetUVs(0, allUvs);
        mesh.SetNormals(allNorms);
        mesh.SetColors(allColors);

        bool hasLeaves = leaf.triangles.Length > 0;
        bool hasGrass = grass.triangles.Length > 0;

        int subMeshCount = 1 + (hasLeaves ? 1 : 0) + (hasGrass ? 1 : 0);
        mesh.subMeshCount = subMeshCount;

        int currentSubMesh = 0;

        int[] mainTris = new int[standard.triangles.Length];
        for (int i = 0; i < standard.triangles.Length; i++) mainTris[i] = standard.triangles[i];
        mesh.SetTriangles(mainTris, currentSubMesh);
        currentSubMesh++;

        if (hasLeaves)
        {
            int[] leafTrisArray = new int[leaf.triangles.Length];
            for (int i = 0; i < leaf.triangles.Length; i++) leafTrisArray[i] = leaf.triangles[i] + leafOffset;
            mesh.SetTriangles(leafTrisArray, currentSubMesh);
            currentSubMesh++;
        }

        if (hasGrass)
        {
            int[] grassTrisArray = new int[grass.triangles.Length];
            for (int i = 0; i < grass.triangles.Length; i++) grassTrisArray[i] = grass.triangles[i] + grassOffset;
            mesh.SetTriangles(grassTrisArray, currentSubMesh);
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material mainMat = mr.sharedMaterials.Length > 0 ? mr.sharedMaterials[0] : null;

        List<Material> materialList = new List<Material> { mainMat };
        if (hasLeaves) materialList.Add(leafMaterial);
        if (hasGrass) materialList.Add(grassOverlayMaterial);
        mr.sharedMaterials = materialList.ToArray();
    }

    void DisposeBuffers(StandardMeshBuffers standard, LeafMeshBuffers leaf, GrassOverlayMeshBuffers grass)
    {
        standard.vertices.Dispose();
        standard.triangles.Dispose();
        standard.uvs.Dispose();
        standard.normals.Dispose();
        standard.vertexColors.Dispose();

        leaf.vertices.Dispose();
        leaf.triangles.Dispose();
        leaf.uvs.Dispose();
        leaf.normals.Dispose();
        leaf.vertexColors.Dispose();

        grass.vertices.Dispose();
        grass.triangles.Dispose();
        grass.uvs.Dispose();
        grass.normals.Dispose();
        grass.vertexColors.Dispose();
    }

    void SpawnCustomModels(WorldManager world)
    {
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