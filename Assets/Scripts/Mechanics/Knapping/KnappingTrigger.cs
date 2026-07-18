using UnityEngine;

public class KnappingTrigger : MonoBehaviour
{
    public ushort requiredItemId = 1001;
    public int requiredCount = 2;

    void Update()
    {
        if (KnappingSession.Instance == null) return;
        if (GameManager.Instance.state != GameState.Playing) return;

        if (InputManager.Instance.InteractPressed)
        {
            ushort selected = Inventory.Instance.slots[Inventory.Instance.selectedSlot].id;
            int count = Inventory.Instance.slots[Inventory.Instance.selectedSlot].count;

            if (selected == requiredItemId && count >= requiredCount)
            {
                int seed = Random.Range(1, int.MaxValue);
                KnappingSession.Instance.StartSession(seed);
            }
        }
    }
}