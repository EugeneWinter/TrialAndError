using UnityEngine;

[CreateAssetMenu(fileName = "New Sound", menuName = "Game Data/Sound")]
public class SoundSO : ScriptableObject
{
    public string soundName;
    public AudioClip[] clips;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float minPitch = 0.9f;
    [Range(0.5f, 2f)] public float maxPitch = 1.1f;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetRandomPitch()
    {
        return Random.Range(minPitch, maxPitch);
    }
}