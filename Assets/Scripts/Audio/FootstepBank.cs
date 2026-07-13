using UnityEngine;
using System.Collections.Generic;

public enum FootstepAction
{
    Walk,
    Run,
    Sneak,
    Jump,
    Drop,
    Shuffle
}

public enum FootstepMaterial
{
    Grass,
    Dirt,
    Stone,
    Wood
}

public class FootstepBank
{
    private Dictionary<(FootstepAction, FootstepMaterial), SampleBank> banks = new();

    public FootstepBank(bool processClips = false)
    {
        LoadAll(FootstepAction.Walk, processClips);
        LoadAll(FootstepAction.Run, processClips);
        LoadAll(FootstepAction.Sneak, processClips);
        LoadAll(FootstepAction.Jump, processClips);
        LoadAll(FootstepAction.Drop, processClips);
        LoadAll(FootstepAction.Shuffle, processClips);
    }

    void LoadAll(FootstepAction action, bool processClips)
    {
        LoadBank(action, FootstepMaterial.Grass, processClips);
        LoadBank(action, FootstepMaterial.Dirt, processClips);
        LoadBank(action, FootstepMaterial.Stone, processClips);
        LoadBank(action, FootstepMaterial.Wood, processClips);
    }

    void LoadBank(FootstepAction action, FootstepMaterial material, bool processClips)
    {
        string path = $"Sounds/Footsteps/{action}/{material}";
        SampleBank bank = new SampleBank(path, false, processClips);
        banks[(action, material)] = bank;
    }

    public AudioClip GetRandom(FootstepAction action, FootstepMaterial material)
    {
        if (banks.TryGetValue((action, material), out SampleBank bank))
        {
            if (bank != null && !bank.IsEmpty)
                return bank.GetRandom();
        }

        if (action != FootstepAction.Walk)
        {
            if (banks.TryGetValue((FootstepAction.Walk, material), out SampleBank fallback))
            {
                if (fallback != null && !fallback.IsEmpty)
                    return fallback.GetRandom();
            }
        }

        return null;
    }

    public int TotalClips()
    {
        int total = 0;
        foreach (var kv in banks)
        {
            if (kv.Value != null) total += kv.Value.Count;
        }
        return total;
    }

    public void LogSummary()
    {
        foreach (var kv in banks)
        {
            int count = kv.Value != null ? kv.Value.Count : 0;
            if (count == 0)
                Debug.LogWarning($"FootstepBank empty: {kv.Key.Item1}/{kv.Key.Item2}");
        }
        Debug.Log($"FootstepBank loaded: {TotalClips()} total clips");
    }
}