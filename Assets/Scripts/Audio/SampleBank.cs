using UnityEngine;

public class SampleBank
{
    private AudioClip[] clips;
    private int lastPlayedIndex = -1;
    private bool singleMode;

    public SampleBank(string resourceFolder, bool useOnlyFirst = false, bool processClips = true)
    {
        AudioClip[] loaded = Resources.LoadAll<AudioClip>(resourceFolder);
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning($"SampleBank: no clips found in Resources/{resourceFolder}");
            clips = new AudioClip[0];
            return;
        }

        if (processClips)
        {
            clips = new AudioClip[loaded.Length];
            for (int i = 0; i < loaded.Length; i++)
                clips[i] = AudioProcessor.Process(loaded[i]);
        }
        else
        {
            clips = loaded;
        }

        singleMode = useOnlyFirst;
    }

    public bool IsEmpty => clips == null || clips.Length == 0;
    public int Count => clips == null ? 0 : clips.Length;

    public AudioClip GetRandom()
    {
        if (IsEmpty) return null;
        if (singleMode || clips.Length == 1) return clips[0];

        int index;
        int attempts = 0;
        do
        {
            index = Random.Range(0, clips.Length);
            attempts++;
        }
        while (index == lastPlayedIndex && attempts < 5);

        lastPlayedIndex = index;
        return clips[index];
    }
}