using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Footsteps")]
    public SfxPreset footstepGrass;
    public SfxPreset footstepStone;
    public SfxPreset footstepDirt;
    public SfxPreset footstepWood;
    public SfxPreset footstepSand;
    public SfxPreset footstepMetal;
    public SfxPreset footstepWater;
    public SfxPreset footstepSnow;

    [Header("Blocks")]
    public SfxPreset blockBreak;
    public SfxPreset blockPlace;
    public SfxPreset blockDig;

    [Header("Items")]
    public SfxPreset itemPickup;
    public SfxPreset itemDrop;
    public SfxPreset itemEquip;

    [Header("Knapping")]
    public SfxPreset knappingHit;
    public SfxPreset knappingSuccess;
    public SfxPreset knappingFail;

    [Header("Materials")]
    public MaterialProfile matStone;
    public MaterialProfile matDirt;
    public MaterialProfile matWood;
    public MaterialProfile matSand;
    public MaterialProfile matMetal;
    public MaterialProfile matGlass;
    public MaterialProfile matClay;

    [Header("Pool")]
    public int poolSize = 24;
    public float minDistance = 1.5f;
    public float maxDistance = 24f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    [Header("Cache")]
    public int maxContextClipCache = 96;

    readonly List<AudioSource> pool = new List<AudioSource>();
    readonly Dictionary<SfxPreset, AudioClip> baseClipCache = new Dictionary<SfxPreset, AudioClip>();
    readonly Dictionary<int, AudioClip> contextClipCache = new Dictionary<int, AudioClip>();

    int poolIndex;
    AudioSource uiSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject($"SfxSource_{i}");
            obj.transform.SetParent(transform);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.rolloffMode = rolloffMode;
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            src.dopplerLevel = 0f;
            pool.Add(src);
        }

        GameObject uiObj = new GameObject("SfxSource_UI");
        uiObj.transform.SetParent(transform);
        uiSource = uiObj.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.spatialBlend = 0f;
        uiSource.dopplerLevel = 0f;
    }

    public void Play3D(SfxPreset preset, Vector3 position)
    {
        if (preset == null) return;

        AudioClip clip = GetBaseClip(preset);
        if (clip == null) return;

        AudioSource src = GetNextSource();
        src.transform.position = position;
        src.clip = clip;
        src.volume = GetRandomizedVolume(preset.volume);
        src.pitch = 1f;
        src.Play();
    }

    public void Play3D(SfxPreset preset, Vector3 position, SfxContext context)
    {
        if (preset == null) return;

        AudioClip clip = GetContextClip(preset, context);
        if (clip == null) return;

        AudioSource src = GetNextSource();
        src.transform.position = position;
        src.clip = clip;
        src.volume = GetRandomizedVolume(preset.volume);
        src.pitch = 1f;
        src.Play();
    }

    public void PlayUI(SfxPreset preset)
    {
        if (preset == null) return;

        AudioClip clip = GetBaseClip(preset);
        if (clip == null) return;

        uiSource.pitch = 1f;
        uiSource.PlayOneShot(clip, GetRandomizedVolume(preset.volume));
    }

    public void PlayUI(SfxPreset preset, SfxContext context)
    {
        if (preset == null) return;

        AudioClip clip = GetContextClip(preset, context);
        if (clip == null) return;

        uiSource.pitch = 1f;
        uiSource.PlayOneShot(clip, GetRandomizedVolume(preset.volume));
    }

    public SfxPreset GetFootstepForBlock(ushort blockId)
    {
        switch (blockId)
        {
            case 1: return footstepStone;
            case 2: return footstepGrass;
            case 3: return footstepDirt;
            case 4: return footstepWood;
            case 5: return footstepSand;
            case 6: return footstepMetal;
            case 7: return footstepWater;
            case 8: return footstepSnow;
            default: return footstepDirt;
        }
    }

    public MaterialProfile GetMaterialForBlock(ushort blockId)
    {
        switch (blockId)
        {
            case 1: return matStone;
            case 3: return matDirt;
            case 4: return matWood;
            case 5: return matSand;
            case 6: return matMetal;
            default: return matStone;
        }
    }

    AudioClip GetBaseClip(SfxPreset preset)
    {
        if (preset == null) return null;

        if (baseClipCache.TryGetValue(preset, out AudioClip cached) && cached != null)
            return cached;

        AudioClip clip = preset.GetClip();
        baseClipCache[preset] = clip;
        return clip;
    }

    AudioClip GetContextClip(SfxPreset preset, SfxContext context)
    {
        if (preset == null) return null;

        int key = MakeContextKey(preset, context);
        if (contextClipCache.TryGetValue(key, out AudioClip cached) && cached != null)
            return cached;

        if (contextClipCache.Count >= maxContextClipCache)
            ClearContextClipCache();

        int variantIndex = Random.Range(0, Mathf.Max(1, preset.variantCount));
        SfxParams p = preset.BuildParamsWithContext(variantIndex, context);
        AudioClip clip = ChipTuneSynth.Generate(p);

        contextClipCache[key] = clip;
        return clip;
    }

    int MakeContextKey(SfxPreset preset, SfxContext context)
    {
        int v = Mathf.RoundToInt(Mathf.Clamp(context.velocity, 0f, 4f) * 100f);
        int m = Mathf.RoundToInt(Mathf.Clamp(context.mass, 0f, 10f) * 100f);
        int w = Mathf.RoundToInt(Mathf.Clamp01(context.wetness) * 100f);

        int surfaceId = context.surfaceMaterial != null ? context.surfaceMaterial.GetInstanceID() : 0;
        int toolId = context.toolMaterial != null ? context.toolMaterial.GetInstanceID() : 0;

        unchecked
        {
            int hash = 17;
            hash = hash * 31 + preset.GetInstanceID();
            hash = hash * 31 + v;
            hash = hash * 31 + m;
            hash = hash * 31 + w;
            hash = hash * 31 + (int)context.room;
            hash = hash * 31 + (int)context.era;
            hash = hash * 31 + surfaceId;
            hash = hash * 31 + toolId;
            return hash;
        }
    }

    float GetRandomizedVolume(float baseVolume)
    {
        return baseVolume * Random.Range(0.96f, 1.04f);
    }

    AudioSource GetNextSource()
    {
        AudioSource src = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;
        return src;
    }

    void ClearContextClipCache()
    {
        foreach (var kv in contextClipCache)
        {
            if (kv.Value == null) continue;

            if (Application.isPlaying)
                Destroy(kv.Value);
            else
                DestroyImmediate(kv.Value);
        }

        contextClipCache.Clear();
    }

    void OnDestroy()
    {
        foreach (var kv in baseClipCache)
        {
            if (kv.Value == null) continue;

            if (Application.isPlaying)
                Destroy(kv.Value);
            else
                DestroyImmediate(kv.Value);
        }

        baseClipCache.Clear();
        ClearContextClipCache();

        if (Instance == this)
            Instance = null;
    }
}