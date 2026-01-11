void AddRandomSun_float(float2 uv, float4 color1, float4 color2, out float4 out_color) {
    int2 v = int2(floor((uv)));
    if (v.x % 3 == 0 && v.y % 3 == 0)
        out_color = color2;
    else
        out_color = color1; 
}

void CalcRoughness_float(float4 color, out float roughness) {
    if (color.x > 0.5 && color.y > 0.5 && color.z > 0.5)
        roughness = 1.0;
    else 
        roughness = 0.75;
}