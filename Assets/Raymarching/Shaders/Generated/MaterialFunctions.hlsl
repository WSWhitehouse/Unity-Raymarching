//---------------------------------------------------------------------
//    This code was generated by a tool.
//
//    Changes to this file may cause incorrect behavior and will be 
//    lost if the code is regenerated.
//
//    Time Generated: 10/29/2021 12:23:38
//---------------------------------------------------------------------

#ifndef MATERIALFUNCTIONS_HLSL
#define MATERIALFUNCTIONS_HLSL

// Unity Includes
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

float4 Mat_TextureMaterial_c3735437331f4f80a12534d02a465e6a(float3 pos, float4 colour, sampler2D Texture)
{
float3x3 R = float3x3(float3(cos(_Time.y),sin(_Time.y),0),float3(-sin(_Time.y),cos(_Time.y),0),float3(0,0,-1));
//pos = mul(pos, R / 8);


return float4(
float3((tex2D(Texture, pos.xy).rgb
+ tex2D(Texture, pos.zy).rgb
 + tex2D(Texture, pos.xz).rgb) / 3.0), 1) * colour;
}

#endif // MATERIALFUNCTIONS_HLSL