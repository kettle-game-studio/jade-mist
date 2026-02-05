Shader "Hidden/Custom/ParallaxFogBlit"
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "NoiseField.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TYPED_TEXTURE2D(half3, _ParallaxFogNormalsFront);
            TYPED_TEXTURE2D(half3, _ParallaxFogNormalsBack);
            TYPED_TEXTURE2D(half, _ParallaxFogInternalCounter);
            TEXTURE2D(_ParallaxFogSceneDepth);
            TEXTURE2D(_ParallaxFogDepthFront);
            TEXTURE2D(_ParallaxFogDepthBack);

            float3 _ParallaxFogInternalColor;
            float3 _ParallaxFogExternalColor;
            float _ParallaxFogDepthValue;
            float _ParallaxFogExternalTransparency;
            float _ParallaxFogInternalTransparency;

            float sample_depth_texture(TEXTURE2D_FLOAT(depth_texture), float2 uv)
            {
                float depth = SAMPLE_TEXTURE2D(depth_texture, sampler_PointClamp, uv).r;
                #if UNITY_REVERSED_Z
                    return depth;
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    return lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
                #endif
            }

            float world_to_hclip_z(float3 position_ws)
            {
                float4 hcs = TransformWorldToHClip(position_ws);
                return hcs.z / hcs.w;
            }

            float3 frag(Varyings input): SV_Target
            {
                float3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointRepeat, input.texcoord).rgb;
                float3 front_normal = SAMPLE_TEXTURE2D(_ParallaxFogNormalsFront, sampler_PointRepeat, input.texcoord);
                float3 back_normal = SAMPLE_TEXTURE2D(_ParallaxFogNormalsBack, sampler_PointRepeat, input.texcoord);
                float internal_counter = SAMPLE_TEXTURE2D(_ParallaxFogInternalCounter, sampler_PointRepeat, input.texcoord);
                float scene_depth = sample_depth_texture(_ParallaxFogSceneDepth, input.texcoord);
                float fog_front_depth = sample_depth_texture(_ParallaxFogDepthFront, input.texcoord);
                float fog_back_depth = sample_depth_texture(_ParallaxFogDepthBack, input.texcoord);

                internal_counter = internal_counter * 2 + 1;

                // Debug checking
                if (abs(internal_counter) > 1.0001 ||
                    internal_counter > 0 && fog_front_depth < fog_back_depth ||
                    internal_counter < 0 && fog_front_depth > fog_back_depth)
                    return float4(1, 0, 1, 1);

                bool in_fog = internal_counter < 0;
                float3 scene_ws = ComputeWorldSpacePosition(input.texcoord, scene_depth, UNITY_MATRIX_I_VP);
                float3 fog_front_ws = ComputeWorldSpacePosition(input.texcoord, fog_front_depth, UNITY_MATRIX_I_VP);
                float3 fog_back_ws = ComputeWorldSpacePosition(input.texcoord, fog_back_depth, UNITY_MATRIX_I_VP);

                if (!in_fog && scene_depth > fog_front_depth)
                    return color;

                float internal_k = 1;
                if (in_fog)
                {
                    float3 nearest_intersection = scene_depth > fog_back_depth ? scene_ws : fog_back_ws;
                    float dist = distance(nearest_intersection, _WorldSpaceCameraPos);
                    internal_k = pow(_ParallaxFogInternalTransparency, dist);
                }

                int iteration_count = int(clamp(internal_k * 100, 0, 10));
                fog_front_ws = trace_fog_field(fog_front_ws, front_normal, _ParallaxFogDepthValue, iteration_count);
                fog_front_depth = world_to_hclip_z(fog_front_ws);

                if (scene_depth < fog_front_depth)
                {
                    float dist = distance(fog_front_ws, scene_ws);
                    float external_k = pow(_ParallaxFogExternalTransparency, dist);
                    float a = fog_field_value(fog_front_ws);
                    float3 fog_surface_color = lerp(_ParallaxFogExternalColor, _ParallaxFogInternalColor, a);
                    color = lerp(fog_surface_color, color, external_k);
                }

                if (in_fog)
                    color = float4(lerp(_ParallaxFogExternalColor, color, internal_k), 1);

                return color;
            }
            ENDHLSL
        }
    }
}
