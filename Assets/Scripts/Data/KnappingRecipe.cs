using UnityEngine;

[CreateAssetMenu(fileName = "New Knapping Recipe", menuName = "Game Data/Knapping Recipe")]
public class KnappingRecipe : ScriptableObject
{
    public ushort inputItemId;
    public int inputCount = 2;

    public ushort outputItemId;
    public int outputCount = 1;

    public string recipeName;
    public int hitCount = 5;
    public float targetZoneWidth = 0.2f;
    public float cursorSpeed = 1.0f;
}