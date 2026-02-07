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

float2 normal_to_simplex_dim2(float2 coord)
{
    float dimension = 2;
    float f = (sqrt(dimension + 1) - 1) / dimension;
    float coord_sum = coord.x + coord.y;
    return coord + coord_sum * f;
}

float2 simplex_to_normal_dim2(float2 coord)
{
    float dimension = 2;
    float g = (1 - 1 / sqrt(dimension + 1)) / dimension;
    float coord_sum = coord.x + coord.y;
    return coord - coord_sum * g;
}

float simplex_voronoi2(float2 coord, float density, float shift = 1)
{
    coord = normal_to_simplex_dim2(coord);
    float2 cell = floor(coord * density);
    float2 offset = frac(coord * density);
    float2 points[3] = {
        float2(0, 0),
        (offset.x > offset.y ? float2(1, 0) : float2(0, 1)),
        float2(1, 1)
    };

    offset = simplex_to_normal_dim2(offset);
    float result = 1;
    for (int i = 0; i < 3; ++i)
    {
        float2 p = points[i];
        p = simplex_to_normal_dim2(p + (hash22(cell + p) - 0.5) * shift * 0.4); // TODO: 0.4??
        result = min(result, distance(p, offset) * 1.7); // TODO: 1.7??
    }
    return result;
}

float3 normal_to_simplex_dim3(float3 coord)
{
    float dimension = 3;
    float f = (sqrt(dimension + 1) - 1) / dimension;
    float coord_sum = coord.x + coord.y + coord.z;
    return coord + coord_sum * f;
}

float3 simplex_to_normal_dim3(float3 coord)
{
    float dimension = 3;
    float g = (1 - 1 / sqrt(dimension + 1)) / dimension;
    float coord_sum = coord.x + coord.y + coord.z;
    return coord - coord_sum * g;
}

float simplex_voronoi3(float3 coord, float density, float shift = 1)
{
    coord = normal_to_simplex_dim3(coord);

    float3 cell = floor(coord * density);
    float3 offset = frac(coord * density);

    bool x_y = offset.x > offset.y;
    bool y_z = offset.y > offset.z;
    bool z_x = offset.z > offset.x;

    float3 points[4] = {
        float3(0, 0, 0),
        x_y && !z_x ? float3(1, 0, 0) : y_z ? float3(0, 1, 0) : float3(0, 0, 1),
        !x_y && z_x ? float3(0, 1, 1) : !y_z ? float3(1, 0, 1) : float3(1, 1, 0),
        float3(1, 1, 1)
    };

    offset = simplex_to_normal_dim3(offset);
    float result = 1;
    for (int i = 0; i < 4; ++i)
    {
        float3 p = points[i];
        p = simplex_to_normal_dim3(p + (hash33(cell + p) - 0.5) * shift * 0.3); // TODO: 0.3??
        result = min(result, distance(p, offset) * 1.6); // TODO: 1.6??
    }
    return result;
}
