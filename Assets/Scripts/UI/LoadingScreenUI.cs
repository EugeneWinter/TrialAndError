using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance;

    public RectTransform progressBar;
    public Text statusText;
    public Text percentText;

    private float targetProgress;
    private float currentProgress;
    private float barMaxWidth;

    void Awake()
    {
        Instance = this;

        if (progressBar != null)
        {
            RectTransform parent = progressBar.parent as RectTransform;
            if (parent != null)
                barMaxWidth = parent.rect.width;
            else
                barMaxWidth = 600f;
        }

        Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        currentProgress = 0f;
        targetProgress = 0f;
        UpdateVisuals();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetStatus(string status, float progress01)
    {
        targetProgress = Mathf.Clamp01(progress01);
        if (statusText != null) statusText.text = status;
    }

    void Update()
    {
        currentProgress = Mathf.Lerp(currentProgress, targetProgress, 12f * Time.unscaledDeltaTime);
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (progressBar != null)
        {
            Vector2 size = progressBar.sizeDelta;
            size.x = barMaxWidth * currentProgress;
            progressBar.sizeDelta = size;
        }

        if (percentText != null)
            percentText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
    }
}