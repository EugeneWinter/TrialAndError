public static class BiomeDefinition
{
    public static bool HasTrees(int biome)
    {
        return biome == WorldLayout.Center ||
               biome == WorldLayout.DeciduousForest ||
               biome == WorldLayout.Taiga ||
               biome == WorldLayout.Tropics ||
               biome == WorldLayout.SnowyMountains;
    }

    public static bool IsWet(int biome)
    {
        return biome == WorldLayout.Center ||
               biome == WorldLayout.DeciduousForest ||
               biome == WorldLayout.Taiga ||
               biome == WorldLayout.Tropics;
    }

    public static bool IsDry(int biome)
    {
        return biome == WorldLayout.Desert ||
               biome == WorldLayout.Canyons ||
               biome == WorldLayout.Savanna;
    }

    public static bool IsCold(int biome)
    {
        return biome == WorldLayout.IceWastes ||
               biome == WorldLayout.SnowyMountains ||
               biome == WorldLayout.Taiga;
    }
}