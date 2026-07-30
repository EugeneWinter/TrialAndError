using UnityEngine;
using System.Collections;

public enum GameState { Loading, Playing, Paused, Minigame }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState state = GameState.Loading;

    [Header("References")]
    public WorldManager worldManager;
    public GameObject player;
    public GameObject pauseMenuUI;
    public Camera startupCamera;

    void Awake()
    {
        Instance = this;
        player.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(LoadWorld());
    }

    void Update()
    {
        if (state == GameState.Loading) return;
        if (state == GameState.Minigame) return;

        if (InputManager.Instance.CancelPressed)
        {
            if (state == GameState.Playing) Pause();
            else if (state == GameState.Paused) Resume();
        }
    }

    public void Pause()
    {
        state = GameState.Paused;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator LoadWorld()
    {
        state = GameState.Loading;

        while (Bootstrap.Instance == null || !Bootstrap.Instance.AllSystemsReady)
            yield return null;

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.SetStatus("Generating terrain...", 0.1f);

        yield return null;

        worldManager.GenerateWorld();

        while (!worldManager.IsWorldReady)
            yield return null;

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.SetStatus("Spawning player...", 0.98f);

        yield return null;

        int spawnX = 0;
        int spawnZ = 0;
        int spawnY = FindSurface(spawnX, spawnZ) + 2;

        player.transform.position = new Vector3(spawnX + 0.5f, spawnY, spawnZ + 0.5f);
        player.SetActive(true);

        if (startupCamera != null)
            startupCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.SetStatus("Ready!", 1f);

        yield return new WaitForSecondsRealtime(0.3f);

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.Hide();

        state = GameState.Playing;
    }

    int FindSurface(int x, int z)
    {
        int seaLevel = worldManager.seaLevel;

        for (int y = 250; y >= 0; y--)
        {
            ushort block = worldManager.GetBlock(x, y, z);
            if (block != BlockIDs.Air && block != BlockIDs.Water)
            {
                if (y >= seaLevel)
                    return y;
            }
        }

        for (int searchRadius = 1; searchRadius < 100; searchRadius++)
        {
            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dz = -searchRadius; dz <= searchRadius; dz++)
                {
                    if (Mathf.Abs(dx) != searchRadius && Mathf.Abs(dz) != searchRadius) continue;

                    for (int y = 250; y >= seaLevel; y--)
                    {
                        ushort block = worldManager.GetBlock(x + dx, y, z + dz);
                        if (block != BlockIDs.Air && block != BlockIDs.Water)
                            return y;
                    }
                }
            }
        }

        return seaLevel + 5;
    }
}