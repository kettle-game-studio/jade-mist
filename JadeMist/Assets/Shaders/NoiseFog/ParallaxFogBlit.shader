Shader "Hidden/Custom/ParallaxFogBlit"
{
    SubShader
    {
        Pass
        {
            Cull Off
            ZWrite On
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
            TYPED_TEXTURE2D(half, _ParallaxFogBlueNoise);
            int2 _ParallaxFogBlueNoiseSize;
            TEXTURE2D(_ParallaxFogSceneDepth);
            TEXTURE2D(_ParallaxFogDepthFront);
            TEXTURE2D(_ParallaxFogDepthBack);

            float3 _ParallaxFogInternalColor;
            float3 _ParallaxFogExternalColor;
            float3 _ParallaxFogBorderColor;
            float _ParallaxFogDepthValue;
            float _ParallaxFogExternalTransparency;
            float _ParallaxFogInternalTransparency;

            struct FragOutput
            {
                float3 color : SV_Target;
                float  depth : SV_Depth;
            };

            float equalize_depth(float depth)
            {
                #if UNITY_REVERSED_Z
                    return depth;
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    return lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
                #endif
            }

            float store_depth(float depth)
            {
                #if UNITY_REVERSED_Z
                    return depth;
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    return (depth + UNITY_NEAR_CLIP_VALUE) / (1 - UNITY_NEAR_CLIP_VALUE);
                #endif
            }

            float sample_depth_texture(TEXTURE2D_FLOAT(depth_texture), float2 uv)
            {
                return equalize_depth(SAMPLE_TEXTURE2D(depth_texture, sampler_PointClamp, uv).r);
            }

            float world_to_hclip_z(float3 position_ws)
            {
                float4 hcs = TransformWorldToHClip(position_ws);
                return equalize_depth(hcs.z / hcs.w);
            }

            FragOutput frag(Varyings input)
            {
                FragOutput output;
                float3 scene_color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointRepeat, input.texcoord).rgb;
                float3 front_normal = SAMPLE_TEXTURE2D(_ParallaxFogNormalsFront, sampler_PointRepeat, input.texcoord);
                float3 back_normal = SAMPLE_TEXTURE2D(_ParallaxFogNormalsBack, sampler_PointRepeat, input.texcoord);
                float internal_counter = SAMPLE_TEXTURE2D(_ParallaxFogInternalCounter, sampler_PointRepeat, input.texcoord);
                float scene_depth = sample_depth_texture(_ParallaxFogSceneDepth, input.texcoord);
                float fog_front_depth = sample_depth_texture(_ParallaxFogDepthFront, input.texcoord);
                float fog_back_depth = sample_depth_texture(_ParallaxFogDepthBack, input.texcoord);
                // float pixel_gradient_noise = 0;
                float pixel_gradient_noise = ign_noise(input.positionCS.xy + _Time.xx * 100);
                // float pixel_gradient_noise = LOAD_TEXTURE2D(_ParallaxFogBlueNoise, int2(input.positionCS.xy + _Time.yy * 100) % _ParallaxFogBlueNoiseSize);
                output.color = scene_color;
                output.depth = store_depth(scene_depth);

                internal_counter = internal_counter * 2 + 1;

                // Debug checking
                if (abs(internal_counter) > 1.0001 ||
                    internal_counter > 0 && fog_front_depth < fog_back_depth ||
                    internal_counter < 0 && fog_front_depth > fog_back_depth)
                {
                    output.color = float3(1, 0, 1);
                    return output;
                }

                bool in_fog = internal_counter < 0;
                float3 scene_ws = ComputeWorldSpacePosition(input.texcoord, scene_depth, UNITY_MATRIX_I_VP);
                float3 fog_front_ws = ComputeWorldSpacePosition(input.texcoord, fog_front_depth, UNITY_MATRIX_I_VP);
                float3 fog_back_ws = ComputeWorldSpacePosition(input.texcoord, fog_back_depth, UNITY_MATRIX_I_VP);

                if (!in_fog && scene_depth > fog_front_depth)
                    return output;

                float internal_k = 1;
                if (in_fog)
                {
                    float3 nearest_intersection = scene_depth > fog_back_depth ? scene_ws : fog_back_ws;
                    float dist = distance(nearest_intersection, _WorldSpaceCameraPos);
                    internal_k = pow(_ParallaxFogInternalTransparency, dist);
                }

                int iteration_count = int(clamp(internal_k * 100, 0, 30));
                fog_front_ws = trace_fog_field(fog_front_ws, front_normal, _ParallaxFogDepthValue, pixel_gradient_noise, iteration_count);
                fog_front_depth = world_to_hclip_z(fog_front_ws);

                // float3 view = normalize(fog_front_ws - _WorldSpaceCameraPos);
                // if (dot(view, front_normal) >= 0) output.color = float3(1, 0, 1);
                // else
                if (scene_depth < fog_front_depth)
                {
                    float dist = distance(fog_front_ws, scene_ws);
                    float external_k = pow(_ParallaxFogExternalTransparency, dist);
                    FogSample fog_sample = sample_fog_field(fog_front_ws, front_normal, _ParallaxFogDepthValue);
                    float3 view_dir = normalize(fog_sample.position - _WorldSpaceCameraPos);
                    float view_cos = abs(dot(fog_sample.normal, view_dir));
                    float view_sin = sqrt(1 - view_cos * view_cos);
                    float a = fog_sample.value;

                    float3 fog_surface_color = lerp(_ParallaxFogExternalColor, _ParallaxFogInternalColor, a);
                    // float3 fog_surface_color = a < 0.13 ? _ParallaxFogExternalColor : _ParallaxFogInternalColor;
                    // fog_surface_color = (1 - view_sin) / dist < 0.0009 ? _ParallaxFogBorderColor : fog_surface_color;
                    // fog_surface_color = view_cos < 0.2 ? lerp(_ParallaxFogBorderColor, fog_surface_color, clamp((view_cos - 0.19) * 100, 0, 1)) : fog_surface_color;
                    
                    output.color = lerp(fog_surface_color, output.color, external_k);
                    output.depth = store_depth(fog_front_depth);
                }

                if (in_fog)
                    output.color = float4(lerp(_ParallaxFogExternalColor, output.color, internal_k), 1);

                return output;
            }
            ENDHLSL
        }
    }
}
