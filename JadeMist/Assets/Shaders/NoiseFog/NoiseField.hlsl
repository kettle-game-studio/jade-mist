#pragma once

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Assets/Shaders/Lib/VoronoiNoise.hlsl"


float fog_field_value(float3 coord)
{
    float a = 0;
    // a += 0.6 * voronoi3(coord + _Time.x * 30/3, 0.5 / 2);
    // a += 0.3 * voronoi3(coord + _Time.x * 20/3, 1.1 / 2);
    // a += 0.1 * voronoi3(coord + _Time.x * 10/3, 1.7);
    a = 1;
    a *= voronoi3(coord + float3(0, _Time.y * 5, _Time.y * 15) / 10, 0.5 / 3);
    a *= voronoi3(coord + float3(0, _Time.y * 5, _Time.y * 10) / 10, 0.9 / 3);
    // a *= voronoi3(coord + _Time.x * 10/3, 0.7 / 2);
    // a = a / 3;
    // a = 1 - a;
    a = a*a;
    // a = a*a;
    // a = a*a;
    // a = a*a;
    // a = a*a;
    // a = a * 1000;
    // a += 0.01;

    float3 delta = coord - _WorldSpaceCameraPos;
    float b = dot(delta, delta);
    b = b * b;
    b = b * 0.001;
    b = clamp(b, 0, 1);
    a = lerp(0, a, b);

    return a;
}

// float fog_field_value(float3 coord)
// {
//     float a = 1;
//     a *= voronoi3(coord, 0.1, 0);
//     a = a*a;
//     return a;
// }

// float fog_field_value(float3 coord)
// {
//     return distance(coord, _WorldSpaceCameraPos) * 0.01;
// }

float3 trace_fog_field(float3 positionWS, float3 normalWS, float depth, int iterations = 10)
{
    if (iterations <= 0)
        return positionWS;

    float3 view = normalize(positionWS - _WorldSpaceCameraPos);
    float  d_proj = dot(view, normalWS);
    float3 v_proj = view - normalWS * d_proj;

    float t1 = 0;
    float depth1 = -fog_field_value(positionWS) * depth;
    float t2 = 0;
    float depth2 = depth1;

    float dist = distance(view, _WorldSpaceCameraPos);
    float step = 1.0 / 200;

    for (int i = 0; i < iterations; ++i)
    {
        t2 = t1;
        depth2 = depth1;

        // t1 += 0.5;
        t1 += dist * step;
        dist += dist * step;
        depth1 = -fog_field_value(positionWS + v_proj * t1) * depth;
        if (d_proj * t1 < depth1)
            break;
    }

    float k = -(d_proj * t1 - depth1) / ((d_proj * t2 - d_proj * t1) - (depth2 - depth1));
    float t = lerp(t1, t2, clamp(k, 0, 1));
    // float t = t2;

    // float t = 0; // 0.5 * depth;
    // for (int i = 0; i < 3; ++i)
    // {
    //     float field_depth = -fog_field_value(positionWS + v_proj * t) * depth;
    //     t = field_depth / d_proj;
    // }
    return positionWS + v_proj * t;
}
