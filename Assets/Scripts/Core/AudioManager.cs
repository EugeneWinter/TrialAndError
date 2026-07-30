using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour, IGameSystem
{
    public static AudioManager Instance;

    [Range(0f, 0.3f)] public float footstepPitchVar = 0.15f;
    [Range(0f, 0.3f)] public float footstepVolumeVar = 0.15f;
    [Range(0f, 1f)] public float footstepBaseVolume = 0.7f;
    [Range(0f, 1f)] public float sneakVolumeScale = 0.5f;
    [Range(0f, 0.3f)] public float sneakPitchVarBoost = 0.05f;
    public bool processFootstepSamples = true;

    [Range(0.3f, 1.0f)] public float blockBreakPitchMin = 0.65f;
    [Range(0.3f, 1.0f)] public float blockBreakPitchMax = 0.85f;
    [Range(0f, 1f)] public float blockBreakVolume = 0.85f;

    [Range(0.5f, 1.2f)] public float digPitchMin = 0.75f;
    [Range(0.5f, 1.2f)] public float digPitchMax = 0.95f;
    [Range(0f, 1f)] public float digVolume = 0.55f;

    public int poolSize = 32;

    private readonly List<AudioSource> pool = new List<AudioSource>();
    private int poolIndex = 0;
    private AudioSource uiSource;
    private FootstepBank footstepBank;

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject($"SfxSource_{i}");
            obj.transform.SetParent(transform);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 1f;
            src.maxDistance = 40f;
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

    public void InitializeSystem()
    {
        footstepBank = new FootstepBank(processFootstepSamples);
    }

    public void PlaySample3D(AudioClip clip, Vector3 position, float volume, float pitch = 1f, float minDist = 1f, float maxDist = 20f, float spatialBlend = 1f)
    {
        if (clip == null) return;

        AudioSource src = GetNextSource();
        src.transform.position = position;
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.minDistance = minDist;
        src.maxDistance = maxDist;
        src.spatialBlend = spatialBlend;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.Play();
    }

    public void PlaySampleUI(AudioClip clip, float volume, float pitch = 1f)
    {
        if (clip == null) return;
        uiSource.pitch = pitch;
        uiSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void PlayFootstep(FootstepAction action, ushort blockId, Vector3 position, float velocity = 1f)
    {
        PlayFootstepBlended(action, blockId, 0, 0f, position, velocity);
    }

    public void PlayFootstepBlended(FootstepAction action, ushort primaryBlockId, ushort secondaryBlockId, float blendFactor, Vector3 position, float velocity = 1f)
    {
        if (footstepBank == null) return;

        FootstepMaterial primaryMat = GetFootstepMaterialForBlock(primaryBlockId);
        AudioClip primaryClip = footstepBank.GetRandom(action, primaryMat);
        if (primaryClip == null) return;

        float pitchVar = footstepPitchVar;
        if (action == FootstepAction.Sneak) pitchVar += sneakPitchVarBoost;

        float pitch = 1f + Random.Range(-pitchVar, pitchVar);
        float volMul = 1f + Random.Range(-footstepVolumeVar, footstepVolumeVar);
        float vel = Mathf.Clamp(velocity, 0.4f, 1.6f);

        float baseVol = footstepBaseVolume;
        if (action == FootstepAction.Sneak) baseVol *= sneakVolumeScale;

        float volume = baseVol * volMul * Mathf.Lerp(0.7f, 1.1f, vel / 1.6f);
        Vector3 footPos = position + Vector3.down * 0.8f;

        float primaryVolume = volume * (1f - blendFactor * 0.5f);
        PlayFootstepSampleInternal(primaryClip, footPos, primaryVolume, pitch);

        if (secondaryBlockId != 0 && blendFactor > 0.15f)
        {
            FootstepMaterial secondaryMat = GetFootstepMaterialForBlock(secondaryBlockId);
            if (secondaryMat != primaryMat)
            {
                AudioClip secondaryClip = footstepBank.GetRandom(action, secondaryMat);
                if (secondaryClip != null)
                {
                    float secondaryPitch = 1f + Random.Range(-pitchVar, pitchVar);
                    float secondaryVolume = volume * blendFactor * 0.6f;
                    PlayFootstepSampleInternal(secondaryClip, footPos, secondaryVolume, secondaryPitch);
                }
            }
        }
    }

    private void PlayFootstepSampleInternal(AudioClip clip, Vector3 position, float volume, float pitch)
    {
        AudioSource src = GetNextSource();
        src.transform.position = position;
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.minDistance = 0.3f;
        src.maxDistance = 15f;
        src.spatialBlend = 0.6f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.Play();
    }

    public void PlayBlockBreak(ushort blockId, Vector3 position)
    {
        if (footstepBank == null) return;

        FootstepMaterial mat = GetFootstepMaterialForBlock(blockId);
        AudioClip mainClip = footstepBank.GetRandom(FootstepAction.Run, mat);
        if (mainClip == null) return;

        float mainPitch = Random.Range(blockBreakPitchMin, blockBreakPitchMax);
        float mainVolume = blockBreakVolume * Random.Range(0.9f, 1.05f);

        AudioSource mainSrc = GetNextSource();
        mainSrc.transform.position = position;
        mainSrc.clip = mainClip;
        mainSrc.volume = Mathf.Clamp01(mainVolume);
        mainSrc.pitch = mainPitch;
        mainSrc.minDistance = 1f;
        mainSrc.maxDistance = 25f;
        mainSrc.spatialBlend = 1f;
        mainSrc.rolloffMode = AudioRolloffMode.Logarithmic;
        mainSrc.Play();

        AudioClip subClip = footstepBank.GetRandom(FootstepAction.Run, mat);
        if (subClip != null)
        {
            float subPitch = mainPitch * Random.Range(0.75f, 0.85f);
            float subVolume = mainVolume * 0.5f;

            AudioSource subSrc = GetNextSource();
            subSrc.transform.position = position;
            subSrc.clip = subClip;
            subSrc.volume = Mathf.Clamp01(subVolume);
            subSrc.pitch = subPitch;
            subSrc.minDistance = 1f;
            subSrc.maxDistance = 25f;
            subSrc.spatialBlend = 1f;
            subSrc.rolloffMode = AudioRolloffMode.Logarithmic;
            subSrc.Play();
        }
    }

    public void PlayDigHit(ushort blockId, Vector3 position, float progress)
    {
        if (footstepBank == null) return;

        FootstepMaterial mat = GetFootstepMaterialForBlock(blockId);
        AudioClip clip = footstepBank.GetRandom(FootstepAction.Run, mat);
        if (clip == null) return;

        float pitchBoost = Mathf.Lerp(0f, 0.1f, progress);
        float pitch = Random.Range(digPitchMin, digPitchMax) + pitchBoost;
        float volume = digVolume * Random.Range(0.9f, 1.1f);

        AudioSource src = GetNextSource();
        src.transform.position = position;
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.minDistance = 1f;
        src.maxDistance = 20f;
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.Play();
    }

    private FootstepMaterial GetFootstepMaterialForBlock(ushort id)
    {
        return id switch
        {
            BlockIDs.Stone => FootstepMaterial.Stone,
            BlockIDs.Grass => FootstepMaterial.Grass,
            BlockIDs.Dirt => FootstepMaterial.Dirt,
            BlockIDs.Log => FootstepMaterial.Wood,
            BlockIDs.Leaf => FootstepMaterial.Dirt,
            BlockIDs.Water => FootstepMaterial.Stone,
            BlockIDs.Sand => FootstepMaterial.Dirt,
            BlockIDs.Deepstone => FootstepMaterial.Stone,
            _ => FootstepMaterial.Dirt
        };
    }

    private AudioSource GetNextSource()
    {
        AudioSource src = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;
        return src;
    }
}