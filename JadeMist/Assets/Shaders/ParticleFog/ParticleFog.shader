Shader "Custom/ParticleFog"
{
    Properties
    {
        // [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Value ("Value", Float) = 0.5
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode" = "ParticleFog" }
            BlendOp Add
            Blend One One
            Cull Back
            ZWrite Off
            ZTest On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
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
                float _Value;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input): SV_Target
            {
                float value = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).r * _Value;
                float2 screenUV = input.positionHCS.xy / _ScaledScreenParams.xy;
                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(screenUV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
                #endif

                float3 worldPosition = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);
                float delta = distance(input.positionWS, worldPosition);
                return float4(min(value, delta), 0, 0, 0);
            }

            ENDHLSL
        }
    }
}
