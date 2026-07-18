Shader "Custom/WaterShader"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.15, 0.45, 0.65, 0.55)
        _DeepColor ("Deep Color", Color) = (0.03, 0.1, 0.25, 0.9)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveScale ("Wave Scale", Float) = 0.08
        _WaveFrequency ("Wave Frequency", Float) = 3.0
        _RippleScale ("Ripple Scale", Float) = 0.3
        _RippleSpeed ("Ripple Speed", Float) = 0.8
        _FresnelPower ("Fresnel Power", Float) = 3.0
        _SpecularPower ("Specular Power", Float) = 64.0
        _SpecularIntensity ("Specular Intensity", Float) = 0.5
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "LightMode"="UniversalForward" 
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        ColorMask RGBA

        Pass
        {
            Name "WaterForward"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float _WaveSpeed;
                float _WaveScale;
                float _WaveFrequency;
                float _RippleScale;
                float _RippleSpeed;
                float _FresnelPower;
                float _SpecularPower;
                float _SpecularIntensity;
            CBUFFER_END

            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float waterNoise(float2 uv, float time)
            {
                float n = 0.0;
                float2 flow1 = float2(time * 0.3, time * 0.2);
                float2 flow2 = float2(-time * 0.25, time * 0.35);

                n += noise((uv + flow1) * 4.0) * 0.5;
                n += noise((uv + flow2) * 8.0) * 0.25;
                n += noise((uv * 2.1 + flow1 * 1.5) * 16.0) * 0.125;

                return n;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                float time = _Time.y * _WaveSpeed;

                float wave1 = sin(worldPos.x * _WaveFrequency + time * 1.1) * 
                              cos(worldPos.z * _WaveFrequency * 0.7 + time * 0.9);
                float wave2 = sin(worldPos.x * _WaveFrequency * 1.3 - time * 0.8 + worldPos.z * 0.5) * 0.5;
                float ripple = waterNoise(worldPos.xz * _RippleScale, time * _RippleSpeed) * 2.0 - 1.0;

                float displacement = (wave1 + wave2) * _WaveScale + ripple * _WaveScale * 0.3;

                worldPos.y += displacement;

                output.worldPos = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.viewDir = normalize(GetWorldSpaceViewDir(worldPos));

                float eps = 0.1;
                float3 wpX = worldPos + float3(eps, 0, 0);
                float3 wpZ = worldPos + float3(0, 0, eps);

                float wX1 = sin(wpX.x * _WaveFrequency + time * 1.1) * 
                            cos(wpX.z * _WaveFrequency * 0.7 + time * 0.9);
                float wX2 = sin(wpX.x * _WaveFrequency * 1.3 - time * 0.8 + wpX.z * 0.5) * 0.5;
                float hX = (wX1 + wX2) * _WaveScale;

                float wZ1 = sin(wpZ.x * _WaveFrequency + time * 1.1) * 
                            cos(wpZ.z * _WaveFrequency * 0.7 + time * 0.9);
                float wZ2 = sin(wpZ.x * _WaveFrequency * 1.3 - time * 0.8 + wpZ.z * 0.5) * 0.5;
                float hZ = (wZ1 + wZ2) * _WaveScale;

                float3 tangentX = normalize(float3(eps, hX - displacement, 0));
                float3 tangentZ = normalize(float3(0, hZ - displacement, eps));
                output.worldNormal = normalize(cross(tangentZ, tangentX));

                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _WaveSpeed;
                float surfaceNoise = waterNoise(input.worldPos.xz * 0.5, time * 0.5);

                float fresnel = pow(1.0 - saturate(dot(input.worldNormal, input.viewDir)), _FresnelPower);

                float4 color = lerp(_ShallowColor, _DeepColor, fresnel);
                color.rgb += surfaceNoise * 0.03;

                Light mainLight = GetMainLight();
                float3 halfDir = normalize(mainLight.direction + input.viewDir);
                float spec = pow(saturate(dot(input.worldNormal, halfDir)), _SpecularPower);
                color.rgb += mainLight.color * spec * _SpecularIntensity * fresnel;

                float edgeFoam = saturate(surfaceNoise * 3.0 - 1.5);
                color.rgb += edgeFoam * 0.08;

                color.a = lerp(_ShallowColor.a, _DeepColor.a, fresnel);

                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}