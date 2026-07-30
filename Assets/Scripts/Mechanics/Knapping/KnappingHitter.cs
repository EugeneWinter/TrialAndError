using UnityEngine;
using System.Collections;

public class KnappingHitter : MonoBehaviour
{
    [Header("Shake")]
    public float shakeMagnitude = 0.02f;
    public float shakeDecay = 10f;

    [Header("Hit Stop")]
    public float hitStopDuration = 0.03f;

    [Header("Particles")]
    public KnappingParticles particles;
    public Color stoneParticleColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("Indicator")]
    public KnappingIndicator indicator;

    private KnappingSession session;
    private KnappingCameraController cam;
    private Vector3 shakeOffset;
    private Vector3 pivotBasePos;
    private bool initialized = false;

    private Vector3Int lastHitVoxel = new Vector3Int(-1, -1, -1);
    private Vector3 lastHitWorldPoint;
    private Vector3Int lastHitNormal = Vector3Int.zero;

    public void Begin()
    {
        session = KnappingSession.Instance;
        cam = session.GetComponent<KnappingCameraController>();
        shakeOffset = Vector3.zero;
        pivotBasePos = session.stonePivot.localPosition;
        lastHitVoxel = new Vector3Int(-1, -1, -1);
        lastHitWorldPoint = Vector3.zero;
        lastHitNormal = Vector3Int.zero;
        initialized = true;
    }

    public void End()
    {
        initialized = false;

        if (indicator != null)
        {
            indicator.HideCursor();
            indicator.HidePower();
            indicator.HideAngle();
        }

        lastHitVoxel = new Vector3Int(-1, -1, -1);
        lastHitNormal = Vector3Int.zero;
    }

    public void Tick()
    {
        if (!initialized || session == null || session.currentStone == null || cam == null) return;

        UpdatePreview();
        UpdateInput();
        UpdateShake();
    }

    void UpdatePreview()
    {
        if (indicator == null) return;

        Ray ray = cam.GetCursorRay();
        KnappingHit hit = KnappingRaycaster.Cast(session.currentStone, ray);

        if (!hit.hit)
        {
            indicator.HideCursor();
            indicator.HidePower();
            indicator.HideAngle();
            lastHitVoxel = new Vector3Int(-1, -1, -1);
            lastHitNormal = Vector3Int.zero;
            return;
        }

        Vector3 worldNormal = session.currentStone.transform.TransformDirection(
            new Vector3(hit.normal.x, hit.normal.y, hit.normal.z));

        indicator.ShowCursor(hit.worldPoint, worldNormal);
        indicator.HidePower();
        indicator.HideAngle();

        lastHitVoxel = hit.voxel;
        lastHitWorldPoint = hit.worldPoint;
        lastHitNormal = hit.normal;
    }

    void UpdateInput()
    {
        if (InputManager.Instance.KnappingStrikePressed)
            ExecuteHit();
    }

    void ExecuteHit()
    {
        if (session.currentStone == null) return;
        if (lastHitVoxel.x < 0) return;

        Vector3 worldPoint = lastHitWorldPoint;

        session.currentStone.RemoveVoxel(lastHitVoxel.x, lastHitVoxel.y, lastHitVoxel.z);
        int fallen = session.currentStone.RemoveDisconnected();

        if (particles != null)
        {
            Vector3 normal = new Vector3(lastHitNormal.x, lastHitNormal.y, lastHitNormal.z);
            if (normal.sqrMagnitude < 0.001f) normal = Vector3.up;
            particles.Burst(worldPoint, normal.normalized, stoneParticleColor);

            if (fallen > 0)
            {
                Vector3 downPoint = worldPoint + Vector3.down * session.currentStone.VoxelSize;
                particles.Burst(downPoint, Vector3.down, stoneParticleColor);
            }
        }

        shakeOffset = Random.insideUnitSphere * shakeMagnitude;
        PlayHitSound(worldPoint);
        StartCoroutine(HitStop());

        session.OnHitComplete();
    }

    IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }

    void PlayHitSound(Vector3 worldPos)
    {
        if (AudioManager.Instance == null) return;

        AudioClip clip = SoundBanks.BlockHitWood.GetRandom();
        if (clip == null) return;

        float pitch = Random.Range(0.9f, 1.1f);
        AudioManager.Instance.PlaySample3D(clip, worldPos, 0.7f, pitch, 0.5f, 15f, 0.7f);
    }

    void UpdateShake()
    {
        if (session == null || session.stonePivot == null) return;

        shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, shakeDecay * Time.deltaTime);
        session.stonePivot.localPosition = pivotBasePos + shakeOffset;
    }
}