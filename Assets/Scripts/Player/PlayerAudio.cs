using UnityEngine;
using Unity.Mathematics;

public class PlayerAudio : MonoBehaviour
{
    private PlayerController master;
    private PlayerCamera cam;

    private bool wasOnGround = false;
    private bool wasInWater = false;
    private float landingCooldown = 0f;

    struct FootBlocks
    {
        public ushort primary;
        public ushort secondary;
        public float blend;
    }

    public void Init(PlayerController controller, PlayerCamera cameraModule)
    {
        master = controller;
        cam = cameraModule;
    }

    public void UpdateAudio(float dt)
    {
        if (landingCooldown > 0f) landingCooldown -= dt;

        // Прыжок
        if (master.isGrounded && InputManager.Instance.JumpPressed && !master.isSneaking && !master.isInWater)
            PlayJump();

        // Приземление
        if (!wasOnGround && master.isGrounded && landingCooldown <= 0f && !master.isInWater)
        {
            PlayLanding();
            landingCooldown = 0.2f;
        }

        // Шаги
        if (cam.JustStepped && landingCooldown <= 0f && master.isGrounded)
            PlayFootstep();

        // Вода
        if (!wasInWater && master.isInWater) OnEnterWater();
        if (wasInWater && !master.isInWater) OnExitWater();

        wasOnGround = master.isGrounded;
        wasInWater = master.isInWater;
    }

    void PlayFootstep()
    {
        if (AudioManager.Instance == null) return;

        FootBlocks blocks = FindBlocksUnderFeet();
        if (blocks.primary == 0) return;

        FootstepAction action = master.isSprinting ? FootstepAction.Run : (master.isSneaking ? FootstepAction.Sneak : FootstepAction.Walk);
        float stepVel = master.isSprinting ? 1.4f : (master.isSneaking ? 0.5f : 1.0f);

        AudioManager.Instance.PlayFootstepBlended(action, blocks.primary, blocks.secondary, blocks.blend, transform.position, stepVel);
    }

    void PlayJump()
    {
        if (AudioManager.Instance == null) return;
        FootBlocks blocks = FindBlocksUnderFeet();
        if (blocks.primary != 0)
            AudioManager.Instance.PlayFootstep(FootstepAction.Jump, blocks.primary, transform.position, 1.2f);

        if (PlayerVoice.Instance != null) PlayerVoice.Instance.OnJump();
    }

    void PlayLanding()
    {
        if (AudioManager.Instance == null) return;
        FootBlocks blocks = FindBlocksUnderFeet();
        if (blocks.primary != 0)
        {
            float landingVel = Mathf.Clamp(Mathf.Abs(master.fallVelocity) / 10f + 1.0f, 1.0f, 1.6f);
            AudioManager.Instance.PlayFootstep(FootstepAction.Drop, blocks.primary, transform.position, landingVel);
        }

        if (PlayerVoice.Instance != null) PlayerVoice.Instance.OnLanding(master.fallVelocity);
        master.fallVelocity = 0f;
    }

    void OnEnterWater()
    {
        master.velocity.y *= 0.3f;
        if (AudioManager.Instance != null && SoundBanks.ItemDrop != null)
            AudioManager.Instance.PlaySample3D(SoundBanks.ItemDrop.GetRandom(), transform.position, 0.7f, 0.7f, 1f, 30f, 0.8f);
    }

    void OnExitWater()
    {
        if (AudioManager.Instance != null && SoundBanks.ItemPickup != null)
            AudioManager.Instance.PlaySample3D(SoundBanks.ItemPickup.GetRandom(), transform.position, 0.5f, 1.2f, 1f, 20f, 0.8f);
    }

    FootBlocks FindBlocksUnderFeet()
    {
        FootBlocks result = new FootBlocks();
        float yBelow = transform.position.y - 0.1f;
        int fy = Mathf.FloorToInt(yBelow);
        float x = transform.position.x;
        float z = transform.position.z;

        int cx = Mathf.FloorToInt(x);
        int cz = Mathf.FloorToInt(z);
        ushort centerBlock = WorldManager.Instance.GetBlock(cx, fy, cz);

        float fracX = x - cx;
        float fracZ = z - cz;
        float distToEdgeX = Mathf.Min(fracX, 1f - fracX);
        float distToEdgeZ = Mathf.Min(fracZ, 1f - fracZ);
        float distToEdge = Mathf.Min(distToEdgeX, distToEdgeZ);

        float blendFactor = 0f;
        int dx = 0, dz = 0;

        if (distToEdge < 0.35f)
        {
            blendFactor = 1f - (distToEdge / 0.35f);
            if (distToEdgeX < distToEdgeZ) dx = fracX < 0.5f ? -1 : 1;
            else dz = fracZ < 0.5f ? -1 : 1;
        }

        ushort neighborBlock = (dx != 0 || dz != 0) ? WorldManager.Instance.GetBlock(cx + dx, fy, cz + dz) : (ushort)0;

        if (centerBlock != BlockIDs.Air && centerBlock != BlockIDs.Water)
        {
            result.primary = centerBlock;
            result.secondary = (neighborBlock != BlockIDs.Water) ? neighborBlock : (ushort)0;
            result.blend = blendFactor;
        }
        else if (neighborBlock != BlockIDs.Air && neighborBlock != BlockIDs.Water)
        {
            result.primary = neighborBlock;
            result.secondary = 0;
            result.blend = 0f;
        }

        return result;
    }
}