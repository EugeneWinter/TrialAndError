Shader "Custom/GroundItemShader"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.5
        _ShadowMinBrightness ("Shadow Min Brightness", Range(0, 1)) = 0.55
        _DesaturationAmount ("Desaturation In Shadow", Range(0, 1)) = 0.3
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
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "SharedWorldLighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _ShadowStrength;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogCoord : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float3 flatPositionWS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.flatPositionWS = flatWorldPos;

                float3 curvedWorldPos = ApplyCurvature(flatWorldPos);
                OUT.positionCS = TransformWorldToHClip(curvedWorldPos);
                OUT.positionWS = curvedWorldPos;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                float4 shadowCoord = TransformWorldToShadowCoord(IN.flatPositionWS);
                Light mainLight = GetMainLight(shadowCoord, IN.flatPositionWS, half4(1, 1, 1, 1));
                mainLight.shadowAttenuation = lerp(mainLight.shadowAttenuation, 1.0h, GetMainLightShadowFade(IN.flatPositionWS));

                float shadowAtten = mainLight.shadowAttenuation;
                float shadowFactor = lerp(_ShadowMinBrightness, 1.0, shadowAtten);
                shadowFactor = lerp(1.0, shadowFactor, _ShadowStrength);

                float ndotl = dot(IN.normalWS, mainLight.direction);
                float halfLambert = saturate(ndotl * 0.5 + 0.5);
                halfLambert *= halfLambert;

                float3 directLight = mainLight.color.rgb * halfLambert * shadowFactor;
                float skyFactor = saturate(IN.normalWS.y * 0.4 + 0.6);
                float3 ambientLight = lerp(float3(1, 1, 1), _SkyLightColor.rgb, 0.6) * 0.5 * skyFactor;

                float3 totalLight = (directLight + ambientLight) * VanillaDiffuse(IN.normalWS);
                totalLight = max(totalLight, float3(0.02, 0.025, 0.03));

                float shadowAmount = 1.0 - halfLambert;
                float3 baseColor = tex.rgb;
                baseColor = lerp(baseColor, BoostSaturation(baseColor, 0.15), shadowAmount);
                baseColor.r += 0.02 * shadowAmount;
                baseColor.b -= 0.01 * shadowAmount;

                float desatShadowAmount = smoothstep(0.25, 1.0, 1.0 - halfLambert * shadowAtten) * _DesaturationAmount;
                baseColor = ApplyDesaturation(baseColor, _ShadowDesatColor.rgb, desatShadowAmount);

                float3 finalColor = baseColor * totalLight;
                finalColor = lerp(finalColor, _HeightFogColor.rgb, ComputeHeightFog(IN.positionWS));
                finalColor = MixFog(finalColor, IN.fogCoord);

                return half4(WorldColorAdjust(finalColor), tex.a);
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
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "SharedWorldLighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _ShadowStrength;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
            CBUFFER_END

            float3 _LightDirection;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings shadowVert(Attributes IN) {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
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

            half4 shadowFrag(Varyings IN) : SV_Target { return 0; }
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
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "SharedWorldLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _ShadowStrength;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings depthVert(Attributes IN) {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
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