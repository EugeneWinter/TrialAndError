using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "New SFX", menuName = "Game Data/SFX Preset")]
public class SfxPreset : ScriptableObject
{
    public float duration = 0.15f;
    [Range(0f, 1f)] public float volume = 0.5f;

    [Header("Click Transient")]
    public float clickLevel = 0f;
    public float clickFreq = 3000f;
    public float clickDecay = 100f;

    [Header("Exciter: Thump")]
    public float thumpFreq = 60f;
    public float thumpDecay = 20f;
    public float thumpPunch = 150f;

    [Header("Exciter: Noise Primary")]
    public NoiseColor noiseColor = NoiseColor.White;
    [Range(0f, 1f)] public float noiseAmount = 0.5f;
    public float noiseDecay = 15f;
    public float noiseLp = 8000f;
    public float noiseHp = 100f;
    public float noiseAttack = 0f;

    [Header("Exciter: Noise Secondary")]
    public NoiseColor noise2Color = NoiseColor.White;
    [Range(0f, 1f)] public float noise2Amount = 0f;
    public float noise2Decay = 15f;
    public float noise2Lp = 8000f;
    public float noise2Hp = 100f;

    [Header("Resonator: Modal")]
    public float f0 = 200f;
    public WaveShape waveShape = WaveShape.Sine;
    public float m1Ratio = 2.1f;
    public float m2Ratio = 3.4f;
    public float m3Ratio = 4.8f;
    public float m4Ratio = 6.3f;
    public float m5Ratio = 8.1f;
    public float m1Decay = 10f;
    public float m2Decay = 15f;
    public float m3Decay = 20f;
    public float m4Decay = 25f;
    public float m5Decay = 30f;
    [Range(0f, 1f)] public float m1Level = 0.5f;
    [Range(0f, 1f)] public float m2Level = 0.25f;
    [Range(0f, 1f)] public float m3Level = 0.125f;
    [Range(0f, 1f)] public float m4Level = 0.0625f;
    [Range(0f, 1f)] public float m5Level = 0.03125f;
    [Range(0f, 1f)] public float modalMix = 0.5f;
    [Range(0f, 1f)] public float modalMixEnd = 0.5f;
    public float modalMixTime = 0f;

    [Header("FM Synthesis")]
    public float fmFreq = 1000f;
    [Range(0f, 5f)] public float fmAmount = 0f;
    public float fmDecay = 25f;
    public float fm2Freq = 500f;
    [Range(0f, 5f)] public float fm2Amount = 0f;
    public float fm2Decay = 25f;

    [Header("Sub Oscillator")]
    public float subOscFreq = 0f;
    [Range(0f, 1f)] public float subOscAmount = 0f;
    public float subOscDecay = 10f;

    [Header("Ring Modulation")]
    public float ringModFreq = 0f;
    [Range(0f, 1f)] public float ringModAmount = 0f;
    public float ringModDecay = 10f;

    [Header("Non-Linear")]
    [Range(0f, 10f)] public float wavefoldAmount = 0f;
    [Range(0f, 5f)] public float drive = 0f;

    [Header("Bitcrusher")]
    [Range(2, 16)] public int bitcrushBits = 16;
    public float bitcrushRate = 48000f;
    [Range(0f, 1f)] public float bitcrushMix = 0f;
    [Range(2, 16)] public int bitcrushBits2 = 16;
    public float bitcrushRate2 = 48000f;
    [Range(0f, 1f)] public float bitcrushMix2 = 0f;

    [Header("Envelope")]
    public float attackTime = 0.001f;
    public float decayTime = 0.05f;
    [Range(0f, 1f)] public float sustainLevel = 0.2f;
    public float releaseTime = 0.05f;

    [Header("Formant Filter")]
    public float formant1Freq = 0f;
    public float formant1Q = 0f;
    public float formant2Freq = 0f;
    public float formant2Q = 0f;
    public float formant3Freq = 0f;
    public float formant3Q = 0f;
    [Range(0f, 1f)] public float formantMix = 0f;

    [Header("Sub-Hit Layer")]
    public float subHitDelay = 0f;
    [Range(0f, 1f)] public float subHitLevel = 0f;
    public float subHitPitchMul = 1f;

    [Header("Tail Layer")]
    public float tailDelay = 0f;
    [Range(0f, 1f)] public float tailLevel = 0f;
    public float tailPitchMul = 1f;
    public NoiseColor tailNoiseColor = NoiseColor.Brown;
    public float tailNoiseLp = 0f;
    public float tailNoiseHp = 0f;

