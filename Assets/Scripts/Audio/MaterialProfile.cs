using UnityEngine;

[CreateAssetMenu(fileName = "New Material Profile", menuName = "Game Data/Material Profile")]
public class MaterialProfile : ScriptableObject
{
    [Range(0f, 1f)] public float hardness = 0.5f;
    [Range(0f, 1f)] public float density = 0.5f;
    [Range(0f, 1f)] public float brittleness = 0.3f;
    [Range(0f, 1f)] public float roughness = 0.5f;
    [Range(0f, 1f)] public float resonance = 0.5f;
    [Range(0f, 1f)] public float brightness = 0.5f;
    [Range(0f, 1f)] public float hollowness = 0f;
    [Range(0f, 1f)] public float graininess = 0.3f;
    [Range(0f, 1f)] public float wetness = 0f;
    [Range(0f, 1f)] public float metallicity = 0f;
    [Range(0f, 1f)] public float warmth = 0.5f;

    public float baseFreq = 200f;
    public float[] partialRatios = { 2.1f, 3.4f, 4.8f, 6.3f, 8.1f };
    public float partialDecayBase = 10f;

    public NoiseColor primaryNoise = NoiseColor.White;
    public NoiseColor secondaryNoise = NoiseColor.Brown;
}