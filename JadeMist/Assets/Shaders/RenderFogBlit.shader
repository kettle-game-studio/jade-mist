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
            // #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/BlitColorAndDepth.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment FragColorAndDepth

            float4 frag(Varyings input) : SV_Target
            {
                // float depth = SampleSceneDepth(input.positionCS.xy/2);
                // return float4(depth, depth, depth, 1);

                // float3 color = LoadSceneColor(input.positionCS * 1000 / _ScaledScreenParams.xy);
                // return float4(color, 1);

                return float4(input.texcoord.xy, 0, 1);
            }
            ENDHLSL
        }
    }
}
