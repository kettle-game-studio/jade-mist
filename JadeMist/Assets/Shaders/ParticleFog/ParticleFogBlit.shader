Shader "Hidden/Custom/ParticleFogBlit"
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D(_ParticleFogBuffer);
            SAMPLER(sampler_ParticleFogBuffer);

            float _ParticleFogGlobalK;
            float3 _ParticleFogGlobalColor;

            float sample_depth(float2 uv)
            {
                float depth = SampleSceneColor(uv).r;
                #if UNITY_REVERSED_Z
                    return depth;
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    return lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
                #endif
            }

            float4 frag(Varyings input): SV_Target
            {
                float3 screen_color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearRepeat, input.texcoord).rgb;
                float4 fog_values = SAMPLE_TEXTURE2D(_ParticleFogBuffer, sampler_LinearRepeat, input.texcoord);
                float k = pow(_ParticleFogGlobalK, fog_values.r);
                return float4(lerp(_ParticleFogGlobalColor, screen_color, k), 1);
            }
            ENDHLSL
        }
    }
}
