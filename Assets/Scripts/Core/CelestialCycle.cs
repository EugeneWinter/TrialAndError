using UnityEngine;

public class CelestialCycle : MonoBehaviour
{
    [Header("References")]
    public Transform sun;
    public Transform moon;
    public Light directionalLight;
    public Transform followTarget;

    [Header("Materials")]
    public Material sunMaterialRef;
    public Material moonMaterialRef;

    [Header("Orbit Settings")]
    public float orbitRadius = 200f;
    public float celestialScale = 30f;

    [Header("Self Rotation")]
    public float sunHoursPerRotation = 6f;
    public float moonHoursPerRotation = 12f;
    public Vector3 sunRotationAxis = new Vector3(0.3f, 1f, 0.2f);
    public Vector3 moonRotationAxis = new Vector3(0.5f, 1f, 0.3f);

    [Header("Sun Visuals")]
    public Color sunEmissionColor = new Color(1f, 0.95f, 0.8f, 1f);
    public float sunEmissionIntensity = 3.0f;
    public Color sunGlowColor = new Color(1f, 0.85f, 0.5f, 1f);
    public float sunGlowIntensity = 1.5f;

    [Header("Moon Visuals")]
    public Color moonEmissionColor = new Color(0.7f, 0.8f, 1.0f, 1f);
    public float moonEmissionIntensity = 2.0f;
    public Color moonGlowColor = new Color(0.5f, 0.6f, 0.9f, 1f);
    public float moonGlowIntensity = 1.0f;

    public static CelestialCycle Instance;

    private Quaternion sunSpinRotation = Quaternion.identity;
    private Quaternion moonSpinRotation = Quaternion.identity;

    private Transform sunPivot;
    private Transform moonPivot;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (sun != null)
        {
            sunPivot = WrapWithCenteredPivot(sun, "SunPivot");
            SetShadowsOffRecursive(sun.gameObject);
            sunSpinRotation = Quaternion.identity;
            if (sunMaterialRef != null)
            {
                ApplyMaterialRecursive(sun.gameObject, sunMaterialRef);
                sunMaterialRef.SetColor("_EmissionColor", sunEmissionColor);
                sunMaterialRef.SetFloat("_EmissionIntensity", sunEmissionIntensity);
                sunMaterialRef.SetColor("_GlowColor", sunGlowColor);
                sunMaterialRef.SetFloat("_GlowIntensity", sunGlowIntensity);
            }
        }

