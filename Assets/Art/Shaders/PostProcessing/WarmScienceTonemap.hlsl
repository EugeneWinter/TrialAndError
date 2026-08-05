#ifndef WARM_SCIENCE_TONEMAP_INCLUDED
#define WARM_SCIENCE_TONEMAP_INCLUDED

float3 ApplyDesaturationMatrix(float3 color, float amount)
{
    float a = 0.03 * amount;
    float b = 0.01 * amount;
    float c = 1.0 - (a + b);
    float d = 1.0 - (a + a);

    float3 result;
    result.r = color.r * c + color.g * a + color.b * b;
    result.g = color.r * a + color.g * d + color.b * a;
    result.b = color.r * b + color.g * a + color.b * c;
    return result;
}

float3 ApplyInverseDesaturationMatrix(float3 color, float amount)
{
    float a = 0.03 * amount;
    float b = 0.01 * amount;
    float c = 1.0 - (a + b);
    float d = 1.0 - (a + a);

    float det = c * (d * c - a * a) - a * (a * c - a * b) + b * (a * a - d * b);
    float invDet = 1.0 / det;

    float m00 = (d * c - a * a) * invDet;
    float m01 = -(a * c - a * b) * invDet;
    float m02 = (a * a - d * b) * invDet;
    float m10 = -(a * c - a * b) * invDet;
    float m11 = (c * c - b * b) * invDet;
    float m12 = -(c * a - b * a) * invDet;
    float m20 = (a * a - d * b) * invDet;
    float m21 = -(c * a - a * b) * invDet;
    float m22 = (c * d - a * a) * invDet;

    float3 result;
    result.r = color.r * m00 + color.g * m01 + color.b * m02;
    result.g = color.r * m10 + color.g * m11 + color.b * m12;
    result.b = color.r * m20 + color.g * m21 + color.b * m22;
    return result;
}

float3 WarmScienceTonemap(
    float3 color,
    float exposure,
    float whitePathAmount,
    float whiteCurve,
    float lowerCurve,
    float upperCurve)
{
    color *= exp2(exposure);

    color = ApplyDesaturationMatrix(color, whitePathAmount);

    float3 whiteBalanced = pow(color, whiteCurve) + 1.0;
    color = color / pow(whiteBalanced, 1.0 / whiteCurve);

    float3 curveBlend = lerp(lowerCurve.xxx, upperCurve.xxx, sqrt(saturate(color)));
    color = pow(saturate(color), curveBlend);

    color = ApplyInverseDesaturationMatrix(color, whitePathAmount);

    return saturate(color);
}

float3 ApplyColorGrading(
    float3 color,
    float3 rMul, float rIntensity, float rMin, float rCurve,
    float3 gMul, float gIntensity, float gMin, float gCurve,
    float3 bMul, float bIntensity, float bMin, float bCurve,
    float3 tint, float tintIntensity, float tintMix)
{
    float3 graded =
        pow(saturate(color.r), rCurve) * rMul +
        pow(saturate(color.g), gCurve) * gMul +
        pow(saturate(color.b), bCurve) * bMul;

    float3 mins = float3(rMin, gMin, bMin);
    float3 intensities = float3(rIntensity, gIntensity, bIntensity);
    graded = (graded * (1.0 - mins) + mins) * intensities;

    float luma = dot(graded, float3(0.299, 0.587, 0.114));
    float3 tinted = tint * luma * tintIntensity;
    graded = lerp(graded, tinted, tintMix);

    return graded;
}

float3 ApplySaturationVibrance(float3 color, float saturation, float vibrance)
{
    float grayVibrance = (color.r + color.g + color.b) / 3.0;
    float graySaturation = dot(color, float3(0.299, 0.587, 0.114));

    float mn = min(color.r, min(color.g, color.b));
    float mx = max(color.r, max(color.g, color.b));
    float sat = (1.0 - (mx - mn)) * (1.0 - mx) * grayVibrance * 5.0;
    float3 lightness = float3((mn + mx) * 0.5, (mn + mx) * 0.5, (mn + mx) * 0.5);

    color = lerp(color, lerp(color, lightness, 1.0 - vibrance), sat);
    color = lerp(color, lightness, (1.0 - lightness) * (2.0 - vibrance) / 2.0 * abs(vibrance - 1.0));
    color = color * saturation - graySaturation.xxx * (saturation - 1.0);

    return color;
}

float FilmGrainHash(float2 p, float t)
{
    return frac(sin(dot(p + t, float2(12.9898, 78.233))) * 43758.5453);
}

#endif