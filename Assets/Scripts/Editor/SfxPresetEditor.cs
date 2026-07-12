using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SfxPreset))]
public class SfxPresetEditor : Editor
{
    static GameObject previewObject;
    static AudioSource previewSource;

    SfxContext testContext = SfxContext.Default();
    bool showContext = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SfxPreset preset = (SfxPreset)target;

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("▶ Play", GUILayout.Height(32)))
        {
            preset.InvalidateCache();
            Play(preset.GetClip());
        }

        if (GUILayout.Button("▶ All Variants", GUILayout.Height(32)))
        {
            PlayAll(preset);
        }

        EditorGUILayout.EndHorizontal();

        showContext = EditorGUILayout.Foldout(showContext, "Test Context");
        if (showContext)
        {
            testContext.velocity = EditorGUILayout.Slider("Velocity", testContext.velocity, 0.1f, 2f);
            testContext.mass = EditorGUILayout.Slider("Mass", testContext.mass, 0.1f, 5f);
            testContext.wetness = EditorGUILayout.Slider("Wetness", testContext.wetness, 0f, 1f);
            testContext.room = (RoomType)EditorGUILayout.EnumPopup("Room", testContext.room);
            testContext.era = (TechEra)EditorGUILayout.EnumPopup("Era", testContext.era);
            testContext.surfaceMaterial = (MaterialProfile)EditorGUILayout.ObjectField(
                "Surface Material", testContext.surfaceMaterial, typeof(MaterialProfile), false);
            testContext.toolMaterial = (MaterialProfile)EditorGUILayout.ObjectField(
                "Tool Material", testContext.toolMaterial, typeof(MaterialProfile), false);

            if (GUILayout.Button("▶ Play with Context", GUILayout.Height(28)))
            {
                SfxParams p = preset.BuildParamsWithContext(0, testContext);
                AudioClip clip = ChipTuneSynth.Generate(p);
                Play(clip);
            }
        }

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Stop"))
            StopPreview();

        if (GUILayout.Button("Clear Cache"))
            preset.InvalidateCache();
    }

    static void EnsurePreview()
    {
        if (previewSource != null && previewSource.gameObject != null)
            return;

        previewObject = GameObject.Find("__SfxPreview");
        if (previewObject == null)
        {
            previewObject = new GameObject("__SfxPreview");
            previewObject.hideFlags = HideFlags.HideAndDontSave;
        }

        previewSource = previewObject.GetComponent<AudioSource>();
        if (previewSource == null)
            previewSource = previewObject.AddComponent<AudioSource>();

        previewSource.playOnAwake = false;
        previewSource.spatialBlend = 0f;
        previewSource.loop = false;
        previewSource.dopplerLevel = 0f;
    }

    static void Play(AudioClip clip)
    {
        EnsurePreview();
        if (previewSource == null || clip == null)
            return;

        previewSource.Stop();
        previewSource.clip = clip;
        previewSource.Play();
    }

    static async void PlayAll(SfxPreset preset)
    {
        EnsurePreview();
        if (previewSource == null || preset == null)
            return;

        int count = Mathf.Max(1, preset.variantCount);
        for (int i = 0; i < count; i++)
        {
            SfxParams p = preset.BuildParams(i);
            AudioClip c = ChipTuneSynth.Generate(p);
            previewSource.Stop();
            previewSource.clip = c;
            previewSource.Play();
            await System.Threading.Tasks.Task.Delay((int)((c.length + 0.12f) * 1000));
        }
    }

    static void StopPreview()
    {
        EnsurePreview();
        if (previewSource == null)
            return;

        previewSource.Stop();
        previewSource.clip = null;
    }
}