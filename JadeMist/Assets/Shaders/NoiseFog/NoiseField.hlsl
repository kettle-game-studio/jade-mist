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
