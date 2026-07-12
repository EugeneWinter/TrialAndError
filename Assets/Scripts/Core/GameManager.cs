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
        if (KnappingGame.Instance != null && KnappingGame.Instance.JustEnded()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == GameState.Playing) Pause();
            else if (state == GameState.Paused) Resume();
        }
    }

    public void Pause()
    {
        state = GameState.Paused;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    public void Resume()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;
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
        Debug.Log("Generating world...");

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
        Debug.Log($"Player spawned at {spawnX}, {spawnY}, {spawnZ}");
    }

    int FindSurface(int x, int z)
    {
        for (int y = 256; y >= 0; y--)
        {
            if (worldManager.IsBlockSolid(x, y, z))
                return y;
        }
        return 10;
    }
}