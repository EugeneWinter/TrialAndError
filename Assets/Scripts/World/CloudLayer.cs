using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CloudLayer : MonoBehaviour
{
    public static CloudLayer Instance;

    [Header("Settings")]
    public GameObject cloudChunkPrefab;
    public Transform followTarget;
    public int renderDistance = 5;

    [Header("Cloud Base Params")]
    public int cloudHeight = 100;
    public int cloudThickness = 8;

    [Header("Current Weather (Read Only)")]
    public float currentThreshold = 0.55f;
    public float currentScale = 0.03f;
    public float currentWindX = 0.5f;
    public float currentWindZ = 0.3f;

    [Header("Weather Change")]
    public float weatherChangeIntervalHours = 3f;
    public float weatherTransitionMinutes = 30f;

    [Header("Weather Regeneration")]
    public float weatherRegenIntervalSeconds = 2f;
    private float weatherRegenTimer = 0f;
    private int regenerationIndex = 0;

    private float targetThreshold = 0.55f;
    private float targetScale = 0.03f;
    private float targetWindX = 0.5f;
    private float targetWindZ = 0.3f;

    private Dictionary<int2, CloudChunk> chunks = new Dictionary<int2, CloudChunk>();
    private float windOffsetX = 0f;
    private float windOffsetZ = 0f;
    private int lastPlayerChunkX, lastPlayerChunkZ;

    private float nextWeatherChangeHour = 0f;

    void Awake() => Instance = this;

    void Start()
    {
        RollNewWeather();
        currentThreshold = targetThreshold;
        currentScale = targetScale;
        currentWindX = targetWindX;
        currentWindZ = targetWindZ;

        int playerChunkX = 0;
        int playerChunkZ = 0;
        if (followTarget != null)
        {
            playerChunkX = Mathf.FloorToInt(followTarget.position.x / 32f);
            playerChunkZ = Mathf.FloorToInt(followTarget.position.z / 32f);
        }
        UpdateChunks(playerChunkX, playerChunkZ);
    }

    void Update()
    {
        if (followTarget == null) return;

        windOffsetX += currentWindX * Time.deltaTime;
        windOffsetZ += currentWindZ * Time.deltaTime;

        foreach (var chunk in chunks.Values)
        {
            chunk.UpdateWindOffset(windOffsetX, windOffsetZ);
        }

        if (TimeManager.Instance != null)
        {
            float totalHours = TimeManager.Instance.currentDay * 24f + TimeManager.Instance.currentTimeMinutes / 60f;
            if (totalHours >= nextWeatherChangeHour)
            {
                RollNewWeather();
                nextWeatherChangeHour = totalHours + weatherChangeIntervalHours;
            }
        }

        float lerpSpeed = Time.deltaTime / (weatherTransitionMinutes * 60f);
        float prevThreshold = currentThreshold;
        currentThreshold = Mathf.MoveTowards(currentThreshold, targetThreshold, lerpSpeed);
        currentScale = Mathf.MoveTowards(currentScale, targetScale, lerpSpeed * 0.1f);
        currentWindX = Mathf.MoveTowards(currentWindX, targetWindX, lerpSpeed * 5f);
        currentWindZ = Mathf.MoveTowards(currentWindZ, targetWindZ, lerpSpeed * 5f);

        weatherRegenTimer += Time.deltaTime;
        if (weatherRegenTimer >= weatherRegenIntervalSeconds)
        {
            RegenerateNextChunk();
            weatherRegenTimer = 0f;
        }

        int playerChunkX = Mathf.FloorToInt(followTarget.position.x / 32f);
        int playerChunkZ = Mathf.FloorToInt(followTarget.position.z / 32f);

        if (playerChunkX != lastPlayerChunkX || playerChunkZ != lastPlayerChunkZ)
        {
            UpdateChunks(playerChunkX, playerChunkZ);
            lastPlayerChunkX = playerChunkX;
            lastPlayerChunkZ = playerChunkZ;
        }
    }

    void RegenerateNextChunk()
    {
        if (chunks.Count == 0) return;

        var chunksList = new List<CloudChunk>(chunks.Values);
        regenerationIndex = (regenerationIndex + 1) % chunksList.Count;
        chunksList[regenerationIndex].Regenerate();
    }

    void RollNewWeather()
    {
        float roll = Random.value;

        if (roll < 0.35f)
            targetThreshold = Random.Range(0.65f, 0.75f);
        else if (roll < 0.75f)
            targetThreshold = Random.Range(0.50f, 0.60f);
        else if (roll < 0.95f)
            targetThreshold = Random.Range(0.35f, 0.45f);
        else
            targetThreshold = Random.Range(0.20f, 0.30f);

        targetScale = Random.Range(0.015f, 0.05f);

        float windAngle = Random.Range(0f, Mathf.PI * 2f);
        float windStrength = Random.Range(0.2f, 1.5f);
        targetWindX = Mathf.Cos(windAngle) * windStrength;
        targetWindZ = Mathf.Sin(windAngle) * windStrength;

        Debug.Log($"New weather: threshold={targetThreshold:F2}, scale={targetScale:F3}, wind=({targetWindX:F2}, {targetWindZ:F2})");
    }

    void UpdateChunks(int centerX, int centerZ)
    {
        HashSet<int2> needed = new HashSet<int2>();

        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                int2 coord = new int2(centerX + x, centerZ + z);
                needed.Add(coord);

                if (!chunks.ContainsKey(coord))
                {
                    CreateChunk(coord);
                }
            }

        List<int2> toRemove = new List<int2>();
        foreach (var kvp in chunks)
        {
            if (!needed.Contains(kvp.Key)) toRemove.Add(kvp.Key);
        }
        foreach (var coord in toRemove)
        {
            Destroy(chunks[coord].gameObject);
            chunks.Remove(coord);
        }
    }

    void CreateChunk(int2 coord)
    {
        GameObject obj = Instantiate(cloudChunkPrefab, new Vector3(coord.x * 32, cloudHeight, coord.y * 32), Quaternion.identity, transform);
        obj.name = $"CloudChunk_{coord.x}_{coord.y}";
        CloudChunk cc = obj.GetComponent<CloudChunk>();
        cc.Initialize(coord, this);
        cc.UpdateWindOffset(windOffsetX, windOffsetZ);
        chunks[coord] = cc;
    }

    public bool IsCloudAt(int worldX, int worldY, int worldZ, float cachedWindX, float cachedWindZ)
    {
        if (worldY < cloudHeight || worldY >= cloudHeight + cloudThickness) return false;

        float sampleX = (worldX + cachedWindX) * currentScale;
        float sampleZ = (worldZ + cachedWindZ) * currentScale;

        float baseNoise = Mathf.PerlinNoise(sampleX + 5000, sampleZ + 5000);

        float heightFactor = (float)(worldY - cloudHeight) / cloudThickness;
        float verticalCurve = 1f - Mathf.Abs(heightFactor - 0.5f) * 2f;

        float detailNoise = Mathf.PerlinNoise(sampleX * 3f + worldY * 0.3f + 9000, sampleZ * 3f + worldY * 0.3f + 9000);
        detailNoise = detailNoise * 0.3f;

        float density = baseNoise * verticalCurve + detailNoise;

        return density > currentThreshold;
    }
}