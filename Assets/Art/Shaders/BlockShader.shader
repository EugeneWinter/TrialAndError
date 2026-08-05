Shader "Custom/BlockShader"
{
    Properties
    {
        _TexArray ("Texture Array", 2DArray) = "" {}
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.55
        _DirectStrength ("Direct Light Strength", Range(0, 2)) = 1.1
        _SkyLightInfluence ("Sky Light Influence", Range(0, 1)) = 0.6
        _ShadowSaturationBoost ("Shadow Saturation Boost", Range(0, 0.5)) = 0.15
        _ShadowWarmth ("Shadow Warmth", Range(-0.1, 0.1)) = 0.02
        _WorldVariation ("World Color Variation", Range(0, 0.1)) = 0.025
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.55
        _ShadowColor ("Shadow Color Tint", Color) = (0.6, 0.7, 0.9, 1)
        _ShadowMinBrightness ("Shadow Min Brightness", Range(0, 1)) = 0.5
        _DesaturationAmount ("Desaturation In Shadow", Range(0, 1)) = 0.3
        _HighlightTameAmount ("Highlight Tame Amount", Range(0, 1)) = 0.4
        _HighlightTameThreshold ("Highlight Tame Threshold", Range(0.3, 1.0)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "SharedWorldLighting.hlsl"

            TEXTURE2D_ARRAY(_TexArray);
            SAMPLER(sampler_TexArray);

            CBUFFER_START(UnityPerMaterial)
                float _AmbientStrength;
                float _DirectStrength;
                float _SkyLightInfluence;
                float _ShadowSaturationBoost;
                float _ShadowWarmth;
                float _WorldVariation;
                float _ShadowStrength;
                float4 _ShadowColor;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
                float _HighlightTameAmount;
                float _HighlightTameThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float3 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogCoord : TEXCOORD2;
                float4 vertexLight : COLOR;
                float3 positionWS : TEXCOORD3;
                float3 flatPositionWS : TEXCOORD4;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.flatPositionWS = flatWorldPos;

                float3 curvedWorldPos = ApplyCurvature(flatWorldPos);
                OUT.positionCS = TransformWorldToHClip(curvedWorldPos);
                OUT.positionWS = curvedWorldPos;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                OUT.vertexLight = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D_ARRAY(_TexArray, sampler_TexArray, IN.uv.xy, IN.uv.z);

                float4 shadowCoord = TransformWorldToShadowCoord(IN.flatPositionWS);
                Light mainLight = GetMainLight(shadowCoord, IN.flatPositionWS, half4(1, 1, 1, 1));
                mainLight.shadowAttenuation = lerp(mainLight.shadowAttenuation, 1.0h, GetMainLightShadowFade(IN.flatPositionWS));

                float shadowAtten = mainLight.shadowAttenuation;
                float ndotl = dot(IN.normalWS, mainLight.direction);
                float halfLambert = saturate(ndotl * 0.5 + 0.5);
                halfLambert *= halfLambert;

                float shadowFactor = lerp(_ShadowMinBrightness, 1.0, shadowAtten);
                shadowFactor = lerp(1.0, shadowFactor, _ShadowStrength);

                float3 directLight = mainLight.color.rgb * halfLambert * _DirectStrength * shadowFactor;
                float3 ambientBase = lerp(float3(1, 1, 1), _SkyLightColor.rgb, _SkyLightInfluence);
                float skyFactor = saturate(IN.normalWS.y * 0.4 + 0.6);
                float3 ambientLight = ambientBase * _AmbientStrength * skyFactor;

                float3 totalLight = (directLight + ambientLight) * IN.vertexLight.r * VanillaDiffuse(IN.normalWS);
                totalLight = max(totalLight, float3(0.02, 0.025, 0.03));

                float3 baseColor = tex.rgb;
                float variation = sin(IN.positionWS.x * 0.07 + 0.3) * sin(IN.positionWS.z * 0.09 + 0.7) * _WorldVariation;
                baseColor *= 1.0 + variation;

                float desatShadowAmount = smoothstep(0.25, 1.0, 1.0 - halfLambert * IN.vertexLight.r * shadowAtten) * _DesaturationAmount;
                baseColor = ApplyDesaturation(baseColor, _ShadowDesatColor.rgb, desatShadowAmount);

                float3 finalColor = baseColor * totalLight;
                finalColor = lerp(finalColor, finalColor * _ShadowColor.rgb, (1.0 - shadowAtten) * _ShadowStrength * 0.25);
                finalColor = lerp(finalColor, _HeightFogColor.rgb, ComputeHeightFog(IN.positionWS));
                finalColor = MixFog(finalColor, IN.fogCoord);

                return half4(WorldColorAdjust(finalColor), 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #pragma multi_compile_shadowcaster
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "SharedWorldLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _AmbientStrength;
                float _DirectStrength;
                float _SkyLightInfluence;
                float _ShadowSaturationBoost;
                float _ShadowWarmth;
                float _WorldVariation;
                float _ShadowStrength;
                float4 _ShadowColor;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
                float _HighlightTameAmount;
                float _HighlightTameThreshold;
            CBUFFER_END

            float3 _LightDirection;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings shadowVert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(flatWorldPos, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                OUT.positionCS = positionCS;
                return OUT;
            }

            half4 shadowFrag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma vertex depthVert
            #pragma fragment depthFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SharedWorldLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _AmbientStrength;
                float _DirectStrength;
                float _SkyLightInfluence;
                float _ShadowSaturationBoost;
                float _ShadowWarmth;
                float _WorldVariation;
                float _ShadowStrength;
                float4 _ShadowColor;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
                float _HighlightTameAmount;
                float _HighlightTameThreshold;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings depthVert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 curvedWorldPos = ApplyCurvature(flatWorldPos);
                OUT.positionCS = TransformWorldToHClip(curvedWorldPos);
                return OUT;
            }

            half4 depthFrag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}