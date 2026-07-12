using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public string currentLanguage = "en";
    private Dictionary<string, string> translations = new Dictionary<string, string>();

    void Awake()
    {
        Instance = this;
        LoadLanguage(currentLanguage);
    }

    public void LoadLanguage(string lang)
    {
        currentLanguage = lang;
        translations.Clear();

        string path = Path.Combine(Application.streamingAssetsPath, "Localization", $"{lang}.json");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Language file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        LocalizationData data = JsonUtility.FromJson<LocalizationData>(json);

        foreach (var entry in data.entries)
        {
            translations[entry.key] = entry.value;
        }

        Debug.Log($"Loaded {translations.Count} translations for {lang}");
    }

    public string Get(string key)
    {
        if (translations.TryGetValue(key, out string value))
            return value;

        return $"[{key}]";
    }
}

[System.Serializable]
public class LocalizationData
{
    public LocalizationEntry[] entries;
}

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}