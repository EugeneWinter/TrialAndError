using UnityEngine;

public enum RoomType
{
    Open,
    Forest,
    Cave,
    WoodInterior,
    StoneInterior,
    MetalRoom,
    SciFiChamber,
    Underwater
}

public enum TechEra
{
    Stone,
    Bronze,
    Iron,
    Medieval,
    Industrial,
    Modern,
    Space
}

public struct SfxContext
{
    public float velocity;
    public float mass;
    public float wetness;
    public RoomType room;
    public TechEra era;
    public MaterialProfile surfaceMaterial;
    public MaterialProfile toolMaterial;

    public static SfxContext Default()
    {
        return new SfxContext
        {
            velocity = 1f,
            mass = 1f,
            wetness = 0f,
            room = RoomType.Open,
            era = TechEra.Stone,
            surfaceMaterial = null,
            toolMaterial = null
        };
    }
}