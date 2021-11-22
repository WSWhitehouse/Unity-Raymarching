//---------------------------------------------------------------------
//    This code was generated by a tool.
//
//    Changes to this file may cause incorrect behavior and will be 
//    lost if the code is regenerated.
//
//    Time Generated: 11/22/2021 18:32:14
//---------------------------------------------------------------------

#ifndef SDFFUNCTIONS_HLSL
#define SDFFUNCTIONS_HLSL

// Unity Includes
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

float SDF_Capsule_6dd9ace8ea81468da4fe7ff03e5332bb(float4 pos, float Height, float Radius)
{
//Height = max(0, Height -1);

float3 pa = pos.xyz - float3(0, abs(Height) * 0.5, 0);
float3 ba = float3(0, -abs(Height) * 0.5, 0) - float3(0, abs(Height) * 0.5, 0);

float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
return length( pa - ba*h ) - Radius;
}

float SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(float4 pos, float4 Dimensions)
{
float4 o = abs(pos) - Dimensions;
return length(max(o, 0.0)) + min(max(o.x, max(o.y, o.z)), 0.0);
}

float SDF_Hypercube_78b7fc6cdf924ebf90ce9825f126d7f5(float4 pos, float4 Dimensions)
{
    float4 d = abs(pos) - Dimensions;
    return min(max(d.x, max(d.y, max(d.z, d.w))), 0.0) + length(max(d, 0.0));
}

float SDF_Sphere_5a5c930dec9347e2970ec043d92e6116(float4 pos, float Radius)
{
return length(pos) - Radius;
}

#endif // SDFFUNCTIONS_HLSL