using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public GameSettings settings;

    void Awake()
    {
        Instance = this;
        Load();
    }

    public void Load()
    {
        settings = new GameSettings
        {
            mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 0.1f),
            stickSensitivity = PlayerPrefs.GetFloat("stickSensitivity", 200f),
            masterVolume = PlayerPrefs.GetFloat("masterVolume", 1.0f),
            musicVolume = PlayerPrefs.GetFloat("musicVolume", 0.7f),
            sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1.0f),
            fov = PlayerPrefs.GetFloat("fov", 75f),
            renderDistance = PlayerPrefs.GetInt("renderDistance", 4),
            language = PlayerPrefs.GetString("language", "en"),
            fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1,
        };
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("mouseSensitivity", settings.mouseSensitivity);
        PlayerPrefs.SetFloat("stickSensitivity", settings.stickSensitivity);
        PlayerPrefs.SetFloat("masterVolume", settings.masterVolume);
        PlayerPrefs.SetFloat("musicVolume", settings.musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", settings.sfxVolume);
        PlayerPrefs.SetFloat("fov", settings.fov);
        PlayerPrefs.SetInt("renderDistance", settings.renderDistance);
        PlayerPrefs.SetString("language", settings.language);
        PlayerPrefs.SetInt("fullscreen", settings.fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        Load();
    }
}

[System.Serializable]
public class GameSettings
{
    public float mouseSensitivity;
    public float stickSensitivity;
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float fov;
    public int renderDistance;
    public string language;
    public bool fullscreen;
}