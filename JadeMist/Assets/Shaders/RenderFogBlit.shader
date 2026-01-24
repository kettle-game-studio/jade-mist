Shader "Hidden/Custom/RenderFogBlit"
{
    SubShader
    {
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Off

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            // #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/BlitColorAndDepth.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D_X_FLOAT(_FogDepthBack);
            TEXTURE2D_X_FLOAT(_FogDepthFront);
            float _FogGlobalK;
            float3 _FogGlobalColor;

            #if UNITY_REVERSED_Z
                #define DEPTH_OP         min
                #define INVERSE_DEPTH_OP max
            #else
                #define DEPTH_OP         max
                #define INVERSE_DEPTH_OP min
            #endif

            float sample_depth_texture(TEXTURE2D_X_FLOAT(depth_texture), float2 uv)
            {
                float depth = SAMPLE_TEXTURE2D_X(depth_texture, sampler_PointClamp, uv).r;
                #if UNITY_REVERSED_Z
                    return depth;
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    return lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
                #endif
            }

            float4 frag(Varyings input) : SV_Target
            {

                float2 sample_uv = input.positionCS.xy / _ScaledScreenParams.xy;
                float3 screen_color = LoadSceneColor(input.positionCS.xy * 0.5); // TODO: * 0.5???
                float2 uv = ClampAndScaleUVForBilinear(sample_uv, _CameraDepthTexture_TexelSize.xy);
                float screen_depth = sample_depth_texture(_CameraDepthTexture, uv);
                float back_depth   = sample_depth_texture(_FogDepthBack, uv);
                float front_depth  = sample_depth_texture(_FogDepthFront, uv);

                float fog_near = screen_depth < front_depth ? front_depth : screen_depth; // TODO: UNITY_REVERSED_Z
                float fog_far = back_depth != 1 ? back_depth : screen_depth;

                float3 near_world = ComputeWorldSpacePosition(sample_uv, fog_near, UNITY_MATRIX_I_VP);
                float3 far_world  = ComputeWorldSpacePosition(sample_uv, fog_far, UNITY_MATRIX_I_VP);
                float delta = distance(near_world, far_world);
                float k = pow(_FogGlobalK, delta);
                return half4(lerp(_FogGlobalColor, screen_color, k), 1);
            }
            ENDHLSL
        }
    }
}
