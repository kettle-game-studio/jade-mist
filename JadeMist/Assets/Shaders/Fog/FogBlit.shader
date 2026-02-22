Shader "Hidden/Custom/FogBlit"
{
    SubShader
    {
        Pass
        {
            Cull Off
            ZWrite On
            ZTest Off

            HLSLPROGRAM
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "FogField.hlsl"



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
            float _ParallaxFogKillFactor;
            float3 _ParallaxFogKillPoint;
            float3 _ParallaxFogKillColor;
            float3 _ParallaxFogLightingFactor;

            struct FragOutput
            {
                float3 color : SV_Target;
                float  depth : SV_Depth;
            };

            float external_transparency() { return _ParallaxFogExternalTransparency * (1 - _ParallaxFogKillFactor); }
            float internal_transparency() { return _ParallaxFogInternalTransparency * (1 - _ParallaxFogKillFactor); }

            float3 fog_point_color(float3 base_color, float3 position)
            {
                float dist = distance(position, _ParallaxFogKillPoint);
                float k = clamp(dist - _ParallaxFogKillFactor * 3, 0, 1);
                return lerp(_ParallaxFogKillColor, base_color, k);
            }

            float sample_depth_texture(TEXTURE2D_FLOAT(depth_texture), float2 uv)
            {
                return SAMPLE_TEXTURE2D(depth_texture, sampler_PointClamp, uv).r;
            }

            float world_to_hclip_z(float3 position_ws)
            {
                float4 hcs = TransformWorldToHClip(position_ws);
                return hcs.z / hcs.w;
            }

            float3 fog_lighting(float3 normalWS, Light light, float fog_value)
            {
                return _ParallaxFogLightingFactor * light.color * light.distanceAttenuation * light.shadowAttenuation;
            }

            float3 apply_lightings(float3 position, float3 normal, float2 positionCS, float fog_value)
            {
                float3 lighting = float3(1, 1, 1) * 0.1;
                
                // Get the main light
                Light main_light = GetMainLight();
                lighting += fog_lighting(normal, main_light, fog_value);
                
                #if defined(_ADDITIONAL_LIGHTS)

                // Additional light loop for non-main directional lights. This block is specific to Forward+.
                #if USE_CLUSTER_LIGHT_LOOP
                UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    Light additional_light = GetAdditionalLight(lightIndex, position, half4(1,1,1,1));
                    lighting += fog_lighting(normal, additional_light, fog_value);
                }
                #endif
                
                InputData inputData = (InputData)0;
                inputData.positionWS = position;
                inputData.normalWS = normal;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(position);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);

                // Additional light loop.
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light additional_light = GetAdditionalLight(lightIndex, position, half4(1,1,1,1));
                    lighting += fog_lighting(normal, additional_light, fog_value);
                LIGHT_LOOP_END
                
                #endif
                
                return lighting;
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
                output.depth = scene_depth;
                // output.color = float3(1, 1, 1) * SampleScreenSpaceShadowmap(input.positionCS);
                // return output;

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
                    internal_k = pow(internal_transparency(), dist);
                }

                int iteration_count = int(clamp(internal_k * 100, 0, 30));
                fog_front_ws = trace_fog_field(fog_front_ws, front_normal, _ParallaxFogDepthValue, pixel_gradient_noise, iteration_count);
                fog_front_depth = world_to_hclip_z(fog_front_ws);

                if (scene_depth < fog_front_depth)
                {
                    float dist = distance(fog_front_ws, scene_ws);
                    float external_k = pow(external_transparency(), dist);
                    FogSample fog_sample = sample_fog_field(fog_front_ws, front_normal, _ParallaxFogDepthValue);
                    float3 view_dir = normalize(fog_sample.position - _WorldSpaceCameraPos);
                    float view_cos = abs(dot(fog_sample.normal, view_dir));
                    float view_sin = sqrt(1 - view_cos * view_cos);
                    float a = fog_sample.value;

                    float3 fog_surface_color = lerp(_ParallaxFogExternalColor, _ParallaxFogInternalColor, a);
                    // float3 fog_surface_color = a < 0.13 ? _ParallaxFogExternalColor : _ParallaxFogInternalColor;
                    // fog_surface_color = (1 - view_sin) / dist < 0.0009 ? _ParallaxFogBorderColor : fog_surface_color;
                    // fog_surface_color = view_cos < 0.2 ? lerp(_ParallaxFogBorderColor, fog_surface_color, clamp((view_cos - 0.19) * 100, 0, 1)) : fog_surface_color;
                    fog_surface_color = fog_surface_color * apply_lightings(fog_sample.position, fog_sample.normal, input.positionCS.xy, pixel_gradient_noise);
                    
                    output.color = lerp(fog_surface_color, output.color, external_k);
                    output.depth = fog_front_depth;
                }

                if (in_fog)
                {
                    float3 offset = fog_back_ws - _WorldSpaceCameraPos;
                    float offset_len = length(offset);
                    offset = offset / offset_len * min(offset_len, 0.5);
                    float3 color_sample_position = _WorldSpaceCameraPos + offset;
                    float3 fog_color =
                        fog_point_color(_ParallaxFogExternalColor, color_sample_position) *
                        apply_lightings(color_sample_position, float3(0, 0, 0), input.positionCS.xy, 1);
                    output.color = lerp(fog_color, output.color, internal_k);
                }

                return output;
            }
            ENDHLSL
        }
    }
}
