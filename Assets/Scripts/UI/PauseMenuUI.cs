using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public Button resumeButton;
    public Button exitButton;

    void Start()
    {
        resumeButton.onClick.AddListener(() => GameManager.Instance.Resume());
        exitButton.onClick.AddListener(() => GameManager.Instance.ExitGame());
    }
}