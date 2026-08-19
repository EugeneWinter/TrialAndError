using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct TerrainGenerationJob : IJob
{
    public int2 ColumnPos;
    public int Seed;
    public int SeaLevel;
    public NativeArray<ushort> Blocks;
    public NativeArray<int> Heightmap;

    private int Idx(int x, int y, int z)
    {
        return x + z * 32 + y * 1024;
    }

    public void Execute()
    {
        NoiseLib noise = new NoiseLib { Seed = Seed };
        FieldSampler sampler = new FieldSampler { Noise = noise, Seed = Seed };
        HeightComposer composer = new HeightComposer { SeaLevel = SeaLevel };
        TerrainRiverSampler riverSampler = new TerrainRiverSampler { Noise = noise, SeaLevel = SeaLevel };
        TerrainCaveCarver caveCarver = new TerrainCaveCarver { seed = Seed };

        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                int wx = ColumnPos.x * 32 + x;
                int wz = ColumnPos.y * 32 + z;

                TerrainFields fields = sampler.Sample(wx, wz);
                int baseSurfaceY = composer.Compose(fields, noise, wx, wz);

                TerrainRiverSampler.RiverSample river = riverSampler.Sample(wx, wz, fields, baseSurfaceY);
                int effectiveSurfaceY = river.HasWater ? river.GroundHeight : baseSurfaceY;

                Heightmap[x + z * 32] = effectiveSurfaceY;

                for (int y = 0; y < 512; y++)
                {
                    ushort block = ResolveBlock(wx, y, wz, effectiveSurfaceY, baseSurfaceY, river, fields, noise);

                    if (block != BlockIDs.Air && block != BlockIDs.Water)
                    {
                        if (caveCarver.ShouldCarve(wx, y, wz, effectiveSurfaceY, fields.PrimaryBiome))
                        {
                            if (y <= SeaLevel && fields.IsOcean)
                                block = BlockIDs.Water;
                            else if (river.HasWater && y <= river.WaterSurfaceHeight)
                                block = BlockIDs.Water;
                            else
                                block = BlockIDs.Air;
                        }
                    }

                    Blocks[Idx(x, y, z)] = block;
                }
            }
        }
    }

    private ushort ResolveBlock(int wx, int wy, int wz, int surfaceY, int baseSurfaceY,
        TerrainRiverSampler.RiverSample river, TerrainFields f, NoiseLib noise)
    {
        if (wy > surfaceY)
        {
            if (river.HasWater && wy <= river.WaterSurfaceHeight) return BlockIDs.Water;
            if (f.IsOcean && wy <= SeaLevel) return BlockIDs.Water;
            if (!f.IsOcean && baseSurfaceY <= SeaLevel + 1 && wy <= SeaLevel) return BlockIDs.Water;
            return BlockIDs.Air;
        }

        int depth = surfaceY - wy;
        int biome = f.PrimaryBiome;

        if (biome == WorldLayout.Center) return ResolveCenter(wx, wz, wy, depth, surfaceY, river.HasWater, noise);
        if (biome == WorldLayout.Canyons) return ResolveCanyons(wx, wz, wy, depth, noise);
        if (biome == WorldLayout.DeciduousForest) return ResolveForest(wy, depth);
        if (biome == WorldLayout.IceWastes) return ResolveIceWastes(wy, depth);
        if (biome == WorldLayout.Savanna) return ResolveSavanna(wy, depth);
        if (biome == WorldLayout.SnowyMountains) return ResolveMountains(wy, depth);
        if (biome == WorldLayout.Taiga) return ResolveTaiga(wy, depth, surfaceY);
        if (biome == WorldLayout.Desert) return ResolveDesert(wx, wz, wy, depth, noise);
        if (biome == WorldLayout.Tropics) return ResolveTropics(wy, depth);
        return ResolveOceanFloor(wy, depth);
    }

    private ushort ResolveCenter(int wx, int wz, int wy, int depth, int surfaceY, bool nearRiver, NoiseLib noise)
    {
        float2 p = new float2(wx, wz);
        float slopeEstimate = math.abs(noise.Fbm(p, 0.015f, 2, 2.0f, 0.5f));
        bool isSteep = slopeEstimate > 0.3f && depth < 3;
        if (isSteep) return BlockIDs.Andesite;

        if (depth == 0)
        {
            if (nearRiver) return BlockIDs.Gravel;
            if (surfaceY <= SeaLevel + 2)
            {
                float clayChance = noise.Fbm(p, 0.03f, 2) * 0.5f + 0.5f;
                return clayChance > 0.5f ? BlockIDs.Clay : BlockIDs.Sand;
            }
            return BlockIDs.Grass;
        }
        if (depth < 4) return BlockIDs.Dirt;
        if (depth < 7)
        {
            float clayNoise = noise.Fbm(p + 500f, 0.008f, 2) * 0.5f + 0.5f;
            return clayNoise > 0.45f ? BlockIDs.Clay : BlockIDs.Dirt;
        }
        if (depth < 12) return BlockIDs.Clay;
        if (depth < 25) return BlockIDs.Limestone;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Andesite;
    }

    private ushort ResolveCanyons(int wx, int wz, int wy, int depth, NoiseLib noise)
    {
        if (depth == 0) return BlockIDs.DryDirt;
        if (depth < 2) return BlockIDs.DryDirt;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        float layerNoise = noise.Fbm(new float2(wx, wz), 0.05f, 2) * 3f;
        int adjustedY = wy + (int)math.round(layerNoise);
        int layer = ((adjustedY % 24) + 24) % 24;
        if (layer < 6) return BlockIDs.RedSandstone;
        if (layer < 10) return BlockIDs.OchreSandstone;
        if (layer < 15) return BlockIDs.YellowSandstone;
        if (layer < 19) return BlockIDs.WhiteSandstone;
        if (layer < 22) return BlockIDs.RedSandstone;
        return BlockIDs.OchreSandstone;
    }

    private ushort ResolveForest(int wy, int depth)
    {
        if (depth == 0) return BlockIDs.Grass;
        if (depth < 5) return BlockIDs.ForestSoil;
        if (depth < 10) return BlockIDs.Clay;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Andesite;
    }

    private ushort ResolveIceWastes(int wy, int depth)
    {
        if (depth == 0) return BlockIDs.Snow;
        if (depth < 3) return BlockIDs.Snow;
        if (depth < 6) return BlockIDs.Permafrost;
        if (depth < 12) return BlockIDs.Gravel;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Andesite;
    }

    private ushort ResolveSavanna(int wy, int depth)
    {
        if (depth == 0) return BlockIDs.DryDirt;
        if (depth < 3) return BlockIDs.DryDirt;
        if (depth < 9) return BlockIDs.Laterite;
        if (depth < 16) return BlockIDs.Kaolin;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Andesite;
    }

    private ushort ResolveMountains(int wy, int depth)
    {
        if (wy > 300) { if (depth == 0) return BlockIDs.Snow; if (depth < 4) return BlockIDs.Gravel; }
        else if (wy > 200) { if (depth < 2) return BlockIDs.Gravel; }
        else { if (depth == 0) return BlockIDs.Grass; if (depth < 4) return BlockIDs.Dirt; }
        if (depth < 7) return BlockIDs.Gravel;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Granite;
    }

    private ushort ResolveTaiga(int wy, int depth, int surfaceY)
    {
        bool boggy = surfaceY <= SeaLevel + 2;
        if (depth == 0) return boggy ? BlockIDs.MudSoil : BlockIDs.Grass;
        if (depth < 4) return BlockIDs.Peat;
        if (depth < 7) return BlockIDs.MudSoil;
        if (depth < 11) return BlockIDs.Clay;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Andesite;
    }

    private ushort ResolveDesert(int wx, int wz, int wy, int depth, NoiseLib noise)
    {
        float sandDepth = 5f + noise.Fbm(new float2(wx, wz), 0.01f, 2) * 5f;
        if (depth <= (int)sandDepth) return BlockIDs.Sand;
        if (depth <= (int)sandDepth + 15) return BlockIDs.Sandstone;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Andesite;
    }

    private ushort ResolveTropics(int wy, int depth)
    {
        if (depth == 0) return BlockIDs.Grass;
        if (depth < 5) return BlockIDs.WetSoil;
        if (depth < 11) return BlockIDs.Laterite;
        if (wy < 5) return BlockIDs.Peridotite;
        if (wy < 15) return BlockIDs.Basalt;
        return BlockIDs.Andesite;
    }

    private ushort ResolveOceanFloor(int wy, int depth)
    {
        if (depth < 3) return BlockIDs.Sand;
        if (depth < 8) return BlockIDs.Gravel;
        return BlockIDs.Andesite;
    }
}