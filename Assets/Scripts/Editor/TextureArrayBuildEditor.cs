using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureArrayConfig))]
public class TextureArrayBuildEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Build Texture Array"))
        {
            TextureArrayConfig config = (TextureArrayConfig)target;
            config.Build();
            EditorUtility.SetDirty(config);
            Debug.Log("Texture Array built with " + config.textures.Length + " layers");
        }
    }
}