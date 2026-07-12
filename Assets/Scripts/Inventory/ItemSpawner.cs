using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance;

    public GameObject droppedItemPrefab;

    void Awake() => Instance = this;

    public void SpawnItem(Vector3 position, ushort blockId, int count = 1)
    {
        if (blockId == 0) return;

        GameObject obj = Instantiate(droppedItemPrefab, position + Vector3.up * 0.5f, Quaternion.identity);
        DroppedItem item = obj.GetComponent<DroppedItem>();
        item.blockId = blockId;
        item.count = count;
    }
}