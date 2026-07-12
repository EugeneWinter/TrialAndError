using UnityEngine;

public enum NoiseColor { White, Pink, Brown }
public enum PitchCurveType { ExpDown, ExpUp, Linear, SCurve }
public enum WaveShape { Sine, Triangle, Saw, Square, Pulse25, Pulse12 }
public enum SfxFilterMode { LowPass, BandPass, HighPass, Notch }

public struct SfxParams
{
    public float duration;
    public float volume;

    public float thumpFreq;
    public float thumpDecay;
    public float thumpPunch;

    public NoiseColor noiseColor;
    public float noiseAmount;
    public float noiseDecay;
    public float noiseLp;
    public float noiseHp;
    public float noiseAttack;

    public NoiseColor noise2Color;
    public float noise2Amount;
    public float noise2Decay;
    public float noise2Lp;
    public float noise2Hp;

    public float f0;
    public WaveShape waveShape;
    public float m1Ratio, m2Ratio, m3Ratio, m4Ratio, m5Ratio;
    public float m1Decay, m2Decay, m3Decay, m4Decay, m5Decay;
    public float m1Level, m2Level, m3Level, m4Level, m5Level;
    public float modalMix;
    public float modalMixEnd;
    public float modalMixTime;

    public float fmFreq;
    public float fmAmount;
    public float fmDecay;
    public float fm2Freq;
    public float fm2Amount;
    public float fm2Decay;

    public float subOscFreq;
    public float subOscAmount;
    public float subOscDecay;

    public float ringModFreq;
    public float ringModAmount;
    public float ringModDecay;

    public float wavefoldAmount;
    public float drive;

    public int bitcrushBits;
    public float bitcrushRate;
    public float bitcrushMix;

    public float attackTime;
    public float decayTime;
    public float sustainLevel;
    public float releaseTime;

    public float formant1Freq;
    public float formant1Q;
    public float formant2Freq;
    public float formant2Q;
    public float formant3Freq;
    public float formant3Q;
    public float formantMix;

    public float subHitDelay;
    public float subHitLevel;
    public float subHitPitchMul;

    public float tailDelay;
    public float tailLevel;
    public float tailPitchMul;
    public NoiseColor tailNoiseColor;
    public float tailNoiseLp;
    public float tailNoiseHp;

    public int grainCount;
    public float grainSpread;
    public float grainPitchSpread;
    public float grainDecay;

    public float pitchStart;
    public float pitchEnd;
    public float pitchEnvTime;
    public PitchCurveType pitchCurveType;

    public float filterFreq;
    public float filterQ;
    public float filterEnvAmount;
    public float filterEnvDecay;
    public SfxFilterMode filterMode;

    public float detuneAmount;
    public int unisonVoices;

    public float chorusRate;
    public float chorusDepth;
    public float chorusMix;

    public int bitcrushBits2;
    public float bitcrushRate2;
    public float bitcrushMix2;

    public float earlyRefLevel;
    public float earlyRefDecay;
    public int earlyRefTaps;

    public float tremoloRate;
    public float tremoloDepth;

    public float combFreq;
    public float combDecay;
    public float combMix;

    public float pluckDecay;
    public float pluckBrightness;
    public float pluckMix;

    public float clickLevel;
    public float clickFreq;
    public float clickDecay;

    public float bodyResonance;
    public float bodySize;

    public float stereoWidth;

    public string name;
    public int seed;
}