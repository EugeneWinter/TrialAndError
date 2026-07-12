using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    public GameObject crosshair;

    void Update()
    {
        if (GameManager.Instance == null)
        {
            crosshair.SetActive(true);
            return;
        }

        bool shouldHide = GameManager.Instance.state == GameState.Paused
                       || GameManager.Instance.state == GameState.Minigame
                       || GameManager.Instance.state == GameState.Loading;

        crosshair.SetActive(!shouldHide);
    }
}