using UnityEngine;
using System.Collections;

public class KnappingSession : MonoBehaviour
{
    public static KnappingSession Instance;

    [Header("Setup")]
    public Camera knappingCamera;
    public Transform stonePivot;
    public GameObject sceneRoot;
    public Material stoneMaterial;

    [Header("Camera")]
    public float rotateSpeed = 3f;
    public float stickRotateSpeed = 120f;

    [Header("Hit")]
    public int minHitRadius = 2;
    public int maxHitRadius = 5;
    public float minPowerChargeTime = 0.15f;
    public float maxPowerChargeTime = 1.2f;
    public float shakeMagnitude = 0.03f;
    public float shakeDecay = 8f;

    [Header("Angle")]
    public float angleStep = 15f;

    [Header("Particles")]
    public KnappingParticles particles;
    public Color stoneParticleColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("Indicator")]
    public KnappingIndicator indicator;

    [Header("Ambience")]
    public Light knappingLight;

    [Header("Completion")]
    public KnappingResultUI resultUI;
    public ushort bladeItemId = 1002;
    public ushort shardsItemId = 1001;
    public int voxelsToComplete = 60;
    public int minVoxelsBeforeBreak = 20;

    private KnappingStone currentStone;
    private bool isActive = false;
    private Vector3 stoneRotation = Vector3.zero;
    private Vector3 shakeOffset = Vector3.zero;
    private Vector3 pivotBasePos;

    private float currentAngle = 0f;
    private float chargeStartTime = 0f;
    private bool isCharging = false;
    private int initialVoxelCount = 0;
    private bool waitingForResult = false;

    // Gamepad virtual cursor
    private Vector3 virtualCursorScreenPos;

    void Awake()
    {
        Instance = this;
        if (sceneRoot != null) sceneRoot.SetActive(false);
    }

    public void StartSession(int seed)
    {
        if (isActive) return;
        StartCoroutine(StartSessionCoroutine(seed));
    }

    IEnumerator StartSessionCoroutine(int seed)
    {
        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut();

        isActive = true;
        waitingForResult = false;

        sceneRoot.SetActive(true);

        if (currentStone != null) Destroy(currentStone.gameObject);

        GameObject stoneObj = new GameObject("KnappingStone");
        stoneObj.transform.SetParent(stonePivot);
        stoneObj.transform.localPosition = Vector3.zero;
        stoneObj.transform.localRotation = Quaternion.identity;

        Material mat = new Material(Shader.Find("Custom/KnappingStone"));
        mat.SetColor("_BaseColor", Color.white);

        currentStone = stoneObj.AddComponent<KnappingStone>();
        currentStone.stoneMaterial = mat;
        currentStone.Generate(seed);

        initialVoxelCount = CountStoneVoxels();

        stoneRotation = new Vector3(-25f, 45f, 15f);
        stonePivot.localEulerAngles = stoneRotation;
        pivotBasePos = stonePivot.localPosition;

        currentAngle = 0f;
        isCharging = false;

        // Initialize virtual cursor to screen center
        virtualCursorScreenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        GameManager.Instance.state = GameState.Minigame;

        bool useGamepad = InputManager.Instance.IsGamepadActive;
        Cursor.lockState = useGamepad ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !useGamepad;

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
        sceneRoot.SetActive(false);

        if (indicator != null)
        {
            indicator.HideCursor();
            indicator.HideAngle();
            indicator.HidePower();
        }

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
            if (InputManager.Instance.KnappingConfirmPressed)
            {
                if (resultUI != null) resultUI.Hide();
                CompleteAndExit();
            }
            return;
        }

        HandleRotation();
        HandleAngleChange();
        UpdateIndicator();
        HandleHit();
        HandleShake();
        CheckCompletion();

        if (InputManager.Instance.CancelPressed)
        {
            EndSession();
        }
    }

    void HandleRotation()
    {
        bool useGamepad = InputManager.Instance.IsGamepadActive;

        if (useGamepad)
        {
            // Gamepad: left stick rotates stone
            Vector2 stick = InputManager.Instance.Move;
            if (stick.magnitude > 0.1f)
            {
                stoneRotation.y += stick.x * stickRotateSpeed * Time.deltaTime;
                stoneRotation.x -= stick.y * stickRotateSpeed * Time.deltaTime;
                stoneRotation.x = Mathf.Clamp(stoneRotation.x, -80f, 80f);
                stonePivot.localEulerAngles = stoneRotation;
            }

            // Gamepad: right stick moves virtual cursor
            Vector2 cursorMove = InputManager.Instance.Look;
            float cursorSpeed = 600f;
            virtualCursorScreenPos.x += cursorMove.x * cursorSpeed * Time.deltaTime;
            virtualCursorScreenPos.y += cursorMove.y * cursorSpeed * Time.deltaTime;
            virtualCursorScreenPos.x = Mathf.Clamp(virtualCursorScreenPos.x, 0, Screen.width);
            virtualCursorScreenPos.y = Mathf.Clamp(virtualCursorScreenPos.y, 0, Screen.height);
        }
        else
        {
            // Mouse: right click rotates
            if (InputManager.Instance.PlacePressed || Input.GetMouseButton(1))
            {
                Vector2 delta = InputManager.Instance.Look;
                stoneRotation.y += delta.x * rotateSpeed;
                stoneRotation.x -= delta.y * rotateSpeed;
                stoneRotation.x = Mathf.Clamp(stoneRotation.x, -80f, 80f);
                stonePivot.localEulerAngles = stoneRotation;
            }

            virtualCursorScreenPos = Input.mousePosition;
        }
    }

    void HandleAngleChange()
    {
        float angleInput = InputManager.Instance.KnappingAngle;
        if (Mathf.Abs(angleInput) > 0.1f)
        {
            currentAngle += Mathf.Sign(angleInput) * angleStep;
            currentAngle = ((currentAngle % 360f) + 360f) % 360f;
        }

        // Also support mouse scroll for angle
        float scroll = InputManager.Instance.ScrollHotbar;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentAngle += Mathf.Sign(scroll) * angleStep;
            currentAngle = ((currentAngle % 360f) + 360f) % 360f;
        }
    }

    KnappingHit GetHitUnderCursor()
    {
        if (currentStone == null) return new KnappingHit { hit = false };
        Ray ray = knappingCamera.ScreenPointToRay(virtualCursorScreenPos);
        return KnappingRaycaster.Cast(currentStone, ray);
    }

    void UpdateIndicator()
    {
        if (indicator == null) return;

        KnappingHit hit = GetHitUnderCursor();

        if (!hit.hit)
        {
            indicator.HideCursor();
            indicator.HideAngle();
            indicator.HidePower();
            return;
        }

        Vector3 worldNormal = currentStone.transform.TransformDirection(
            new Vector3(hit.normal.x, hit.normal.y, hit.normal.z));

        indicator.ShowCursor(hit.worldPoint, worldNormal);
        indicator.ShowAngle(hit.worldPoint, worldNormal, currentAngle);

        if (isCharging)
        {
            float charge = GetCurrentCharge();
            indicator.ShowPower(hit.worldPoint, worldNormal, charge);
        }
        else
        {
            indicator.HidePower();
        }
    }

    void HandleHit()
    {
        if (InputManager.Instance.KnappingStrikePressed)
        {
            isCharging = true;
            chargeStartTime = Time.time;
        }

        if (InputManager.Instance.KnappingStrikeReleased && isCharging)
        {
            isCharging = false;
            float charge = GetCurrentCharge();
            ExecuteHit(charge);
        }
    }

    float GetCurrentCharge()
    {
        float elapsed = Time.time - chargeStartTime;
        return Mathf.Clamp01(elapsed / maxPowerChargeTime);
    }

    void ExecuteHit(float power01)
    {
        if (currentStone == null) return;
        if (power01 < minPowerChargeTime / maxPowerChargeTime) return;

        KnappingHit hit = GetHitUnderCursor();
        if (!hit.hit) return;

        int radius = Mathf.RoundToInt(Mathf.Lerp(minHitRadius, maxHitRadius, power01));

        Vector3 angleDir = ComputeAngleDirection(hit.normal, currentAngle);
        RemoveVoxelsShaped(hit.voxel, radius, angleDir, power01);

        Vector3 worldPoint = currentStone.transform.TransformPoint(
            LocalVoxelToLocalSpace(hit.voxel));

        Vector3 worldNormal = currentStone.transform.TransformDirection(
            new Vector3(hit.normal.x, hit.normal.y, hit.normal.z));

        if (particles != null)
        {
            int particleBoost = Mathf.RoundToInt(1f + power01 * 2f);
            for (int i = 0; i < particleBoost; i++)
                particles.Burst(worldPoint, worldNormal, stoneParticleColor);
        }

        shakeOffset = Random.insideUnitSphere * shakeMagnitude * (0.5f + power01);

        PlayHitSound(worldPoint, power01);
    }

    void PlayHitSound(Vector3 worldPos, float power01)
    {
        if (AudioManager.Instance == null) return;

        AudioClip clip = SoundBanks.BlockHitWood.GetRandom();
        if (clip == null) return;

        float pitch = Mathf.Lerp(1.2f, 0.75f, power01) + Random.Range(-0.05f, 0.05f);
        float volume = Mathf.Lerp(0.5f, 0.9f, power01);

        AudioManager.Instance.PlaySample3D(clip, worldPos, volume, pitch, 0.5f, 15f, 0.7f);
    }

    Vector3 ComputeAngleDirection(Vector3Int normal, float angleDegrees)
    {
        Vector3 n = new Vector3(normal.x, normal.y, normal.z);
        Vector3 up = Mathf.Abs(n.y) > 0.9f ? Vector3.forward : Vector3.up;
        Vector3 right = Vector3.Cross(up, n).normalized;
        Vector3 realUp = Vector3.Cross(n, right).normalized;

        float rad = angleDegrees * Mathf.Deg2Rad;
        return (right * Mathf.Cos(rad) + realUp * Mathf.Sin(rad)).normalized;
    }

    void RemoveVoxelsShaped(Vector3Int center, int radius, Vector3 direction, float power01)
    {
        float stretch = 1.5f + power01 * 1.5f;
        Vector3 dir = direction.normalized;

        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                for (int dz = -radius; dz <= radius; dz++)
                {
                    Vector3 offset = new Vector3(dx, dy, dz);
                    float distAlong = Vector3.Dot(offset, dir);
                    Vector3 perp = offset - dir * distAlong;
                    float perpDistSq = perp.sqrMagnitude;

                    float alongScaled = distAlong / stretch;
                    float totalDistSq = perpDistSq + alongScaled * alongScaled;

                    if (totalDistSq > radius * radius) continue;

                    currentStone.RemoveVoxel(center.x + dx, center.y + dy, center.z + dz);
                }
    }

    Vector3 LocalVoxelToLocalSpace(Vector3Int voxel)
    {
        float vs = currentStone.voxelSize;
        return new Vector3(
            (voxel.x - currentStone.Width * 0.5f + 0.5f) * vs,
            (voxel.y + 0.5f) * vs,
            (voxel.z - currentStone.Depth * 0.5f + 0.5f) * vs);
    }

    void HandleShake()
    {
        shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, shakeDecay * Time.deltaTime);
        stonePivot.localPosition = pivotBasePos + shakeOffset;
    }

    int CountStoneVoxels()
    {
        if (currentStone == null) return 0;
        int count = 0;
        for (int x = 0; x < currentStone.Width; x++)
            for (int y = 0; y < currentStone.Height; y++)
                for (int z = 0; z < currentStone.Depth; z++)
                    if (currentStone.Voxels[x, y, z]) count++;
        return count;
    }

    void CheckCompletion()
    {
        if (currentStone == null) return;
        int current = CountStoneVoxels();

        if (current <= voxelsToComplete)
        {
            FinishAndShowResult();
        }
    }

    void FinishAndShowResult()
    {
        waitingForResult = true;
        KnappingResult result = KnappingEvaluator.Evaluate(currentStone, out float score);

        PlayResultSound(result);

        if (indicator != null)
        {
            indicator.HideCursor();
            indicator.HideAngle();
            indicator.HidePower();
        }

        if (resultUI != null)
            resultUI.Show(result, score);
        else
            CompleteAndExit();
    }

    void PlayResultSound(KnappingResult result)
    {
        if (AudioManager.Instance == null) return;

        SampleBank bank = result == KnappingResult.Broken ? SoundBanks.ItemDrop : SoundBanks.ItemPickup;
        AudioClip clip = bank.GetRandom();
        if (clip == null) return;

        AudioManager.Instance.PlaySampleUI(clip, 0.8f, 1f);
    }

    public void CompleteAndExit()
    {
        KnappingResult result = KnappingEvaluator.Evaluate(currentStone, out float score);

        ushort itemToGive = result == KnappingResult.Broken ? shardsItemId : bladeItemId;
        int countToGive = result == KnappingResult.Broken ? 2 : 1;

        if (Inventory.Instance != null)
        {
            Inventory.Instance.slots[Inventory.Instance.selectedSlot].count -= 2;
            if (Inventory.Instance.slots[Inventory.Instance.selectedSlot].count <= 0)
                Inventory.Instance.slots[Inventory.Instance.selectedSlot].id = 0;

            Inventory.Instance.AddItem(itemToGive, countToGive);
        }

        EndSession();
    }
}