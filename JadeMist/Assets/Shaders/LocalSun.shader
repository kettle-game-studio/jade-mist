Shader "Custom/LocalSun"
{
    Properties
    {
        [MainColor] [HDR] _SkyColor("Sky Color", Color) = (0.5, 0.5, 1, 1)
        [HDR] _SunColor("Sun Color", Color) = (1, 1, 1, 1)
        _SunPosition("Sun Position", Vector) = (1, 1, 1, 1)
        _SunAngle("Sun Angle", Range(0.0, 3.14)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS: POSITION;
            };

            struct Varyings
            {
                float4 positionHCS: SV_POSITION;
                float3 positionWS: TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half3 _SkyColor;
                half3 _SunColor;
                half3 _SunPosition;
                float _SunAngle;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 direction = normalize(input.positionWS - _WorldSpaceCameraPos.xyz);
                float3 sunPosition = normalize(_SunPosition - _WorldSpaceCameraPos.xyz);
                float value = acos(dot(direction, sunPosition));
                float3 color = 
                            value < _SunAngle / 4 ? _SunColor :
                            value < 7 * _SunAngle / 8 ? 0 : 
                            value < _SunAngle ? _SunColor 
                            : _SkyColor ;
                    // value < _SunAngle ? _SkyColor : _SunColor;
                    // (value < (_SunAngle + 0.001) ? _SunColor : _SkyColor);
                    // value < _SunAngle / 3 ? _SkyColor : _SunColor;
                return float4(color, 1);
            }
            ENDHLSL
        }
    }
}
