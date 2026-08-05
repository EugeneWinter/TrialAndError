Shader "Custom/WaterShader"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.15, 0.5, 0.65, 0.5)
        _DeepColor ("Deep Color", Color) = (0.03, 0.12, 0.3, 0.88)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveScale ("Wave Scale", Float) = 0.08
        _WaveFrequency ("Wave Frequency", Float) = 3.0
        _RippleScale ("Ripple Scale", Float) = 0.3
        _RippleSpeed ("Ripple Speed", Float) = 0.8
        _FresnelPower ("Fresnel Power", Float) = 3.0
        _SpecularPower ("Specular Power", Float) = 64.0
        _SpecularIntensity ("Specular Intensity", Float) = 0.6
        _SkyReflectionStrength ("Sky Reflection", Range(0, 1)) = 0.35
        _SubsurfaceStrength ("Subsurface Scattering", Range(0, 1)) = 0.25
        _SubsurfaceColor ("Subsurface Color", Color) = (0.1, 0.6, 0.4, 1)
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "WaterForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "SharedWorldLighting.hlsl"

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
                float3 worldPos : TEXCOORD0;
                half3 worldNormal : TEXCOORD1;
                half3 viewDir : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                float3 flatPositionWS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                float _WaveSpeed;
                float _WaveScale;
                float _WaveFrequency;
                float _RippleScale;
                float _RippleSpeed;
                half _FresnelPower;
                half _SpecularPower;
                half _SpecularIntensity;
                half _SkyReflectionStrength;
                half _SubsurfaceStrength;
                half4 _SubsurfaceColor;
                float _ShadowStrength;
            CBUFFER_END

            half fast_hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            half simple_noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                half a = fast_hash(i);
                half b = fast_hash(i + float2(1.0, 0.0));
                half c = fast_hash(i + float2(0.0, 1.0));
                half d = fast_hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float SampleWaveHeight(float2 xz, float time)
            {
                float2 wc = xz * _WaveFrequency;
                float wave1 = sin(wc.x + time * 1.1) * cos(wc.y * 0.7 + time * 0.9);
                float wave2 = sin(wc.x * 1.3 - time * 0.8) * 0.5;
                float wave3 = sin(wc.x * 0.7 + wc.y * 1.3 + time * 0.6) * 0.3;
                return (wave1 + wave2 + wave3) * _WaveScale;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float time = _Time.y * _WaveSpeed;

                float h  = SampleWaveHeight(worldPos.xz, time);
                float eps = 0.15;
                float hX = SampleWaveHeight(worldPos.xz + float2(eps, 0), time);
                float hZ = SampleWaveHeight(worldPos.xz + float2(0, eps), time);

                worldPos.y += h;
                output.flatPositionWS = worldPos;

                float3 tangentX = float3(eps, hX - h, 0);
                float3 tangentZ = float3(0, hZ - h, eps);
                output.worldNormal = half3(normalize(cross(tangentZ, tangentX)));

                worldPos = ApplyCurvature(worldPos);

                output.worldPos = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.viewDir = half3(normalize(GetWorldSpaceViewDir(worldPos)));
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half time = half(_Time.y * _RippleSpeed);
                half2 rippleUV = half2(input.worldPos.xz * _RippleScale);

                half n1 = simple_noise(rippleUV + time * float2(0.17, 0.23));
                half n2 = simple_noise(rippleUV * 1.7 - time * float2(0.13, 0.19));
                half noiseSum = (n1 + n2) * 0.5;

                half3 normal = normalize(input.worldNormal + half3((n1 - 0.5) * 0.15, 0, (n2 - 0.5) * 0.15));

                half nv = saturate(dot(normal, input.viewDir));
                half fresnel = pow(1.0 - nv, _FresnelPower);

                half4 color = lerp(_ShallowColor, _DeepColor, fresnel);

                half3 skyColor = _SkyLightColor.rgb;
                color.rgb += skyColor * fresnel * _SkyReflectionStrength;
                color.rgb *= lerp(half3(1,1,1), skyColor, 0.15);

                float4 shadowCoord = TransformWorldToShadowCoord(input.flatPositionWS);
                Light mainLight = GetMainLight(shadowCoord, input.flatPositionWS, half4(1, 1, 1, 1));
                mainLight.shadowAttenuation = lerp(mainLight.shadowAttenuation, 1.0h, GetMainLightShadowFade(input.flatPositionWS));

                half shadowAtten = mainLight.shadowAttenuation;
                half shadowFactor = lerp(1.0 - _ShadowStrength, 1.0, shadowAtten);

                half3 halfDir = normalize(half3(mainLight.direction) + input.viewDir);
                half nh = saturate(dot(normal, halfDir));
                half spec = pow(nh, _SpecularPower);
                color.rgb += mainLight.color.rgb * spec * _SpecularIntensity * shadowFactor;

                half sss = saturate(dot(-input.viewDir, mainLight.direction));
                sss = pow(sss, 3.0) * _SubsurfaceStrength;
                color.rgb += _SubsurfaceColor.rgb * mainLight.color.rgb * sss * shadowFactor;

                color.rgb *= shadowFactor;

                color.rgb += noiseSum * 0.04;

                half edgeFoam = saturate(noiseSum * 4.0 - 2.5);
                color.rgb += edgeFoam * 0.08;

                color.a = lerp(_ShallowColor.a, _DeepColor.a, fresnel);

                float heightFogAmount = ComputeHeightFog(input.worldPos);
                color.rgb = lerp(color.rgb, _HeightFogColor.rgb, heightFogAmount);

                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}