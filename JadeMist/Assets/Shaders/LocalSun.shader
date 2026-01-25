Shader "Custom/LocalSun"
{
    Properties
    {
        [MainColor] [HDR] _SkyColor("Sky Color", Color) = (0.5, 0.5, 1, 1)
        [HDR] _SunColor("Sun Color", Color) = (1, 1, 1, 1)
        _SunPosition("Sun Position", Vector) = (1, 1, 1, 1)
        _SunAngle("Sun Angle", Range(0.0, 3.14)) = 0.5
        [MainTexture] _SunMap("Sun Map", 2D) = "white"
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

            TEXTURE2D(_SunMap);
            SAMPLER(sampler_SunMap);

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

                float3 v_v = normalize(cross(sunPosition, float3(1, 0, 0)));
                float3 v_v2 = normalize(cross(sunPosition, v_v));
                float u = dot(direction, v_v) / _SunAngle;
                float v = dot(direction, v_v2) / _SunAngle;

                float3 color = 0;

                float3 control = SAMPLE_TEXTURE2D(_SunMap, sampler_SunMap, float2(u + 0.5, v + 0.5));

                if (abs(u) >= 0.5 || abs(v) >= 0.5)
                    color = _SkyColor;
                else {
                    if (control.g > 0.1) {
                        color = _SunColor;
                    } else {
                        color = _SkyColor;
                    }
                }

                return float4(color, 1);
            }
            ENDHLSL
        }
    }
}
