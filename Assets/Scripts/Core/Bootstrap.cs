using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-1000)]
public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance;
    public bool AllSystemsReady { get; private set; }

    [SerializeField] private MonoBehaviour[] systemObjects;

    private float startTime;

    void Awake()
    {
        Instance = this;
        AllSystemsReady = false;
        startTime = Time.realtimeSinceStartup;
    }

    IEnumerator Start()
    {
        List<SystemEntry> systems = new List<SystemEntry>();

        foreach (var obj in systemObjects)
        {
            if (obj is IGameSystem sys)
                systems.Add(new SystemEntry { name = obj.GetType().Name, system = sys });
        }

        for (int i = 0; i < systems.Count; i++)
        {
            float progress = (float)i / systems.Count;
            string name = systems[i].name;

            if (LoadingScreenUI.Instance != null)
                LoadingScreenUI.Instance.SetStatus($"Initializing {name}...", progress);

            float before = Time.realtimeSinceStartup;
            systems[i].system.InitializeSystem();
            float elapsed = Time.realtimeSinceStartup - before;

            Debug.Log($"[Bootstrap] {name}: {elapsed * 1000f:F0}ms");

            yield return null;
        }

        if (LoadingScreenUI.Instance != null)
            LoadingScreenUI.Instance.SetStatus("Generating world...", 0.9f);

        AllSystemsReady = true;

        float totalTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"[Bootstrap] All systems ready in {totalTime:F1}s");
    }

    private struct SystemEntry
    {
        public string name;
        public IGameSystem system;
    }
}