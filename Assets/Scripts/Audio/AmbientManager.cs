using UnityEngine;
using System.Collections.Generic;

public class AmbientManager : MonoBehaviour
{
    public static AmbientManager Instance;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterAmbientVolume = 0.15f;
    [Range(0f, 1f)] public float windDayVolume = 0.35f;
    [Range(0f, 1f)] public float windNightVolume = 0.5f;
    [Range(0f, 1f)] public float birdsMorningVolume = 0.5f;
    [Range(0f, 1f)] public float birdsDayVolume = 0.15f;
    [Range(0f, 1f)] public float nightVolume = 0.45f;
    [Range(0f, 1f)] public float caveVolume = 0.6f;

    [Header("Cave Detection")]
    public bool useBlocksAboveDetection = true;
    public int blocksAboveThreshold = 5;
    public int caveCheckHeight = 20;
    public float caveDepthThreshold = -10f;

    [Header("Time Thresholds")]
    public float dayStartHour = 6f;
    public float dayEndHour = 20f;
    public float birdsMorningStart = 5f;
    public float birdsMorningEnd = 11f;
    public float birdsDayEnd = 18f;

    [Header("Fade Speed")]
    public float fadeSpeed = 0.3f;

    private class AmbientLoop
    {
        public AudioSource source;
        public float targetVolume;
        public string debugName;
    }

    private AmbientLoop windLoop;
    private AmbientLoop birdsLoop;
    private AmbientLoop nightLoop;
    private AmbientLoop caveLoop;

    private Transform playerTransform;
    private float caveCheckTimer = 0f;
    private bool cachedIsInCave = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        windLoop = CreateLoop(SoundBanks.AmbientWind, "Wind");
        birdsLoop = CreateLoop(SoundBanks.AmbientBirds, "Birds");
        nightLoop = CreateLoop(SoundBanks.AmbientNight, "Night");
        caveLoop = CreateLoop(SoundBanks.AmbientCave, "Cave");
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        caveCheckTimer -= Time.deltaTime;
        if (caveCheckTimer <= 0f)
        {
            cachedIsInCave = CheckIfInCave();
            caveCheckTimer = 0.5f;
        }

        UpdateTargetVolumes();

        FadeLoop(windLoop);
        FadeLoop(birdsLoop);
        FadeLoop(nightLoop);
        FadeLoop(caveLoop);
    }

    bool CheckIfInCave()
    {
        if (!useBlocksAboveDetection)
            return playerTransform.position.y < caveDepthThreshold;

        if (WorldManager.Instance == null) return false;

        int px = Mathf.FloorToInt(playerTransform.position.x);
        int py = Mathf.FloorToInt(playerTransform.position.y);
        int pz = Mathf.FloorToInt(playerTransform.position.z);

        int solidBlocksAbove = 0;

        for (int i = 2; i < caveCheckHeight; i++)
        {
            ushort blockId = WorldManager.Instance.GetBlock(px, py + i, pz);
            if (IsCaveBlock(blockId))
            {
                solidBlocksAbove++;
                if (solidBlocksAbove >= blocksAboveThreshold)
                    return true;
            }
        }

        return false;
    }

    bool IsCaveBlock(ushort blockId)
    {
        return blockId switch
        {
            1 => true,
            2 => true,
            3 => true,
            _ => false
        };
    }

    void UpdateTargetVolumes()
    {
        float hour = TimeManager.Instance != null ? TimeManager.Instance.currentTimeMinutes / 60f : 12f;
        bool isInCave = cachedIsInCave;
        bool isDay = hour >= dayStartHour && hour < dayEndHour;

        float birdsTarget = 0f;
        if (hour >= birdsMorningStart && hour < birdsMorningEnd)
            birdsTarget = birdsMorningVolume;
        else if (hour >= birdsMorningEnd && hour < birdsDayEnd)
            birdsTarget = birdsDayVolume;

        if (isInCave)
        {
            windLoop.targetVolume = windDayVolume * 0.1f;
            birdsLoop.targetVolume = 0f;
            nightLoop.targetVolume = 0f;
            caveLoop.targetVolume = caveVolume;
        }
        else
        {
            caveLoop.targetVolume = 0f;

            if (isDay)
            {
                windLoop.targetVolume = windDayVolume;
                nightLoop.targetVolume = 0f;
                birdsLoop.targetVolume = birdsTarget;
            }
            else
            {
                windLoop.targetVolume = windNightVolume;
                nightLoop.targetVolume = nightVolume;
                birdsLoop.targetVolume = 0f;
            }
        }
    }

    AmbientLoop CreateLoop(SampleBank bank, string name)
    {
        if (bank == null || bank.IsEmpty)
        {
            Debug.LogWarning($"AmbientLoop {name}: no clips found");
            return new AmbientLoop { debugName = name };
        }

        GameObject obj = new GameObject($"Ambient_{name}");
        obj.transform.SetParent(transform);

        AudioSource src = obj.AddComponent<AudioSource>();
        src.clip = bank.GetRandom();
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = 0f;
        src.playOnAwake = false;
        src.Play();

        return new AmbientLoop
        {
            source = src,
            targetVolume = 0f,
            debugName = name
        };
    }

    void FadeLoop(AmbientLoop loop)
    {
        if (loop == null || loop.source == null) return;

        float target = loop.targetVolume * masterAmbientVolume;
        loop.source.volume = Mathf.MoveTowards(loop.source.volume, target, fadeSpeed * Time.deltaTime);
    }
}