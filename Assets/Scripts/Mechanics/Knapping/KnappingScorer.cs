using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class KnappingScorer : MonoBehaviour
{
    [Header("Template Protection")]
    [Tooltip("Если целостность шаблона падает ниже этого % — сразу BROKEN. 0.9 = 90% лезвия должно быть цело.")]
    [Range(0.5f, 1.0f)]
    public float minTemplateIntegrity = 0.90f;

    [Header("Grades (Integrity Thresholds)")]
    [Tooltip("Порог для Perfect (целостность шаблона >= %)")]
    [Range(0.5f, 1.0f)] public float perfectThreshold = 0.98f;
    [Tooltip("Порог для Good")]
    [Range(0.5f, 1.0f)] public float goodThreshold = 0.95f;
    [Tooltip("Порог для Average")]
    [Range(0.5f, 1.0f)] public float averageThreshold = 0.90f;

    private KnappingSession session;
    private int initialExcessVoxels = -1;
    private int hitNumber = 0;
    private List<string> hitLog = new List<string>();

    void Awake()
    {
        session = GetComponent<KnappingSession>();
    }

    public void RecordInitialState()
    {
        if (session.currentStone == null || session.goalBladeTemplate == null)
        {
            initialExcessVoxels = -1;
            return;
        }

        initialExcessVoxels = CountExcessVoxels();
        hitNumber = 0;
        hitLog.Clear();

        hitLog.Add($"=== KNAPPING SESSION START ===");
        hitLog.Add($"Excess to remove: {initialExcessVoxels} | Min Integrity: {minTemplateIntegrity:P0}");
        hitLog.Add($"---");
    }

    public void CheckCompletion()
    {
        if (session.currentStone == null || session.goalBladeTemplate == null) return;

        hitNumber++;
        int currentExcess = CountExcessVoxels();
        float integrity = GetTemplateIntegrity();

        hitLog.Add($"Hit {hitNumber}: excess left={currentExcess}, integrity={integrity:P0}");

        if (integrity < minTemplateIntegrity)
        {
            hitLog.Add($">>> BROKEN — integrity dropped to {integrity:P0}");
            PrintLog();
            session.ShowResult(KnappingResult.Broken, integrity);
            return;
        }

        if (currentExcess <= 0)
        {
            KnappingResult result;

            if (integrity >= perfectThreshold) result = KnappingResult.Perfect;
            else if (integrity >= goodThreshold) result = KnappingResult.Good;
            else if (integrity >= averageThreshold) result = KnappingResult.Average;
            else result = KnappingResult.Poor;

            hitLog.Add($">>> COMPLETE — Result: {result} (Integrity: {integrity:P0})");
            PrintLog();
            session.ShowResult(result, integrity);
        }
    }

    void PrintLog()
    {
        hitLog.Add($"=== KNAPPING SESSION END ===");
        StringBuilder sb = new StringBuilder();
        foreach (string line in hitLog) sb.AppendLine(line);
        Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", sb.ToString());
    }

    public float EvaluateAccuracy()
    {
        return GetTemplateIntegrity();
    }

    float GetTemplateIntegrity()
    {
        if (session.currentStone == null || session.goalBladeTemplate == null) return 0f;

        KnappingTemplate template = session.goalBladeTemplate;
        Vector3Int offset = GetTemplateOffset();

        int templateTotal = 0;
        int preserved = 0;

        for (int tx = 0; tx < template.width; tx++)
            for (int ty = 0; ty < template.height; ty++)
                for (int tz = 0; tz < template.depth; tz++)
                {
                    if (!template.GetVoxel(tx, ty, tz)) continue;
                    templateTotal++;

                    int sx = tx + offset.x;
                    int sy = ty + offset.y;
                    int sz = tz + offset.z;

                    if (sx >= 0 && sx < session.currentStone.Width &&
                        sy >= 0 && sy < session.currentStone.Height &&
                        sz >= 0 && sz < session.currentStone.Depth &&
                        session.currentStone.Voxels[sx, sy, sz])
                    {
                        preserved++;
                    }
                }

        return templateTotal == 0 ? 0f : (float)preserved / templateTotal;
    }

    int CountExcessVoxels()
    {
        KnappingTemplate template = session.goalBladeTemplate;
        Vector3Int offset = GetTemplateOffset();
        int excess = 0;

        for (int x = 0; x < session.currentStone.Width; x++)
            for (int y = 0; y < session.currentStone.Height; y++)
                for (int z = 0; z < session.currentStone.Depth; z++)
                {
                    if (!session.currentStone.Voxels[x, y, z]) continue;

                    int tx = x - offset.x;
                    int ty = y - offset.y;
                    int tz = z - offset.z;

                    bool insideTemplate = tx >= 0 && tx < template.width &&
                                          ty >= 0 && ty < template.height &&
                                          tz >= 0 && tz < template.depth &&
                                          template.GetVoxel(tx, ty, tz);
                    if (!insideTemplate) excess++;
                }
        return excess;
    }

    Vector3Int GetTemplateOffset()
    {
        return new Vector3Int(
            Mathf.RoundToInt((session.currentStone.Width - session.goalBladeTemplate.width) * 0.5f),
            Mathf.RoundToInt((session.currentStone.Height - session.goalBladeTemplate.height) * 0.5f),
            Mathf.RoundToInt((session.currentStone.Depth - session.goalBladeTemplate.depth) * 0.5f)
        );
    }
}