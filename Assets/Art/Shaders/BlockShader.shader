Shader "Custom/BlockShader"
{
    Properties
    {
        _TexArray ("Texture Array", 2DArray) = "" {}
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.5
        _DirectStrength ("Direct Light Strength", Range(0, 2)) = 0.9
        _SkyLightInfluence ("Sky Light Influence", Range(0, 1)) = 0.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D_ARRAY(_TexArray);
            SAMPLER(sampler_TexArray);

            float4 _SkyLightColor;

            CBUFFER_START(UnityPerMaterial)
                float _AmbientStrength;
                float _DirectStrength;
                float _SkyLightInfluence;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float3 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogCoord : TEXCOORD2;
                float4 vertexAO : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = posInputs.positionCS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                OUT.vertexAO = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D_ARRAY(_TexArray, sampler_TexArray, IN.uv.xy, IN.uv.z);

                Light mainLight = GetMainLight();
                float ndotl = saturate(dot(IN.normalWS, mainLight.direction));
                float aoLight = IN.vertexAO.r;

                float3 sunColor = mainLight.color.rgb;
                float3 skyColor = _SkyLightColor.rgb;

                float3 directContribution = sunColor * ndotl * _DirectStrength;
                float3 ambientContribution = lerp(float3(1,1,1), skyColor, _SkyLightInfluence) * _AmbientStrength;

                float3 totalLight = (directContribution + ambientContribution) * aoLight;
                totalLight = max(totalLight, float3(0.05, 0.05, 0.05));

                float3 finalColor = tex.rgb * totalLight;

                finalColor = MixFog(finalColor, IN.fogCoord);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}