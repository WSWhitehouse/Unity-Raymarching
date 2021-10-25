//---------------------------------------------------------------------

//    This code was generated by a tool.                               

//                                                                     

//    Changes to this file may cause incorrect behavior and will be    

//    lost if the code is regenerated.                                 

//                                                                     

//    Time Generated: 10/25/2021 15:03:06     

//---------------------------------------------------------------------

#ifndef SDFFUNCTIONS_HLSL
#define SDFFUNCTIONS_HLSL

// Unity Includes 
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

float SDF_Capsule_6dd9ace8ea81468da4fe7ff03e5332bb(float3 pos, float3 scale, float3 a, float3 b, float r)
{
float3 pa = pos - a;
float3 ba = b - a;
float h = clamp(dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
return length( pa - ba*h ) - r;
}

float SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(float3 pos, float3 scale)
{
float3 o = abs(pos) - scale;
return length(max(o, 0.0)) + min(max(o.x, max(o.y, o.z)), 0.0);
}

float SDF_Sphere_5a5c930dec9347e2970ec043d92e6116(float3 pos, float3 scale)
{
return length(pos) - min(scale.x, min(scale.y, scale.z));
}

float SDF_Test_78b7fc6cdf924ebf90ce9825f126d7f5(float3 pos, float3 scale)
{
return 0; 
}

#endif // SDFFUNCTIONS_HLSL
