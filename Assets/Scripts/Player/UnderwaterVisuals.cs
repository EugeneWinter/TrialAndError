using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnderwaterVisuals : MonoBehaviour
{
    public static UnderwaterVisuals Instance;

    [Header("Underwater Fog")]
    public Color underwaterFogColor = new Color(0.01f, 0.04f, 0.08f);
    public float underwaterFogDensity = 0.15f;

    [Header("Normal Fog")]
    public float normalFogDensity = 0.003f;

    [Header("Transition")]
    public float transitionSpeed = 6f;

    [Header("Screen Overlay")]
    public Image underwaterOverlay;
    public Color underwaterOverlayColor = new Color(0.02f, 0.1f, 0.2f, 0.35f);

    [Header("Camera Clipping")]
    public float normalFarClip = 500f;
    public float underwaterFarClip = 60f;

    private bool isUnderwater = false;
    public bool IsUnderwater => isUnderwater;

    private Color savedFogColor;
    private float targetFogDensity;
    private Color targetFogColor;
    private Color targetOverlayColor;
    private float targetFarClip;

    private Volume postProcessVolume;
    private Vignette vignette;
    private Camera playerCamera;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        savedFogColor = RenderSettings.fogColor;
        targetFogDensity = normalFogDensity;
        targetFogColor = savedFogColor;
        targetOverlayColor = Color.clear;
        targetFarClip = normalFarClip;

        if (underwaterOverlay != null)
        {
            underwaterOverlay.color = Color.clear;
            underwaterOverlay.raycastTarget = false;
        }

        playerCamera = Camera.main;
        if (playerCamera != null)
            normalFarClip = playerCamera.farClipPlane;

        postProcessVolume = FindObjectOfType<Volume>();
        if (postProcessVolume != null && postProcessVolume.profile != null)
            postProcessVolume.profile.TryGet(out vignette);
    }

    void Update()
    {
        PlayerController player = FindPlayerController();
        if (player == null) return;

        bool shouldBeUnderwater = player.IsSubmerged;

        if (shouldBeUnderwater != isUnderwater)
        {
            isUnderwater = shouldBeUnderwater;

            if (isUnderwater)
            {
                savedFogColor = RenderSettings.fogColor;
                targetFogDensity = underwaterFogDensity;
                targetFogColor = underwaterFogColor;
                targetOverlayColor = underwaterOverlayColor;
                targetFarClip = underwaterFarClip;
            }
            else
            {
                targetFogDensity = normalFogDensity;
                targetFogColor = savedFogColor;
                targetOverlayColor = Color.clear;
                targetFarClip = normalFarClip;
            }
        }

        float speed = transitionSpeed * Time.deltaTime;

        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, speed);
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, speed);

        if (underwaterOverlay != null)
            underwaterOverlay.color = Color.Lerp(underwaterOverlay.color, targetOverlayColor, speed);

        if (playerCamera != null)
            playerCamera.farClipPlane = Mathf.Lerp(playerCamera.farClipPlane, targetFarClip, speed);

        if (vignette != null)
        {
            float targetIntensity = isUnderwater ? 0.45f : 0.2f;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, speed);
        }

        if (!isUnderwater)
            savedFogColor = RenderSettings.fogColor;
    }

    PlayerController FindPlayerController()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) return player.GetComponent<PlayerController>();
        return null;
    }
}