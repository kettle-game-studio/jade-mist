Shader "Custom/CatTextureHardFogFront"
{
    Properties
    {
        _CatValue ("Cat Value", Range(0.0, 1.0)) = 0.5
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode" = "FogFront" }
            Cull Back
            ZWrite On
            ZTest On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float _CatValue;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float frag(Varyings input): SV_Depth
            {
                float value = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).r;
                if (value < _CatValue)
                    discard;
                return input.positionHCS.z * lerp(0.5, 1, value);
            }
            ENDHLSL
        }
    }
}
