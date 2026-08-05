Shader "Custom/CelestialBody"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionIntensity ("Emission Intensity", Float) = 1.5
        _GlowColor ("Glow Color", Color) = (1, 0.9, 0.7, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 0.6
        _CoreBoost ("Core Highlight", Range(0, 2)) = 0.8
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="Transparent-100" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        ZWrite Off
        ZTest LEqual
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EmissionColor;
                float _EmissionIntensity;
                float4 _GlowColor;
                float _GlowIntensity;
                float _CoreBoost;
                float _AlphaCutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = normalize(GetWorldSpaceViewDir(worldPos));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float ndotv = saturate(dot(IN.normalWS, IN.viewDirWS));

                half3 baseColor = tex.rgb * _EmissionIntensity;
                half3 tinted = baseColor * _EmissionColor.rgb;
                baseColor = lerp(baseColor, tinted, 0.4);

                float coreHighlight = pow(ndotv, 2.0) * _CoreBoost;
                baseColor += _GlowColor.rgb * coreHighlight * 0.5;

                float rimSoft = pow(1.0 - ndotv, 4.0);
                baseColor += _GlowColor.rgb * rimSoft * _GlowIntensity * 0.3;

                return half4(baseColor, tex.a);
            }
            ENDHLSL
        }
    }
}