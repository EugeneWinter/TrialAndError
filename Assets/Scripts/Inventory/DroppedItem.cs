using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public ushort blockId;
    public int count = 1;

    private float bobTime;
    private Vector3 startPos;
    private float pickupDelay = 0.5f;
    private GameObject visualModel;

    void Start()
    {
        startPos = transform.position;
        bobTime = Random.Range(0f, Mathf.PI * 2);
        SpawnVisual();
    }

    void SpawnVisual()
    {
        ItemSO item = Inventory.Instance.itemDatabase.GetItem(blockId);
        if (item != null && item.heldModel != null)
        {
            visualModel = Instantiate(item.heldModel, transform);
            visualModel.transform.localPosition = Vector3.zero;
            visualModel.transform.localScale = Vector3.one * 0.5f;
        }
    }

    void Update()
    {
        bobTime += Time.deltaTime * 2f;
        transform.position = startPos + Vector3.up * Mathf.Sin(bobTime) * 0.1f;
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);

        pickupDelay -= Time.deltaTime;
        if (pickupDelay > 0) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist < 2.0f)
        {
            if (Inventory.Instance.AddItem(blockId, count))
            {
                try
                {
                    if (AudioManager.Instance != null && AudioManager.Instance.itemPickup != null)
                        AudioManager.Instance.PlayUI(AudioManager.Instance.itemPickup);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to play pickup sound: {e.Message}");
                }

                Destroy(gameObject);
            }
        }
    }
}