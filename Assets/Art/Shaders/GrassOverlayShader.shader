Shader "Custom/GrassOverlayShader"
{
    Properties
    {
        _MainTex ("Grass Overlay Texture", 2D) = "white" {}
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
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _WindStrength ("Wind Strength", Range(0, 0.5)) = 0.08
        _WindSpeed ("Wind Speed", Range(0, 5)) = 2.0
        _WindScale ("Wind Scale", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest+10" "RenderPipeline"="UniversalPipeline" }
        Cull Off

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
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
                float _AlphaCutoff;
                float _WindStrength;
                float _WindSpeed;
                float _WindScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogCoord : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float4 vertexData : COLOR;
                float3 flatPositionWS : TEXCOORD4;
            };

            float3 ApplyGrassWind(float3 flatWorldPos, float vertexB)
            {
                float time = _Time.y * _WindSpeed;
                float sway = sin(flatWorldPos.x * _WindScale + time) * _WindStrength * vertexB;
                flatWorldPos.x += sway;
                return flatWorldPos;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                flatWorldPos = ApplyGrassWind(flatWorldPos, IN.color.b);
                OUT.flatPositionWS = flatWorldPos;

                float3 curvedWorldPos = ApplyCurvature(flatWorldPos);
                OUT.positionCS = TransformWorldToHClip(curvedWorldPos);
                OUT.positionWS = curvedWorldPos;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                OUT.vertexData = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(tex.a - _AlphaCutoff);

                float3 normal = IS_FRONT_VFACE(isFrontFace, true, false) ? IN.normalWS : -IN.normalWS;

                float4 shadowCoord = TransformWorldToShadowCoord(IN.flatPositionWS);
                Light mainLight = GetMainLight(shadowCoord, IN.flatPositionWS, half4(1, 1, 1, 1));
                mainLight.shadowAttenuation = lerp(mainLight.shadowAttenuation, 1.0h, GetMainLightShadowFade(IN.flatPositionWS));

                float shadowAtten = mainLight.shadowAttenuation;
                float ndotl = dot(normal, mainLight.direction);
                float halfLambert = saturate(ndotl * 0.5 + 0.5);
                halfLambert *= halfLambert;

                float shadowFactor = lerp(_ShadowMinBrightness, 1.0, shadowAtten);
                shadowFactor = lerp(1.0, shadowFactor, _ShadowStrength);

                float3 directLight = mainLight.color.rgb * halfLambert * _DirectStrength * shadowFactor;
                float3 ambientBase = lerp(float3(1, 1, 1), _SkyLightColor.rgb, _SkyLightInfluence);
                float skyFactor = saturate(normal.y * 0.4 + 0.6);
                float3 ambientLight = ambientBase * _AmbientStrength * skyFactor;

                float aoLight = IN.vertexData.r;
                float3 totalLight = (directLight + ambientLight) * aoLight * VanillaDiffuse(normal);
                totalLight = max(totalLight, float3(0.02, 0.025, 0.03));

                float3 baseColor = tex.rgb;
                float variation = sin(IN.positionWS.x * 0.07 + 0.3) * sin(IN.positionWS.z * 0.09 + 0.7) * _WorldVariation;
                baseColor *= 1.0 + variation;

                float desatShadowAmount = smoothstep(0.25, 1.0, 1.0 - halfLambert * aoLight * shadowAtten) * _DesaturationAmount;
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
            ZWrite On ZTest LEqual ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #pragma multi_compile_shadowcaster
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "SharedWorldLighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
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
                float _AlphaCutoff;
                float _WindStrength;
                float _WindSpeed;
                float _WindScale;
            CBUFFER_END

            float3 _LightDirection;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float3 ApplyGrassWind(float3 flatWorldPos, float vertexB)
            {
                float time = _Time.y * _WindSpeed;
                float sway = sin(flatWorldPos.x * _WindScale + time) * _WindStrength * vertexB;
                flatWorldPos.x += sway;
                return flatWorldPos;
            }

            Varyings shadowVert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                flatWorldPos = ApplyGrassWind(flatWorldPos, IN.color.b);

                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(flatWorldPos, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                OUT.positionCS = positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 shadowFrag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(tex.a - _AlphaCutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma vertex depthVert
            #pragma fragment depthFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SharedWorldLighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
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
                float _AlphaCutoff;
                float _WindStrength;
                float _WindSpeed;
                float _WindScale;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float3 ApplyGrassWind(float3 flatWorldPos, float vertexB)
            {
                float time = _Time.y * _WindSpeed;
                float sway = sin(flatWorldPos.x * _WindScale + time) * _WindStrength * vertexB;
                flatWorldPos.x += sway;
                return flatWorldPos;
            }

            Varyings depthVert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                flatWorldPos = ApplyGrassWind(flatWorldPos, IN.color.b);

                float3 curvedWorldPos = ApplyCurvature(flatWorldPos);
                OUT.positionCS = TransformWorldToHClip(curvedWorldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 depthFrag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(tex.a - _AlphaCutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}