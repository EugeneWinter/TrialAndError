using UnityEngine;
using System.Collections;

public class KnappingSession : MonoBehaviour
{
    public static KnappingSession Instance;

    [Header("Setup")]
    public Camera knappingCamera;
    public Transform stonePivot;
    public GameObject sceneRoot;

    [Header("Templates")]
    public KnappingTemplate rawStoneTemplate;
    public KnappingTemplate goalBladeTemplate;

    public Material ghostMaterial;
    public KnappingResultUI resultUI;
    public Light knappingLight;

    [HideInInspector] public KnappingStone currentStone;
    [HideInInspector] public KnappingPreview preview;
    [HideInInspector] public bool isActive = false;
    [HideInInspector] public bool waitingForResult = false;
    [HideInInspector] public bool stoneModified = false;

    private KnappingCameraController cam;
    private KnappingHitter hitter;
    private KnappingScorer scorer;
    private KnappingResult lastResult;

    void Awake()
    {
        Instance = this;

        if (sceneRoot != null)
            sceneRoot.SetActive(false);

        cam = GetComponent<KnappingCameraController>();
        hitter = GetComponent<KnappingHitter>();
        scorer = GetComponent<KnappingScorer>();
    }

    public void StartSession()
    {
        if (isActive) return;

        if (rawStoneTemplate == null || goalBladeTemplate == null)
        {
            Debug.LogError("KnappingSession: templates are missing");
            return;
        }

        StartCoroutine(StartSessionCoroutine());
    }

    IEnumerator StartSessionCoroutine()
    {
        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut();

        isActive = true;
        waitingForResult = false;
        stoneModified = false;
        lastResult = KnappingResult.Broken;

        sceneRoot.SetActive(true);

        if (currentStone != null) Destroy(currentStone.gameObject);
        if (preview != null) Destroy(preview.gameObject);

        GameObject stoneObj = new GameObject("KnappingStone");
        stoneObj.transform.SetParent(stonePivot);
        stoneObj.transform.localPosition = Vector3.zero;
        stoneObj.transform.localRotation = Quaternion.identity;

        Material mat = new Material(Shader.Find("Custom/KnappingStone"));

        currentStone = stoneObj.AddComponent<KnappingStone>();
        currentStone.stoneMaterial = mat;
        currentStone.GenerateFromTemplate(rawStoneTemplate);

        scorer.RecordInitialState();

        GameObject previewObj = new GameObject("KnappingPreview");
        previewObj.transform.SetParent(stonePivot);
        previewObj.transform.localRotation = Quaternion.identity;
        previewObj.transform.localPosition = CalculateTemplatePlacementOffset(currentStone, goalBladeTemplate);

        preview = previewObj.AddComponent<KnappingPreview>();
        preview.Setup(goalBladeTemplate, ghostMaterial, currentStone.VoxelSize);

        cam.Begin(stonePivot);
        hitter.Begin();

        GameManager.Instance.state = GameState.Minigame;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = !InputManager.Instance.IsGamepadActive;

        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeIn();
    }

    public void EndSession()
    {
        if (!isActive) return;
        StartCoroutine(EndSessionCoroutine());
    }

    IEnumerator EndSessionCoroutine()
    {
        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut();

        isActive = false;
        waitingForResult = false;

        if (currentStone != null) Destroy(currentStone.gameObject);
        if (preview != null) Destroy(preview.gameObject);

        sceneRoot.SetActive(false);

        hitter.End();
        GameManager.Instance.state = GameState.Playing;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeIn();
    }

    void Update()
    {
        if (!isActive) return;

        if (waitingForResult)
        {
            if (InputManager.Instance.KnappingConfirmPressed || InputManager.Instance.CancelPressed)
            {
                if (resultUI != null) resultUI.Hide();
                CompleteAndExit();
            }
            return;
        }

        cam.Tick();
        hitter.Tick();

        if (InputManager.Instance.CancelPressed)
        {
            if (stoneModified)
            {
                lastResult = KnappingResult.Broken;
                CompleteAndExit();
            }
            else
            {
                EndSession();
            }
        }
    }

    public void OnHitComplete()
    {
        stoneModified = true;
        scorer.CheckCompletion();
    }

    public void ShowResult(KnappingResult result, float score)
    {
        lastResult = result;
        waitingForResult = true;
        hitter.End();

        if (resultUI != null) resultUI.Show(result, score);
        else CompleteAndExit();
    }

    public void CompleteAndExit()
    {
        if (Inventory.Instance != null && stoneModified)
        {
            Inventory.Instance.RemoveSelected(1);

            if (lastResult == KnappingResult.Broken)
                Inventory.Instance.AddItem(goalBladeTemplate.failItemId, goalBladeTemplate.failCount);
            else
                Inventory.Instance.AddItem(goalBladeTemplate.resultItemId, 1);
        }

        EndSession();
    }

    Vector3 CalculateTemplatePlacementOffset(KnappingStone stone, KnappingTemplate template)
    {
        if (stone == null || template == null)
            return Vector3.zero;

        float vs = stone.VoxelSize;

        int offsetX = Mathf.RoundToInt((stone.Width - template.width) * 0.5f);
        int offsetY = Mathf.RoundToInt((stone.Height - template.height) * 0.5f);
        int offsetZ = Mathf.RoundToInt((stone.Depth - template.depth) * 0.5f);

        Vector3 stoneMin = new Vector3(
            -stone.Width * 0.5f * vs,
            -stone.Height * 0.5f * vs,
            -stone.Depth * 0.5f * vs);

        Vector3 templateMin = new Vector3(
            -template.width * 0.5f * vs,
            -template.height * 0.5f * vs,
            -template.depth * 0.5f * vs);

        Vector3 targetTemplateMin = stoneMin + new Vector3(
            offsetX * vs,
            offsetY * vs,
            offsetZ * vs);

        return targetTemplateMin - templateMin;
    }
}