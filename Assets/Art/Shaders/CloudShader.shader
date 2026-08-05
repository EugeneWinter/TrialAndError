Shader "Custom/CloudShader"
{
    Properties
    {
        _Color ("Cloud Color", Color) = (1, 1, 1, 0.85)
        _ShadowColor ("Shadow Color", Color) = (0.7, 0.75, 0.85, 0.7)
        _RimPower ("Rim Power", Range(1, 8)) = 3.0
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.3
        _TranslucencyStrength ("Translucency", Range(0, 1)) = 0.25
        _SkyTintStrength ("Sky Tint Strength", Range(0, 1)) = 0.5
        _OpacityBoost ("Opacity Boost (for sun occlusion)", Range(1.0, 4.0)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend One OneMinusSrcAlpha
        ZWrite On
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "SharedWorldLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ShadowColor;
                float _RimPower;
                float _RimStrength;
                float _TranslucencyStrength;
                float _SkyTintStrength;
                float _OpacityBoost;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                worldPos = ApplyCurvature(worldPos);

                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = normalize(GetWorldSpaceViewDir(worldPos));
                OUT.positionWS = worldPos;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light mainLight = GetMainLight();
                float ndotl = saturate(dot(IN.normalWS, mainLight.direction));

                half3 baseColor = lerp(_ShadowColor.rgb, _Color.rgb, ndotl);
                half alpha = lerp(_ShadowColor.a, _Color.a, ndotl);

                half3 skyTint = lerp(half3(1,1,1), _SkyLightColor.rgb, _SkyTintStrength);
                baseColor *= skyTint;
                baseColor *= lerp(half3(1,1,1), mainLight.color.rgb, 0.6);

                float rim = 1.0 - saturate(dot(IN.normalWS, IN.viewDirWS));
                rim = pow(rim, _RimPower);
                baseColor += mainLight.color.rgb * rim * _RimStrength;

                float translucency = saturate(dot(-IN.viewDirWS, mainLight.direction));
                translucency = pow(translucency, 2.0) * _TranslucencyStrength;
                baseColor += mainLight.color.rgb * translucency;

                float3 camPos = GetCameraPositionWS();
                float dist = distance(IN.positionWS, camPos);
                float distanceFade = saturate(dist / 300.0);
                alpha *= lerp(1.0, 0.6, distanceFade);

                alpha = saturate(alpha * _OpacityBoost);

                baseColor = MixFog(baseColor, IN.fogCoord);

                return half4(baseColor * alpha, alpha);
            }
            ENDHLSL
        }
    }
}