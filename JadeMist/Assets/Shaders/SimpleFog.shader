Shader "Custom/SimpleFog"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _FogK ("Fog K", Range(0.0, 1.0)) = 0.5
        _TextureK ("Texture K", Range(0.0, 1.0)) = 0.5
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags {
            "RenderType" = "Transparent"
            // "RenderType" = "Fog"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

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
                half3 _BaseColor;
                float _FogK;
                float _TextureK;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input): SV_Target
            {
                float2 screenUV = input.positionHCS.xy / _ScaledScreenParams.xy;
                float3 color = lerp(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).xyz, float3(1, 1, 1), _TextureK) * _BaseColor;

                float3 sceneColor = SampleSceneColor(screenUV);
                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(screenUV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
                #endif

                float3 worldPosition = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);
                float delta = distance(input.positionWS, worldPosition);

                float k = pow(_FogK, delta);
                // return float4(sceneColor, 1);
                return half4(lerp(color, sceneColor, k), 1);
                // return half4(lerp(color, sceneColor, 1-color.x), 1);
            }
            ENDHLSL
        }
    }
}