    [Header("Grain Scatter")]
    public int grainCount = 0;
    public float grainSpread = 0f;
    public float grainPitchSpread = 0f;
    [Range(0f, 1f)] public float grainDecay = 0.5f;

    [Header("Pitch Envelope")]
    public float pitchStart = 1f;
    public float pitchEnd = 1f;
    public float pitchEnvTime = 0f;
    public PitchCurveType pitchCurveType = PitchCurveType.Linear;

    [Header("Resonant Filter")]
    public float filterFreq = 0f;
    public float filterQ = 0f;
    public float filterEnvAmount = 0f;
    public float filterEnvDecay = 10f;
    public SfxFilterMode filterMode = SfxFilterMode.BandPass;

    [Header("Body Resonance")]
    [Range(0f, 1f)] public float bodyResonance = 0f;
    [Range(0.1f, 5f)] public float bodySize = 1f;

    [Header("Comb Filter")]
    public float combFreq = 0f;
    public float combDecay = 0.5f;
    [Range(0f, 1f)] public float combMix = 0f;

    [Header("Karplus-Strong Pluck")]
    public float pluckDecay = 0.5f;
    [Range(0f, 1f)] public float pluckBrightness = 0.5f;
    [Range(0f, 1f)] public float pluckMix = 0f;

    [Header("Chorus / Detune")]
    public float detuneAmount = 0f;
    public int unisonVoices = 1;
    public float chorusRate = 0f;
    public float chorusDepth = 0f;
    [Range(0f, 1f)] public float chorusMix = 0f;

    [Header("Tremolo")]
    public float tremoloRate = 0f;
    [Range(0f, 1f)] public float tremoloDepth = 0f;

    [Header("Early Reflections")]
    [Range(0f, 1f)] public float earlyRefLevel = 0f;
    public float earlyRefDecay = 10f;
    [Range(0, 8)] public int earlyRefTaps = 0;

    [Header("Stereo")]
    [Range(0f, 1f)] public float stereoWidth = 0f;

    [Header("Randomization")]
    [Range(0f, 0.3f)] public float pitchVar = 0.1f;
    [Range(0f, 0.3f)] public float timbreVar = 0.1f;
    public int variantCount = 4;

    private AudioClip[] clips;

    static float RandRange(System.Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    static int SeedForVariant(string presetName, int variantIndex)
    {
        int h = string.IsNullOrEmpty(presetName) ? 1234567 : presetName.GetHashCode();
        unchecked
        {
            h ^= variantIndex * 486187739;
            h ^= 0x2C9277B5;
            if (h == int.MinValue) h = int.MaxValue;
            if (h < 0) h = -h;
            return h + 1;
        }
    }

    public AudioClip GetClip()
    {
        if (clips == null || clips.Length == 0)
        {
            GenerateVariants();
        }
        else
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                {
                    GenerateVariants();
                    break;
                }
            }
        }

