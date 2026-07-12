using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Time Settings")]
    public float minutesPerHour = 1f;
    public int startDay = 121;
    public int startHour = 12;

    [Header("Current Time (Read Only)")]
    public int currentDay;
    public float currentTimeMinutes;

    public int Hour => Mathf.FloorToInt(currentTimeMinutes / 60f);
    public int Minute => Mathf.FloorToInt(currentTimeMinutes % 60f);

    public float DayProgress => currentTimeMinutes / 1440f;

    public int DayOfYear => ((currentDay - 1) % 365) + 1;
    public int Year => (currentDay - 1) / 365 + 1;

    public Season CurrentSeason
    {
        get
        {
            int doy = DayOfYear;
            if (doy >= 60 && doy < 152) return Season.Spring;
            if (doy >= 152 && doy < 244) return Season.Summer;
            if (doy >= 244 && doy < 335) return Season.Autumn;
            return Season.Winter;
        }
    }

    public float DaylightHours
    {
        get
        {
            float dayAngle = ((DayOfYear - 80) / 365f) * Mathf.PI * 2f;
            return 12f + 6f * Mathf.Sin(dayAngle);
        }
    }

    public float SunriseHour => 12f - DaylightHours / 2f;
    public float SunsetHour => 12f + DaylightHours / 2f;

    public bool IsDaytime
    {
        get
        {
            float hour = currentTimeMinutes / 60f;
            return hour >= SunriseHour && hour <= SunsetHour;
        }
    }

    void Awake()
    {
        Instance = this;
        currentDay = startDay;
        currentTimeMinutes = startHour * 60f;
    }

    void Update()
    {
        if (GameManager.Instance.state != GameState.Playing) return;

        float secondsPerGameMinute = 60f / (60f * minutesPerHour);
        currentTimeMinutes += Time.deltaTime / secondsPerGameMinute;

        if (currentTimeMinutes >= 1440f)
        {
            currentTimeMinutes -= 1440f;
            currentDay++;
        }
    }
}

public enum Season { Spring, Summer, Autumn, Winter }