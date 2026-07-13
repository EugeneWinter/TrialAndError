using UnityEngine;

public class KnappingTrigger : MonoBehaviour
{
    public KnappingRecipe stoneBladeRecipe;

    void Update()
    {
        if (KnappingGame.Instance == null) return;
        if (GameManager.Instance.state != GameState.Playing) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            TryStartKnapping();
        }
    }

    void TryStartKnapping()
    {
        ushort selectedId = Inventory.Instance.slots[Inventory.Instance.selectedSlot].id;
        int selectedCount = Inventory.Instance.slots[Inventory.Instance.selectedSlot].count;

        if (selectedId == stoneBladeRecipe.inputItemId && selectedCount >= stoneBladeRecipe.inputCount)
        {
            KnappingGame.Instance.StartGame(stoneBladeRecipe);
        }
        else
        {
            Debug.Log($"Насри себе на голову");
        }
    }
}