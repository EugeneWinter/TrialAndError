#ifndef SHARED_WORLD_LIGHTING_INCLUDED
#define SHARED_WORLD_LIGHTING_INCLUDED

float _GlobalCurvature;
float _CurvatureStartDistance;
float4 _SkyLightColor;
float4 _ShadowDesatColor;
float4 _WorldEastVec;
float _WorldContrast;
float _WorldSaturation;
float _WorldBrightnessPunch;
float4 _HeightFogColor;
float _HeightFogDensity;
float _HeightFogBottom;
float _HeightFogTop;
float _RealtimeShadowFadeStart;
float _RealtimeShadowFadeEnd;

float3 ApplyCurvatureImpl(float3 worldPos, float3 cameraPosWS)
{
    float2 delta = worldPos.xz - cameraPosWS.xz;
    float distSq = dot(delta, delta);
    float start = _CurvatureStartDistance;
    float startSq = start * start;
    float excess = max(0.0, distSq - startSq);
    float smoothFactor = excess / (excess + startSq * 0.5 + 1.0);
    worldPos.y -= distSq * _GlobalCurvature * smoothFactor;
    return worldPos;
}

float VanillaDiffuse(float3 normalWS)
{
    float3 upVec = float3(0, 1, 0);
    float3 eastVec = float3(1, 0, 0);
    float NoU = clamp(dot(normalWS, upVec), -1.0, 1.0);
    float NoE = clamp(dot(normalWS, eastVec), -1.0, 1.0);
    float diffuse = (0.25 * NoU + 0.75) + (0.667 - abs(NoE)) * (1.0 - abs(NoU)) * 0.15;
    return diffuse * diffuse;
}

float3 WorldColorAdjust(float3 color)
{
    float3 adjusted = color + _WorldBrightnessPunch;
    float3 midPoint = 0.5;
    adjusted = (adjusted - midPoint) * _WorldContrast + midPoint;
    adjusted = max(adjusted, 0.0);
    float luma = dot(adjusted, float3(0.299, 0.587, 0.114));
    adjusted = lerp(luma.xxx, adjusted, _WorldSaturation);
    return adjusted;
}

float ComputeHeightFogImpl(float3 worldPos, float3 cameraPos)
{
    float heightRange = max(_HeightFogTop - _HeightFogBottom, 0.001);
    float heightT = saturate((worldPos.y - _HeightFogBottom) / heightRange);
    float heightFactor = 1.0 - heightT;
    heightFactor = heightFactor * heightFactor;
    float dist = distance(worldPos, cameraPos);
    float distFactor = 1.0 - exp(-dist * 0.008);
    return saturate(heightFactor * distFactor * _HeightFogDensity);
}

float3 BoostSaturation(float3 color, float boost)
{
    float grey = dot(color, float3(0.299, 0.587, 0.114));
    return lerp(float3(grey, grey, grey), color, 1.0 + boost);
}

float3 ApplyDesaturation(float3 color, float3 desatTarget, float amount)
{
    float luma = dot(color, float3(0.299, 0.587, 0.114));
    float3 desaturated = luma * desatTarget * 1.7;
    return lerp(color, desaturated, amount);
}

float ApplyRealtimeShadowFade(float shadowAtten, float3 positionWS)
{
    float dist = distance(positionWS, GetCameraPositionWS());
    float shadowWeight = 1.0 - smoothstep(_RealtimeShadowFadeStart, _RealtimeShadowFadeEnd, dist);
    return lerp(1.0, shadowAtten, shadowWeight);
}

#define ApplyCurvature(worldPos) ApplyCurvatureImpl(worldPos, GetCameraPositionWS())
#define ComputeHeightFog(worldPos) ComputeHeightFogImpl(worldPos, GetCameraPositionWS())

#endif