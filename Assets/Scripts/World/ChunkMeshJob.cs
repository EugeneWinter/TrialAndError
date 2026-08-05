using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ChunkMeshJob : IJob
{
    [ReadOnly] public NativeArray<ushort> blocks;
    [ReadOnly] public NativeArray<BlockDatabase.BlockVisualData> visualData;
    [ReadOnly] public NativeArray<byte> lightMap;

    [ReadOnly] public NativeArray<ushort> neighborXNeg;
    [ReadOnly] public NativeArray<ushort> neighborXPos;
    [ReadOnly] public NativeArray<ushort> neighborYNeg;
    [ReadOnly] public NativeArray<ushort> neighborYPos;
    [ReadOnly] public NativeArray<ushort> neighborZNeg;
    [ReadOnly] public NativeArray<ushort> neighborZPos;

    public bool hasNeighborXNeg;
    public bool hasNeighborXPos;
    public bool hasNeighborYNeg;
    public bool hasNeighborYPos;
    public bool hasNeighborZNeg;
    public bool hasNeighborZPos;

    public StandardMeshBuffers standardBuffers;
    public LeafMeshBuffers leafBuffers;
    public GrassOverlayMeshBuffers grassOverlayBuffers;

    public void Execute()
    {
        ChunkBlockAccess access = new ChunkBlockAccess
        {
            blocks = blocks,
            visualData = visualData,
            neighborXNeg = neighborXNeg,
            neighborXPos = neighborXPos,
            neighborYNeg = neighborYNeg,
            neighborYPos = neighborYPos,
            neighborZNeg = neighborZNeg,
            neighborZPos = neighborZPos,
            hasNeighborXNeg = hasNeighborXNeg,
            hasNeighborXPos = hasNeighborXPos,
            hasNeighborYNeg = hasNeighborYNeg,
            hasNeighborYPos = hasNeighborYPos,
            hasNeighborZNeg = hasNeighborZNeg,
            hasNeighborZPos = hasNeighborZPos
        };

        ChunkLighting lightingCtx = new ChunkLighting
        {
            lightMap = lightMap
        };

        StandardFaceBuilder faceBuilder = new StandardFaceBuilder
        {
            access = access,
            lighting = lightingCtx
        };

        LeafMeshBuilder leafBuilder = new LeafMeshBuilder
        {
            access = access,
            lighting = lightingCtx
        };

        GrassOverlayBuilder grassBuilder = new GrassOverlayBuilder
        {
            access = access,
            lighting = lightingCtx
        };

        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                for (int z = 0; z < 32; z++)
                {
                    ushort block = access.GetBlock(x, y, z);
                    if (block == 0) continue;
                    if (block == 6) continue;
                    if (block >= visualData.Length) continue;

                    var v = visualData[block];

                    if (v.isCustomModel) continue;

                    bool isLeaf = (block == LeafMeshBuilder.LEAF_ID);
                    bool isGrass = (block == GrassOverlayBuilder.GRASS_ID);
                    float leafDensity = isLeaf ? leafBuilder.CalculateLeafDensity(x, y, z) : 0f;

                    if (FaceCullingRules.ShouldDrawFace(block, access.GetBlock(x, y + 1, z), access))
                        faceBuilder.AddFaceTop(x, y, z, v.top, ref standardBuffers, ref leafBuffers, isLeaf, leafDensity);
                    if (FaceCullingRules.ShouldDrawFace(block, access.GetBlock(x, y - 1, z), access))
                        faceBuilder.AddFaceBottom(x, y, z, v.bottom, ref standardBuffers, ref leafBuffers, isLeaf, leafDensity);
                    if (FaceCullingRules.ShouldDrawFace(block, access.GetBlock(x, y, z - 1), access))
                        faceBuilder.AddFaceNorth(x, y, z, v.north, ref standardBuffers, ref leafBuffers, isLeaf, leafDensity);
                    if (FaceCullingRules.ShouldDrawFace(block, access.GetBlock(x, y, z + 1), access))
                        faceBuilder.AddFaceSouth(x, y, z, v.south, ref standardBuffers, ref leafBuffers, isLeaf, leafDensity);
                    if (FaceCullingRules.ShouldDrawFace(block, access.GetBlock(x - 1, y, z), access))
                        faceBuilder.AddFaceWest(x, y, z, v.west, ref standardBuffers, ref leafBuffers, isLeaf, leafDensity);
                    if (FaceCullingRules.ShouldDrawFace(block, access.GetBlock(x + 1, y, z), access))
                        faceBuilder.AddFaceEast(x, y, z, v.east, ref standardBuffers, ref leafBuffers, isLeaf, leafDensity);

                    if (isLeaf)
                        leafBuilder.AddCrossQuads(x, y, z, v.top, leafDensity, ref leafBuffers);

                    if (isGrass)
                    {
                        ushort blockAbove = access.GetBlock(x, y + 1, z);
                        if (!access.IsOpaque(blockAbove))
                            grassBuilder.AddOverlays(x, y, z, ref grassOverlayBuffers);
                    }
                }
    }
}