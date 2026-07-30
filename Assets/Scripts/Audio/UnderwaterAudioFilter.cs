using UnityEngine;

public class UnderwaterAudioFilter : MonoBehaviour
{
    public static UnderwaterAudioFilter Instance;

    [Header("Filter Settings")]
    public float underwaterLowpassFreq = 600f;
    public float normalLowpassFreq = 22000f;
    public float underwaterVolume = 0.4f;
    public float normalVolume = 1.0f;
    public float transitionSpeed = 5f;

    [Header("Underwater Ambience")]
    public float underwaterAmbienceVolume = 0.3f;

    private AudioLowPassFilter lowPassFilter;
    private AudioSource underwaterAmbienceSource;
    private bool isUnderwater = false;
    private float targetFreq;
    private float targetVolume;
    private float targetAmbienceVolume;

    private bool firstFrame = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener == null) return;

        lowPassFilter = listener.gameObject.GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null)
            lowPassFilter = listener.gameObject.AddComponent<AudioLowPassFilter>();

        lowPassFilter.cutoffFrequency = normalLowpassFreq;
        lowPassFilter.lowpassResonanceQ = 1f;

        GameObject ambiObj = new GameObject("UnderwaterAmbience");
        ambiObj.transform.SetParent(listener.transform);
        underwaterAmbienceSource = ambiObj.AddComponent<AudioSource>();
        underwaterAmbienceSource.loop = true;
        underwaterAmbienceSource.spatialBlend = 0f;
        underwaterAmbienceSource.volume = 0f;
        underwaterAmbienceSource.playOnAwake = false;

        AudioClip clip = CreateUnderwaterDrone();
        underwaterAmbienceSource.clip = clip;
        underwaterAmbienceSource.Play();

        targetFreq = normalLowpassFreq;
        targetVolume = normalVolume;
        targetAmbienceVolume = 0f;
    }

    void Update()
    {
        PlayerController player = FindPlayerController();
        if (player == null) return;

        bool shouldBeUnderwater = player.IsSubmerged;

        if (shouldBeUnderwater != isUnderwater || firstFrame)
        {
            isUnderwater = shouldBeUnderwater;

            if (isUnderwater)
            {
                targetFreq = underwaterLowpassFreq;
                targetVolume = underwaterVolume;
                targetAmbienceVolume = underwaterAmbienceVolume;
            }
            else
            {
                targetFreq = normalLowpassFreq;
                targetVolume = normalVolume;
                targetAmbienceVolume = 0f;
            }
        }

        if (firstFrame)
        {
            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = targetFreq;
                lowPassFilter.lowpassResonanceQ = isUnderwater ? 1.4f : 1f;
            }
            AudioListener.volume = targetVolume;
            if (underwaterAmbienceSource != null)
                underwaterAmbienceSource.volume = targetAmbienceVolume;

            firstFrame = false;
        }
        else
        {
            float speed = transitionSpeed * Time.deltaTime;

            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetFreq, speed);
                lowPassFilter.lowpassResonanceQ = isUnderwater ? 1.4f : 1f;
            }

            AudioListener.volume = Mathf.Lerp(AudioListener.volume, targetVolume, speed);

            if (underwaterAmbienceSource != null)
                underwaterAmbienceSource.volume = Mathf.Lerp(underwaterAmbienceSource.volume, targetAmbienceVolume, speed);
        }
    }

    AudioClip CreateUnderwaterDrone()
    {
        int sampleRate = 44100;
        int lengthSeconds = 4;
        int sampleCount = sampleRate * lengthSeconds;
        float[] samples = new float[sampleCount];

        System.Random rng = new System.Random(42);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            float drone = Mathf.Sin(t * 2f * Mathf.PI * 55f) * 0.15f;
            float drone2 = Mathf.Sin(t * 2f * Mathf.PI * 82.5f) * 0.08f;
            float drone3 = Mathf.Sin(t * 2f * Mathf.PI * 36f) * 0.1f;

            float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.05f;

            float wobble = Mathf.Sin(t * 0.3f) * 0.3f + 0.7f;

            samples[i] = (drone + drone2 + drone3 + noise) * wobble;

            float fadeIn = Mathf.Clamp01(t / 0.5f);
            float fadeOut = Mathf.Clamp01((lengthSeconds - t) / 0.5f);
            samples[i] *= fadeIn * fadeOut;
        }

        AudioClip clip = AudioClip.Create("UnderwaterDrone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    PlayerController FindPlayerController()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) return player.GetComponent<PlayerController>();
        return null;
    }
}