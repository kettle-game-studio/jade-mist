Shader "Custom/DebugNoiseFog"
{
    Properties
    {
        [MainColor] [HDR] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HDR] _Color2("Color 2", Color) = (1, 1, 1, 1)
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
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            struct FragOutput
            {
                float4 color : SV_Target;
                float  depth : SV_Depth;
            };


            CBUFFER_START(UnityPerMaterial)
                half3 _BaseColor;
                half3 _Color2;
            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input): SV_Target
            {
                float a = fog_field_value(input.positionWS);
                return float4(lerp(_BaseColor, _Color2, a), 1);
            }
            ENDHLSL
        }
    }
}
