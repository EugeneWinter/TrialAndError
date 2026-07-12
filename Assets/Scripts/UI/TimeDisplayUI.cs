using UnityEngine;
using UnityEngine.UI;

public class TimeDisplayUI : MonoBehaviour
{
    public Text timeText;
    public Text dateText;

    void Update()
    {
        if (TimeManager.Instance == null) return;

        int hour = TimeManager.Instance.Hour;
        int minute = TimeManager.Instance.Minute;
        timeText.text = $"{hour:D2}:{minute:D2}";

        Season s = TimeManager.Instance.CurrentSeason;
        string seasonName = s switch
        {
            Season.Spring => "Spring",
            Season.Summer => "Summer",
            Season.Autumn => "Autumn",
            Season.Winter => "Winter",
            _ => ""
        };

        int doy = TimeManager.Instance.DayOfYear;
        float daylight = TimeManager.Instance.DaylightHours;

        dateText.text = $"Day {doy} ({seasonName})   Daylight: {daylight:F1}h";
    }
}