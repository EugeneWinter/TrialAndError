using UnityEngine;

public class KnappingGame : MonoBehaviour
{
    public static KnappingGame Instance;

    public bool isActive = false;
    public int hitsRemaining;
    public int mistakesRemaining;

    public float cursorPosition = 0f;
    public float cursorSpeed = 1f;
    public bool movingRight = true;

    public float targetMin = 0.4f;
    public float targetMax = 0.6f;
    public float targetWidth = 0.2f;

    public float impactAnim = 0f;

    private KnappingRecipe currentRecipe;
    private int totalMistakesAllowed = 3;
    private float justEndedTimer = 0f;

    void Awake() => Instance = this;

    public void StartGame(KnappingRecipe recipe)
    {
        if (isActive) return;

        currentRecipe = recipe;
        isActive = true;

        cursorPosition = 0f;
        cursorSpeed = recipe.cursorSpeed;
        movingRight = true;
        targetWidth = recipe.targetZoneWidth;
        hitsRemaining = recipe.hitCount;
        mistakesRemaining = totalMistakesAllowed;

        RandomizeTarget();

        GameManager.Instance.state = GameState.Minigame;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Stop(bool success)
    {
        isActive = false;
        justEndedTimer = 0.3f;

        if (currentRecipe != null)
        {
            Inventory.Instance.RemoveSelected();

            if (success)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayUI(AudioManager.Instance.knappingSuccess);

                Inventory.Instance.AddItem(currentRecipe.outputItemId, currentRecipe.outputCount);
                Debug.Log($"Knapping success! Created {currentRecipe.recipeName}");
            }
            else
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayUI(AudioManager.Instance.knappingFail);

                Debug.Log("Knapping failed! Stone lost.");
            }
        }

        GameManager.Instance.state = GameState.Playing;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (justEndedTimer > 0f)
        {
            justEndedTimer -= Time.deltaTime;
        }

        if (!isActive) return;

        if (movingRight)
        {
            cursorPosition += cursorSpeed * Time.deltaTime;
            if (cursorPosition >= 1f) { cursorPosition = 1f; movingRight = false; }
        }
        else
        {
            cursorPosition -= cursorSpeed * Time.deltaTime;
            if (cursorPosition <= 0f) { cursorPosition = 0f; movingRight = true; }
        }

        if (impactAnim > 0f)
        {
            impactAnim -= Time.deltaTime * 5f;
            if (impactAnim < 0f) impactAnim = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Hit();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Stop(false);
        }
    }

    void Hit()
    {
        impactAnim = 1f;

        if (cursorPosition >= targetMin && cursorPosition <= targetMax)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayUI(AudioManager.Instance.knappingHit);

            hitsRemaining--;
            cursorSpeed += 0.15f;
            RandomizeTarget();

            if (hitsRemaining <= 0)
            {
                Stop(true);
            }
        }
        else
        {
            mistakesRemaining--;

            if (mistakesRemaining <= 0)
            {
                Stop(false);
            }
        }
    }

    void RandomizeTarget()
    {
        float center = 0.15f + Random.value * 0.7f;
        targetMin = center - targetWidth / 2f;
        targetMax = center + targetWidth / 2f;
    }

    public bool JustEnded()
    {
        return justEndedTimer > 0f;
    }
}