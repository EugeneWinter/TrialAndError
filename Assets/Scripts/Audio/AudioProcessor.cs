using UnityEngine;

public static class AudioProcessor
{
    public static float targetPeak = 0.6f;
    public static float highpassFreq = 150f;
    public static float lowpassFreq = 8000f;
    public static float silenceThreshold = 0.005f;
    public static float fadeMs = 5f;

    public static float softAttackMs = 12f;
    public static float compressorThreshold = 0.4f;
    public static float compressorRatio = 4f;
    public static float compressorMakeup = 1.3f;
    public static float saturationAmount = 0.2f;
    public static float presenceCut = 0.15f;

    public static int highpassPasses = 2;

    public static AudioClip Process(AudioClip source)
    {
        if (source == null) return null;

        int channels = source.channels;
        int sampleRate = source.frequency;
        int originalLen = source.samples;

        float[] data = new float[originalLen * channels];
        source.GetData(data, 0);

        float[] mono = channels == 1 ? data : StereoToMono(data, originalLen);

        int startSample, endSample;
        FindNonSilentRegion(mono, out startSample, out endSample);

        if (endSample <= startSample)
            return source;

        int trimmedLen = endSample - startSample;
        float[] trimmed = new float[trimmedLen];
        System.Array.Copy(mono, startSample, trimmed, 0, trimmedLen);

        for (int pass = 0; pass < highpassPasses; pass++)
            ApplyHighpass(trimmed, sampleRate, highpassFreq);

        ApplyLowpass(trimmed, sampleRate, lowpassFreq);
        ApplyPresenceCut(trimmed, sampleRate);
        ApplySoftAttack(trimmed, sampleRate);
        ApplyCompressor(trimmed);
        ApplySaturation(trimmed);
        NormalizePeak(trimmed, targetPeak);
        ApplyFades(trimmed, sampleRate);

        AudioClip result = AudioClip.Create(
            source.name + "_processed",
            trimmedLen, 1, sampleRate, false);
        result.SetData(trimmed, 0);
        return result;
    }

    static float[] StereoToMono(float[] stereo, int sampleCount)
    {
        float[] mono = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
            mono[i] = (stereo[i * 2] + stereo[i * 2 + 1]) * 0.5f;
        return mono;
    }

    static void FindNonSilentRegion(float[] data, out int start, out int end)
    {
        start = 0;
        end = data.Length;

        for (int i = 0; i < data.Length; i++)
        {
            if (Mathf.Abs(data[i]) > silenceThreshold)
            {
                start = Mathf.Max(0, i - 64);
                break;
            }
        }

        for (int i = data.Length - 1; i >= 0; i--)
        {
            if (Mathf.Abs(data[i]) > silenceThreshold)
            {
                end = Mathf.Min(data.Length, i + 64);
                break;
            }
        }
    }

    static void ApplyHighpass(float[] data, int sampleRate, float cutoffHz)
    {
        float rc = 1f / (2f * Mathf.PI * cutoffHz);
        float dt = 1f / sampleRate;
        float alpha = rc / (rc + dt);

        float prevIn = data[0];
        float prevOut = data[0];

        for (int i = 1; i < data.Length; i++)
        {
            float currIn = data[i];
            float currOut = alpha * (prevOut + currIn - prevIn);
            prevIn = currIn;
            prevOut = currOut;
            data[i] = currOut;
        }
    }

    static void ApplyLowpass(float[] data, int sampleRate, float cutoffHz)
    {
        float rc = 1f / (2f * Mathf.PI * cutoffHz);
        float dt = 1f / sampleRate;
        float alpha = dt / (rc + dt);

        float prev = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            prev = prev + alpha * (data[i] - prev);
            data[i] = prev;
        }
    }

    static void ApplyPresenceCut(float[] data, int sampleRate)
    {
        if (presenceCut <= 0f) return;

        float centerFreq = 3000f;
        float q = 0.7f;
        float gain = 1f - presenceCut;

        float omega = 2f * Mathf.PI * centerFreq / sampleRate;
        float sn = Mathf.Sin(omega);
        float cs = Mathf.Cos(omega);
        float alpha = sn / (2f * q);
        float A = Mathf.Sqrt(gain);

        float b0 = 1f + alpha * A;
        float b1 = -2f * cs;
        float b2 = 1f - alpha * A;
        float a0 = 1f + alpha / A;
        float a1 = -2f * cs;
        float a2 = 1f - alpha / A;

        b0 /= a0; b1 /= a0; b2 /= a0;
        a1 /= a0; a2 /= a0;

        float x1 = 0f, x2 = 0f, y1 = 0f, y2 = 0f;
        for (int i = 0; i < data.Length; i++)
        {
            float x = data[i];
            float y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = x;
            y2 = y1; y1 = y;
            data[i] = y;
        }
    }

    static void ApplySoftAttack(float[] data, int sampleRate)
    {
        int attackSamples = Mathf.RoundToInt(sampleRate * softAttackMs / 1000f);
        attackSamples = Mathf.Min(attackSamples, data.Length / 2);

        for (int i = 0; i < attackSamples; i++)
        {
            float t = (float)i / attackSamples;
            float smoothed = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            data[i] *= smoothed;
        }
    }

    static void ApplyCompressor(float[] data)
    {
        float threshold = compressorThreshold;
        float ratio = compressorRatio;
        float makeup = compressorMakeup;

        for (int i = 0; i < data.Length; i++)
        {
            float abs = Mathf.Abs(data[i]);
            float sign = data[i] >= 0 ? 1f : -1f;

            if (abs > threshold)
            {
                float excess = abs - threshold;
                float compressed = threshold + excess / ratio;
                data[i] = sign * compressed;
            }

            data[i] *= makeup;
        }
    }

    static void ApplySaturation(float[] data)
    {
        if (saturationAmount <= 0f) return;

        float drive = 1f + saturationAmount * 2f;

        for (int i = 0; i < data.Length; i++)
        {
            float x = data[i] * drive;
            float shaped = x / (1f + Mathf.Abs(x));
            data[i] = Mathf.Lerp(data[i], shaped, saturationAmount);
        }
    }

    static void NormalizePeak(float[] data, float targetPeak)
    {
        float peak = 0f;
        for (int i = 0; i < data.Length; i++)
        {
            float abs = Mathf.Abs(data[i]);
            if (abs > peak) peak = abs;
        }

        if (peak < 0.001f) return;

        float gain = targetPeak / peak;
        for (int i = 0; i < data.Length; i++)
            data[i] *= gain;
    }

    static void ApplyFades(float[] data, int sampleRate)
    {
        int fadeSamples = Mathf.RoundToInt(sampleRate * fadeMs / 1000f);
        fadeSamples = Mathf.Min(fadeSamples, data.Length / 4);

        for (int i = 0; i < fadeSamples; i++)
        {
            float t = (float)i / fadeSamples;
            data[i] *= t * t;
        }

        for (int i = 0; i < fadeSamples; i++)
        {
            float t = (float)i / fadeSamples;
            data[data.Length - 1 - i] *= t * t;
        }
    }
}