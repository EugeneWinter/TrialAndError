using UnityEngine;

public class PlayerVoice : MonoBehaviour
{
    public static PlayerVoice Instance;

    [Header("Voice Settings")]
    [Range(0f, 1f)] public float voiceVolume = 0.7f;
    [Range(0f, 0.3f)] public float pitchVariation = 0.1f;

    [Header("Jump Voice")]
    [Range(0f, 1f)] public float jumpVoiceChance = 0.4f;

    [Header("Landing Voice")]
    public float gaspFallThreshold = 15f;
    [Range(0f, 1f)] public float landingHurtChance = 0.6f;

    [Header("Breathing")]
    public float sprintTimeBeforeBreathing = 3f;
    public float breathingCooldown = 8f;
    public float breathingRecoveryRate = 0.3f;

    private float sprintTimer = 0f;
    private float breathingTimer = 0f;
    private float lastJumpVoiceTime = -10f;
    private float lastHurtVoiceTime = -10f;
    private float jumpVoiceCooldown = 0.3f;
    private float hurtVoiceCooldown = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (breathingTimer > 0f) breathingTimer -= Time.deltaTime;
    }

    public void OnSprint(float deltaTime, bool actuallyMoving)
    {
/*        if (actuallyMoving)
            sprintTimer += deltaTime;
        else
            sprintTimer = Mathf.Max(0f, sprintTimer - deltaTime * breathingRecoveryRate);

        if (sprintTimer >= sprintTimeBeforeBreathing && breathingTimer <= 0f)
        {
            PlayBreathing();
            breathingTimer = breathingCooldown;
            sprintTimer = sprintTimeBeforeBreathing * 0.5f;
        }*/
    }

    public void OnStopSprint(float deltaTime)
    {
        sprintTimer = Mathf.Max(0f, sprintTimer - deltaTime * breathingRecoveryRate);
    }

    public void OnJump()
    {
        if (Time.time - lastJumpVoiceTime < jumpVoiceCooldown) return;
        if (Random.value > jumpVoiceChance) return;

        PlayVoice(SoundBanks.VoiceJump, voiceVolume * 0.8f);
        lastJumpVoiceTime = Time.time;
    }

    public void OnLanding(float fallVelocity)
    {
        float absVel = Mathf.Abs(fallVelocity);

        if (absVel >= gaspFallThreshold * 1.5f)
        {
            PlayVoice(SoundBanks.VoiceHurt, voiceVolume);
            lastHurtVoiceTime = Time.time;
        }
        else if (absVel >= gaspFallThreshold)
        {
            if (Random.value < landingHurtChance)
                PlayVoice(SoundBanks.VoiceGasp, voiceVolume * 0.8f);
        }
    }

    public void OnHurt()
    {
        if (Time.time - lastHurtVoiceTime < hurtVoiceCooldown) return;
        PlayVoice(SoundBanks.VoiceHurt, voiceVolume);
        lastHurtVoiceTime = Time.time;
    }

    public void OnDeath()
    {
        PlayVoice(SoundBanks.VoiceDeath, voiceVolume);
    }

    public void OnCough()
    {
        PlayVoice(SoundBanks.VoiceCough, voiceVolume * 0.7f);
    }

    public void OnFrozen()
    {
        PlayVoice(SoundBanks.VoiceFrozen, voiceVolume * 0.6f);
    }

    public void OnShocked()
    {
        PlayVoice(SoundBanks.VoiceShocked, voiceVolume);
    }

    public void OnReflection()
    {
        PlayVoice(SoundBanks.VoiceReflection, voiceVolume * 0.5f);
    }

    void PlayBreathing()
    {
        PlayVoice(SoundBanks.VoiceRun, voiceVolume * 0.6f);
    }

    void PlayVoice(SampleBank bank, float volume)
    {
        if (bank == null || bank.IsEmpty) return;
        if (AudioManager.Instance == null) return;

        AudioClip clip = bank.GetRandom();
        if (clip == null) return;

        float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        AudioManager.Instance.PlaySampleUI(clip, volume, pitch);
    }
}