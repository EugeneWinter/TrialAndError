Shader "Hidden/WarmScience/BloomUpsample"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _PrevTex ("Previous Level", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always
        Blend One One

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

            float _TileWeight;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float2 texelSize = _MainTex_TexelSize.xy;

                float3 c0 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2(-1.0, -1.0)).rgb;
                float3 c1 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 0.0, -1.0)).rgb * 2.0;
                float3 c2 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 1.0, -1.0)).rgb;
                float3 c3 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2(-1.0,  0.0)).rgb * 2.0;
                float3 c4 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 0.0,  0.0)).rgb * 4.0;
                float3 c5 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 1.0,  0.0)).rgb * 2.0;
                float3 c6 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2(-1.0,  1.0)).rgb;
                float3 c7 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 0.0,  1.0)).rgb * 2.0;
                float3 c8 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + texelSize * float2( 1.0,  1.0)).rgb;

                float3 avg = (c0 + c1 + c2 + c3 + c4 + c5 + c6 + c7 + c8) / 16.0;
                return float4(avg * _TileWeight, 1.0);
            }
            ENDHLSL
        }
    }
}