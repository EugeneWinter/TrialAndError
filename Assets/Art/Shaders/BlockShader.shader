Shader "Custom/BlockShader"
{
    Properties
    {
        _TexArray ("Texture Array", 2DArray) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D_ARRAY(_TexArray);
            SAMPLER(sampler_TexArray);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float3 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D_ARRAY(_TexArray, sampler_TexArray, IN.uv.xy, IN.uv.z);

                Light mainLight = GetMainLight();
                float ndotl = saturate(dot(IN.normalWS, mainLight.direction));
                float lighting = ndotl * 0.7 + 0.3;

                col.rgb *= lighting * mainLight.color.rgb;
                return col;
            }
            ENDHLSL
        }
    }
}