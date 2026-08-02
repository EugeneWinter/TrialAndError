using UnityEngine;
using System.Collections.Generic;

public class InWorldCraftingManager : MonoBehaviour
{
    public static InWorldCraftingManager Instance;

    public List<InWorldRecipeSO> recipes;

    void Awake()
    {
        Instance = this;
    }

    public InWorldRecipeSO FindMatchingRecipe(ushort[] groundItems)
    {
        if (groundItems == null || groundItems.Length == 0) return null;

        Dictionary<ushort, int> groundCounts = new Dictionary<ushort, int>();
        foreach (ushort id in groundItems)
        {
            if (id == 0) continue;
            if (groundCounts.ContainsKey(id)) groundCounts[id]++;
            else groundCounts[id] = 1;
        }

        if (groundCounts.Count == 0) return null;

        foreach (var recipe in recipes)
        {
            if (recipe != null && IsMatch(groundCounts, recipe))
                return recipe;
        }

        return null;
    }

    private bool IsMatch(Dictionary<ushort, int> groundCounts, InWorldRecipeSO recipe)
    {
        int totalRecipeItems = 0;

        foreach (var comp in recipe.components)
        {
            if (!groundCounts.ContainsKey(comp.itemId)) return false;
            if (groundCounts[comp.itemId] != comp.count) return false;
            totalRecipeItems += comp.count;
        }

        int totalGroundItems = 0;
        foreach (var count in groundCounts.Values)
            totalGroundItems += count;

        return totalGroundItems == totalRecipeItems;
    }
}