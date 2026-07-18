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

        worldManager.GenerateWorld();
        yield return null;

        int spawnX = 0;
        int spawnZ = 0;
        int spawnY = FindSurface(spawnX, spawnZ) + 2;

        player.transform.position = new Vector3(spawnX + 0.5f, spawnY, spawnZ + 0.5f);
        player.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        state = GameState.Playing;
    }

    int FindSurface(int x, int z)
    {
        int seaLevel = worldManager.seaLevel;

        for (int y = 250; y >= 0; y--)
        {
            ushort block = worldManager.GetBlock(x, y, z);
            if (block != 0 && block != 6)
            {
                if (y >= seaLevel)
                    return y;
            }
        }

        for (int searchRadius = 1; searchRadius < 100; searchRadius++)
        {
            for (int dx = -searchRadius; dx <= searchRadius; dx++)
                for (int dz = -searchRadius; dz <= searchRadius; dz++)
                {
                    if (Mathf.Abs(dx) != searchRadius && Mathf.Abs(dz) != searchRadius) continue;

                    for (int y = 250; y >= seaLevel; y--)
                    {
                        ushort block = worldManager.GetBlock(x + dx, y, z + dz);
                        if (block != 0 && block != 6)
                            return y;
                    }
                }

            x += searchRadius;
            z += searchRadius;
            break;
        }

        return seaLevel + 5;
    }
}