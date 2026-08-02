Shader "Custom/BlockHighlight"
{
    Properties
    {
        _DarkenAmount ("Darken Amount", Range(0, 0.6)) = 0.15
        _Thickness ("Edge Thickness", Range(0.001, 0.08)) = 0.03
        _EdgeSoftness ("Edge Softness", Range(0.0, 0.05)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" "RenderPipeline"="UniversalPipeline" }
        Blend DstColor Zero
        ZWrite Off
        ZTest LEqual
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _DarkenAmount;
                float _Thickness;
                float _EdgeSoftness;
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
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 expandedPos = IN.positionOS.xyz + IN.normalOS * 0.001;

                OUT.positionCS = TransformObjectToHClip(expandedPos);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float distX = min(uv.x, 1.0 - uv.x);
                float distY = min(uv.y, 1.0 - uv.y);
                float distToEdge = min(distX, distY);

                float edge = 1.0 - smoothstep(_Thickness - _EdgeSoftness, _Thickness + _EdgeSoftness, distToEdge);

                float darken = 1.0 - (_DarkenAmount * edge);

                return half4(darken, darken, darken, 1.0);
            }
            ENDHLSL
        }
    }
}