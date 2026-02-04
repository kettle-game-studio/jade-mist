#pragma once

#include "Hash.hlsl"

float voronoi2(float2 coord, float density, float shift = 1)
{
    float2 cell = floor(coord * density);
    float2 offset = frac(coord * density);
    float result = 1;
    for (int y = -1; y <= 1; ++y)
    {
        for (int x = -1; x <= 1; ++x)
        {
            float2 cell_center = hash22(cell + float2(x, y)) * shift;
            float2 local_center = cell_center + float2(x, y);
            result = min(result, distance(offset, local_center));
        }
    }
    return result;
}

float voronoi3(float3 coord, float density, float shift = 1)
{
    float3 cell = floor(coord * density);
    float3 offset = frac(coord * density);
    float result = 1;
    for (int y = -1; y <= 1; ++y)
    {
        for (int x = -1; x <= 1; ++x)
        {
            for (int z = -1; z <= 1; ++z)
            {
                float3 cell_center = hash33(cell + float3(x, y, z)) * shift;
                float3 local_center = cell_center + float3(x, y, z);
                result = min(result, distance(offset, local_center));
            }
        }
    }
    return result;
}
