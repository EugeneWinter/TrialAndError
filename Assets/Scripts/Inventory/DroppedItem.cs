using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class DroppedItem : MonoBehaviour
{
    public ushort blockId;
    public int count = 1;

    private static List<DroppedItem> allDroppedItems = new List<DroppedItem>();

    private float bobTime;
    private float pickupDelay = 0.5f;
    private GameObject visualModel;

    private Vector3 velocity;
    private bool onGround = false;
    private float gravity = 15f;
    private float3 size = new float3(0.3f, 0.3f, 0.3f);

    private const float MERGE_DISTANCE = 0.6f;
    private const float PICKUP_DISTANCE = 2.0f;

    private float currentLift = 0f;
    private Transform cachedPlayerTransform;

    void Start()
    {
        bobTime = UnityEngine.Random.Range(0f, Mathf.PI * 2);
        SpawnVisual();
        velocity = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(2f, 4f), UnityEngine.Random.Range(-1f, 1f));
        allDroppedItems.Add(this);
    }

    void OnDestroy()
    {
        allDroppedItems.Remove(this);
    }

    void SpawnVisual()
    {
        if (TrySpawnItemModel()) return;
        if (TrySpawnBlockModel()) return;
        SpawnFallbackCube();
    }

    bool TrySpawnItemModel()
    {
        ItemDatabase db = null;

        if (Inventory.Instance != null && Inventory.Instance.itemDatabase != null)
            db = Inventory.Instance.itemDatabase;

        if (db == null) return false;

        ItemSO item = db.GetItem(blockId);
        if (item == null) return false;

        GameObject model = item.groundModel != null ? item.groundModel : item.heldModel;
        if (model == null) return false;

        visualModel = Instantiate(model, transform);

        visualModel.transform.localPosition = Vector3.zero;
        visualModel.transform.localRotation = Quaternion.identity;
        visualModel.transform.localScale = Vector3.one * 0.5f;

        foreach (Transform t in visualModel.GetComponentsInChildren<Transform>())
        {
            if (t == visualModel.transform) continue;
            t.localRotation = Quaternion.identity;
        }

        return true;
    }

    bool TrySpawnBlockModel()
    {
        if (WorldManager.Instance == null) return false;

        BlockSO block = WorldManager.Instance.blockDatabase.GetBlockSO(blockId);
        if (block == null) return false;

        if (block.isCustomModel && block.customModelPrefab != null)
        {
            visualModel = Instantiate(block.customModelPrefab, transform);
            visualModel.transform.localPosition = Vector3.zero;
            visualModel.transform.localRotation = Quaternion.identity;
            visualModel.transform.localScale = Vector3.one * 0.4f;
            return true;
        }

        Texture2DArray texArray = WorldManager.Instance.blockDatabase.textureArray;
        if (texArray == null) return false;

        visualModel = BlockPreviewFactory.CreateMiniBlock(block, texArray);
        if (visualModel == null) return false;

        visualModel.transform.SetParent(transform);
        visualModel.transform.localPosition = Vector3.zero;
        visualModel.transform.localRotation = Quaternion.identity;
        visualModel.transform.localScale = Vector3.one * 0.3f;
        return true;
    }

    void SpawnFallbackCube()
    {
        visualModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(visualModel.GetComponent<Collider>());
        visualModel.transform.SetParent(transform);
        visualModel.transform.localPosition = Vector3.zero;
        visualModel.transform.localRotation = Quaternion.identity;
        visualModel.transform.localScale = Vector3.one * 0.25f;

        Renderer r = visualModel.GetComponent<Renderer>();
        if (r != null)
            r.material.color = new Color(0.5f, 0.5f, 0.5f);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        UpdatePhysics(dt);
        UpdateVisual(dt);

        pickupDelay -= dt;
        if (pickupDelay > 0) return;

        TryMergeWithNearby();
        TryPickup();
    }

    void TryPickup()
    {
        if (cachedPlayerTransform == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
                cachedPlayerTransform = GameManager.Instance.player.transform;
            else
                return;
        }

        float dist = Vector3.Distance(transform.position, cachedPlayerTransform.position);

        if (dist < PICKUP_DISTANCE)
        {
            if (Inventory.Instance.AddItem(blockId, count))
            {
                if (AudioManager.Instance != null)
                {
                    AudioClip clip = SoundBanks.ItemPickup.GetRandom();
                    if (clip != null)
                        AudioManager.Instance.PlaySampleUI(clip, 0.6f, UnityEngine.Random.Range(0.95f, 1.05f));
                }
                Destroy(gameObject);
            }
        }
    }

    void UpdatePhysics(float dt)
    {
        velocity.y -= gravity * dt;

        Vector3 pos = transform.position;

        pos.x += velocity.x * dt;
        if (CheckCollision(pos)) { pos.x -= velocity.x * dt; velocity.x *= -0.3f; }

        pos.z += velocity.z * dt;
        if (CheckCollision(pos)) { pos.z -= velocity.z * dt; velocity.z *= -0.3f; }

        onGround = false;
        pos.y += velocity.y * dt;
        if (CheckCollision(pos))
        {
            pos.y -= velocity.y * dt;
            if (velocity.y < 0) onGround = true;
            velocity.y = 0;
        }

        if (onGround)
        {
            velocity.x *= 0.85f;
            velocity.z *= 0.85f;
        }

        transform.position = pos;
    }

    void UpdateVisual(float dt)
    {
        if (visualModel == null) return;

        float modelHalfHeight = visualModel.transform.localScale.y * 0.5f;
        float targetLift = onGround ? 0.15f : 0f;

        currentLift = Mathf.Lerp(currentLift, targetLift, 8f * dt);

        if (onGround)
        {
            bobTime += dt * 1.5f;
            float bobY = (Mathf.Sin(bobTime) + 1f) * 0.5f * 0.04f;
            visualModel.transform.localPosition = new Vector3(0, modelHalfHeight + currentLift + bobY, 0);

            visualModel.transform.localRotation = Quaternion.Euler(0, bobTime * 40f, 0);
        }
        else
        {
            visualModel.transform.localPosition = new Vector3(0, modelHalfHeight + currentLift, 0);
            visualModel.transform.Rotate(new Vector3(45f, 90f, 30f) * dt, Space.Self);
        }
    }

    void TryMergeWithNearby()
    {
        for (int i = allDroppedItems.Count - 1; i >= 0; i--)
        {
            if (i >= allDroppedItems.Count) continue;

            DroppedItem other = allDroppedItems[i];
            if (other == null || other == this) continue;
            if (other.blockId != blockId) continue;
            if (other.pickupDelay > 0) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist > MERGE_DISTANCE) continue;

            if (other.GetInstanceID() < GetInstanceID())
            {
                other.count += count;
                Destroy(gameObject);
                return;
            }
        }
    }

    bool CheckCollision(Vector3 pos)
    {
        AABB box = AABB.FromPositionSize(pos, size);
        int minX = (int)math.floor(box.min.x);
        int maxX = (int)math.floor(box.max.x);
        int minY = (int)math.floor(box.min.y);
        int maxY = (int)math.floor(box.max.y);
        int minZ = (int)math.floor(box.min.z);
        int maxZ = (int)math.floor(box.max.z);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    AABB blockBox = new AABB(new float3(x, y, z), new float3(x + 1, y + 1, z + 1));
                    if (WorldManager.Instance.IsBlockSolid(x, y, z) && box.Intersects(blockBox))
                        return true;
                }
        return false;
    }
}