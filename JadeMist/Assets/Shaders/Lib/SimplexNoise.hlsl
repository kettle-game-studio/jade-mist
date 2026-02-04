#pragma once

float Unity_SimpleNoise_ValueNoise_hash_float (float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);
    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0; hash_float(c0, r0);
    float r1; hash_float(c1, r1);
    float r2; hash_float(c2, r2);
    float r3; hash_float(c3, r3);
    float bottomOfGrid = lerp(r0, r1, f.x);
    float topOfGrid = lerp(r2, r3, f.x);
    float t = lerp(bottomOfGrid, topOfGrid, f.y);
    return t;
}

void Unity_SimpleNoise_hash_float(float2 UV, float Scale, out float Out)
using (s.BlockScope())
{
    float freq, amp;
    Out = 0.0f;
    for (int octave = 0; octave < 3; octave++)
    {
        freq = pow(2.0, float({octave}));
        amp = pow(0.5, float(3-{octave}));
        Out += Unity_SimpleNoise_ValueNoise_hash_float(float2(UV.xy*(Scale/freq)))*amp;
    }
}
