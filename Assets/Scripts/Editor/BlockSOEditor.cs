using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockSO))]
public class BlockSOEditor : Editor
{
    private Texture2D previewTop;
    private Texture2D previewSide;
    private Texture2D previewBottom;
    private bool previewDirty = true;

    void OnEnable() { previewDirty = true; }

    void OnDisable()
    {
        DestroyPreview(ref previewTop);
        DestroyPreview(ref previewSide);
        DestroyPreview(ref previewBottom);
    }

    private void DestroyPreview(ref Texture2D tex)
    {
        if (tex != null) { DestroyImmediate(tex); tex = null; }
    }

    public override void OnInspectorGUI()
    {
        BlockSO block = (BlockSO)target;
        serializedObject.Update();

        DrawProperty("id");
        DrawProperty("blockName");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Gameplay", EditorStyles.boldLabel);
        DrawProperty("hardness");
        DrawProperty("breakable");
        DrawProperty("dropId");
        DrawProperty("dropCount");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Mining Requirements", EditorStyles.boldLabel);
        DrawProperty("category");
        DrawProperty("requiredTool");
        DrawProperty("requiredTier");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Visual Type", EditorStyles.boldLabel);
        DrawProperty("isSolid");
        DrawProperty("isTransparent");
        DrawProperty("isCustomModel");
        if (block.isCustomModel)
            DrawProperty("customModelPrefab");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Palette Mode", EditorStyles.boldLabel);
        DrawProperty("usePalette");
        DrawProperty("useMultiFacePalette");

        if (block.usePalette && !block.useMultiFacePalette)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Single Face Palette", EditorStyles.boldLabel);
            DrawProperty("masterTexture");
            DrawProperty("palette");
        }

