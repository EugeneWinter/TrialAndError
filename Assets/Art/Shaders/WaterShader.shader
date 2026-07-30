Shader "Custom/WaterShader_Optimized"
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float time = _Time.y * _WaveSpeed;

                float2 waveCoords = worldPos.xz * _WaveFrequency;
                float wave1 = sin(waveCoords.x + time * 1.1) * cos(waveCoords.y * 0.7 + time * 0.9);
                float wave2 = sin(waveCoords.x * 1.3 - time * 0.8 + worldPos.z * 0.5) * 0.5;
                
                float displacement = (wave1 + wave2) * _WaveScale;
                worldPos.y += displacement;

                float3 tangent = normalize(float3(1, (cos(waveCoords.x + time * 1.1) * cos(waveCoords.y * 0.7 + time * 0.9)) * _WaveScale * _WaveFrequency, 0));
                float3 bitangent = normalize(float3(0, (sin(waveCoords.x + time * 1.1) * -sin(waveCoords.y * 0.7 + time * 0.9) * 0.7) * _WaveScale * _WaveFrequency, 1));
                
                output.worldNormal = half3(cross(bitangent, tangent));
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
                
                half n1 = simple_noise(rippleUV + time * 0.2);
                half n2 = simple_noise(rippleUV * 1.5 - time * 0.15);
                half noiseSum = (n1 + n2) * 0.5;

                half3 normal = normalize(input.worldNormal + half3(n1 * 0.1, 0, n2 * 0.1));
                half nv = saturate(dot(normal, input.viewDir));
                half fresnel = pow(1.0 - nv, _FresnelPower);

                half4 color = lerp(_ShallowColor, _DeepColor, fresnel);
                
                Light mainLight = GetMainLight();
                half3 halfDir = normalize(half3(mainLight.direction) + input.viewDir);
                half nh = saturate(dot(normal, halfDir));
                half spec = pow(nh, _SpecularPower);
                
                color.rgb += mainLight.color * spec * _SpecularIntensity;
                color.rgb += noiseSum * 0.05;

                half edgeFoam = saturate(noiseSum * 4.0 - 2.2);
                color.rgb += edgeFoam * 0.1;

                color.a = lerp(_ShallowColor.a, _DeepColor.a, fresnel);
                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}