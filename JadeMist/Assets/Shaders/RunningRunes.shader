Shader "Custom/RunningRunes"
{
    Properties
    {
        [MainColor] [HDR] _RuneColor("Rune Color", Color) = (0.0, 0.5, 1, 1)
        [MainTexture] _RuneMap("Rune Map", 2D) = "white"
        _Length("Length", float) = 100
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Cull Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS: POSITION;
                float2 uv: TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv: TEXCOORD0;
                float4 positionHCS: SV_POSITION;
            };

            TEXTURE2D(_RuneMap);
            SAMPLER(sampler_RuneMap);

            CBUFFER_START(UnityPerMaterial)
                half3 _RuneColor;
                float _Length;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.uv = input.uv;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            float3 frag(Varyings input) : SV_Target
            {
                // return float3(input.uv * float2(1, 0), 0);
                // input.uv.x += input.uv.y * 0.3; 
                // input.uv.x %= 1;
                // if (input.uv.x > 1 ||  input.uv.x < 0) discard; 

                if (input.uv.y > _Length) discard;

                float2 time = float2(-_Time.y, 0) * 0.1;
                float4 pixel = SAMPLE_TEXTURE2D(_RuneMap, sampler_RuneMap, (input.uv.yx + time ) * float2(0.1, 1));
                if (pixel.z < 0.5) discard;

                return pixel * _RuneColor;
            }
            ENDHLSL
        }
    }
}