        return clips[Random.Range(0, clips.Length)];
    }

    public void GenerateVariants()
    {
        int count = Mathf.Max(1, variantCount);
        clips = new AudioClip[count];

        for (int i = 0; i < count; i++)
            clips[i] = ChipTuneSynth.Generate(BuildParams(i));
    }

    public SfxParams BuildParams(int variantIndex)
    {
        int seed = SeedForVariant(name, variantIndex);
        System.Random rng = new System.Random(seed);

        float pv = 1f + RandRange(rng, -pitchVar, pitchVar);
        float tv = 1f + RandRange(rng, -timbreVar, timbreVar);

        return new SfxParams
        {
            duration = duration,
            volume = volume,

            clickLevel = clickLevel * tv,
            clickFreq = clickFreq * pv,
            clickDecay = clickDecay,

            thumpFreq = thumpFreq * pv,
            thumpDecay = thumpDecay,
            thumpPunch = thumpPunch * pv,

            noiseColor = noiseColor,
            noiseAmount = noiseAmount * tv,
            noiseDecay = noiseDecay,
            noiseLp = noiseLp * tv,
            noiseHp = noiseHp,
            noiseAttack = noiseAttack,

            noise2Color = noise2Color,
            noise2Amount = noise2Amount * tv,
            noise2Decay = noise2Decay,
            noise2Lp = noise2Lp * tv,
            noise2Hp = noise2Hp,

            f0 = f0 * pv,
            waveShape = waveShape,
            m1Ratio = m1Ratio,
            m2Ratio = m2Ratio,
            m3Ratio = m3Ratio,
            m4Ratio = m4Ratio,
            m5Ratio = m5Ratio,
            m1Decay = m1Decay,
            m2Decay = m2Decay,
            m3Decay = m3Decay,
            m4Decay = m4Decay,
            m5Decay = m5Decay,
            m1Level = m1Level,
            m2Level = m2Level,
            m3Level = m3Level,
            m4Level = m4Level,
            m5Level = m5Level,
            modalMix = modalMix,
            modalMixEnd = modalMixEnd,
            modalMixTime = modalMixTime,

            fmFreq = fmFreq * pv,
            fmAmount = fmAmount * tv,
            fmDecay = fmDecay,
            fm2Freq = fm2Freq * pv,
            fm2Amount = fm2Amount * tv,
            fm2Decay = fm2Decay,

            subOscFreq = subOscFreq * pv,
            subOscAmount = subOscAmount,
            subOscDecay = subOscDecay,

            ringModFreq = ringModFreq * pv,
            ringModAmount = ringModAmount,
            ringModDecay = ringModDecay,

            wavefoldAmount = wavefoldAmount * tv,
            drive = drive,

            bitcrushBits = bitcrushBits,
            bitcrushRate = bitcrushRate,
            bitcrushMix = bitcrushMix,
            bitcrushBits2 = bitcrushBits2,
            bitcrushRate2 = bitcrushRate2,
            bitcrushMix2 = bitcrushMix2,

            attackTime = attackTime,
            decayTime = decayTime,
            sustainLevel = sustainLevel,
            releaseTime = releaseTime,

            formant1Freq = formant1Freq,
            formant1Q = formant1Q,
            formant2Freq = formant2Freq,
            formant2Q = formant2Q,
            formant3Freq = formant3Freq,
            formant3Q = formant3Q,
            formantMix = formantMix,

            subHitDelay = subHitDelay,
            subHitLevel = subHitLevel,
            subHitPitchMul = subHitPitchMul,

            tailDelay = tailDelay,
            tailLevel = tailLevel,
            tailPitchMul = tailPitchMul,
            tailNoiseColor = tailNoiseColor,
            tailNoiseLp = tailNoiseLp,
            tailNoiseHp = tailNoiseHp,

            grainCount = grainCount,
            grainSpread = grainSpread,
            grainPitchSpread = grainPitchSpread,
            grainDecay = grainDecay,

            pitchStart = pitchStart,
            pitchEnd = pitchEnd,
            pitchEnvTime = pitchEnvTime,
            pitchCurveType = pitchCurveType,

            filterFreq = filterFreq,
            filterQ = filterQ,
            filterEnvAmount = filterEnvAmount,
            filterEnvDecay = filterEnvDecay,
            filterMode = filterMode,

            bodyResonance = bodyResonance,
            bodySize = bodySize,

            combFreq = combFreq,
            combDecay = combDecay,
            combMix = combMix,

            pluckDecay = pluckDecay,
            pluckBrightness = pluckBrightness,
            pluckMix = pluckMix,

            detuneAmount = detuneAmount,
            unisonVoices = unisonVoices,

            chorusRate = chorusRate,
            chorusDepth = chorusDepth,
            chorusMix = chorusMix,

            tremoloRate = tremoloRate,
            tremoloDepth = tremoloDepth,

            earlyRefLevel = earlyRefLevel,
            earlyRefDecay = earlyRefDecay,
            earlyRefTaps = earlyRefTaps,

            stereoWidth = stereoWidth,

            name = $"{name}_v{variantIndex}",
            seed = seed
        };
    }

    public SfxParams BuildParamsWithContext(int variantIndex, SfxContext ctx)
    {
        SfxParams p = BuildParams(variantIndex);

        MaterialProfile surface = ctx.surfaceMaterial;
        MaterialProfile tool = ctx.toolMaterial != null ? ctx.toolMaterial : ctx.surfaceMaterial;

        float velocity = Mathf.Clamp(ctx.velocity, 0.05f, 3f);
        float mass = Mathf.Clamp(ctx.mass, 0.1f, 8f);
        float wetness = Mathf.Clamp01(ctx.wetness);

        float vel01 = Mathf.InverseLerp(0.05f, 2.5f, velocity);
        float mass01 = Mathf.InverseLerp(0.1f, 5f, Mathf.Min(5f, mass));
        float impulse01 = Mathf.Clamp01(vel01 * 0.7f + Mathf.Sqrt(mass01) * 0.3f);

        p.volume *= Mathf.Lerp(0.7f, 1.15f, impulse01);
        p.duration *= Mathf.Lerp(0.92f, 1.18f, mass01);
        p.attackTime *= Mathf.Lerp(1.1f, 0.75f, vel01);
        p.releaseTime *= Mathf.Lerp(0.95f, 1.15f, mass01);

        p.bitcrushMix = 0f;
        p.bitcrushMix2 = 0f;
        p.wavefoldAmount = 0f;
        p.fmAmount *= 0.1f;
        p.fm2Amount *= 0.1f;
        p.ringModAmount *= 0.1f;
        p.chorusMix *= 0.15f;
        p.formantMix *= 0.1f;
        p.tremoloDepth = 0f;

        if (surface != null)
        {
            p.f0 = Mathf.Lerp(700f, 90f, Mathf.Clamp01(surface.density * 0.75f + mass01 * 0.25f));
            p.thumpFreq = Mathf.Lerp(28f, surface.baseFreq * 0.35f, 0.55f) / Mathf.Lerp(1f, 1.12f, mass01);
            p.thumpPunch *= Mathf.Lerp(0.8f, 1.25f, impulse01) * Mathf.Lerp(0.9f, 1.15f, surface.density);
            p.thumpDecay = Mathf.Lerp(8f, 22f, surface.density * 0.5f + surface.hardness * 0.5f);

            p.noiseColor = surface.primaryNoise;
            p.noise2Color = surface.secondaryNoise;

            p.modalMix = Mathf.Lerp(0.05f, 0.88f, surface.resonance * (1f - surface.graininess * 0.28f));
            p.noiseAmount = Mathf.Lerp(0.06f, 0.65f, surface.roughness * 0.45f + surface.graininess * 0.35f + surface.brittleness * 0.2f);
            p.noiseDecay = Mathf.Lerp(8f, 36f, surface.hardness * 0.35f + surface.brittleness * 0.3f + surface.resonance * 0.35f);
            p.noiseLp = Mathf.Lerp(900f, 6800f, surface.brightness);
            p.noiseHp = Mathf.Lerp(25f, 1300f, surface.hardness * 0.6f + surface.brightness * 0.4f);

            p.noise2Amount = Mathf.Lerp(0.02f, 0.55f, surface.brittleness * 0.65f + surface.graininess * 0.35f);
            p.noise2Decay = Mathf.Lerp(5f, 18f, surface.brittleness * 0.75f + surface.graininess * 0.25f);
            p.noise2Lp = Mathf.Lerp(500f, 4200f, surface.graininess * 0.65f + surface.brightness * 0.35f);
            p.noise2Hp = Mathf.Lerp(20f, 700f, surface.graininess * 0.7f + surface.brittleness * 0.3f);

            float bodyQ = Mathf.Lerp(4f, 28f, surface.resonance);
            bodyQ *= Mathf.Lerp(0.9f, 1.12f, surface.metallicity);
            bodyQ *= 1f - surface.wetness * 0.2f;

            p.m1Decay = bodyQ * 1.1f;
            p.m2Decay = bodyQ * 0.95f;
            p.m3Decay = bodyQ * 0.78f;
            p.m4Decay = bodyQ * 0.6f;
            p.m5Decay = bodyQ * 0.45f;

            float overtone = Mathf.Clamp01(surface.resonance * 0.55f + surface.metallicity * 0.45f);
            p.m1Level = 1f;
            p.m2Level = Mathf.Lerp(0.18f, 0.72f, overtone);
            p.m3Level = Mathf.Lerp(0.05f, 0.44f, overtone);
            p.m4Level = Mathf.Lerp(0.015f, 0.22f, overtone);
            p.m5Level = Mathf.Lerp(0.004f, 0.09f, overtone);

            if (surface.partialRatios != null && surface.partialRatios.Length >= 5)
            {
                p.m1Ratio = surface.partialRatios[0];
                p.m2Ratio = surface.partialRatios[1];
                p.m3Ratio = surface.partialRatios[2];
                p.m4Ratio = surface.partialRatios[3];
                p.m5Ratio = surface.partialRatios[4];
            }

            p.drive = Mathf.Min(0.22f, surface.brittleness * 0.08f + surface.roughness * 0.03f);
        }

        if (tool != null)
        {
            float contactHard = surface != null ? Mathf.Sqrt(Mathf.Max(0.0001f, tool.hardness * surface.hardness)) : tool.hardness;
            float contactBright = surface != null ? Mathf.Sqrt(Mathf.Max(0.0001f, tool.brightness * surface.brightness)) : tool.brightness;
            float toolWeight = tool.density * 0.45f + tool.hardness * 0.35f + tool.metallicity * 0.2f;

            p.clickLevel = Mathf.Lerp(0.08f, 1.35f, contactHard * 0.65f + vel01 * 0.25f + toolWeight * 0.1f);
            p.clickFreq = Mathf.Lerp(450f, 6500f, contactBright * 0.65f + contactHard * 0.35f);
            p.clickDecay = Mathf.Lerp(70f, 520f, contactHard * 0.65f + vel01 * 0.35f);

            p.noiseLp = Mathf.Lerp(p.noiseLp, Mathf.Max(p.noiseLp, Mathf.Lerp(1200f, 7600f, contactBright)), 0.45f);
            p.noiseHp = Mathf.Lerp(p.noiseHp, Mathf.Max(p.noiseHp, Mathf.Lerp(30f, 1500f, contactHard)), 0.35f);
            p.thumpPunch *= Mathf.Lerp(0.92f, 1.12f, toolWeight);

            float metalContact = surface != null ? Mathf.Sqrt(Mathf.Max(0.0001f, tool.metallicity * surface.metallicity)) : tool.metallicity;
            if (metalContact > 0.01f)
            {
                p.m2Level *= Mathf.Lerp(1f, 1.12f, metalContact);
                p.m3Level *= Mathf.Lerp(1f, 1.2f, metalContact);
                p.m4Level *= Mathf.Lerp(1f, 1.25f, metalContact);
                p.modalMix = Mathf.Lerp(p.modalMix, Mathf.Min(0.92f, p.modalMix + metalContact * 0.08f), 0.5f);
            }
        }
        else
        {
            p.clickLevel *= Mathf.Lerp(0.8f, 1.1f, vel01);
        }

        if (surface != null)
            wetness = Mathf.Clamp01(wetness + surface.wetness * 0.7f);
        if (tool != null)
            wetness = Mathf.Clamp01(wetness + tool.wetness * 0.2f);

        if (wetness > 0.001f)
        {
            p.clickLevel *= 1f - wetness * 0.45f;
            p.noiseLp = Mathf.Lerp(p.noiseLp, Mathf.Min(p.noiseLp, 1400f), wetness);
            p.noiseHp *= 1f - wetness * 0.4f;
            p.modalMix *= 1f - wetness * 0.18f;
            p.m1Decay *= 1f - wetness * 0.28f;
            p.m2Decay *= 1f - wetness * 0.34f;
            p.m3Decay *= 1f - wetness * 0.4f;
            p.m4Decay *= 1f - wetness * 0.45f;
            p.m5Decay *= 1f - wetness * 0.5f;
            p.thumpPunch *= 1f + wetness * 0.16f;
        }

        p.clickLevel *= Mathf.Lerp(0.8f, 1.18f, vel01);
        p.noiseAmount *= Mathf.Lerp(0.92f, 1.15f, vel01);
        p.noise2Amount *= Mathf.Lerp(0.9f, 1.2f, vel01);
        p.f0 /= Mathf.Lerp(1f, 1.12f, mass01);

        switch (ctx.room)
        {
            case RoomType.Cave:
                p.earlyRefLevel = Mathf.Max(p.earlyRefLevel, 0.18f);
                p.earlyRefDecay = 4.5f;
                p.earlyRefTaps = Mathf.Max(p.earlyRefTaps, 4);
                p.releaseTime *= 1.08f;
                break;
            case RoomType.StoneInterior:
                p.earlyRefLevel = Mathf.Max(p.earlyRefLevel, 0.12f);
                p.earlyRefDecay = 5.5f;
                p.earlyRefTaps = Mathf.Max(p.earlyRefTaps, 3);
                break;
            case RoomType.WoodInterior:
                p.earlyRefLevel = Mathf.Max(p.earlyRefLevel, 0.08f);
                p.earlyRefDecay = 7f;
                p.earlyRefTaps = Mathf.Max(p.earlyRefTaps, 2);
                p.noiseLp *= 0.92f;
                break;
            case RoomType.MetalRoom:
                p.earlyRefLevel = Mathf.Max(p.earlyRefLevel, 0.16f);
                p.earlyRefDecay = 4.5f;
                p.earlyRefTaps = Mathf.Max(p.earlyRefTaps, 4);
                break;
            case RoomType.Forest:
                p.earlyRefLevel = Mathf.Min(p.earlyRefLevel, 0.04f);
                p.noiseLp *= 0.92f;
                break;
            case RoomType.Underwater:
                p.filterFreq = 900f;
                p.filterQ = 0.8f;
                p.filterMode = SfxFilterMode.LowPass;
                p.clickLevel *= 0.2f;
                p.f0 *= 0.78f;
                break;
        }

        switch (ctx.era)
        {
            case TechEra.Stone:
                p.noiseAmount *= 1.03f;
                break;
            case TechEra.Bronze:
            case TechEra.Iron:
                p.clickLevel *= 1.01f;
                break;
            case TechEra.Industrial:
                p.noiseAmount *= 0.98f;
                break;
            case TechEra.Modern:
            case TechEra.Space:
                p.noiseAmount *= 0.96f;
                break;
        }

        p.volume = Mathf.Clamp(p.volume, 0f, 1f);
        p.duration = Mathf.Max(0.01f, p.duration);

        p.clickLevel = Mathf.Clamp(p.clickLevel, 0f, 2f);
        p.clickFreq = Mathf.Clamp(p.clickFreq, 100f, 8000f);
        p.clickDecay = Mathf.Clamp(p.clickDecay, 40f, 800f);

        p.thumpFreq = Mathf.Clamp(p.thumpFreq, 20f, 600f);
        p.thumpDecay = Mathf.Clamp(p.thumpDecay, 2f, 40f);
        p.thumpPunch = Mathf.Clamp(p.thumpPunch, 0f, 600f);

        p.f0 = Mathf.Clamp(p.f0, 40f, 4000f);

        p.m1Decay = Mathf.Clamp(p.m1Decay, 1f, 60f);
        p.m2Decay = Mathf.Clamp(p.m2Decay, 1f, 60f);
        p.m3Decay = Mathf.Clamp(p.m3Decay, 1f, 60f);
        p.m4Decay = Mathf.Clamp(p.m4Decay, 1f, 60f);
        p.m5Decay = Mathf.Clamp(p.m5Decay, 1f, 60f);

        p.m1Level = Mathf.Clamp01(p.m1Level);
        p.m2Level = Mathf.Clamp01(p.m2Level);
        p.m3Level = Mathf.Clamp01(p.m3Level);
        p.m4Level = Mathf.Clamp01(p.m4Level);
        p.m5Level = Mathf.Clamp01(p.m5Level);
        p.modalMix = Mathf.Clamp01(p.modalMix);

        p.noiseAmount = Mathf.Clamp(p.noiseAmount, 0f, 1.2f);
        p.noise2Amount = Mathf.Clamp(p.noise2Amount, 0f, 1.2f);
        p.noiseDecay = Mathf.Clamp(p.noiseDecay, 1f, 60f);
        p.noise2Decay = Mathf.Clamp(p.noise2Decay, 1f, 40f);

        p.noiseLp = Mathf.Clamp(p.noiseLp, 120f, 12000f);
        p.noiseHp = Mathf.Clamp(p.noiseHp, 20f, p.noiseLp * 0.85f);
        p.noise2Lp = Mathf.Clamp(p.noise2Lp, 120f, 10000f);
        p.noise2Hp = Mathf.Clamp(p.noise2Hp, 20f, p.noise2Lp * 0.85f);

        p.attackTime = Mathf.Max(0f, p.attackTime);
        p.decayTime = Mathf.Max(0.005f, p.decayTime);
        p.releaseTime = Mathf.Max(0.01f, p.releaseTime);

        p.subHitLevel = Mathf.Clamp01(p.subHitLevel);
        p.tailLevel = Mathf.Clamp01(p.tailLevel);
        p.earlyRefLevel = Mathf.Clamp01(p.earlyRefLevel);
        p.earlyRefTaps = Mathf.Clamp(p.earlyRefTaps, 0, 8);

        return p;
    }

    public void InvalidateCache()
    {
        clips = null;
    }

    public float GetRandomVolume()
    {
        return volume * (1f + Random.Range(-0.05f, 0.05f));
    }
}