Shader "Custom/ParallaxNoiseFog"
{
    Properties
    {
        [MainColor] [HDR] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HDR] _Color2("Color 2", Color) = (1, 1, 1, 1)
        _Depth ("Depth", Float) = 5
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            ZWrite On
            ZTest On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "NoiseField.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            struct FragOutput
            {
                float4 color : SV_Target;
                float  depth : SV_Depth;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half3 _BaseColor;
                half3 _Color2;
                float _Depth;
            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            FragOutput frag(Varyings input)
            {
                FragOutput output;

                float3 positionWS = input.positionWS;
                float3 normalWS = normalize(input.normalWS);

                float3 view = normalize(positionWS - _WorldSpaceCameraPos);
                float  d_proj = dot(view, normalWS);
                float3 v_proj = view - normalWS * d_proj;

                float t1 = 0;
                float depth1 = -fog_field_value(positionWS) * _Depth;
                float t2 = 0;
                float depth2 = 0;

                float dist = distance(view, _WorldSpaceCameraPos);
                float step = 1.0 / 200;

                int i;
                int count = 100;
                for (i = 0; i < count; ++i)
                {
                    t2 = t1;
                    depth2 = depth1;

                    // t1 += 0.5;
                    t1 += dist * step;
                    dist += dist * step;
                    depth1 = -fog_field_value(positionWS + v_proj * t1) * _Depth;
                    if (d_proj * t1 < depth1)
                        break;
                }

                float k = -(d_proj * t1 - depth1) / ((d_proj * t2 - d_proj * t1) - (depth2 - depth1));
                float t = lerp(t1, t2, clamp(k, 0, 1));
                // float t = t2;

                // float t = 0; // 0.5 * _Depth;
                // for (int i = 0; i < 3; ++i)
                // {
                //     float field_depth = -fog_field_value(positionWS + v_proj * t) * _Depth;
                //     t = field_depth / d_proj;
                // }

                positionWS = positionWS + v_proj * t;
                float a = fog_field_value(positionWS);
                // a = lerp(a, 0.5, clamp(t / -d_proj, 0 , 1));

                output.color = float4(lerp(_BaseColor, _Color2, a), 1);
                float4 hcs = TransformWorldToHClip(positionWS);
                output.depth = hcs.z / hcs.w;

                // a = float(i) / count;
                // output.color = float4(a, a, a, 1);
                // output.color = float4(_WorldSpaceCameraPos, 1);
                // output.color = float4(normalWS, 1);
                return output;
            }
            ENDHLSL
        }
    }
}
