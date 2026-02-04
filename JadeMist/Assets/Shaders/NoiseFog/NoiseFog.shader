Shader "Custom/NoiseFog"
{
    Properties
    {
        [MainColor] [HDR] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HDR] _Color2("Color 2", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            ZWrite On
            ZTest On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "NoiseField.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half3 _BaseColor;
                half3 _Color2;
            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float a = fog_field_value(output.positionWS);
                // a = 1 - a;
                // a = a * 4 - 3;
                // a = a * 2;
                a = -a * 10;
                float3 pos = output.positionWS + normalWS * a;
                output.positionHCS = TransformWorldToHClip(pos);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input): SV_Target
            {
                float a = fog_field_value(input.positionWS);
                // a = a > 0.1 ? 1 : 0;
                // a = a * 10;
                // a = clamp(a, 0, 1);
                return float4(lerp(_BaseColor, _Color2, a), 1);
            }
            ENDHLSL
        }
    }
}
