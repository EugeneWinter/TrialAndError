using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ushort itemId;
    public int count = 1;
    public float bobSpeed = 1.5f;
    public float bobHeight = 0.04f;
    public float rotateSpeed = 30f;

    private float bobTime;
    private Vector3 basePosition;
    private bool initialized = false;
    private Transform cachedPlayer;

    private const float INTERACT_DISTANCE = 2.5f;

    public void Setup(ushort id, int amount, Vector3 position)
    {
        itemId = id;
        count = amount;
        transform.position = position;
        basePosition = position;
        bobTime = Random.Range(0f, Mathf.PI * 2f);
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        bobTime += Time.deltaTime * bobSpeed;
        float yOffset = Mathf.Sin(bobTime) * bobHeight;
        transform.position = basePosition + new Vector3(0, yOffset + 0.15f, 0);
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        if (GameManager.Instance.state != GameState.Playing) return;

        if (cachedPlayer == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) cachedPlayer = player.transform;
            else return;
        }

        float dist = Vector3.Distance(transform.position, cachedPlayer.position);
        if (dist > INTERACT_DISTANCE) return;

        if (InputManager.Instance.InteractPressed)
        {
            if (Inventory.Instance.AddItem(itemId, count))
            {
                if (AudioManager.Instance != null)
                {
                    AudioClip clip = SoundBanks.ItemPickup.GetRandom();
                    if (clip != null)
                        AudioManager.Instance.PlaySampleUI(clip, 0.6f, Random.Range(0.95f, 1.05f));
                }
                Destroy(gameObject);
            }
        }
    }
}