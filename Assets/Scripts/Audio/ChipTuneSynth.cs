using UnityEngine;

public static class ChipTuneSynth
{
    public const int SR = 48000;
    const int MAX_CHORUS_DELAY = 2400;

    public static AudioClip Generate(SfxParams p)
    {
        int len = Mathf.Max(1, Mathf.CeilToInt(p.duration * SR));
        float[] buf = new float[len];

        RenderLayer(buf, p, 1f, 1f);

        if (p.subHitDelay > 0f && p.subHitLevel > 0.001f)
        {
            float[] sub = new float[len];
            SfxParams sp = p;
            sp.f0 *= Mathf.Max(0.1f, p.subHitPitchMul);
            sp.clickFreq *= Mathf.Max(0.1f, p.subHitPitchMul);
            sp.thumpFreq *= Mathf.Max(0.1f, p.subHitPitchMul);
            sp.duration = Mathf.Max(0.01f, p.duration - p.subHitDelay);
            sp.seed = p.seed + 7919;
            sp.grainCount = 0;
            sp.earlyRefTaps = 0;
            RenderLayer(sub, sp, p.subHitLevel, 1f);
            FadeOut(sub);

            int offset = Mathf.RoundToInt(p.subHitDelay * SR);
            for (int i = 0; i < len; i++)
            {
                int si = i - offset;
                if (si >= 0 && si < sub.Length)
                    buf[i] += sub[si];
            }
        }

        if (p.tailDelay > 0f && p.tailLevel > 0.001f)
        {
            float[] tail = new float[len];
            SfxParams tp = p;
            tp.f0 *= Mathf.Max(0.1f, p.tailPitchMul);
            tp.clickLevel *= 0.3f;
            tp.clickDecay *= 0.6f;
            tp.duration = Mathf.Max(0.01f, p.duration - p.tailDelay);
            tp.seed = p.seed + 15731;
            tp.grainCount = 0;
            tp.earlyRefTaps = 0;
            tp.noiseColor = p.tailNoiseColor;
            if (p.tailNoiseLp > 0f) tp.noiseLp = p.tailNoiseLp;
            if (p.tailNoiseHp > 0f) tp.noiseHp = p.tailNoiseHp;
            tp.modalMix *= 0.75f;
            RenderLayer(tail, tp, p.tailLevel, 1f);
            FadeOut(tail);

            int offset = Mathf.RoundToInt(p.tailDelay * SR);
            for (int i = 0; i < len; i++)
            {
                int si = i - offset;
                if (si >= 0 && si < tail.Length)
                    buf[i] += tail[si];
            }
        }

        if (p.grainCount > 0 && p.grainSpread > 0f)
        {
            System.Random grainRng = new System.Random(p.seed + 31337);
            for (int g = 0; g < p.grainCount; g++)
            {
                float delay = (float)grainRng.NextDouble() * p.grainSpread;
                float pitchMul = 1f + ((float)grainRng.NextDouble() * 2f - 1f) * p.grainPitchSpread;
                float ampFade = 1f - (delay / Mathf.Max(0.0001f, p.grainSpread)) * p.grainDecay;
                ampFade = Mathf.Max(ampFade, 0.08f);

                float[] grain = new float[len];
                SfxParams gp = p;
                gp.f0 *= Mathf.Max(0.1f, pitchMul);
                gp.clickFreq *= Mathf.Max(0.1f, pitchMul);
                gp.thumpFreq *= Mathf.Max(0.1f, pitchMul);
                gp.duration = Mathf.Min(Mathf.Max(0.01f, p.duration * 0.22f), 0.06f);
                gp.seed = p.seed + g * 6971;
                gp.grainCount = 0;
                gp.earlyRefTaps = 0;
                gp.clickLevel *= 0.7f;
                gp.modalMix *= 0.85f;
                RenderLayer(grain, gp, ampFade * 0.22f, 1f);
                FadeOut(grain);

                int offset = Mathf.RoundToInt(delay * SR);
                for (int i = 0; i < len; i++)
                {
                    int si = i - offset;
                    if (si >= 0 && si < grain.Length)
                        buf[i] += grain[si];
                }
            }
        }

        if (p.earlyRefTaps > 0 && p.earlyRefLevel > 0.001f)
            ApplyEarlyReflections(buf, len, p);

        FadeOut(buf);

        if (p.stereoWidth > 0.01f)
        {
            float[] left = new float[len];
            float[] right = new float[len];
            GenerateStereo(buf, left, right, len, p);

            FadeOut(left);
            FadeOut(right);

            float peak = 0f;
            for (int i = 0; i < len; i++)
            {
                float al = Mathf.Abs(left[i]);
                float ar = Mathf.Abs(right[i]);
                if (al > peak) peak = al;
                if (ar > peak) peak = ar;
            }

            float gain = peak > 0.95f ? 0.95f / peak : 1f;

            float[] stereo = new float[len * 2];
            for (int i = 0; i < len; i++)
            {
                stereo[i * 2] = Mathf.Clamp(left[i] * gain * p.volume, -1f, 1f);
                stereo[i * 2 + 1] = Mathf.Clamp(right[i] * gain * p.volume, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(p.name, len, 2, SR, false);
            clip.SetData(stereo, 0);
            return clip;
        }
        else
        {
            float peak = 0f;
            for (int i = 0; i < len; i++)
            {
                float a = Mathf.Abs(buf[i]);
                if (a > peak) peak = a;
            }

            float gain = peak > 0.95f ? 0.95f / peak : 1f;

            for (int i = 0; i < len; i++)
                buf[i] = Mathf.Clamp(buf[i] * gain * p.volume, -1f, 1f);

            AudioClip clip = AudioClip.Create(p.name, len, 1, SR, false);
            clip.SetData(buf, 0);
            return clip;
        }
    }

    static void RenderLayer(float[] buf, SfxParams p, float amplitude, float pitchMul)
    {
        int len = buf.Length;

        uint noiseSeed = (uint)p.seed;
        uint noise2Seed = (uint)(p.seed + 92821);

        float pinkB0 = 0f, pinkB1 = 0f, pinkB2 = 0f, pinkB3 = 0f, pinkB4 = 0f, pinkB5 = 0f;
        float brownState = 0f;
        float pink2B0 = 0f, pink2B1 = 0f, pink2B2 = 0f, pink2B3 = 0f, pink2B4 = 0f, pink2B5 = 0f;
        float brown2State = 0f;

        float impactLp = 0f, impactHp = 0f;
        float bodyLp = 0f, bodyHp = 0f;
        float debrisLp = 0f, debrisHp = 0f;
        float lowBody = 0f;

        float r1lp = 0f, r1bp = 0f;
        float r2lp = 0f, r2bp = 0f;
        float r3lp = 0f, r3bp = 0f;
        float r4lp = 0f, r4bp = 0f;
        float r5lp = 0f, r5bp = 0f;

        float postLp = 0f, postBp = 0f;

        float noiseLpBase = Mathf.Clamp(p.noiseLp > 0f ? p.noiseLp : 4000f, 200f, 14000f);
        float noiseHpBase = Mathf.Clamp(p.noiseHp > 0f ? p.noiseHp : 40f, 20f, noiseLpBase * 0.85f);

        float impactLpBase = Mathf.Clamp(Mathf.Max(p.clickFreq * 1.4f, noiseLpBase), 500f, 14000f);
        float impactHpBase = Mathf.Clamp(Mathf.Max(40f, p.clickFreq * 0.18f), 20f, impactLpBase * 0.8f);

        float debrisLpBase = Mathf.Clamp(p.noise2Lp > 0f ? p.noise2Lp : noiseLpBase * 0.55f, 200f, 10000f);
        float debrisHpBase = Mathf.Clamp(p.noise2Hp > 0f ? p.noise2Hp : noiseHpBase, 20f, debrisLpBase * 0.8f);

        float lowAlpha = 1f - Mathf.Exp(-2f * Mathf.PI * Mathf.Clamp(p.thumpFreq, 20f, 500f) / SR);

        float baseExciter = Mathf.Lerp(0.12f, 0.42f, Mathf.Clamp01(p.modalMix));
        float modalGain = Mathf.Lerp(0.65f, 1.65f, Mathf.Clamp01(p.modalMix));

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / SR;
            float env = Envelope(t, p);

            float pitchEnvMul = 1f;
            if (p.pitchEnvTime > 0f)
            {
                float pe = Mathf.Clamp01(t / p.pitchEnvTime);
                pe = ApplyPitchCurve(pe, p.pitchCurveType);
                pitchEnvMul = Mathf.Lerp(p.pitchStart, p.pitchEnd, pe);
            }

            float currentPitch = pitchMul * pitchEnvMul;

            noiseSeed = XorShift(noiseSeed);
            float rawNoise = NoiseFromSeed(
                noiseSeed,
                p.noiseColor,
                ref pinkB0, ref pinkB1, ref pinkB2, ref pinkB3, ref pinkB4, ref pinkB5, ref brownState
            );

            float attackMul = p.noiseAttack > 0.0001f ? Mathf.Clamp01(t / p.noiseAttack) : 1f;

            float impactEnv = Mathf.Exp(-t * Mathf.Max(30f, p.clickDecay));
            float impactLpFreq = Mathf.Clamp(impactLpBase * currentPitch, 300f, 14000f);
            float impactHpFreq = Mathf.Clamp(impactHpBase * Mathf.Sqrt(Mathf.Max(0.01f, currentPitch)), 20f, impactLpFreq * 0.8f);
            float impact = FilterNoise(ref impactLp, ref impactHp, rawNoise, impactLpFreq, impactHpFreq) * p.clickLevel * impactEnv * attackMul;

            float bodyEnv = Mathf.Exp(-t * Mathf.Max(1f, p.noiseDecay));
            float broad = FilterNoise(ref bodyLp, ref bodyHp, rawNoise, noiseLpBase, noiseHpBase) * p.noiseAmount * bodyEnv * attackMul;

            lowBody += (rawNoise - lowBody) * lowAlpha;
            float thumpEnv = Mathf.Exp(-t * Mathf.Max(1f, p.thumpDecay));
            float thump = lowBody * thumpEnv * (0.08f + p.thumpPunch * 0.0035f);

            float debris = 0f;
            if (p.noise2Amount > 0.0001f)
            {
                noise2Seed = XorShift(noise2Seed);
                float rawNoise2 = NoiseFromSeed(
                    noise2Seed,
                    p.noise2Color,
                    ref pink2B0, ref pink2B1, ref pink2B2, ref pink2B3, ref pink2B4, ref pink2B5, ref brown2State
                );

                float debrisEnv = Mathf.Exp(-t * Mathf.Max(1f, p.noise2Decay));
                debris = FilterNoise(ref debrisLp, ref debrisHp, rawNoise2, debrisLpBase, debrisHpBase) * p.noise2Amount * debrisEnv;
            }

            float exciter = impact + broad + thump + debris * 0.7f;

            float f1 = Mathf.Clamp(p.f0 * currentPitch, 40f, SR * 0.45f);
            float f2 = Mathf.Clamp(p.f0 * p.m1Ratio * currentPitch, 40f, SR * 0.45f);
            float f3 = Mathf.Clamp(p.f0 * p.m2Ratio * currentPitch, 40f, SR * 0.45f);
            float f4 = Mathf.Clamp(p.f0 * p.m3Ratio * currentPitch, 40f, SR * 0.45f);
            float f5 = Mathf.Clamp(p.f0 * p.m4Ratio * currentPitch, 40f, SR * 0.45f);

            float q1 = ModeQ(p.m1Decay, p.modalMix);
            float q2 = ModeQ(p.m2Decay, p.modalMix);
            float q3 = ModeQ(p.m3Decay, p.modalMix);
            float q4 = ModeQ(p.m4Decay, p.modalMix);
            float q5 = ModeQ(p.m5Decay, p.modalMix);

            float modal = 0f;
            modal += Reson(exciter, f1, q1, ref r1lp, ref r1bp) * (p.m1Level > 0f ? p.m1Level : 1f);
            modal += Reson(exciter, f2, q2, ref r2lp, ref r2bp) * (p.m2Level > 0f ? p.m2Level : 0.45f);
            modal += Reson(exciter, f3, q3, ref r3lp, ref r3bp) * (p.m3Level > 0f ? p.m3Level : 0.18f);
            modal += Reson(exciter, f4, q4, ref r4lp, ref r4bp) * (p.m4Level > 0f ? p.m4Level : 0.08f);
            modal += Reson(exciter, f5, q5, ref r5lp, ref r5bp) * (p.m5Level > 0f ? p.m5Level : 0.03f);

            float mix = exciter * baseExciter + modal * modalGain;

            if (p.filterFreq > 20f)
            {
                float cutoff = Mathf.Clamp(p.filterFreq, 20f, SR * 0.45f);
                float q = Mathf.Clamp(p.filterQ > 0.01f ? p.filterQ : 0.8f, 0.5f, 20f);
                float f = 2f * Mathf.Sin(Mathf.PI * cutoff / SR);
                float damp = 1f / q;
                float hp = mix - postLp - damp * postBp;
                postBp += f * hp;
                postLp += f * postBp;

                switch (p.filterMode)
                {
                    case SfxFilterMode.LowPass:
                        mix = postLp;
                        break;
                    case SfxFilterMode.HighPass:
                        mix = hp;
                        break;
                    case SfxFilterMode.BandPass:
                        mix = postBp;
                        break;
                    case SfxFilterMode.Notch:
                        mix = postLp + hp;
                        break;
                }
            }

            if (p.drive > 0.0001f)
            {
                float d = 1f + p.drive * 3f;
                mix = Mathf.Atan(mix * d) / Mathf.Atan(d);
            }

            mix *= env * amplitude;

            if (env < 0.0005f)
                mix = 0f;

            buf[i] += mix;
        }
    }

    static float Reson(float input, float freqHz, float q, ref float lp, ref float bp)
    {
        float f = 2f * Mathf.Sin(Mathf.PI * Mathf.Clamp(freqHz, 20f, SR * 0.45f) / SR);
        float damp = 1f / Mathf.Clamp(q, 0.6f, 48f);
        float hp = input - lp - damp * bp;
        bp += f * hp;
        lp += f * bp;
        return bp;
    }

    static float ModeQ(float decay, float modalMix)
    {
        return Mathf.Clamp(1.2f + decay * 0.9f + modalMix * 8f, 1.2f, 48f);
    }

    static void FadeOut(float[] buf)
    {
        int len = buf.Length;
        int fadeLen = Mathf.Min(Mathf.RoundToInt(0.012f * SR), len / 4);
        for (int i = 0; i < fadeLen; i++)
        {
            float fade = (float)i / fadeLen;
            fade *= fade;
            buf[len - 1 - i] *= fade;
        }
    }

    static void GenerateStereo(float[] mono, float[] left, float[] right, int len, SfxParams p)
    {
        float w = Mathf.Clamp01(p.stereoWidth);
        int decorrelation = Mathf.RoundToInt(0.00035f * SR * w);

        for (int i = 0; i < len; i++)
        {
            float m = mono[i];
            float mid = m * (1f - w * 0.25f);
            float sideL = 0f;
            float sideR = 0f;

            int iL = i - decorrelation;
            int iR = i + decorrelation;

            if (iL >= 0 && iL < len) sideL = mono[iL] * w * 0.35f;
            if (iR >= 0 && iR < len) sideR = mono[iR] * w * 0.35f;

            left[i] = mid + sideL - sideR * 0.2f;
            right[i] = mid + sideR - sideL * 0.2f;
        }
    }

    static void ApplyEarlyReflections(float[] buf, int len, SfxParams p)
    {
        int taps = Mathf.Clamp(p.earlyRefTaps, 1, 8);
        int[] delays = new int[taps];
        float[] gains = new float[taps];
        System.Random rng = new System.Random(p.seed + 54321);

        for (int t = 0; t < taps; t++)
        {
            float delayMs = 6f + (float)rng.NextDouble() * 34f;
            delays[t] = Mathf.RoundToInt(delayMs * 0.001f * SR);
            float dist = delayMs / 40f;
            gains[t] = p.earlyRefLevel * Mathf.Exp(-dist * p.earlyRefDecay * 0.25f);
            if ((t & 1) == 1) gains[t] *= -0.5f;
        }

        float[] copy = new float[len];
        System.Array.Copy(buf, copy, len);

        for (int t = 0; t < taps; t++)
        {
            int d = delays[t];
            float g = gains[t];
            for (int i = d; i < len; i++)
                buf[i] += copy[i - d] * g;
        }
    }

    static float ApplyPitchCurve(float t01, PitchCurveType type)
    {
        switch (type)
        {
            case PitchCurveType.ExpDown:
                return 1f - Mathf.Exp(-t01 * 4f);
            case PitchCurveType.ExpUp:
                return Mathf.Pow(t01, 0.3f);
            case PitchCurveType.SCurve:
                return t01 * t01 * (3f - 2f * t01);
            default:
                return t01;
        }
    }

    static uint XorShift(uint x)
    {
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        return x;
    }

    static float UintToFloat(uint x)
    {
        return (x & 0x7FFFFFFF) / (float)0x7FFFFFFF * 2f - 1f;
    }

    static float NoiseFromSeed(uint seed, NoiseColor color,
        ref float b0, ref float b1, ref float b2, ref float b3,
        ref float b4, ref float b5, ref float brownState)
    {
        float white = UintToFloat(seed);
        switch (color)
        {
            case NoiseColor.White:
                return white;
            case NoiseColor.Pink:
                b0 = 0.99886f * b0 + white * 0.0555179f;
                b1 = 0.99332f * b1 + white * 0.0750759f;
                b2 = 0.96900f * b2 + white * 0.1538520f;
                b3 = 0.86650f * b3 + white * 0.3104856f;
                b4 = 0.55000f * b4 + white * 0.5329522f;
                b5 = -0.7616f * b5 - white * 0.0168980f;
                return Mathf.Clamp((b0 + b1 + b2 + b3 + b4 + b5 + white * 0.5362f) * 0.11f, -1f, 1f);
            case NoiseColor.Brown:
                brownState = Mathf.Clamp(brownState + white * 0.02f, -1f, 1f);
                return brownState;
            default:
                return white;
        }
    }

    static float FilterNoise(ref float lpState, ref float hpState, float input, float lpFreq, float hpFreq)
    {
        lpFreq = Mathf.Clamp(lpFreq, 20f, SR * 0.45f);
        hpFreq = Mathf.Clamp(hpFreq, 20f, lpFreq * 0.9f);

        float alphaLp = 1f - Mathf.Exp(-2f * Mathf.PI * lpFreq / SR);
        lpState += (input - lpState) * alphaLp;

        float alphaHp = 1f - Mathf.Exp(-2f * Mathf.PI * hpFreq / SR);
        hpState += (lpState - hpState) * alphaHp;

        return lpState - hpState;
    }

    static float Envelope(float t, SfxParams p)
    {
        float a = p.attackTime;
        float d = a + p.decayTime;
        float r = Mathf.Max(d, p.duration - p.releaseTime);

        if (t < a)
            return a > 0f ? t / a : 1f;
        if (t < d)
            return p.decayTime > 0f
                ? Mathf.Lerp(1f, p.sustainLevel, (t - a) / p.decayTime)
                : p.sustainLevel;
        if (t < r)
            return p.sustainLevel;
        return p.releaseTime > 0f
            ? Mathf.Lerp(p.sustainLevel, 0f, (t - r) / p.releaseTime)
            : 0f;
    }
}