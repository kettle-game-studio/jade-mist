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
    // a *= voronoi3(coord, 0.5 / 3, 0);
    // a *= voronoi3(coord + float3(0, _Time.y * 5, _Time.y * 15) / 10, 0.5 / 3);
    // a *= voronoi3(coord + float3(0, _Time.y * 5, _Time.y * 10) / 10, 0.9 / 3);
    a *= simplex_voronoi3(coord + float3(0, _Time.y * 5, _Time.y * 15) / 10, 0.5 / 3 * 0.5);
    a *= simplex_voronoi3(coord + float3(0, _Time.y * 5, _Time.y * 10) / 10, 0.9 / 3 * 0.5);
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

float3 trace_fog_field(float3 positionWS, float3 normalWS, float depth, float gradient_noise, int iterations = 10)
{
    if (iterations <= 0)
        return positionWS;

    float3 view = normalize(positionWS - _WorldSpaceCameraPos);
    float  d_proj = dot(view, normalWS);
    float3 v_proj = view - normalWS * d_proj;

    float t1 = -0.5 * gradient_noise;
    // float t1 = 0.5 * hash12(screen_uv + _Time.xy) - 0.25;
    float depth1 = -fog_field_value(positionWS) * depth;
    float t2 = 0;
    float depth2 = depth1;

    float dist = distance(view, _WorldSpaceCameraPos);
    float step = 1.0 / 200;

    for (int i = 0; i < iterations; ++i)
    {
        t2 = t1;
        depth2 = depth1;

        t1 += 0.5;
        // t1 += dist * step;
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

struct FogSample
{
    float3 value;
    float3 position;
    float3 normal;
};

FogSample sample_fog_field(float3 position, float3 normal, float depth)
{
    FogSample output;

    float3 n = abs(normal);
    float3 tangent = float3(
        n.x <= n.y && n.x <= n.z ? 1 : 0,
        n.y <= n.z && n.y <= n.x ? 1 : 0,
        n.z <= n.x && n.z <= n.y ? 1 : 0
    );
    float3 bitangent = normalize(cross(normal, tangent));
    tangent = cross(normal, bitangent);

    float delta = 0.05;
    float3 t_position = position + delta * tangent;
    float3 b_position = position + delta * bitangent;

    float value   = fog_field_value(position);
    float t_value = fog_field_value(t_position);
    float b_value = fog_field_value(b_position);

    position   = position - depth * normal * value;
    t_position = t_position - depth * normal * t_value;
    b_position = b_position - depth * normal * b_value;

    output.normal = float3(value, t_value, b_value);
    output.value = value;
    output.position = position;
    output.normal = -normalize(cross(
        position - t_position,
        position - b_position
    ));
    return output;
}
