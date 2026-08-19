public enum BiomeType
{
    Center,
    IceWastes,
    Canyons,
    Savanna,
    DeciduousForest,
    SnowyMountains,
    Taiga,
    Desert,
    Tropics,
    Ocean
}

public struct BiomeSample
{
    public BiomeType primary;
    public BiomeType secondary;
    public float blend;
    public bool isOcean;
    public float temperature;
    public float moisture;
    public float continentFactor;
}