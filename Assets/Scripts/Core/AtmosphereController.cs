using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    [Header("References")]
    public Light directionalLight;
    public Material skyboxMaterial;

    [Header("Sun Light Colors")]
    public Color nightSunColor = new Color(0.1f, 0.15f, 0.3f);
    public Color dawnSunColor = new Color(1.0f, 0.6f, 0.4f);
    public Color daySunColor = new Color(1.0f, 0.96f, 0.88f);
    public Color duskSunColor = new Color(1.0f, 0.5f, 0.3f);

    [Header("Sun Light Intensity")]
    public float nightIntensity = 0.05f;
    public float dawnIntensity = 0.8f;
    public float dayIntensity = 1.5f;
    public float duskIntensity = 0.7f;

    [Header("Sky Top Colors")]
    public Color nightSkyTop = new Color(0.01f, 0.02f, 0.06f);
    public Color dawnSkyTop = new Color(0.3f, 0.35f, 0.6f);
    public Color daySkyTop = new Color(0.25f, 0.55f, 0.95f);
    public Color duskSkyTop = new Color(0.4f, 0.25f, 0.5f);

    [Header("Sky Middle Colors")]
    public Color nightSkyMiddle = new Color(0.05f, 0.06f, 0.12f);
    public Color dawnSkyMiddle = new Color(0.9f, 0.55f, 0.5f);
    public Color daySkyMiddle = new Color(0.6f, 0.8f, 1.0f);
    public Color duskSkyMiddle = new Color(1.0f, 0.5f, 0.3f);

    [Header("Sky Bottom Colors (Horizon)")]
    public Color nightSkyBottom = new Color(0.03f, 0.04f, 0.1f);
    public Color dawnSkyBottom = new Color(1.0f, 0.7f, 0.5f);
    public Color daySkyBottom = new Color(0.85f, 0.9f, 1.0f);
    public Color duskSkyBottom = new Color(1.0f, 0.4f, 0.2f);

    [Header("Ambient Colors")]
    public Color nightAmbient = new Color(0.05f, 0.08f, 0.15f);
    public Color dawnAmbient = new Color(0.5f, 0.4f, 0.35f);
    public Color dayAmbient = new Color(0.55f, 0.6f, 0.65f);
    public Color duskAmbient = new Color(0.5f, 0.35f, 0.3f);

    [Header("Fog")]
    public bool enableFog = true;
    public float fogDensity = 0.003f;

    [Header("Skylight Color (ambient tint)")]
    public Color nightSkyLight = new Color(0.08f, 0.1f, 0.2f);
    public Color dawnSkyLight = new Color(1.0f, 0.6f, 0.4f);
    public Color daySkyLight = new Color(0.85f, 0.95f, 1.0f);
    public Color duskSkyLight = new Color(1.0f, 0.55f, 0.35f);

    [Header("Lighting Update")]
    public float lightingUpdateIntervalSeconds = 30f;
    private float lastLightUpdateHour = -999f;
    private float lightUpdateThresholdDegrees = 15f;

    public static AtmosphereController Instance;

    void Start()
    {
        Instance = this;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;
        }
    }

    void Update()
    {
        if (TimeManager.Instance == null) return;

        float hour = TimeManager.Instance.currentTimeMinutes / 60f;
        float sunriseHour = TimeManager.Instance.SunriseHour;
        float sunsetHour = TimeManager.Instance.SunsetHour;

        Color sunColor, skyTop, skyMiddle, skyBottom, ambientColor, skyLight;
        float sunIntensity;
        float starIntensity;

        float dawnDuration = 1.5f;
        float duskDuration = 1.5f;

        if (hour < sunriseHour - dawnDuration || hour > sunsetHour + duskDuration)
        {
            sunColor = nightSunColor;
            sunIntensity = nightIntensity;
            skyTop = nightSkyTop;
            skyMiddle = nightSkyMiddle;
            skyBottom = nightSkyBottom;
            ambientColor = nightAmbient;
            skyLight = nightSkyLight;
            starIntensity = 1f;
        }
        else if (hour < sunriseHour)
        {
            float t = Mathf.InverseLerp(sunriseHour - dawnDuration, sunriseHour, hour);
            sunColor = Color.Lerp(nightSunColor, dawnSunColor, t);
            sunIntensity = Mathf.Lerp(nightIntensity, dawnIntensity, t);
            skyTop = Color.Lerp(nightSkyTop, dawnSkyTop, t);
            skyMiddle = Color.Lerp(nightSkyMiddle, dawnSkyMiddle, t);
            skyBottom = Color.Lerp(nightSkyBottom, dawnSkyBottom, t);
            ambientColor = Color.Lerp(nightAmbient, dawnAmbient, t);
            skyLight = Color.Lerp(nightSkyLight, dawnSkyLight, t);
            starIntensity = Mathf.Lerp(1f, 0f, t);
        }
        else if (hour < sunriseHour + dawnDuration)
        {
            float t = Mathf.InverseLerp(sunriseHour, sunriseHour + dawnDuration, hour);
            sunColor = Color.Lerp(dawnSunColor, daySunColor, t);
            sunIntensity = Mathf.Lerp(dawnIntensity, dayIntensity, t);
            skyTop = Color.Lerp(dawnSkyTop, daySkyTop, t);
            skyMiddle = Color.Lerp(dawnSkyMiddle, daySkyMiddle, t);
            skyBottom = Color.Lerp(dawnSkyBottom, daySkyBottom, t);
            ambientColor = Color.Lerp(dawnAmbient, dayAmbient, t);
            skyLight = Color.Lerp(dawnSkyLight, daySkyLight, t);
            starIntensity = 0f;
        }
        else if (hour < sunsetHour - duskDuration)
        {
            sunColor = daySunColor;
            sunIntensity = dayIntensity;
            skyTop = daySkyTop;
            skyMiddle = daySkyMiddle;
            skyBottom = daySkyBottom;
            ambientColor = dayAmbient;
            skyLight = daySkyLight;
            starIntensity = 0f;
        }
        else if (hour < sunsetHour)
        {
            float t = Mathf.InverseLerp(sunsetHour - duskDuration, sunsetHour, hour);
            sunColor = Color.Lerp(daySunColor, duskSunColor, t);
            sunIntensity = Mathf.Lerp(dayIntensity, duskIntensity, t);
            skyTop = Color.Lerp(daySkyTop, duskSkyTop, t);
            skyMiddle = Color.Lerp(daySkyMiddle, duskSkyMiddle, t);
            skyBottom = Color.Lerp(daySkyBottom, duskSkyBottom, t);
            ambientColor = Color.Lerp(dayAmbient, duskAmbient, t);
            skyLight = Color.Lerp(daySkyLight, duskSkyLight, t);
            starIntensity = 0f;
        }
        else
        {
            float t = Mathf.InverseLerp(sunsetHour, sunsetHour + duskDuration, hour);
            sunColor = Color.Lerp(duskSunColor, nightSunColor, t);
            sunIntensity = Mathf.Lerp(duskIntensity, nightIntensity, t);
            skyTop = Color.Lerp(duskSkyTop, nightSkyTop, t);
            skyMiddle = Color.Lerp(duskSkyMiddle, nightSkyMiddle, t);
            skyBottom = Color.Lerp(duskSkyBottom, nightSkyBottom, t);
            ambientColor = Color.Lerp(duskAmbient, nightAmbient, t);
            skyLight = Color.Lerp(duskSkyLight, nightSkyLight, t);
            starIntensity = Mathf.Lerp(0f, 1f, t);
        }

        if (directionalLight != null)
        {
            directionalLight.color = sunColor;
            directionalLight.intensity = sunIntensity;
        }

        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor("_TopColor", skyTop);
            skyboxMaterial.SetColor("_MiddleColor", skyMiddle);
            skyboxMaterial.SetColor("_BottomColor", skyBottom);
            skyboxMaterial.SetFloat("_StarIntensity", starIntensity);
        }

        RenderSettings.ambientLight = ambientColor;

        Shader.SetGlobalColor("_SkyLightColor", skyLight);

        bool underwaterActive = UnderwaterVisuals.Instance != null && UnderwaterVisuals.Instance.IsUnderwater;
        if (!underwaterActive)
            RenderSettings.fogColor = skyMiddle;

        float currentSunAngle = 0f;
        if (CelestialCycle.Instance == null)
        {
            if (directionalLight != null)
                currentSunAngle = directionalLight.transform.eulerAngles.x;
        }
        else
        {
            currentSunAngle = hour * 15f;
        }

        if (Mathf.Abs(currentSunAngle - lastLightUpdateHour) > lightUpdateThresholdDegrees)
        {
            lastLightUpdateHour = currentSunAngle;
            if (WorldManager.Instance != null)
                WorldManager.Instance.RecalculateLightingForAllChunks();
        }
    }
}