Shader "Custom/ParallaxNoiseFog"
{
    Properties
    {
        [MainColor] [HDR] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HDR] _Color2("Color 2", Color) = (1, 1, 1, 1)
        _Depth ("Depth", Float) = 5
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
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            struct FragOutput
            {
                float4 color : SV_Target;
                float  depth : SV_Depth;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half3 _BaseColor;
                half3 _Color2;
                float _Depth;
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

            FragOutput frag(Varyings input)
            {
                FragOutput output;
                float3 normalWS = normalize(input.normalWS);
                float3 positionWS = input.positionWS;
                float gn = ign_noise(input.positionHCS.xy);

                positionWS = trace_fog_field(positionWS, normalWS, gn, _Depth);
                float a = fog_field_value(positionWS);

                output.color = float4(a, a, a, 1);
                // output.color = float4(lerp(_BaseColor, _Color2, a), 1);
                float4 hcs = TransformWorldToHClip(positionWS);
                output.depth = hcs.z / hcs.w;

                return output;
            }
            ENDHLSL
        }
    }
}
