Shader "Hidden/WarmScience/BloomDownsample"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float2 texelSize = _MainTex_TexelSize.xy;

                float3 c0 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2(-1.5, -1.5)).rgb;
                float3 c1 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 0.5, -1.5)).rgb;
                float3 c2 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2(-1.5,  0.5)).rgb;
                float3 c3 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 0.5,  0.5)).rgb;

                float3 avg = (c0 + c1 + c2 + c3) * 0.25;
                return float4(avg, 1.0);
            }
            ENDHLSL
        }
    }
}