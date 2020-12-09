#ifndef INTERPOLATION_F
#define INTERPOLATION_F

// LERP
inline float Lerp(float from, float to, float rel)
{
    return ((1 - rel) * from) + (rel * to);
}

inline float2 Lerp(float2 from, float2 to, float2 rel)
{
    return ((1 - rel) * from) + (rel * to);
}

inline float3 Lerp(float3 from, float3 to, float3 rel)
{
    return ((1 - rel) * from) + (rel * to);
}

inline float4 Lerp(float4 from, float4 to, float4 rel)
{
    return ((1 - rel) * from) + (rel * to);
}

// INVERSE LERP
inline float InvLerp(float from, float to, float value)
{
    return (value - from) / (to - from);
}

inline float2 InvLerp(float2 from, float2 to, float2 value)
{
    return (value - from) / (to - from);
}

inline float3 InvLerp(float3 from, float3 to, float3 value)
{
    return (value - from) / (to - from);
}

inline float4 InvLerp(float4 from, float4 to, float4 value)
{
    return (value - from) / (to - from);
}

// REMAP
inline float Remap(float orig_from, float orig_to, float target_from, float target_to, float value)
{
    float rel = InvLerp(orig_from, orig_to, value);
    return Lerp(target_from, target_to, rel);
}

inline float2 Remap(float2 orig_from, float2 orig_to, float2 target_from, float2 target_to, float2 value)
{
    float2 rel = InvLerp(orig_from, orig_to, value);
    return Lerp(target_from, target_to, rel);
}

inline float3 Remap(float3 orig_from, float3 orig_to, float3 target_from, float3 target_to, float3 value)
{
    float3 rel = InvLerp(orig_from, orig_to, value);
    return Lerp(target_from, target_to, rel);
}

inline float4 Remap(float4 orig_from, float4 orig_to, float4 target_from, float4 target_to, float4 value)
{
    float4 rel = InvLerp(orig_from, orig_to, value);
    return Lerp(target_from, target_to, rel);
}

#endif