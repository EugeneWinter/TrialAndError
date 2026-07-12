using UnityEngine;
using UnityEngine.UI;

public class KnappingUI : MonoBehaviour
{
    public GameObject panel;
    public RectTransform barBackground;
    public RectTransform targetZone;
    public RectTransform cursor;
    public Text hitsText;
    public Text instructionText;

    void Update()
    {
        if (KnappingGame.Instance == null) return;

        bool active = KnappingGame.Instance.isActive;
        panel.SetActive(active);

        if (!active) return;

        float barWidth = barBackground.rect.width;

        float targetCenter = (KnappingGame.Instance.targetMin + KnappingGame.Instance.targetMax) / 2f;
        float targetW = (KnappingGame.Instance.targetMax - KnappingGame.Instance.targetMin) * barWidth;
        targetZone.anchoredPosition = new Vector2(targetCenter * barWidth - barWidth / 2f, 0);
        targetZone.sizeDelta = new Vector2(targetW, targetZone.sizeDelta.y);

        float cursorX = KnappingGame.Instance.cursorPosition * barWidth - barWidth / 2f;
        cursor.anchoredPosition = new Vector2(cursorX, 0);

        hitsText.text = $"Hits left: {KnappingGame.Instance.hitsRemaining}   Mistakes left: {KnappingGame.Instance.mistakesRemaining}";
        instructionText.text = "Press SPACE when cursor is in green zone";
    }
}