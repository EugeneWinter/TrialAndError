/*using UnityEngine;

public static class SfxLibrary
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/SFX/Regenerate All Presets")]
    static void MenuRegenerate()
    {
        CreateAll();
        Debug.Log("[SFX] All presets regenerated.");
    }
#endif

    static void CreateAll()
    {
        string folder = "Resources";

        Apply(LoadOrCreate(folder, "FootstepGrass"), FootstepGrass());
        Apply(LoadOrCreate(folder, "FootstepDirt"), FootstepDirt());
        Apply(LoadOrCreate(folder, "FootstepStone"), FootstepStone());
        Apply(LoadOrCreate(folder, "FootstepWood"), FootstepWood());
        Apply(LoadOrCreate(folder, "FootstepSand"), FootstepSand());
        Apply(LoadOrCreate(folder, "FootstepMetal"), FootstepMetal());
        Apply(LoadOrCreate(folder, "FootstepWater"), FootstepWater());
        Apply(LoadOrCreate(folder, "FootstepSnow"), FootstepSnow());

        Apply(LoadOrCreate(folder, "BlockBreak"), BlockBreak());
        Apply(LoadOrCreate(folder, "BlockPlace"), BlockPlace());
        Apply(LoadOrCreate(folder, "BlockDig"), BlockDig());

        Apply(LoadOrCreate(folder, "ItemPickup"), ItemPickup());
        Apply(LoadOrCreate(folder, "ItemDrop"), ItemDrop());
        Apply(LoadOrCreate(folder, "ItemEquip"), ItemEquip());

        Apply(LoadOrCreate(folder, "KnappingHit"), KnappingHit());
        Apply(LoadOrCreate(folder, "KnappingSuccess"), KnappingSuccess());
        Apply(LoadOrCreate(folder, "KnappingFail"), KnappingFail());
    }

    static SfxPreset LoadOrCreate(string folder, string presetName)
    {
        SfxPreset preset = Resources.Load<SfxPreset>($"{folder}/{presetName}");

#if UNITY_EDITOR
        if (preset == null)
        {
            string dir = $"Assets/Data/{folder}";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            preset = ScriptableObject.CreateInstance<SfxPreset>();
            UnityEditor.AssetDatabase.CreateAsset(preset, $"{dir}/{presetName}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif

        return preset;
    }

    struct Cfg
    {
        public float duration, volume;
        public float clickLevel, clickFreq, clickDecay;
        public float thumpFreq, thumpDecay, thumpPunch;
        public NoiseColor noiseColor;
        public float noiseAmount, noiseDecay, noiseLp, noiseHp, noiseAttack;
        public NoiseColor noise2Color;
        public float noise2Amount, noise2Decay, noise2Lp, noise2Hp;
        public float f0;
        public WaveShape waveShape;
        public float m1Ratio, m2Ratio, m3Ratio, m4Ratio, m5Ratio;
        public float m1Decay, m2Decay, m3Decay, m4Decay, m5Decay;
        public float m1Level, m2Level, m3Level, m4Level, m5Level;
        public float modalMix, modalMixEnd, modalMixTime;
        public float fmFreq, fmAmount, fmDecay;
        public float fm2Freq, fm2Amount, fm2Decay;
        public float subOscFreq, subOscAmount, subOscDecay;
        public float ringModFreq, ringModAmount, ringModDecay;
        public float wavefoldAmount, drive;
        public int bitcrushBits;
        public float bitcrushRate, bitcrushMix;
        public int bitcrushBits2;
        public float bitcrushRate2, bitcrushMix2;
        public float attackTime, decayTime, sustainLevel, releaseTime;
        public float formant1Freq, formant1Q;
        public float formant2Freq, formant2Q;
        public float formant3Freq, formant3Q;
        public float formantMix;
        public float subHitDelay, subHitLevel, subHitPitchMul;
        public float tailDelay, tailLevel, tailPitchMul;
        public NoiseColor tailNoiseColor;
        public float tailNoiseLp, tailNoiseHp;
        public int grainCount;
        public float grainSpread, grainPitchSpread, grainDecay;
        public float pitchStart, pitchEnd, pitchEnvTime;
        public PitchCurveType pitchCurveType;
        public float filterFreq, filterQ, filterEnvAmount, filterEnvDecay;
        public SfxFilterMode filterMode;
        public float bodyResonance, bodySize;
        public float combFreq, combDecay, combMix;
        public float pluckDecay, pluckBrightness, pluckMix;
        public float detuneAmount;
        public int unisonVoices;
        public float chorusRate, chorusDepth, chorusMix;
        public float tremoloRate, tremoloDepth;
        public float earlyRefLevel, earlyRefDecay;
        public int earlyRefTaps;
        public float stereoWidth;
        public float pitchVar, timbreVar;
        public int variantCount;
    }

    static Cfg D()
    {
        return new Cfg
        {
            duration = 0.25f,
            volume = 0.5f,
            clickLevel = 0f,
            clickFreq = 3000f,
            clickDecay = 80f,
            thumpFreq = 60f,
            thumpDecay = 12f,
            thumpPunch = 80f,
            noiseColor = NoiseColor.Pink,
            noiseAmount = 0.3f,
            noiseDecay = 8f,
            noiseLp = 6000f,
            noiseHp = 80f,
            noiseAttack = 0f,
            noise2Color = NoiseColor.Brown,
            noise2Amount = 0f,
            noise2Decay = 8f,
            noise2Lp = 3000f,
            noise2Hp = 60f,
            f0 = 200f,
            waveShape = WaveShape.Triangle,
            m1Ratio = 2.1f,
            m2Ratio = 3.4f,
            m3Ratio = 4.8f,
            m4Ratio = 6.3f,
            m5Ratio = 8.1f,
            m1Decay = 6f,
            m2Decay = 9f,
            m3Decay = 13f,
            m4Decay = 18f,
            m5Decay = 24f,
            m1Level = 0.6f,
            m2Level = 0.35f,
            m3Level = 0.18f,
            m4Level = 0.09f,
            m5Level = 0.045f,
            modalMix = 0.55f,
            modalMixEnd = 0.65f,
            modalMixTime = 0.04f,
            fmFreq = 800f,
            fmAmount = 0f,
            fmDecay = 15f,
            fm2Freq = 400f,
            fm2Amount = 0f,
            fm2Decay = 15f,
            subOscFreq = 0f,
            subOscAmount = 0f,
            subOscDecay = 8f,
            ringModFreq = 0f,
            ringModAmount = 0f,
            ringModDecay = 8f,
            wavefoldAmount = 0f,
            drive = 0f,
            bitcrushBits = 16,
            bitcrushRate = 48000f,
            bitcrushMix = 0f,
            bitcrushBits2 = 16,
            bitcrushRate2 = 48000f,
            bitcrushMix2 = 0f,
            attackTime = 0.001f,
            decayTime = 0.04f,
            sustainLevel = 0.15f,
            releaseTime = 0.07f,
            formant1Freq = 0f,
            formant1Q = 0f,
            formant2Freq = 0f,
            formant2Q = 0f,
            formant3Freq = 0f,
            formant3Q = 0f,
            formantMix = 0f,
            subHitDelay = 0f,
            subHitLevel = 0f,
            subHitPitchMul = 1f,
            tailDelay = 0f,
            tailLevel = 0f,
            tailPitchMul = 1f,
            tailNoiseColor = NoiseColor.Brown,
            tailNoiseLp = 0f,
            tailNoiseHp = 0f,
            grainCount = 0,
            grainSpread = 0f,
            grainPitchSpread = 0f,
            grainDecay = 0.5f,
            pitchStart = 1f,
            pitchEnd = 1f,
            pitchEnvTime = 0f,
            pitchCurveType = PitchCurveType.Linear,
            filterFreq = 0f,
            filterQ = 0f,
            filterEnvAmount = 0f,
            filterEnvDecay = 8f,
            filterMode = SfxFilterMode.LowPass,
            bodyResonance = 0f,
            bodySize = 1f,
            combFreq = 0f,
            combDecay = 0.5f,
            combMix = 0f,
            pluckDecay = 0.5f,
            pluckBrightness = 0.5f,
            pluckMix = 0f,
            detuneAmount = 0f,
            unisonVoices = 1,
            chorusRate = 0f,
            chorusDepth = 0f,
            chorusMix = 0f,
            tremoloRate = 0f,
            tremoloDepth = 0f,
            earlyRefLevel = 0f,
            earlyRefDecay = 8f,
            earlyRefTaps = 0,
            stereoWidth = 0f,
            pitchVar = 0.12f,
            timbreVar = 0.12f,
            variantCount = 6
        };
    }

    static void Apply(SfxPreset p, Cfg c)
    {
        if (p == null) return;
        p.duration = c.duration; p.volume = c.volume;
        p.clickLevel = c.clickLevel; p.clickFreq = c.clickFreq; p.clickDecay = c.clickDecay;
        p.thumpFreq = c.thumpFreq; p.thumpDecay = c.thumpDecay; p.thumpPunch = c.thumpPunch;
        p.noiseColor = c.noiseColor; p.noiseAmount = c.noiseAmount; p.noiseDecay = c.noiseDecay;
        p.noiseLp = c.noiseLp; p.noiseHp = c.noiseHp; p.noiseAttack = c.noiseAttack;
        p.noise2Color = c.noise2Color; p.noise2Amount = c.noise2Amount; p.noise2Decay = c.noise2Decay;
        p.noise2Lp = c.noise2Lp; p.noise2Hp = c.noise2Hp;
        p.f0 = c.f0; p.waveShape = c.waveShape;
        p.m1Ratio = c.m1Ratio; p.m2Ratio = c.m2Ratio; p.m3Ratio = c.m3Ratio;
        p.m4Ratio = c.m4Ratio; p.m5Ratio = c.m5Ratio;
        p.m1Decay = c.m1Decay; p.m2Decay = c.m2Decay; p.m3Decay = c.m3Decay;
        p.m4Decay = c.m4Decay; p.m5Decay = c.m5Decay;
        p.m1Level = c.m1Level; p.m2Level = c.m2Level; p.m3Level = c.m3Level;
        p.m4Level = c.m4Level; p.m5Level = c.m5Level;
        p.modalMix = c.modalMix; p.modalMixEnd = c.modalMixEnd; p.modalMixTime = c.modalMixTime;
        p.fmFreq = c.fmFreq; p.fmAmount = c.fmAmount; p.fmDecay = c.fmDecay;
        p.fm2Freq = c.fm2Freq; p.fm2Amount = c.fm2Amount; p.fm2Decay = c.fm2Decay;
        p.subOscFreq = c.subOscFreq; p.subOscAmount = c.subOscAmount; p.subOscDecay = c.subOscDecay;
        p.ringModFreq = c.ringModFreq; p.ringModAmount = c.ringModAmount; p.ringModDecay = c.ringModDecay;
        p.wavefoldAmount = c.wavefoldAmount; p.drive = c.drive;
        p.bitcrushBits = c.bitcrushBits; p.bitcrushRate = c.bitcrushRate; p.bitcrushMix = c.bitcrushMix;
        p.bitcrushBits2 = c.bitcrushBits2; p.bitcrushRate2 = c.bitcrushRate2; p.bitcrushMix2 = c.bitcrushMix2;
        p.attackTime = c.attackTime; p.decayTime = c.decayTime;
        p.sustainLevel = c.sustainLevel; p.releaseTime = c.releaseTime;
        p.formant1Freq = c.formant1Freq; p.formant1Q = c.formant1Q;
        p.formant2Freq = c.formant2Freq; p.formant2Q = c.formant2Q;
        p.formant3Freq = c.formant3Freq; p.formant3Q = c.formant3Q;
        p.formantMix = c.formantMix;
        p.subHitDelay = c.subHitDelay; p.subHitLevel = c.subHitLevel; p.subHitPitchMul = c.subHitPitchMul;
        p.tailDelay = c.tailDelay; p.tailLevel = c.tailLevel; p.tailPitchMul = c.tailPitchMul;
        p.tailNoiseColor = c.tailNoiseColor; p.tailNoiseLp = c.tailNoiseLp; p.tailNoiseHp = c.tailNoiseHp;
        p.grainCount = c.grainCount; p.grainSpread = c.grainSpread;
        p.grainPitchSpread = c.grainPitchSpread; p.grainDecay = c.grainDecay;
        p.pitchStart = c.pitchStart; p.pitchEnd = c.pitchEnd; p.pitchEnvTime = c.pitchEnvTime;
        p.pitchCurveType = c.pitchCurveType;
        p.filterFreq = c.filterFreq; p.filterQ = c.filterQ;
        p.filterEnvAmount = c.filterEnvAmount; p.filterEnvDecay = c.filterEnvDecay;
        p.filterMode = c.filterMode;
        p.bodyResonance = c.bodyResonance; p.bodySize = c.bodySize;
        p.combFreq = c.combFreq; p.combDecay = c.combDecay; p.combMix = c.combMix;
        p.pluckDecay = c.pluckDecay; p.pluckBrightness = c.pluckBrightness; p.pluckMix = c.pluckMix;
        p.detuneAmount = c.detuneAmount; p.unisonVoices = c.unisonVoices;
        p.chorusRate = c.chorusRate; p.chorusDepth = c.chorusDepth; p.chorusMix = c.chorusMix;
        p.tremoloRate = c.tremoloRate; p.tremoloDepth = c.tremoloDepth;
        p.earlyRefLevel = c.earlyRefLevel; p.earlyRefDecay = c.earlyRefDecay; p.earlyRefTaps = c.earlyRefTaps;
        p.stereoWidth = c.stereoWidth;
        p.pitchVar = c.pitchVar; p.timbreVar = c.timbreVar; p.variantCount = c.variantCount;
        p.InvalidateCache();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(p);
#endif
    }

    static Cfg FootstepGrass()
    {
        var c = D();
        c.duration = 0.22f;
        c.volume = 0.42f;

        c.clickLevel = 0.15f;
        c.clickDecay = 200f;

        c.thumpFreq = 38f;
        c.thumpDecay = 12f;
        c.thumpPunch = 10f;

        c.noiseColor = NoiseColor.Pink;
        c.noiseAmount = 0.5f;
        c.noiseDecay = 10f;
        c.noiseLp = 3500f;
        c.noiseHp = 400f;

        c.noise2Color = NoiseColor.Brown;
        c.noise2Amount = 0.3f;
        c.noise2Decay = 7f;
        c.noise2Lp = 1200f;
        c.noise2Hp = 80f;

        c.f0 = 140f;
        c.m1Ratio = 1.7f; c.m2Ratio = 2.6f; c.m3Ratio = 3.8f; c.m4Ratio = 5.2f;
        c.m1Decay = 5f; c.m2Decay = 7f; c.m3Decay = 9f; c.m4Decay = 12f; c.m5Decay = 15f;
        c.m1Level = 0.4f; c.m2Level = 0.2f; c.m3Level = 0.08f; c.m4Level = 0.03f; c.m5Level = 0.01f;
        c.modalMix = 0.08f;

        c.attackTime = 0.003f;
        c.decayTime = 0.05f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.1f;

        c.pitchVar = 0.18f;
        c.timbreVar = 0.2f;
        c.variantCount = 8;
        return c;
    }

    static Cfg FootstepDirt()
    {
        var c = D();
        c.duration = 0.2f;
        c.volume = 0.45f;

        c.clickLevel = 0.2f;
        c.clickDecay = 180f;

        c.thumpFreq = 48f;
        c.thumpDecay = 14f;
        c.thumpPunch = 20f;

        c.noiseColor = NoiseColor.Brown;
        c.noiseAmount = 0.55f;
        c.noiseDecay = 12f;
        c.noiseLp = 1600f;
        c.noiseHp = 40f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.15f;
        c.noise2Decay = 8f;
        c.noise2Lp = 2800f;
        c.noise2Hp = 200f;

        c.f0 = 90f;
        c.m1Ratio = 1.9f; c.m2Ratio = 2.8f; c.m3Ratio = 4.1f; c.m4Ratio = 5.7f;
        c.m1Decay = 6f; c.m2Decay = 9f; c.m3Decay = 12f; c.m4Decay = 16f; c.m5Decay = 20f;
        c.m1Level = 0.35f; c.m2Level = 0.15f; c.m3Level = 0.06f; c.m4Level = 0.02f; c.m5Level = 0.008f;
        c.modalMix = 0.1f;

        c.attackTime = 0.002f;
        c.decayTime = 0.05f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.08f;

        c.pitchVar = 0.14f;
        c.timbreVar = 0.18f;
        c.variantCount = 8;
        return c;
    }

    static Cfg FootstepStone()
    {
        var c = D();
        c.duration = 0.25f;
        c.volume = 0.5f;

        c.clickLevel = 0.5f;
        c.clickDecay = 300f;

        c.thumpFreq = 55f;
        c.thumpDecay = 18f;
        c.thumpPunch = 35f;

        c.noiseColor = NoiseColor.Brown;
        c.noiseAmount = 0.4f;
        c.noiseDecay = 18f;
        c.noiseLp = 1800f;
        c.noiseHp = 60f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.12f;
        c.noise2Decay = 10f;
        c.noise2Lp = 3500f;
        c.noise2Hp = 400f;

        c.f0 = 160f;
        c.m1Ratio = 1.8f; c.m2Ratio = 2.5f; c.m3Ratio = 3.2f; c.m4Ratio = 4.1f;
        c.m1Decay = 8f; c.m2Decay = 12f; c.m3Decay = 16f; c.m4Decay = 20f; c.m5Decay = 25f;
        c.m1Level = 1.0f; c.m2Level = 0.6f; c.m3Level = 0.3f; c.m4Level = 0.1f; c.m5Level = 0.05f;
        c.modalMix = 0.3f;

        c.attackTime = 0.001f;
        c.decayTime = 0.04f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.12f;

        c.pitchVar = 0.1f;
        c.timbreVar = 0.12f;
        c.variantCount = 6;
        return c;
    }

    static Cfg FootstepWood()
    {
        var c = D();
        c.duration = 0.25f;
        c.volume = 0.5f;

        c.clickLevel = 0.35f;
        c.clickDecay = 220f;

        c.thumpFreq = 65f;
        c.thumpDecay = 14f;
        c.thumpPunch = 25f;

        c.noiseColor = NoiseColor.Pink;
        c.noiseAmount = 0.25f;
        c.noiseDecay = 14f;
        c.noiseLp = 2200f;
        c.noiseHp = 120f;

        c.noise2Color = NoiseColor.Brown;
        c.noise2Amount = 0.2f;
        c.noise2Decay = 10f;
        c.noise2Lp = 900f;
        c.noise2Hp = 50f;

        c.f0 = 180f;
        c.m1Ratio = 1.58f; c.m2Ratio = 2.51f; c.m3Ratio = 3.89f; c.m4Ratio = 5.32f;
        c.m1Decay = 10f; c.m2Decay = 14f; c.m3Decay = 18f; c.m4Decay = 24f; c.m5Decay = 30f;
        c.m1Level = 1.0f; c.m2Level = 0.65f; c.m3Level = 0.35f; c.m4Level = 0.15f; c.m5Level = 0.06f;
        c.modalMix = 0.55f;

        c.attackTime = 0.001f;
        c.decayTime = 0.035f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.12f;

        c.pitchVar = 0.1f;
        c.timbreVar = 0.12f;
        c.variantCount = 6;
        return c;
    }

    static Cfg FootstepSand()
    {
        var c = D();
        c.duration = 0.28f;
        c.volume = 0.35f;

        c.clickLevel = 0.08f;
        c.clickDecay = 150f;

        c.thumpFreq = 30f;
        c.thumpDecay = 8f;
        c.thumpPunch = 8f;

        c.noiseColor = NoiseColor.Pink;
        c.noiseAmount = 0.6f;
        c.noiseDecay = 8f;
        c.noiseLp = 2800f;
        c.noiseHp = 300f;

        c.noise2Color = NoiseColor.Brown;
        c.noise2Amount = 0.3f;
        c.noise2Decay = 5f;
        c.noise2Lp = 1000f;
        c.noise2Hp = 60f;

        c.f0 = 100f;
        c.m1Ratio = 2.0f; c.m2Ratio = 3.1f; c.m3Ratio = 4.5f; c.m4Ratio = 6.2f;
        c.m1Decay = 4f; c.m2Decay = 5f; c.m3Decay = 7f; c.m4Decay = 9f; c.m5Decay = 11f;
        c.m1Level = 0.2f; c.m2Level = 0.08f; c.m3Level = 0.03f; c.m4Level = 0.01f; c.m5Level = 0.004f;
        c.modalMix = 0.04f;

        c.attackTime = 0.008f;
        c.decayTime = 0.06f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.1f;

        c.pitchVar = 0.2f;
        c.timbreVar = 0.22f;
        c.variantCount = 8;
        return c;
    }

    static Cfg FootstepMetal()
    {
        var c = D();
        c.duration = 0.35f;
        c.volume = 0.48f;

        c.clickLevel = 0.6f;
        c.clickDecay = 350f;

        c.thumpFreq = 80f;
        c.thumpDecay = 16f;
        c.thumpPunch = 45f;

        c.noiseColor = NoiseColor.Pink;
        c.noiseAmount = 0.15f;
        c.noiseDecay = 20f;
        c.noiseLp = 4000f;
        c.noiseHp = 600f;

        c.noise2Color = NoiseColor.Brown;
        c.noise2Amount = 0.08f;
        c.noise2Decay = 12f;
        c.noise2Lp = 1500f;
        c.noise2Hp = 80f;

        c.f0 = 280f;
        c.m1Ratio = 2.76f; c.m2Ratio = 5.41f; c.m3Ratio = 8.93f; c.m4Ratio = 12.7f;
        c.m1Decay = 12f; c.m2Decay = 18f; c.m3Decay = 25f; c.m4Decay = 33f; c.m5Decay = 42f;
        c.m1Level = 1.0f; c.m2Level = 0.7f; c.m3Level = 0.4f; c.m4Level = 0.2f; c.m5Level = 0.08f;
        c.modalMix = 0.75f;

        c.attackTime = 0.0005f;
        c.decayTime = 0.02f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.2f;

        c.pitchVar = 0.08f;
        c.timbreVar = 0.1f;
        c.variantCount = 6;
        return c;
    }

    static Cfg FootstepWater()
    {
        var c = D();
        c.duration = 0.3f;
        c.volume = 0.42f;

        c.clickLevel = 0.1f;
        c.clickDecay = 120f;

        c.thumpFreq = 55f;
        c.thumpDecay = 8f;
        c.thumpPunch = 80f;

        c.noiseColor = NoiseColor.Pink;
        c.noiseAmount = 0.55f;
        c.noiseDecay = 8f;
        c.noiseLp = 4500f;
        c.noiseHp = 200f;

        c.noise2Color = NoiseColor.White;
        c.noise2Amount = 0.2f;
        c.noise2Decay = 10f;
        c.noise2Lp = 8000f;
        c.noise2Hp = 2000f;

        c.f0 = 110f;
        c.m1Ratio = 1.7f; c.m2Ratio = 2.8f; c.m3Ratio = 4.3f; c.m4Ratio = 6.1f;
        c.m1Decay = 5f; c.m2Decay = 7f; c.m3Decay = 10f; c.m4Decay = 13f; c.m5Decay = 17f;
        c.m1Level = 0.5f; c.m2Level = 0.25f; c.m3Level = 0.1f; c.m4Level = 0.04f; c.m5Level = 0.015f;
        c.modalMix = 0.35f;

        c.attackTime = 0.003f;
        c.decayTime = 0.06f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.12f;

        c.pitchVar = 0.22f;
        c.timbreVar = 0.25f;
        c.variantCount = 8;
        return c;
    }

    static Cfg FootstepSnow()
    {
        var c = D();
        c.duration = 0.28f;
        c.volume = 0.32f;

        c.clickLevel = 0.05f;
        c.clickDecay = 120f;

        c.thumpFreq = 32f;
        c.thumpDecay = 10f;
        c.thumpPunch = 8f;

        c.noiseColor = NoiseColor.Brown;
        c.noiseAmount = 0.5f;
        c.noiseDecay = 8f;
        c.noiseLp = 1400f;
        c.noiseHp = 60f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.2f;
        c.noise2Decay = 12f;
        c.noise2Lp = 3000f;
        c.noise2Hp = 600f;

        c.f0 = 80f;
        c.m1Ratio = 2.0f; c.m2Ratio = 3.1f; c.m3Ratio = 4.5f; c.m4Ratio = 6.0f;
        c.m1Decay = 4f; c.m2Decay = 6f; c.m3Decay = 8f; c.m4Decay = 10f; c.m5Decay = 13f;
        c.m1Level = 0.15f; c.m2Level = 0.06f; c.m3Level = 0.02f; c.m4Level = 0.008f; c.m5Level = 0.003f;
        c.modalMix = 0.05f;

        c.attackTime = 0.005f;
        c.decayTime = 0.06f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.1f;

        c.pitchVar = 0.16f;
        c.timbreVar = 0.2f;
        c.variantCount = 8;
        return c;
    }

    static Cfg BlockBreak()
    {
        var c = D();
        c.duration = 0.4f;
        c.volume = 0.7f;

        c.clickLevel = 0.8f;
        c.clickDecay = 250f;

        c.thumpFreq = 42f;
        c.thumpDecay = 8f;
        c.thumpPunch = 50f;

        c.noiseColor = NoiseColor.Brown;
        c.noiseAmount = 0.6f;
        c.noiseDecay = 6f;
        c.noiseLp = 2000f;
        c.noiseHp = 40f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.3f;
        c.noise2Decay = 12f;
        c.noise2Lp = 4000f;
        c.noise2Hp = 300f;

        c.f0 = 120f;
        c.m1Ratio = 2.3f; c.m2Ratio = 3.8f; c.m3Ratio = 5.9f; c.m4Ratio = 8.5f;
        c.m1Decay = 6f; c.m2Decay = 9f; c.m3Decay = 13f; c.m4Decay = 17f; c.m5Decay = 22f;
        c.m1Level = 1.0f; c.m2Level = 0.55f; c.m3Level = 0.25f; c.m4Level = 0.1f; c.m5Level = 0.04f;
        c.modalMix = 0.35f;

        c.attackTime = 0.0001f;
        c.decayTime = 0.05f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.2f;

        c.tailDelay = 0.08f;
        c.tailLevel = 0.35f;
        c.tailPitchMul = 0.4f;
        c.tailNoiseColor = NoiseColor.Brown;
        c.tailNoiseLp = 1500f;
        c.tailNoiseHp = 30f;

        c.pitchVar = 0.12f;
        c.timbreVar = 0.15f;
        c.variantCount = 6;
        return c;
    }

    static Cfg BlockPlace()
    {
        var c = D();
        c.duration = 0.22f;
        c.volume = 0.55f;

        c.clickLevel = 0.4f;
        c.clickDecay = 220f;

        c.thumpFreq = 60f;
        c.thumpDecay = 16f;
        c.thumpPunch = 30f;

        c.noiseColor = NoiseColor.Brown;
        c.noiseAmount = 0.35f;
        c.noiseDecay = 15f;
        c.noiseLp = 1800f;
        c.noiseHp = 50f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.12f;
        c.noise2Decay = 10f;
        c.noise2Lp = 3500f;
        c.noise2Hp = 300f;

        c.f0 = 130f;
        c.m1Ratio = 2.15f; c.m2Ratio = 3.87f; c.m3Ratio = 6.22f; c.m4Ratio = 8.9f;
        c.m1Decay = 9f; c.m2Decay = 13f; c.m3Decay = 18f; c.m4Decay = 23f; c.m5Decay = 29f;
        c.m1Level = 1.0f; c.m2Level = 0.55f; c.m3Level = 0.25f; c.m4Level = 0.1f; c.m5Level = 0.04f;
        c.modalMix = 0.45f;

        c.attackTime = 0.0005f;
        c.decayTime = 0.03f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.1f;

        c.pitchVar = 0.08f;
        c.timbreVar = 0.1f;
        c.variantCount = 5;
        return c;
    }

    static Cfg BlockDig()
    {
        var c = D();
        c.duration = 0.2f;
        c.volume = 0.5f;

        c.clickLevel = 0.35f;
        c.clickDecay = 200f;

        c.thumpFreq = 50f;
        c.thumpDecay = 14f;
        c.thumpPunch = 25f;

        c.noiseColor = NoiseColor.Brown;
        c.noiseAmount = 0.5f;
        c.noiseDecay = 10f;
        c.noiseLp = 1600f;
        c.noiseHp = 50f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.15f;
        c.noise2Decay = 8f;
        c.noise2Lp = 3000f;
        c.noise2Hp = 300f;

        c.f0 = 100f;
        c.m1Ratio = 2.5f; c.m2Ratio = 4.8f; c.m3Ratio = 7.3f; c.m4Ratio = 10.2f;
        c.m1Decay = 7f; c.m2Decay = 10f; c.m3Decay = 14f; c.m4Decay = 18f; c.m5Decay = 23f;
        c.m1Level = 1.0f; c.m2Level = 0.5f; c.m3Level = 0.22f; c.m4Level = 0.09f; c.m5Level = 0.03f;
        c.modalMix = 0.25f;

        c.attackTime = 0.001f;
        c.decayTime = 0.04f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.08f;

        c.pitchVar = 0.14f;
        c.timbreVar = 0.16f;
        c.variantCount = 6;
        return c;
    }

    static Cfg ItemPickup()
    {
        var c = D();
        c.duration = 0.3f;
        c.volume = 0.5f;

        c.clickLevel = 0.3f;
        c.clickDecay = 400f;

        c.thumpFreq = 400f;
        c.thumpDecay = 8f;
        c.thumpPunch = 600f;

        c.noiseColor = NoiseColor.White;
        c.noiseAmount = 0.05f;
        c.noiseDecay = 20f;
        c.noiseLp = 6000f;
        c.noiseHp = 800f;

        c.f0 = 680f;
        c.m1Ratio = 1.5f; c.m2Ratio = 2.0f; c.m3Ratio = 3.0f; c.m4Ratio = 4.0f;
        c.m1Decay = 15f; c.m2Decay = 22f; c.m3Decay = 30f; c.m4Decay = 40f; c.m5Decay = 50f;
        c.m1Level = 1.0f; c.m2Level = 0.6f; c.m3Level = 0.3f; c.m4Level = 0.12f; c.m5Level = 0.05f;
        c.modalMix = 0.85f;

        c.attackTime = 0.001f;
        c.decayTime = 0.05f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.2f;

        c.subHitDelay = 0.06f;
        c.subHitLevel = 0.4f;
        c.subHitPitchMul = 1.335f;

        c.stereoWidth = 0.3f;

        c.pitchVar = 0.04f;
        c.timbreVar = 0.05f;
        c.variantCount = 3;
        return c;
    }

    static Cfg ItemDrop()
    {
        var c = D();
        c.duration = 0.22f;
        c.volume = 0.45f;

        c.clickLevel = 0.4f;
        c.clickDecay = 220f;

        c.thumpFreq = 120f;
        c.thumpDecay = 12f;
        c.thumpPunch = 80f;

        c.noiseColor = NoiseColor.Pink;
        c.noiseAmount = 0.2f;
        c.noiseDecay = 10f;
        c.noiseLp = 2500f;
        c.noiseHp = 100f;

        c.noise2Color = NoiseColor.Brown;
        c.noise2Amount = 0.15f;
        c.noise2Decay = 8f;
        c.noise2Lp = 900f;
        c.noise2Hp = 40f;

        c.f0 = 260f;
        c.m1Ratio = 2.0f; c.m2Ratio = 3.0f; c.m3Ratio = 4.0f; c.m4Ratio = 5.5f;
        c.m1Decay = 8f; c.m2Decay = 11f; c.m3Decay = 15f; c.m4Decay = 20f; c.m5Decay = 25f;
        c.m1Level = 1.0f; c.m2Level = 0.55f; c.m3Level = 0.25f; c.m4Level = 0.1f; c.m5Level = 0.04f;
        c.modalMix = 0.5f;

        c.attackTime = 0.001f;
        c.decayTime = 0.035f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.1f;

        c.pitchVar = 0.07f;
        c.timbreVar = 0.09f;
        c.variantCount = 4;
        return c;
    }

    static Cfg ItemEquip()
    {
        var c = D();
        c.duration = 0.2f;
        c.volume = 0.5f;

        c.clickLevel = 0.45f;
        c.clickDecay = 280f;

        c.thumpFreq = 100f;
        c.thumpDecay = 18f;
        c.thumpPunch = 60f;

        c.noiseColor = NoiseColor.Pink;
        c.noiseAmount = 0.15f;
        c.noiseDecay = 14f;
        c.noiseLp = 3000f;
        c.noiseHp = 200f;

        c.noise2Color = NoiseColor.Brown;
        c.noise2Amount = 0.1f;
        c.noise2Decay = 10f;
        c.noise2Lp = 1200f;
        c.noise2Hp = 60f;

        c.f0 = 380f;
        c.m1Ratio = 2.0f; c.m2Ratio = 3.5f; c.m3Ratio = 5.0f; c.m4Ratio = 6.5f;
        c.m1Decay = 10f; c.m2Decay = 14f; c.m3Decay = 19f; c.m4Decay = 25f; c.m5Decay = 32f;
        c.m1Level = 1.0f; c.m2Level = 0.55f; c.m3Level = 0.25f; c.m4Level = 0.1f; c.m5Level = 0.04f;
        c.modalMix = 0.6f;

        c.attackTime = 0.0005f;
        c.decayTime = 0.025f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.12f;

        c.pitchVar = 0.05f;
        c.timbreVar = 0.06f;
        c.variantCount = 3;
        return c;
    }

    static Cfg KnappingHit()
    {
        var c = D();
        c.duration = 0.2f;
        c.volume = 0.8f;

        c.clickLevel = 1.2f;
        c.clickDecay = 400f;

        c.thumpFreq = 80f;
        c.thumpDecay = 25f;
        c.thumpPunch = 30f;

        c.noiseColor = NoiseColor.White;
        c.noiseAmount = 0.8f;
        c.noiseDecay = 45f;
        c.noiseLp = 12000f;
        c.noiseHp = 800f;

        c.noise2Color = NoiseColor.Brown;
        c.noise2Amount = 0.5f;
        c.noise2Decay = 15f;
        c.noise2Lp = 2000f;
        c.noise2Hp = 100f;

        c.f0 = 420f;
        c.m1Ratio = 2.41f; c.m2Ratio = 3.66f; c.m3Ratio = 4.81f; c.m4Ratio = 6.22f;
        c.m1Decay = 25f; c.m2Decay = 35f; c.m3Decay = 45f; c.m4Decay = 55f; c.m5Decay = 70f;
        c.m1Level = 1.0f; c.m2Level = 0.7f; c.m3Level = 0.4f; c.m4Level = 0.2f; c.m5Level = 0.1f;
        c.modalMix = 0.6f;

        c.attackTime = 0.0001f;
        c.decayTime = 0.08f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.1f;

        c.pitchVar = 0.05f;
        c.timbreVar = 0.1f;
        c.variantCount = 5;
        return c;
    }

    static Cfg KnappingSuccess()
    {
        var c = D();
        c.duration = 0.5f;
        c.volume = 0.65f;

        c.clickLevel = 0.8f;
        c.clickDecay = 350f;

        c.thumpFreq = 350f;
        c.thumpDecay = 10f;
        c.thumpPunch = 400f;

        c.noiseColor = NoiseColor.White;
        c.noiseAmount = 0.15f;
        c.noiseDecay = 20f;
        c.noiseLp = 7000f;
        c.noiseHp = 500f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.2f;
        c.noise2Decay = 10f;
        c.noise2Lp = 3000f;
        c.noise2Hp = 200f;

        c.f0 = 520f;
        c.m1Ratio = 1.5f; c.m2Ratio = 2.0f; c.m3Ratio = 3.0f; c.m4Ratio = 4.0f;
        c.m1Decay = 20f; c.m2Decay = 28f; c.m3Decay = 38f; c.m4Decay = 50f; c.m5Decay = 64f;
        c.m1Level = 1.0f; c.m2Level = 0.65f; c.m3Level = 0.35f; c.m4Level = 0.15f; c.m5Level = 0.06f;
        c.modalMix = 0.8f;

        c.attackTime = 0.001f;
        c.decayTime = 0.06f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.35f;

        c.subHitDelay = 0.07f;
        c.subHitLevel = 0.45f;
        c.subHitPitchMul = 1.335f;

        c.stereoWidth = 0.35f;

        c.pitchVar = 0.03f;
        c.timbreVar = 0.04f;
        c.variantCount = 2;
        return c;
    }

    static Cfg KnappingFail()
    {
        var c = D();
        c.duration = 0.35f;
        c.volume = 0.5f;

        c.clickLevel = 0.6f;
        c.clickDecay = 180f;

        c.thumpFreq = 42f;
        c.thumpDecay = 10f;
        c.thumpPunch = 15f;

        c.noiseColor = NoiseColor.Brown;
        c.noiseAmount = 0.5f;
        c.noiseDecay = 8f;
        c.noiseLp = 1500f;
        c.noiseHp = 40f;

        c.noise2Color = NoiseColor.Pink;
        c.noise2Amount = 0.2f;
        c.noise2Decay = 12f;
        c.noise2Lp = 3000f;
        c.noise2Hp = 200f;

        c.f0 = 130f;
        c.m1Ratio = 1.8f; c.m2Ratio = 2.7f; c.m3Ratio = 4.1f; c.m4Ratio = 5.8f;
        c.m1Decay = 5f; c.m2Decay = 7f; c.m3Decay = 10f; c.m4Decay = 13f; c.m5Decay = 17f;
        c.m1Level = 1.0f; c.m2Level = 0.6f; c.m3Level = 0.3f; c.m4Level = 0.12f; c.m5Level = 0.05f;
        c.modalMix = 0.25f;

        c.attackTime = 0.001f;
        c.decayTime = 0.06f;
        c.sustainLevel = 0f;
        c.releaseTime = 0.18f;

        c.tailDelay = 0.08f;
        c.tailLevel = 0.25f;
        c.tailPitchMul = 0.5f;
        c.tailNoiseColor = NoiseColor.Brown;
        c.tailNoiseLp = 1200f;
        c.tailNoiseHp = 35f;

        c.pitchVar = 0.07f;
        c.timbreVar = 0.09f;
        c.variantCount = 4;
        return c;
    }
}*/