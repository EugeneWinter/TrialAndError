using UnityEngine;
using UnityEngine.UI;

public class KnappingResultUI : MonoBehaviour
{
    public GameObject panel;
    public Text titleText;
    public Text descriptionText;
    public Button continueButton;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Show(KnappingResult result, float score)
    {
        if (panel == null) return;
        panel.SetActive(true);

        titleText.text = KnappingEvaluator.GetResultName(result);
        descriptionText.text = GetDescription(result, score);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnContinueClicked()
    {
        Hide();
        if (KnappingSession.Instance != null)
            KnappingSession.Instance.CompleteAndExit();
    }

    string GetDescription(KnappingResult r, float score)
    {
        switch (r)
        {
            case KnappingResult.Perfect:
                string[] perfectTexts = {
                    "Even Otzi would ask you for tips.\nDurability: +50%",
                    "Somewhere, an archaeologist is crying with joy.\nDurability: +50%",
                    "Sharp enough to shave a mammoth.\nDurability: +50%"
                };
                return $"{perfectTexts[Random.Range(0, perfectTexts.Length)]}\n\nQuality: {(score * 100):F0}%";

            case KnappingResult.Good:
                string[] goodTexts = {
                    "A solid working blade.\nYour ancestors would nod approvingly.",
                    "Functional. Sharp. Vaguely intentional.",
                    "Not bad. Not great. Just... blade."
                };
                return $"{goodTexts[Random.Range(0, goodTexts.Length)]}\nDurability: normal\n\nQuality: {(score * 100):F0}%";

            case KnappingResult.Average:
                string[] avgTexts = {
                    "It's technically a blade.\nDurability: -30%",
                    "Sharp on one side. That's enough, right?\nDurability: -30%",
                    "You'll do better next time. Probably.\nDurability: -30%"
                };
                return $"{avgTexts[Random.Range(0, avgTexts.Length)]}\n\nQuality: {(score * 100):F0}%";

            case KnappingResult.Poor:
                string[] poorTexts = {
                    "This is a rock. A slightly pointed rock.\nDurability: -60%",
                    "You call it a blade. History disagrees.\nDurability: -60%",
                    "Cavemen everywhere are laughing at you.\nDurability: -60%"
                };
                return $"{poorTexts[Random.Range(0, poorTexts.Length)]}\n\nQuality: {(score * 100):F0}%";

            case KnappingResult.Broken:
                string[] brokenTexts = {
                    "You struck too hard. The stone shattered.\nYou salvage a few shards.",
                    "The stone gave up on you.\nAt least you have gravel now.",
                    "Congratulations on inventing gravel."
                };
                return brokenTexts[Random.Range(0, brokenTexts.Length)];

            default:
                return "";
        }
    }
}