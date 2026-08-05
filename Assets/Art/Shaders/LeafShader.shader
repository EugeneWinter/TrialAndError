Shader "Custom/LeafShader"
{
    Properties
    {
        _TexArray ("Texture Array", 2DArray) = "" {}
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.55
        _DirectStrength ("Direct Light Strength", Range(0, 2)) = 1.1
        _SkyLightInfluence ("Sky Light Influence", Range(0, 1)) = 0.6
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _SubsurfaceColor ("Subsurface Color", Color) = (0.8, 1.0, 0.4, 1)
        _SubsurfaceStrength ("Subsurface Strength", Range(0, 3)) = 1.2
        _SubsurfacePower ("Subsurface Power", Range(1, 16)) = 4.0
        _InteriorDarkness ("Interior Darkness", Range(0, 1)) = 0.55
        _InteriorSaturation ("Interior Saturation Boost", Range(0, 1)) = 0.35
        _InteriorTint ("Interior Tint", Color) = (0.35, 0.55, 0.25, 1)
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.3
        _WindSpeed ("Wind Speed", Range(0, 5)) = 1.5
        _WindScale ("Wind Scale", Range(0, 2)) = 0.4
        _WindGustStrength ("Wind Gust Strength", Range(0, 1)) = 0.4
        _HueShiftStrength ("Hue Variation", Range(0, 0.15)) = 0.05
        _DarkTint ("Deep Leaf Tint", Color) = (0.85, 0.95, 0.7, 1)
        _LightTint ("Sunlit Leaf Tint", Color) = (1.1, 1.05, 0.85, 1)
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.4
        _ShadowMinBrightness ("Shadow Min Brightness", Range(0, 1)) = 0.55
        _DesaturationAmount ("Desaturation In Shadow", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline" }
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

            TEXTURE2D_ARRAY(_TexArray);
            SAMPLER(sampler_TexArray);

            CBUFFER_START(UnityPerMaterial)
                float _AmbientStrength;
                float _DirectStrength;
                float _SkyLightInfluence;
                float _AlphaCutoff;
                float4 _SubsurfaceColor;
                float _SubsurfaceStrength;
                float _SubsurfacePower;
                float _InteriorDarkness;
                float _InteriorSaturation;
                float4 _InteriorTint;
                float _WindStrength;
                float _WindSpeed;
                float _WindScale;
                float _WindGustStrength;
                float _HueShiftStrength;
                float4 _DarkTint;
                float4 _LightTint;
                float _ShadowStrength;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
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
                float4 vertexData : COLOR;
                float3 positionWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                float variation : TEXCOORD5;
                float3 flatPositionWS : TEXCOORD6;
            };

            float3 ApplyLeafWind(float3 flatWorldPos, float vertexG)
            {
                float time = _Time.y * _WindSpeed;
                float phaseA = flatWorldPos.x * _WindScale + flatWorldPos.z * _WindScale * 0.7 + time;
                float phaseB = flatWorldPos.z * _WindScale * 1.3 - flatWorldPos.x * _WindScale * 0.5 + time * 0.8;
                float gust = lerp(1.0 - _WindGustStrength, 1.0, sin(flatWorldPos.x * 0.05 + flatWorldPos.z * 0.05 + time * 0.3) * 0.5 + 0.5);
                float swayX = (sin(phaseA) + sin(phaseA * 1.7) * 0.5) * _WindStrength * gust;
                float swayZ = (cos(phaseB) + cos(phaseB * 1.3) * 0.4) * _WindStrength * gust;
                float leafDensityFactor = 1.0 - vertexG * 0.6;
                flatWorldPos.x += swayX * leafDensityFactor;
                flatWorldPos.z += swayZ * leafDensityFactor;
                return flatWorldPos;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                flatWorldPos = ApplyLeafWind(flatWorldPos, IN.color.g);
                OUT.flatPositionWS = flatWorldPos;

                float3 curvedWorldPos = ApplyCurvature(flatWorldPos);
                OUT.positionCS = TransformWorldToHClip(curvedWorldPos);
                OUT.positionWS = curvedWorldPos;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                OUT.vertexData = IN.color;
                OUT.viewDirWS = normalize(GetWorldSpaceViewDir(curvedWorldPos));
                OUT.variation = sin(curvedWorldPos.x * 0.11 + 0.5) * sin(curvedWorldPos.z * 0.13 + 1.7) + sin(curvedWorldPos.y * 0.09) * 0.4;
                return OUT;
            }

            half4 frag(Varyings IN, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D_ARRAY(_TexArray, sampler_TexArray, IN.uv.xy, IN.uv.z);
                clip(tex.a - _AlphaCutoff);

                float3 normal = IS_FRONT_VFACE(isFrontFace, true, false) ? IN.normalWS : -IN.normalWS;

                float4 shadowCoord = TransformWorldToShadowCoord(IN.flatPositionWS);
                Light mainLight = GetMainLight(shadowCoord, IN.flatPositionWS, half4(1, 1, 1, 1));
                mainLight.shadowAttenuation = lerp(mainLight.shadowAttenuation, 1.0h, GetMainLightShadowFade(IN.flatPositionWS));

                float shadowAtten = mainLight.shadowAttenuation;
                float shadowFactor = lerp(_ShadowMinBrightness, 1.0, shadowAtten);
                float ndotl = dot(normal, mainLight.direction);
                float halfLambert = saturate(ndotl * 0.5 + 0.5);
                halfLambert *= halfLambert;

                float3 directLight = mainLight.color.rgb * halfLambert * _DirectStrength * shadowFactor;
                float3 ambientLight = lerp(float3(1, 1, 1), _SkyLightColor.rgb, _SkyLightInfluence) * _AmbientStrength * saturate(normal.y * 0.4 + 0.6);
                float sss = pow(saturate(dot(-IN.viewDirWS, mainLight.direction)), _SubsurfacePower) * (1.0 - IN.vertexData.g) * _SubsurfaceStrength;
                float3 finalLight = (directLight + ambientLight) * IN.vertexData.r * (1.0 - IN.vertexData.g * _InteriorDarkness);

                float3 baseColor = tex.rgb * lerp(_DarkTint.rgb, _LightTint.rgb, saturate(IN.variation * 0.5 + 0.5));
                baseColor.g *= 1.0 + IN.variation * _HueShiftStrength;

                float desatAmount = smoothstep(0.25, 1.0, 1.0 - halfLambert * IN.vertexData.r * shadowAtten) * _DesaturationAmount;
                baseColor = ApplyDesaturation(baseColor, _ShadowDesatColor.rgb, desatAmount);

                float3 finalColor = baseColor * finalLight + _SubsurfaceColor.rgb * mainLight.color.rgb * sss;
                finalColor = lerp(finalColor, _HeightFogColor.rgb, ComputeHeightFog(IN.positionWS));
                return half4(WorldColorAdjust(MixFog(finalColor, IN.fogCoord)), 1.0);
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

            TEXTURE2D_ARRAY(_TexArray);
            SAMPLER(sampler_TexArray);

            CBUFFER_START(UnityPerMaterial)
                float _AmbientStrength;
                float _DirectStrength;
                float _SkyLightInfluence;
                float _AlphaCutoff;
                float4 _SubsurfaceColor;
                float _SubsurfaceStrength;
                float _SubsurfacePower;
                float _InteriorDarkness;
                float _InteriorSaturation;
                float4 _InteriorTint;
                float _WindStrength;
                float _WindSpeed;
                float _WindScale;
                float _WindGustStrength;
                float _HueShiftStrength;
                float4 _DarkTint;
                float4 _LightTint;
                float _ShadowStrength;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
            CBUFFER_END

            float3 _LightDirection;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float3 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 uv : TEXCOORD0; };

            float3 ApplyLeafWind(float3 flatWorldPos, float vertexG)
            {
                float time = _Time.y * _WindSpeed;
                float phaseA = flatWorldPos.x * _WindScale + flatWorldPos.z * _WindScale * 0.7 + time;
                float phaseB = flatWorldPos.z * _WindScale * 1.3 - flatWorldPos.x * _WindScale * 0.5 + time * 0.8;
                float gust = lerp(1.0 - _WindGustStrength, 1.0, sin(flatWorldPos.x * 0.05 + flatWorldPos.z * 0.05 + time * 0.3) * 0.5 + 0.5);
                float swayX = (sin(phaseA) + sin(phaseA * 1.7) * 0.5) * _WindStrength * gust;
                float swayZ = (cos(phaseB) + cos(phaseB * 1.3) * 0.4) * _WindStrength * gust;
                float leafDensityFactor = 1.0 - vertexG * 0.6;
                flatWorldPos.x += swayX * leafDensityFactor;
                flatWorldPos.z += swayZ * leafDensityFactor;
                return flatWorldPos;
            }

            Varyings shadowVert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                flatWorldPos = ApplyLeafWind(flatWorldPos, IN.color.g);

                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(flatWorldPos, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                OUT.positionCS = positionCS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 shadowFrag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D_ARRAY(_TexArray, sampler_TexArray, IN.uv.xy, IN.uv.z);
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

            TEXTURE2D_ARRAY(_TexArray);
            SAMPLER(sampler_TexArray);

            CBUFFER_START(UnityPerMaterial)
                float _AmbientStrength;
                float _DirectStrength;
                float _SkyLightInfluence;
                float _AlphaCutoff;
                float4 _SubsurfaceColor;
                float _SubsurfaceStrength;
                float _SubsurfacePower;
                float _InteriorDarkness;
                float _InteriorSaturation;
                float4 _InteriorTint;
                float _WindStrength;
                float _WindSpeed;
                float _WindScale;
                float _WindGustStrength;
                float _HueShiftStrength;
                float4 _DarkTint;
                float4 _LightTint;
                float _ShadowStrength;
                float _ShadowMinBrightness;
                float _DesaturationAmount;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 uv : TEXCOORD0; };

            float3 ApplyLeafWind(float3 flatWorldPos, float vertexG)
            {
                float time = _Time.y * _WindSpeed;
                float phaseA = flatWorldPos.x * _WindScale + flatWorldPos.z * _WindScale * 0.7 + time;
                float phaseB = flatWorldPos.z * _WindScale * 1.3 - flatWorldPos.x * _WindScale * 0.5 + time * 0.8;
                float gust = lerp(1.0 - _WindGustStrength, 1.0, sin(flatWorldPos.x * 0.05 + flatWorldPos.z * 0.05 + time * 0.3) * 0.5 + 0.5);
                float swayX = (sin(phaseA) + sin(phaseA * 1.7) * 0.5) * _WindStrength * gust;
                float swayZ = (cos(phaseB) + cos(phaseB * 1.3) * 0.4) * _WindStrength * gust;
                float leafDensityFactor = 1.0 - vertexG * 0.6;
                flatWorldPos.x += swayX * leafDensityFactor;
                flatWorldPos.z += swayZ * leafDensityFactor;
                return flatWorldPos;
            }

            Varyings depthVert(Attributes IN)
            {
                Varyings OUT;
                float3 flatWorldPos = TransformObjectToWorld(IN.positionOS.xyz);
                flatWorldPos = ApplyLeafWind(flatWorldPos, IN.color.g);

                float3 curvedWorldPos = ApplyCurvature(flatWorldPos);
                OUT.positionCS = TransformWorldToHClip(curvedWorldPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 depthFrag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D_ARRAY(_TexArray, sampler_TexArray, IN.uv.xy, IN.uv.z);
                clip(tex.a - _AlphaCutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}