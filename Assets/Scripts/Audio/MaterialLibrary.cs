/*using UnityEngine;

public static class MaterialLibrary
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/SFX/Regenerate Materials")]
    static void MenuRegenerate()
    {
        CreateAll();
        Debug.Log("[MAT] All material profiles regenerated.");
    }
#endif

    static void CreateAll()
    {
        string folder = "Resources";

        Apply(LoadOrCreate(folder, "Flint"), Flint());
        Apply(LoadOrCreate(folder, "Granite"), Granite());
        Apply(LoadOrCreate(folder, "Limestone"), Limestone());
        Apply(LoadOrCreate(folder, "Sandstone"), Sandstone());
        Apply(LoadOrCreate(folder, "Clay"), Clay());
        Apply(LoadOrCreate(folder, "Dirt"), Dirt());
        Apply(LoadOrCreate(folder, "Sand"), Sand());
        Apply(LoadOrCreate(folder, "Gravel"), Gravel());
        Apply(LoadOrCreate(folder, "Oak"), Oak());
        Apply(LoadOrCreate(folder, "Birch"), Birch());
        Apply(LoadOrCreate(folder, "Pine"), Pine());
        Apply(LoadOrCreate(folder, "Bamboo"), Bamboo());
        Apply(LoadOrCreate(folder, "Grass"), Grass());
        Apply(LoadOrCreate(folder, "Leaf"), Leaf());
        Apply(LoadOrCreate(folder, "Bronze"), Bronze());
        Apply(LoadOrCreate(folder, "Iron"), Iron());
        Apply(LoadOrCreate(folder, "Steel"), Steel());
        Apply(LoadOrCreate(folder, "Gold"), Gold());
        Apply(LoadOrCreate(folder, "Copper"), Copper());
        Apply(LoadOrCreate(folder, "Tin"), Tin());
        Apply(LoadOrCreate(folder, "Glass"), Glass());
        Apply(LoadOrCreate(folder, "Brick"), Brick());
        Apply(LoadOrCreate(folder, "Concrete"), Concrete());
        Apply(LoadOrCreate(folder, "Plastic"), Plastic());
        Apply(LoadOrCreate(folder, "Rubber"), Rubber());
        Apply(LoadOrCreate(folder, "Bone"), Bone());
        Apply(LoadOrCreate(folder, "Leather"), Leather());
        Apply(LoadOrCreate(folder, "Fabric"), Fabric());
        Apply(LoadOrCreate(folder, "Ice"), Ice());
        Apply(LoadOrCreate(folder, "Snow"), Snow());
        Apply(LoadOrCreate(folder, "Titanium"), Titanium());
        Apply(LoadOrCreate(folder, "CarbonFiber"), CarbonFiber());
        Apply(LoadOrCreate(folder, "Ceramic"), Ceramic());
    }

    static MaterialProfile LoadOrCreate(string folder, string profileName)
    {
        MaterialProfile profile = Resources.Load<MaterialProfile>($"{folder}/{profileName}");

#if UNITY_EDITOR
        if (profile == null)
        {
            string dir = $"Assets/Data/{folder}";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            profile = ScriptableObject.CreateInstance<MaterialProfile>();
            UnityEditor.AssetDatabase.CreateAsset(profile, $"{dir}/{profileName}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif

        return profile;
    }

    static void Apply(MaterialProfile p, MatCfg c)
    {
        if (p == null) return;
        p.hardness = c.hardness;
        p.density = c.density;
        p.brittleness = c.brittleness;
        p.roughness = c.roughness;
        p.resonance = c.resonance;
        p.brightness = c.brightness;
        p.hollowness = c.hollowness;
        p.graininess = c.graininess;
        p.wetness = c.wetness;
        p.metallicity = c.metallicity;
        p.warmth = c.warmth;
        p.baseFreq = c.baseFreq;
        p.partialRatios = c.partialRatios;
        p.partialDecayBase = c.partialDecayBase;
        p.primaryNoise = c.primaryNoise;
        p.secondaryNoise = c.secondaryNoise;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(p);
#endif
    }

    struct MatCfg
    {
        public float hardness, density, brittleness, roughness, resonance;
        public float brightness, hollowness, graininess, wetness, metallicity, warmth;
        public float baseFreq;
        public float[] partialRatios;
        public float partialDecayBase;
        public NoiseColor primaryNoise, secondaryNoise;
    }

    static MatCfg Flint() => new MatCfg
    {
        hardness = 0.9f,
        density = 0.7f,
        brittleness = 0.85f,
        roughness = 0.3f,
        resonance = 0.6f,
        brightness = 0.8f,
        hollowness = 0f,
        graininess = 0.2f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.2f,
        baseFreq = 320f,
        partialRatios = new[] { 2.76f, 4.93f, 7.81f, 11.3f, 14.7f },
        partialDecayBase = 15f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Granite() => new MatCfg
    {
        hardness = 0.95f,
        density = 0.85f,
        brittleness = 0.4f,
        roughness = 0.5f,
        resonance = 0.45f,
        brightness = 0.6f,
        hollowness = 0f,
        graininess = 0.4f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.3f,
        baseFreq = 280f,
        partialRatios = new[] { 3.17f, 5.83f, 9.41f, 12.7f, 16.3f },
        partialDecayBase = 18f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Limestone() => new MatCfg
    {
        hardness = 0.5f,
        density = 0.6f,
        brittleness = 0.6f,
        roughness = 0.4f,
        resonance = 0.35f,
        brightness = 0.4f,
        hollowness = 0.05f,
        graininess = 0.3f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.5f,
        baseFreq = 200f,
        partialRatios = new[] { 2.3f, 4.1f, 6.7f, 9.5f, 13.1f },
        partialDecayBase = 12f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Sandstone() => new MatCfg
    {
        hardness = 0.4f,
        density = 0.5f,
        brittleness = 0.7f,
        roughness = 0.7f,
        resonance = 0.25f,
        brightness = 0.3f,
        hollowness = 0.1f,
        graininess = 0.7f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.5f,
        baseFreq = 160f,
        partialRatios = new[] { 2.5f, 4.3f, 7.1f, 10.2f, 13.8f },
        partialDecayBase = 10f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Clay() => new MatCfg
    {
        hardness = 0.2f,
        density = 0.55f,
        brittleness = 0.15f,
        roughness = 0.3f,
        resonance = 0.2f,
        brightness = 0.2f,
        hollowness = 0f,
        graininess = 0.15f,
        wetness = 0.3f,
        metallicity = 0f,
        warmth = 0.7f,
        baseFreq = 120f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 8f,
        primaryNoise = NoiseColor.Brown,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Dirt() => new MatCfg
    {
        hardness = 0.15f,
        density = 0.45f,
        brittleness = 0.1f,
        roughness = 0.5f,
        resonance = 0.1f,
        brightness = 0.15f,
        hollowness = 0f,
        graininess = 0.5f,
        wetness = 0.1f,
        metallicity = 0f,
        warmth = 0.6f,
        baseFreq = 90f,
        partialRatios = new[] { 2.4f, 4.7f, 7.2f, 10.1f, 13.5f },
        partialDecayBase = 8f,
        primaryNoise = NoiseColor.Brown,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Sand() => new MatCfg
    {
        hardness = 0.1f,
        density = 0.4f,
        brittleness = 0.05f,
        roughness = 0.6f,
        resonance = 0.05f,
        brightness = 0.35f,
        hollowness = 0f,
        graininess = 0.9f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.5f,
        baseFreq = 150f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 6f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.White
    };

    static MatCfg Gravel() => new MatCfg
    {
        hardness = 0.6f,
        density = 0.55f,
        brittleness = 0.5f,
        roughness = 0.8f,
        resonance = 0.15f,
        brightness = 0.4f,
        hollowness = 0f,
        graininess = 0.85f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.3f,
        baseFreq = 220f,
        partialRatios = new[] { 2.7f, 5.1f, 8.3f, 11.7f, 15.2f },
        partialDecayBase = 12f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Oak() => new MatCfg
    {
        hardness = 0.5f,
        density = 0.5f,
        brittleness = 0.2f,
        roughness = 0.3f,
        resonance = 0.7f,
        brightness = 0.4f,
        hollowness = 0.15f,
        graininess = 0.2f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.8f,
        baseFreq = 175f,
        partialRatios = new[] { 1.58f, 2.51f, 3.89f, 5.32f, 7.1f },
        partialDecayBase = 10f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Birch() => new MatCfg
    {
        hardness = 0.45f,
        density = 0.42f,
        brittleness = 0.25f,
        roughness = 0.2f,
        resonance = 0.75f,
        brightness = 0.55f,
        hollowness = 0.2f,
        graininess = 0.15f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.75f,
        baseFreq = 200f,
        partialRatios = new[] { 1.52f, 2.41f, 3.71f, 5.1f, 6.8f },
        partialDecayBase = 9f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Pine() => new MatCfg
    {
        hardness = 0.3f,
        density = 0.35f,
        brittleness = 0.3f,
        roughness = 0.25f,
        resonance = 0.65f,
        brightness = 0.35f,
        hollowness = 0.25f,
        graininess = 0.2f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.85f,
        baseFreq = 150f,
        partialRatios = new[] { 1.6f, 2.6f, 4.0f, 5.5f, 7.3f },
        partialDecayBase = 8f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Bamboo() => new MatCfg
    {
        hardness = 0.55f,
        density = 0.3f,
        brittleness = 0.35f,
        roughness = 0.2f,
        resonance = 0.85f,
        brightness = 0.6f,
        hollowness = 0.6f,
        graininess = 0.1f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.6f,
        baseFreq = 300f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.5f, 7.0f },
        partialDecayBase = 7f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.White
    };

    static MatCfg Grass() => new MatCfg
    {
        hardness = 0.02f,
        density = 0.1f,
        brittleness = 0.01f,
        roughness = 0.4f,
        resonance = 0.02f,
        brightness = 0.5f,
        hollowness = 0f,
        graininess = 0.6f,
        wetness = 0.1f,
        metallicity = 0f,
        warmth = 0.6f,
        baseFreq = 220f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 5f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.White
    };

    static MatCfg Leaf() => new MatCfg
    {
        hardness = 0.01f,
        density = 0.05f,
        brittleness = 0.3f,
        roughness = 0.3f,
        resonance = 0.01f,
        brightness = 0.6f,
        hollowness = 0f,
        graininess = 0.5f,
        wetness = 0.05f,
        metallicity = 0f,
        warmth = 0.5f,
        baseFreq = 300f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 4f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.White
    };

    static MatCfg Bronze() => new MatCfg
    {
        hardness = 0.65f,
        density = 0.75f,
        brittleness = 0.3f,
        roughness = 0.25f,
        resonance = 0.8f,
        brightness = 0.65f,
        hollowness = 0f,
        graininess = 0.1f,
        wetness = 0f,
        metallicity = 0.8f,
        warmth = 0.5f,
        baseFreq = 320f,
        partialRatios = new[] { 2.76f, 5.41f, 8.93f, 12.7f, 16.2f },
        partialDecayBase = 6f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Iron() => new MatCfg
    {
        hardness = 0.8f,
        density = 0.82f,
        brittleness = 0.2f,
        roughness = 0.3f,
        resonance = 0.75f,
        brightness = 0.55f,
        hollowness = 0f,
        graininess = 0.1f,
        wetness = 0f,
        metallicity = 0.85f,
        warmth = 0.35f,
        baseFreq = 350f,
        partialRatios = new[] { 2.83f, 5.47f, 8.91f, 12.3f, 16.1f },
        partialDecayBase = 7f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Steel() => new MatCfg
    {
        hardness = 0.9f,
        density = 0.85f,
        brittleness = 0.15f,
        roughness = 0.2f,
        resonance = 0.85f,
        brightness = 0.7f,
        hollowness = 0f,
        graininess = 0.05f,
        wetness = 0f,
        metallicity = 0.95f,
        warmth = 0.25f,
        baseFreq = 420f,
        partialRatios = new[] { 2.91f, 5.39f, 8.72f, 12.1f, 15.9f },
        partialDecayBase = 5f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Gold() => new MatCfg
    {
        hardness = 0.3f,
        density = 0.95f,
        brittleness = 0.05f,
        roughness = 0.1f,
        resonance = 0.7f,
        brightness = 0.5f,
        hollowness = 0f,
        graininess = 0.02f,
        wetness = 0f,
        metallicity = 0.9f,
        warmth = 0.7f,
        baseFreq = 250f,
        partialRatios = new[] { 2.5f, 4.8f, 7.3f, 10.1f, 13.2f },
        partialDecayBase = 4f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.White
    };

    static MatCfg Copper() => new MatCfg
    {
        hardness = 0.45f,
        density = 0.78f,
        brittleness = 0.1f,
        roughness = 0.2f,
        resonance = 0.8f,
        brightness = 0.6f,
        hollowness = 0f,
        graininess = 0.05f,
        wetness = 0f,
        metallicity = 0.85f,
        warmth = 0.6f,
        baseFreq = 300f,
        partialRatios = new[] { 2.71f, 5.13f, 8.47f, 12.1f, 15.8f },
        partialDecayBase = 5f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Tin() => new MatCfg
    {
        hardness = 0.25f,
        density = 0.65f,
        brittleness = 0.15f,
        roughness = 0.15f,
        resonance = 0.6f,
        brightness = 0.45f,
        hollowness = 0f,
        graininess = 0.05f,
        wetness = 0f,
        metallicity = 0.7f,
        warmth = 0.5f,
        baseFreq = 280f,
        partialRatios = new[] { 2.6f, 5.0f, 8.1f, 11.5f, 14.9f },
        partialDecayBase = 5f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Glass() => new MatCfg
    {
        hardness = 0.7f,
        density = 0.6f,
        brittleness = 0.95f,
        roughness = 0.05f,
        resonance = 0.9f,
        brightness = 0.95f,
        hollowness = 0.1f,
        graininess = 0.02f,
        wetness = 0f,
        metallicity = 0.1f,
        warmth = 0.15f,
        baseFreq = 600f,
        partialRatios = new[] { 2.83f, 5.47f, 9.11f, 13.2f, 17.4f },
        partialDecayBase = 6f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Brick() => new MatCfg
    {
        hardness = 0.6f,
        density = 0.6f,
        brittleness = 0.5f,
        roughness = 0.6f,
        resonance = 0.3f,
        brightness = 0.35f,
        hollowness = 0.05f,
        graininess = 0.35f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.5f,
        baseFreq = 180f,
        partialRatios = new[] { 2.5f, 4.3f, 7.1f, 10.2f, 13.8f },
        partialDecayBase = 13f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Concrete() => new MatCfg
    {
        hardness = 0.75f,
        density = 0.7f,
        brittleness = 0.4f,
        roughness = 0.5f,
        resonance = 0.2f,
        brightness = 0.3f,
        hollowness = 0f,
        graininess = 0.4f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.3f,
        baseFreq = 160f,
        partialRatios = new[] { 2.7f, 5.1f, 8.3f, 11.7f, 15.2f },
        partialDecayBase = 15f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Plastic() => new MatCfg
    {
        hardness = 0.4f,
        density = 0.3f,
        brittleness = 0.2f,
        roughness = 0.15f,
        resonance = 0.35f,
        brightness = 0.5f,
        hollowness = 0.2f,
        graininess = 0.05f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.4f,
        baseFreq = 250f,
        partialRatios = new[] { 2.0f, 3.5f, 5.0f, 7.0f, 9.0f },
        partialDecayBase = 10f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.White
    };

    static MatCfg Rubber() => new MatCfg
    {
        hardness = 0.15f,
        density = 0.35f,
        brittleness = 0.02f,
        roughness = 0.5f,
        resonance = 0.1f,
        brightness = 0.15f,
        hollowness = 0f,
        graininess = 0.1f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.6f,
        baseFreq = 80f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 6f,
        primaryNoise = NoiseColor.Brown,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Bone() => new MatCfg
    {
        hardness = 0.65f,
        density = 0.55f,
        brittleness = 0.5f,
        roughness = 0.3f,
        resonance = 0.55f,
        brightness = 0.5f,
        hollowness = 0.15f,
        graininess = 0.15f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.4f,
        baseFreq = 250f,
        partialRatios = new[] { 2.3f, 4.1f, 6.5f, 9.2f, 12.4f },
        partialDecayBase = 10f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Leather() => new MatCfg
    {
        hardness = 0.2f,
        density = 0.35f,
        brittleness = 0.05f,
        roughness = 0.4f,
        resonance = 0.15f,
        brightness = 0.25f,
        hollowness = 0f,
        graininess = 0.2f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.7f,
        baseFreq = 100f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 6f,
        primaryNoise = NoiseColor.Brown,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Fabric() => new MatCfg
    {
        hardness = 0.05f,
        density = 0.15f,
        brittleness = 0.02f,
        roughness = 0.5f,
        resonance = 0.03f,
        brightness = 0.3f,
        hollowness = 0f,
        graininess = 0.4f,
        wetness = 0f,
        metallicity = 0f,
        warmth = 0.65f,
        baseFreq = 150f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 4f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.Brown
    };

    static MatCfg Ice() => new MatCfg
    {
        hardness = 0.6f,
        density = 0.5f,
        brittleness = 0.8f,
        roughness = 0.1f,
        resonance = 0.7f,
        brightness = 0.85f,
        hollowness = 0.05f,
        graininess = 0.1f,
        wetness = 0.2f,
        metallicity = 0.05f,
        warmth = 0.1f,
        baseFreq = 400f,
        partialRatios = new[] { 2.5f, 4.8f, 7.9f, 11.3f, 15.1f },
        partialDecayBase = 8f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Snow() => new MatCfg
    {
        hardness = 0.05f,
        density = 0.15f,
        brittleness = 0.1f,
        roughness = 0.3f,
        resonance = 0.02f,
        brightness = 0.2f,
        hollowness = 0f,
        graininess = 0.6f,
        wetness = 0.15f,
        metallicity = 0f,
        warmth = 0.3f,
        baseFreq = 80f,
        partialRatios = new[] { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f },
        partialDecayBase = 5f,
        primaryNoise = NoiseColor.Brown,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg Titanium() => new MatCfg
    {
        hardness = 0.92f,
        density = 0.6f,
        brittleness = 0.1f,
        roughness = 0.1f,
        resonance = 0.9f,
        brightness = 0.8f,
        hollowness = 0f,
        graininess = 0.03f,
        wetness = 0f,
        metallicity = 0.95f,
        warmth = 0.2f,
        baseFreq = 500f,
        partialRatios = new[] { 2.91f, 5.47f, 9.03f, 12.8f, 16.5f },
        partialDecayBase = 4f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };

    static MatCfg CarbonFiber() => new MatCfg
    {
        hardness = 0.85f,
        density = 0.25f,
        brittleness = 0.6f,
        roughness = 0.1f,
        resonance = 0.5f,
        brightness = 0.6f,
        hollowness = 0.1f,
        graininess = 0.05f,
        wetness = 0f,
        metallicity = 0.1f,
        warmth = 0.25f,
        baseFreq = 380f,
        partialRatios = new[] { 2.3f, 4.5f, 7.1f, 10.2f, 13.8f },
        partialDecayBase = 8f,
        primaryNoise = NoiseColor.Pink,
        secondaryNoise = NoiseColor.White
    };

    static MatCfg Ceramic() => new MatCfg
    {
        hardness = 0.7f,
        density = 0.55f,
        brittleness = 0.75f,
        roughness = 0.15f,
        resonance = 0.65f,
        brightness = 0.7f,
        hollowness = 0.2f,
        graininess = 0.1f,
        wetness = 0f,
        metallicity = 0.05f,
        warmth = 0.35f,
        baseFreq = 350f,
        partialRatios = new[] { 2.71f, 5.13f, 8.47f, 12.1f, 15.8f },
        partialDecayBase = 7f,
        primaryNoise = NoiseColor.White,
        secondaryNoise = NoiseColor.Pink
    };
}*/