        if (block.useMultiFacePalette)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Multi Face Masters", EditorStyles.boldLabel);
            DrawProperty("masterTop");
            DrawProperty("masterSide");
            DrawProperty("masterBottom");

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Top Palette", EditorStyles.boldLabel);
            DrawProperty("paletteTop");

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Side Palette", EditorStyles.boldLabel);
            DrawProperty("paletteSide");

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Bottom Palette", EditorStyles.boldLabel);
            DrawProperty("paletteBottom");
        }

        if (!block.usePalette && !block.useMultiFacePalette)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Manual Textures", EditorStyles.boldLabel);
            DrawProperty("texTop");
            DrawProperty("texBottom");
            DrawProperty("texNorth");
            DrawProperty("texSouth");
            DrawProperty("texEast");
            DrawProperty("texWest");
        }

        if (serializedObject.ApplyModifiedProperties())
            previewDirty = true;

        if (block.useMultiFacePalette)
            DrawMultiFacePreview(block);
        else if (block.usePalette && block.masterTexture != null)
            DrawSingleFacePreview(block);
        else
            DrawManualTexturePreview(block);
    }

    private void DrawProperty(string name)
    {
        SerializedProperty prop = serializedObject.FindProperty(name);
        if (prop != null) EditorGUILayout.PropertyField(prop, true);
    }

    private void DrawSingleFacePreview(BlockSO block)
    {
        if (block.palette == null || block.palette.Length == 0) return;

        if (previewDirty)
        {
            DestroyPreview(ref previewTop);
            EnsureReadable(block.masterTexture);
            previewTop = GeneratePreview(block.masterTexture, block.palette);
            previewDirty = false;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Palette Preview", EditorStyles.boldLabel);
        if (previewTop != null) DrawCenteredPreview(previewTop, 128);
        DrawSwatches(block.palette);
        DrawUtilityButtons(block);
    }

    private void DrawMultiFacePreview(BlockSO block)
    {
        if (previewDirty)
        {
            DestroyPreview(ref previewTop);
            DestroyPreview(ref previewSide);
            DestroyPreview(ref previewBottom);

            if (block.masterTop != null && block.paletteTop != null && block.paletteTop.Length > 0)
            {
                EnsureReadable(block.masterTop);
                previewTop = GeneratePreview(block.masterTop, block.paletteTop);
            }
            if (block.masterSide != null && block.paletteSide != null && block.paletteSide.Length > 0)
            {
                EnsureReadable(block.masterSide);
                previewSide = GeneratePreview(block.masterSide, block.paletteSide);
            }
            if (block.masterBottom != null && block.paletteBottom != null && block.paletteBottom.Length > 0)
            {
                EnsureReadable(block.masterBottom);
                previewBottom = GeneratePreview(block.masterBottom, block.paletteBottom);
            }
            previewDirty = false;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Multi-Face Preview", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (previewTop != null) DrawLabeledPreview(previewTop, "Top", 80);
        GUILayout.Space(8);
        if (previewSide != null) DrawLabeledPreview(previewSide, "Side", 80);
        GUILayout.Space(8);
        if (previewBottom != null) DrawLabeledPreview(previewBottom, "Bottom", 80);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        DrawUtilityButtons(block);
    }

    private void DrawManualTexturePreview(BlockSO block)
    {
        Texture2D tex = block.texTop ?? block.texNorth ?? block.texBottom;
        if (tex == null) return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Texture Preview", EditorStyles.boldLabel);
        DrawCenteredPreview(tex, 128);
    }

    private void DrawCenteredPreview(Texture2D tex, int size)
    {
        Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
        rect.x = (EditorGUIUtility.currentViewWidth - size) * 0.5f;
        EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, size + 4, size + 4), new Color(0.15f, 0.15f, 0.15f));
        EditorGUI.DrawPreviewTexture(rect, tex, null, ScaleMode.ScaleToFit);
    }

    private void DrawLabeledPreview(Texture2D tex, string label, int size)
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(size));
        Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size));
        EditorGUI.DrawRect(new Rect(rect.x - 1, rect.y - 1, size + 2, size + 2), new Color(0.15f, 0.15f, 0.15f));
        EditorGUI.DrawPreviewTexture(rect, tex, null, ScaleMode.ScaleToFit);
        EditorGUILayout.EndVertical();
    }

    private void DrawSwatches(Color[] colors)
    {
        if (colors == null || colors.Length == 0) return;

        EditorGUILayout.Space(5);
        int totalWidth = colors.Length * 30;
        Rect rect = GUILayoutUtility.GetRect(totalWidth, 24, GUILayout.ExpandWidth(false));
        rect.x = (EditorGUIUtility.currentViewWidth - totalWidth) * 0.5f;

        for (int i = 0; i < colors.Length; i++)
        {
            Rect swatch = new Rect(rect.x + i * 30, rect.y, 30, 24);
            EditorGUI.DrawRect(swatch, colors[i]);
        }
    }

    private void DrawUtilityButtons(BlockSO block)
    {
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Randomize Palette Variation"))
            RandomizePaletteVariation(block);
    }

    private Texture2D GeneratePreview(Texture2D master, Color[] palette)
    {
        int paletteSize = palette.Length;
        Texture2D preview = new Texture2D(master.width, master.height, TextureFormat.RGBA32, false);
        preview.filterMode = FilterMode.Point;

        Color[] pixels = master.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.01f)
            {
                pixels[i] = new Color(0f, 0f, 0f, 0f);
                continue;
            }

            float brightness = pixels[i].r;
            int index = Mathf.FloorToInt(brightness * paletteSize);
            index = Mathf.Clamp(index, 0, paletteSize - 1);

            Color swapped = palette[index];
            swapped.a = pixels[i].a;
            pixels[i] = swapped;
        }

        preview.SetPixels(pixels);
        preview.Apply();
        return preview;
    }

    private void RandomizePaletteVariation(BlockSO block)
    {
        Undo.RecordObject(block, "Randomize Palette");

        float hueShift = Random.Range(-0.05f, 0.05f);
        float satShift = Random.Range(-0.1f, 0.1f);
        float valShift = Random.Range(-0.08f, 0.08f);

        if (block.palette != null)
            for (int i = 0; i < block.palette.Length; i++)
                block.palette[i] = ShiftColor(block.palette[i], hueShift, satShift, valShift);

        if (block.useMultiFacePalette)
        {
            if (block.paletteTop != null)
                for (int i = 0; i < block.paletteTop.Length; i++)
                    block.paletteTop[i] = ShiftColor(block.paletteTop[i], hueShift, satShift, valShift);
            if (block.paletteSide != null)
                for (int i = 0; i < block.paletteSide.Length; i++)
                    block.paletteSide[i] = ShiftColor(block.paletteSide[i], hueShift, satShift, valShift);
            if (block.paletteBottom != null)
                for (int i = 0; i < block.paletteBottom.Length; i++)
                    block.paletteBottom[i] = ShiftColor(block.paletteBottom[i], hueShift, satShift, valShift);
        }

        previewDirty = true;
        EditorUtility.SetDirty(block);
    }

    private Color ShiftColor(Color color, float hueShift, float satShift, float valShift)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        h = Mathf.Repeat(h + hueShift, 1f);
        s = Mathf.Clamp01(s + satShift);
        v = Mathf.Clamp01(v + valShift);
        return Color.HSVToRGB(h, s, v);
    }

    private void EnsureReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}