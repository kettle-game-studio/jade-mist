Shader "Hidden/Custom/NoiseBlit"
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
            #include "Assets/Shaders/Lib/VoronoiNoise.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            float4 frag(Varyings input): SV_Target
            {
                float3 screen_color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearRepeat, input.texcoord).rgb;
                float pixel_gradient_noise = ign_noise(input.positionCS.xy + _Time.xx * 100);
                float k = 256;
                screen_color = screen_color * k;
                screen_color = floor(screen_color) + floor(frac(screen_color) + pixel_gradient_noise);
                screen_color = screen_color / k;
                return float4(screen_color, 1);
            }
            ENDHLSL
        }
    }
}
