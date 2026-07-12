Shader "Custom/ProceduralSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.2, 0.4, 0.8, 1)
        _MiddleColor ("Middle Color", Color) = (0.5, 0.7, 1.0, 1)
        _BottomColor ("Bottom Color", Color) = (0.8, 0.85, 0.9, 1)
        _HorizonWidth ("Horizon Width", Range(0.0, 1.0)) = 0.2
        _MiddleHeight ("Middle Height", Range(-0.5, 0.5)) = 0.0
        
        _StarIntensity ("Star Intensity", Range(0, 1)) = 0
        _StarDensity ("Star Density", Range(50, 500)) = 200
        _StarBrightness ("Star Brightness", Range(0, 5)) = 1.5
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

            float4 _TopColor;
            float4 _MiddleColor;
            float4 _BottomColor;
            float _HorizonWidth;
            float _MiddleHeight;
            float _StarIntensity;
            float _StarDensity;
            float _StarBrightness;

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

            float stars(float3 dir)
            {
                float3 p = dir * _StarDensity;
                float3 fp = floor(p);
                float h = hash(fp);
                
                if (h > 0.995)
                {
                    float3 local = frac(p) - 0.5;
                    float d = length(local);
                    float brightness = smoothstep(0.5, 0.0, d);
                    return brightness * _StarBrightness * (h - 0.995) * 200;
                }
                return 0;
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

                if (_StarIntensity > 0.01 && height > 0)
                {
                    float starBrightness = stars(dir) * _StarIntensity;
                    color += starBrightness;
                }

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}