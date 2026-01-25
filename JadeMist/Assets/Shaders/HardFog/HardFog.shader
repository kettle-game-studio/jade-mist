Shader "Custom/HardFog"
{
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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            void frag(Varyings input) { }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "FogBack" }
            Cull Front
            ZWrite On
            ZTest Greater

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            #if UNITY_REVERSED_Z
                #define DEPTH_OP <
            #else
                #define DEPTH_OP >
            #endif

            void frag(Varyings input)
            {
                float2 screenUV = input.positionHCS.xy / _ScaledScreenParams.xy;
                if (input.positionHCS.z DEPTH_OP SampleSceneDepth(screenUV))
                    discard;
            }
            ENDHLSL
        }
    }
}
