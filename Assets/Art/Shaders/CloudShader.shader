Shader "Custom/CloudShader"
{
    Properties
    {
        _Color ("Cloud Color", Color) = (1, 1, 1, 0.85)
        _ShadowColor ("Shadow Color", Color) = (0.7, 0.75, 0.85, 0.8)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Back
        Offset 0, -1

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _Color;
            float4 _ShadowColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light mainLight = GetMainLight();
                float ndotl = saturate(dot(IN.normalWS, mainLight.direction));
                
                half3 baseColor = lerp(_ShadowColor.rgb, _Color.rgb, ndotl);
                half alpha = lerp(_ShadowColor.a, _Color.a, ndotl);
                
                baseColor *= mainLight.color.rgb;
                
                return half4(baseColor, alpha);
            }
            ENDHLSL
        }
    }
}