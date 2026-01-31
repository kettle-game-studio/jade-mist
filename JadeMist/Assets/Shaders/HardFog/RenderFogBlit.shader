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
            TEXTURE2D(_FogNoiseMask);
            float _FogGlobalK;
            float3 _FogGlobalColor;
            float _FogMaskK;

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

            float get_noise(float2 coord)
            {
                float noise1 = 7 * SAMPLE_TEXTURE2D(_FogNoiseMask, sampler_LinearRepeat, coord * 0.07456 + float2(_Time.y * 0.03, 0)).r;
                float noise2 = 2 * SAMPLE_TEXTURE2D(_FogNoiseMask, sampler_LinearRepeat, coord * 0.1578 + float2(_Time.y * 0.11, 0)).r;
                float noise3 = 1 * SAMPLE_TEXTURE2D(_FogNoiseMask, sampler_LinearRepeat, coord * 0.24 + float2(_Time.y* 0.17, 0)).r;
                return (noise1 + noise2 + noise3) / 10;
                // return noise1 / 7;
                // return max(max(noise1, noise2), noise3) / 7
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 sample_uv = input.positionCS.xy / _ScaledScreenParams.xy;
                float3 screen_color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearRepeat, input.texcoord).rgb;
                float2 uv = ClampAndScaleUVForBilinear(sample_uv, _CameraDepthTexture_TexelSize.xy);
                float screen_depth = sample_depth_texture(_CameraDepthTexture, uv);
                float back_depth   = sample_depth_texture(_FogDepthBack, uv);
                float front_depth  = sample_depth_texture(_FogDepthFront, uv);

                float fog_near = screen_depth < front_depth ? front_depth : screen_depth; // TODO: UNITY_REVERSED_Z
                float fog_far = back_depth != 1 ? back_depth : screen_depth;

                float3 near_world = ComputeWorldSpacePosition(sample_uv, fog_near, UNITY_MATRIX_I_VP);
                float3 far_world  = ComputeWorldSpacePosition(sample_uv, fog_far, UNITY_MATRIX_I_VP);

                float noise = get_noise(near_world.zy);// - 0.11;
                // float noise_x = get_noise(near_world.zy + float2(1e-3, 0));
                // float noise_y = get_noise(near_world.zy + float2(0, 1e-3));
                // float3 normal = normalize(float3(noise_x - noise, noise_y - noise, 0.01));
                // float color_k = max(0, dot(normal, float3(1, -1, 1)));
                // return float4(-normal, 1);
                float3 color_add = lerp(0, 1, noise);

                // noise = noise;

                float delta = distance(near_world, far_world);
                float k = pow(_FogGlobalK, max(delta - (1-noise) * _FogMaskK, 0));
                // float asdf = clamp((delta) - noise * _FogMaskK / 3, 0, 1);
                // float k = (1 - pow(asdf, 1));
                // return k;
                return half4(lerp(_FogGlobalColor + float3(0.5, 1, 1) * lerp(0, 0.5, noise), screen_color, k), 1);
            }
            ENDHLSL
        }
    }
}
