using UnityEngine;

[System.Serializable]
public struct RecipeComponent
{
    public ushort itemId;
    public int count;
}

[CreateAssetMenu(fileName = "New InWorld Recipe", menuName = "Game Data/In-World Recipe")]
public class InWorldRecipeSO : ScriptableObject
{
    public string recipeName = "Stone Axe";
    public RecipeComponent[] components;
    public ushort resultItemId;
    public int resultCount = 1;
    public float craftTime = 1.5f;
}