using UnityEngine;

public class CelestialCycle : MonoBehaviour
{
    [Header("References")]
    public Transform sun;
    public Transform moon;
    public Light directionalLight;
    public Transform followTarget;

    [Header("Orbit Settings")]
    public float orbitRadius = 400f;
    public float celestialScale = 100f;

    [Header("Self Rotation (in game hours per full rotation)")]
    public float sunHoursPerRotation = 6f;
    public float moonHoursPerRotation = 12f;
    public Vector3 sunRotationAxis = new Vector3(0.3f, 1f, 0.2f);
    public Vector3 moonRotationAxis = new Vector3(0.5f, 1f, 0.3f);

    public static CelestialCycle Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (sun != null)
            SetShadowsOffRecursive(sun.gameObject);
        if (moon != null)
            SetShadowsOffRecursive(moon.gameObject);
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

        Vector3 center = followTarget.position;
        transform.position = center;

        float hour = TimeManager.Instance.currentTimeMinutes / 60f;
        float sunriseHour = TimeManager.Instance.SunriseHour;
        float sunsetHour = TimeManager.Instance.SunsetHour;

        float sunAngle = CalculateSunAngle(hour, sunriseHour, sunsetHour);
        UpdateCelestialBody(sun, sunAngle, sunHoursPerRotation, sunRotationAxis);

        float moonAngle = sunAngle + 180f;
        UpdateCelestialBody(moon, moonAngle, moonHoursPerRotation, moonRotationAxis);

        UpdateDirectionalLight(sunAngle);
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

    void UpdateCelestialBody(Transform body, float angleDegrees, float hoursPerRotation, Vector3 rotAxis)
    {
        if (body == null) return;

        float angleRad = angleDegrees * Mathf.Deg2Rad;

        float x = -Mathf.Cos(angleRad) * orbitRadius;
        float y = Mathf.Sin(angleRad) * orbitRadius;
        float z = 0f;

        body.position = transform.position + new Vector3(x, y, z);
        body.localScale = Vector3.one * celestialScale;

        float gameHoursPerSecond = 1f / (60f * TimeManager.Instance.minutesPerHour);
        float degreesPerGameHour = 360f / hoursPerRotation;
        float rotationThisFrame = degreesPerGameHour * gameHoursPerSecond * Time.deltaTime;

        body.Rotate(rotAxis.normalized, rotationThisFrame, Space.World);
    }

    void UpdateDirectionalLight(float sunAngleDegrees)
    {
        if (directionalLight == null) return;

        float angleRad = sunAngleDegrees * Mathf.Deg2Rad;

        Vector3 sunPosition = new Vector3(
            -Mathf.Cos(angleRad),
            Mathf.Sin(angleRad),
            0.15f);

        Vector3 lightDirection = -sunPosition.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lightDirection);

        directionalLight.transform.rotation = Quaternion.RotateTowards(
            directionalLight.transform.rotation,
            targetRotation,
            5f * Time.deltaTime);
    }
}