        if (moon != null)
        {
            moonPivot = WrapWithCenteredPivot(moon, "MoonPivot");
            SetShadowsOffRecursive(moon.gameObject);
            moonSpinRotation = Quaternion.identity;
            if (moonMaterialRef != null)
            {
                ApplyMaterialRecursive(moon.gameObject, moonMaterialRef);
                moonMaterialRef.SetColor("_EmissionColor", moonEmissionColor);
                moonMaterialRef.SetFloat("_EmissionIntensity", moonEmissionIntensity);
                moonMaterialRef.SetColor("_GlowColor", moonGlowColor);
                moonMaterialRef.SetFloat("_GlowIntensity", moonGlowIntensity);
            }
        }
    }

    Transform WrapWithCenteredPivot(Transform bodyRoot, string pivotName)
    {
        Bounds combined = ComputeCombinedBoundsLocal(bodyRoot);
        GameObject pivotGO = new GameObject(pivotName);
        Transform pivot = pivotGO.transform;
        Transform originalParent = bodyRoot.parent;
        pivot.SetParent(originalParent, false);
        pivot.position = bodyRoot.position;
        pivot.rotation = bodyRoot.rotation;
        pivot.localScale = bodyRoot.localScale;
        bodyRoot.SetParent(pivot, true);
        Vector3 centerOffsetLocal = combined.center;
        bodyRoot.localPosition = -centerOffsetLocal;
        return pivot;
    }

    Bounds ComputeCombinedBoundsLocal(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.one);
        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);
        Vector3 localCenter = root.InverseTransformPoint(worldBounds.center);
        Vector3 localSize = root.InverseTransformVector(worldBounds.size);
        localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        return new Bounds(localCenter, localSize);
    }

    void ApplyMaterialRecursive(GameObject obj, Material mat)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            r.sharedMaterials = mats;
        }
        foreach (Transform child in obj.transform)
            ApplyMaterialRecursive(child.gameObject, mat);
    }

    void SetShadowsOffRecursive(GameObject obj)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
        foreach (Transform child in obj.transform)
            SetShadowsOffRecursive(child.gameObject);
    }

    void LateUpdate()
    {
        if (TimeManager.Instance == null) return;
        if (followTarget == null) return;

        Vector3 anchor = followTarget.position;
        transform.position = anchor;

        float dt = Time.deltaTime;
        float gameHoursPerSecond = 1f / (60f * TimeManager.Instance.minutesPerHour);

        float sunDegPerSec = (360f / sunHoursPerRotation) * gameHoursPerSecond;
        float moonDegPerSec = (360f / moonHoursPerRotation) * gameHoursPerSecond;

        sunSpinRotation = Quaternion.AngleAxis(sunDegPerSec * dt, sunRotationAxis.normalized) * sunSpinRotation;
        moonSpinRotation = Quaternion.AngleAxis(moonDegPerSec * dt, moonRotationAxis.normalized) * moonSpinRotation;

        float hour = TimeManager.Instance.currentTimeMinutes / 60f;
        float sunriseHour = TimeManager.Instance.SunriseHour;
        float sunsetHour = TimeManager.Instance.SunsetHour;

        float sunAngle = CalculateSunAngle(hour, sunriseHour, sunsetHour);
        PlaceCelestialBody(sunPivot != null ? sunPivot : sun, sunAngle, sunSpinRotation, anchor);

        float moonAngle = sunAngle + 180f;
        PlaceCelestialBody(moonPivot != null ? moonPivot : moon, moonAngle, moonSpinRotation, anchor);

        UpdateDirectionalLightFromSun(anchor);
        UpdateEmissionWithTimeOfDay(hour, sunriseHour, sunsetHour);
    }

    void UpdateEmissionWithTimeOfDay(float hour, float sunrise, float sunset)
    {
        if (sunMaterialRef != null)
        {
            float sunAltitude = 0f;
            if (hour >= sunrise && hour <= sunset)
            {
                float dayProgress = Mathf.InverseLerp(sunrise, sunset, hour);
                sunAltitude = Mathf.Sin(dayProgress * Mathf.PI);
            }
            float horizonBoost = 1.0f + (1.0f - sunAltitude) * 0.5f;
            Color adjustedGlow = sunGlowColor * horizonBoost;
            if (hour > sunrise - 1.5f && hour < sunrise + 1.5f)
                adjustedGlow = Color.Lerp(sunGlowColor, new Color(1f, 0.5f, 0.2f, 1f), 0.4f);
            else if (hour > sunset - 1.5f && hour < sunset + 1.5f)
                adjustedGlow = Color.Lerp(sunGlowColor, new Color(1f, 0.4f, 0.15f, 1f), 0.5f);
            sunMaterialRef.SetColor("_GlowColor", adjustedGlow);
        }
        if (moonMaterialRef != null)
        {
            float moonVisibility = 0f;
            if (hour < sunrise - 0.5f || hour > sunset + 0.5f)
                moonVisibility = 1f;
            else if (hour < sunrise + 0.5f)
                moonVisibility = Mathf.InverseLerp(sunrise + 0.5f, sunrise - 0.5f, hour);
            else if (hour > sunset - 0.5f)
                moonVisibility = Mathf.InverseLerp(sunset - 0.5f, sunset + 0.5f, hour);
            moonMaterialRef.SetFloat("_EmissionIntensity", moonEmissionIntensity * moonVisibility);
            moonMaterialRef.SetFloat("_GlowIntensity", moonGlowIntensity * moonVisibility);
        }
    }

    float CalculateSunAngle(float hour, float sunrise, float sunset)
    {
        if (hour >= sunrise && hour <= sunset)
        {
            float dayProgress = Mathf.InverseLerp(sunrise, sunset, hour);
            return dayProgress * 180f;
        }
        else
        {
            float nightHour = hour < sunrise ? hour + 24f : hour;
            float nightSunset = sunset;
            float nightSunrise = sunrise + 24f;
            float nightProgress = Mathf.InverseLerp(nightSunset, nightSunrise, nightHour);
            return 180f + nightProgress * 180f;
        }
    }

    void PlaceCelestialBody(Transform body, float angleDegrees, Quaternion spinRotation, Vector3 anchor)
    {
        if (body == null) return;
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float x = -Mathf.Cos(angleRad) * orbitRadius;
        float y = Mathf.Sin(angleRad) * orbitRadius;
        body.position = anchor + new Vector3(x, y, 0f);
        body.rotation = spinRotation;
        body.localScale = Vector3.one * celestialScale;
    }

    void UpdateDirectionalLightFromSun(Vector3 anchor)
    {
        if (directionalLight == null) return;
        Transform sunAnchor = sunPivot != null ? sunPivot : sun;
        if (sunAnchor == null) return;
        Vector3 sunToAnchor = anchor - sunAnchor.position;
        if (sunToAnchor.sqrMagnitude < 0.0001f) return;
        directionalLight.transform.rotation = Quaternion.LookRotation(sunToAnchor.normalized);
    }
}