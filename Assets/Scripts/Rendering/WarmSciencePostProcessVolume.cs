using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Warm Science/Full Post-Processing", typeof(UnityEngine.Rendering.Universal.UniversalRenderPipeline))]
public class WarmSciencePostProcessVolume : VolumeComponent, IPostProcessComponent
{
    [Header("Enable")]
    public BoolParameter enableEffect = new BoolParameter(false);

    [Header("Exposure and Tonemap")]
    public ClampedFloatParameter exposure = new ClampedFloatParameter(0.0f, -3.0f, 3.0f);
    public ClampedFloatParameter whitePathAmount = new ClampedFloatParameter(1.0f, 0.0f, 4.0f);
    public ClampedFloatParameter whiteCurve = new ClampedFloatParameter(2.0f, 1.0f, 4.0f);
    public ClampedFloatParameter lowerCurve = new ClampedFloatParameter(1.0f, 0.5f, 1.5f);
    public ClampedFloatParameter upperCurve = new ClampedFloatParameter(1.0f, 0.5f, 1.5f);

    [Header("Bloom")]
    public ClampedFloatParameter bloomIntensity = new ClampedFloatParameter(1.0f, 0.0f, 4.0f);
    public ClampedFloatParameter bloomContrast = new ClampedFloatParameter(0.0f, -4.0f, 4.0f);
    public ClampedIntParameter bloomIterations = new ClampedIntParameter(6, 3, 8);

    [Header("Color Grading Red Channel")]
    public ColorParameter cgRMul = new ColorParameter(new Color(1.0f, 0.0f, 0.0f), false, false, true);
    public ClampedFloatParameter cgRIntensity = new ClampedFloatParameter(1.0f, 0.0f, 4.0f);
    public ClampedFloatParameter cgRMin = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public ClampedFloatParameter cgRCurve = new ClampedFloatParameter(1.0f, 0.5f, 4.0f);

    [Header("Color Grading Green Channel")]
    public ColorParameter cgGMul = new ColorParameter(new Color(0.0f, 1.0f, 0.0f), false, false, true);
    public ClampedFloatParameter cgGIntensity = new ClampedFloatParameter(1.0f, 0.0f, 4.0f);
    public ClampedFloatParameter cgGMin = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public ClampedFloatParameter cgGCurve = new ClampedFloatParameter(1.0f, 0.5f, 4.0f);

    [Header("Color Grading Blue Channel")]
    public ColorParameter cgBMul = new ColorParameter(new Color(0.0f, 0.0f, 1.0f), false, false, true);
    public ClampedFloatParameter cgBIntensity = new ClampedFloatParameter(1.0f, 0.0f, 4.0f);
    public ClampedFloatParameter cgBMin = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public ClampedFloatParameter cgBCurve = new ClampedFloatParameter(1.0f, 0.5f, 4.0f);

    [Header("Color Grading Tint")]
    public ColorParameter cgTint = new ColorParameter(new Color(1.0f, 0.95f, 0.85f), false, false, true);
    public ClampedFloatParameter cgTintIntensity = new ClampedFloatParameter(1.0f, 0.0f, 2.0f);
    public ClampedFloatParameter cgTintMix = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);

    [Header("Saturation and Vibrance")]
    public ClampedFloatParameter saturation = new ClampedFloatParameter(1.05f, 0.0f, 2.0f);
    public ClampedFloatParameter vibrance = new ClampedFloatParameter(1.1f, 0.0f, 2.0f);

    [Header("Vignette")]
    public ClampedFloatParameter vignetteStrength = new ClampedFloatParameter(0.6f, 0.0f, 2.0f);
    public ClampedFloatParameter vignetteSmoothness = new ClampedFloatParameter(0.4f, 0.01f, 1.0f);
    public ColorParameter vignetteColor = new ColorParameter(new Color(0.85f, 0.8f, 0.75f), false, false, true);

    [Header("Film Grain")]
    public ClampedFloatParameter filmGrainIntensity = new ClampedFloatParameter(0.02f, 0.0f, 0.2f);
    public ClampedFloatParameter filmGrainSize = new ClampedFloatParameter(1024.0f, 128.0f, 4096.0f);

    public bool IsActive() => enableEffect.value;
    public bool IsTileCompatible() => false;
}