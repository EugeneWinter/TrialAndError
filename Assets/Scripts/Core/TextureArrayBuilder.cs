using UnityEngine;

[CreateAssetMenu(fileName = "TextureArray", menuName = "Game Data/Texture Array Config")]
public class TextureArrayConfig : ScriptableObject
{
    public Texture2D[] textures;
    public Texture2DArray builtArray;

    public void Build()
    {
        if (textures == null || textures.Length == 0) return;

        int size = textures[0].width;
        builtArray = new Texture2DArray(size, size, textures.Length, TextureFormat.RGBA32, false);
        builtArray.filterMode = FilterMode.Point;
        builtArray.wrapMode = TextureWrapMode.Repeat;

        for (int i = 0; i < textures.Length; i++)
        {
            builtArray.SetPixels(textures[i].GetPixels(), i);
        }
        builtArray.Apply();
    }
}