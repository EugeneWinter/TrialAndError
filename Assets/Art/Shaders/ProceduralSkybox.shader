Shader "Custom/ProceduralSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.2, 0.4, 0.8, 1)
        _MiddleColor ("Middle Color", Color) = (0.5, 0.7, 1.0, 1)
        _BottomColor ("Bottom Color", Color) = (0.8, 0.85, 0.9, 1)
        _MiddleHeight ("Middle Height", Range(-0.5, 0.5)) = 0.0

        _SunDir ("Sun Direction", Vector) = (0, 1, 0, 0)
        _SunAngularRadius ("Sun Angular Radius (deg)", Range(0.5, 20.0)) = 5.0
        _SunGlowRadius ("Sun Glow Radius (deg)", Range(1.0, 60.0)) = 20.0
        _SunGlowIntensity ("Sun Glow Intensity", Range(0, 4)) = 1.0
        _SunColor ("Sun Color Override", Color) = (1, 0.95, 0.8, 1)

        _MoonAngularRadius ("Moon Angular Radius (deg)", Range(0.5, 15.0)) = 3.0
        _MoonGlowRadius ("Moon Glow Radius (deg)", Range(1.0, 40.0)) = 12.0
        _MoonColor ("Moon Color", Color) = (0.7, 0.8, 1.0, 1)
        _MoonDir ("Moon Direction", Vector) = (0.3, 0.5, -0.8, 0)
        _MoonGlowIntensity ("Moon Glow Intensity", Range(0, 3)) = 0.6

        _StarIntensity ("Star Intensity", Range(0, 1)) = 0
        _StarDensity ("Star Density", Range(50, 500)) = 200
        _StarBrightness ("Star Brightness", Range(0, 5)) = 1.5
        _StarTwinkleSpeed ("Star Twinkle Speed", Range(0, 10)) = 3.0

        _HorizonGlowStrength ("Horizon Glow Strength", Range(0, 1)) = 0.3
        _HorizonGlowFalloff ("Horizon Glow Falloff", Range(1, 20)) = 5.0

        _CataclysmTint ("Cataclysm Tint", Color) = (0, 0, 0, 0)
        _CataclysmStrength ("Cataclysm Strength", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _TopColor;
            float4 _MiddleColor;
            float4 _BottomColor;
            float _MiddleHeight;

            float4 _SunDir;
            float _SunAngularRadius;
            float _SunGlowRadius;
            float _SunGlowIntensity;
            float4 _SunColor;

            float _MoonAngularRadius;
            float _MoonGlowRadius;
            float4 _MoonColor;
            float4 _MoonDir;
            float _MoonGlowIntensity;

            float _StarIntensity;
            float _StarDensity;
            float _StarBrightness;
            float _StarTwinkleSpeed;

            float _HorizonGlowStrength;
            float _HorizonGlowFalloff;

            float4 _CataclysmTint;
            float _CataclysmStrength;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.viewDir = IN.positionOS.xyz;
                return OUT;
            }

            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float stars(float3 dir, float time)
            {
                float3 p = dir * _StarDensity;
                float3 fp = floor(p);
                float h = hash(fp);

                if (h > 0.995)
                {
                    float3 localPos = frac(p) - 0.5;
                    float d = length(localPos);
                    float brightness = smoothstep(0.4, 0.0, d);
                    float twinkle = sin(time * _StarTwinkleSpeed + h * 137.0) * 0.3 + 0.7;
                    float sizeFactor = (h - 0.995) * 200.0;
                    return brightness * _StarBrightness * sizeFactor * twinkle;
                }
                return 0;
            }

            float AngularDist(float3 a, float3 b)
            {
                return degrees(acos(clamp(dot(a, b), -1.0, 1.0)));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.viewDir);
                float height = dir.y;

                half3 color;
                if (height > _MiddleHeight)
                {
                    float t = smoothstep(_MiddleHeight, 1.0, height);
                    t = pow(t, 0.6);
                    color = lerp(_MiddleColor.rgb, _TopColor.rgb, t);
                }
                else
                {
                    float t = smoothstep(_MiddleHeight, -1.0, height);
                    t = pow(t, 0.8);
                    color = lerp(_MiddleColor.rgb, _BottomColor.rgb, t);
                }

                float horizonFactor = exp(-abs(height) * _HorizonGlowFalloff);
                color += _MiddleColor.rgb * horizonFactor * _HorizonGlowStrength;

                Light mainLight = GetMainLight();
                float3 sunDir = normalize(_SunDir.xyz);
                float3 moonDir = normalize(_MoonDir.xyz);

                float sunAngle = AngularDist(dir, sunDir);
                float moonAngle = AngularDist(dir, moonDir);

                float sunGlow = 1.0 - smoothstep(_SunAngularRadius, _SunGlowRadius, sunAngle);
                color += _SunColor.rgb * mainLight.color.rgb * sunGlow * _SunGlowIntensity;

                float sunHorizon = sunGlow * horizonFactor;
                color += _SunColor.rgb * mainLight.color.rgb * sunHorizon * 0.4;

                float moonGlow = 1.0 - smoothstep(_MoonAngularRadius, _MoonGlowRadius, moonAngle);
                color += _MoonColor.rgb * moonGlow * _MoonGlowIntensity * _StarIntensity;

                if (_StarIntensity > 0.01 && height > -0.1)
                {
                    float starBright = stars(dir, _Time.y);
                    float horizonMask = smoothstep(-0.1, 0.15, height);
                    float sunMask = smoothstep(0.0, _SunGlowRadius * 1.5, sunAngle);
                    float moonMask = smoothstep(0.0, _MoonGlowRadius * 1.5, moonAngle);
                    color += starBright * _StarIntensity * horizonMask * sunMask * moonMask;
                }

                if (_CataclysmStrength > 0.001)
                {
                    color = lerp(color, _CataclysmTint.rgb, _CataclysmStrength * 0.5);
                    color += _CataclysmTint.rgb * horizonFactor * _CataclysmStrength * 0.3;
                }

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}