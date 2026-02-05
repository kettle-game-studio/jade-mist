Shader "Custom/FeaturedParallaxNoiseFog"
{
    SubShader
    {
        HLSLINCLUDE
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "ParallaxFogNormalsFront" }

            ZWrite On
            ZTest On
            Cull Back

            HLSLPROGRAM
                half3 frag(Varyings input): SV_Target
                { return normalize(input.normalWS); }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "ParallaxFogNormalsBack" }

            ZWrite On
            ZTest On
            Cull Front

            HLSLPROGRAM
                half3 frag(Varyings input): SV_Target
                { return normalize(input.normalWS); }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "ParallaxFogInternalCounterFront" }

            Blend One One
            BlendOp Add
            ZWrite Off
            ZTest Off
            Cull Back

            HLSLPROGRAM
                half frag(Varyings input): SV_Target
                { return 1; }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "ParallaxFogInternalCounterBack" }

            Blend One One
            BlendOp Add
            ZWrite Off
            ZTest Off
            Cull Front

            HLSLPROGRAM
                half frag(Varyings input): SV_Target
                { return -1; }
            ENDHLSL
        }
    }
}
