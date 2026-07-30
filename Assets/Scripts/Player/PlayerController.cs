using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerWater))]
[RequireComponent(typeof(PlayerCamera), typeof(PlayerAudio))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isSneaking;
    [HideInInspector] public bool isInWater;
    [HideInInspector] public bool isSubmerged;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public Vector3 currentMoveVelocity;
    [HideInInspector] public float fallVelocity;

    public float3 size = new float3(0.6f, 1.8f, 0.6f);
    public float3 sneakSize = new float3(0.6f, 1.5f, 0.6f);
    public float3 CurrentSize => isSneaking ? sneakSize : size;

    public bool IsInWater => isInWater;
    public bool IsSubmerged => isSubmerged;

    private PlayerMovement movement;
    private PlayerWater water;
    private PlayerCamera cam;
    private PlayerAudio audioComp;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        water = GetComponent<PlayerWater>();
        cam = GetComponent<PlayerCamera>();
        audioComp = GetComponent<PlayerAudio>();

        movement.Init(this);
        water.Init(this);
        cam.Init(this);
        audioComp.Init(this, cam);
    }

    void Update()
    {
        if (GameManager.Instance.state != GameState.Playing) return;
        float dt = Time.deltaTime;

        // 1. Проверяем воду
        water.CheckWaterState();

        // 2. Вращение камеры
        cam.HandleLook(dt);

        // 3. Движение
        if (isInWater)
            water.HandleSwimming(dt);
        else
            movement.HandleMovement(dt);

        // 4. Физика и Коллизии
        movement.HandleCollisions(dt);

        // 5. Визуал камеры
        cam.UpdateCamera(dt);

        // 6. Звуки
        audioComp.UpdateAudio(dt);

        // Интеграция с голосом
        if (PlayerVoice.Instance != null)
            PlayerVoice.Instance.OnSprint(dt, isSprinting && movement.IsMovingHorizontally() && !isInWater);
    }
}