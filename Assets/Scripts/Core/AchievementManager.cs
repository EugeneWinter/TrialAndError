using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    private HashSet<string> unlockedAchievements = new HashSet<string>();

    void Awake()
    {
        Instance = this;
        LoadUnlocked();
    }

    public void Unlock(string achievementId)
    {
        if (unlockedAchievements.Contains(achievementId)) return;

        unlockedAchievements.Add(achievementId);
        SaveUnlocked();

        Debug.Log($"Achievement Unlocked: {achievementId}");

        // TODO: SteamUserStats.SetAchievement(achievementId);
        // TODO: SteamUserStats.StoreStats();

        // TODO: ѕоказать всплывающее уведомление в UI
    }

    public bool IsUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    private void LoadUnlocked()
    {
        string data = PlayerPrefs.GetString("achievements", "");
        if (string.IsNullOrEmpty(data)) return;

        foreach (var id in data.Split(';'))
        {
            if (!string.IsNullOrEmpty(id))
                unlockedAchievements.Add(id);
        }
    }

    private void SaveUnlocked()
    {
        PlayerPrefs.SetString("achievements", string.Join(";", unlockedAchievements));
        PlayerPrefs.Save();
    }
}