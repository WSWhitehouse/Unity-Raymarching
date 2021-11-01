//---------------------------------------------------------------------
//    This code was generated by a tool.
//
//    Changes to this file may cause incorrect behavior and will be 
//    lost if the code is regenerated.
//
//    Time Generated: 10/29/2021 12:23:52
//---------------------------------------------------------------------

#ifndef OPERATIONFUNCTIONS_HLSL
#define OPERATIONFUNCTIONS_HLSL

// Unity Includes
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

void Oper_Blend_c08c11b6fc54453486aa264d1da70b87(inout float resultDistance, inout float4 resultColour, float objDistance, float4 objColour, float Smooth)
{
float h = clamp(0.5 + 0.5 * (objDistance - resultDistance) / Smooth, 0.0, 1.0);
                    
resultDistance = lerp(objDistance, resultDistance, h) - Smooth * h * (1.0 - h);
resultColour = lerp(objColour, resultColour, h);
}

void Oper_Cut_e668285f5b654bfab03360efeb593db7(inout float resultDistance, inout float4 resultColour, float objDistance, float4 objColour)
{
if (-objDistance > resultDistance)
{
 resultDistance = -objDistance;
}
}

void Oper_Mask_062237fd8d9d405e96ab73d8d481bdad(inout float resultDistance, inout float4 resultColour, float objDistance, float4 objColour)
{
if (objDistance > resultDistance)
{
resultDistance = objDistance;
}
}

void Oper_SmoothCut_e1e49a0edb304f6ba07df634e603543e(inout float resultDistance, inout float4 resultColour, float objDistance, float4 objColour, float Smooth)
{
float h = clamp(0.5 - 0.5 * (resultDistance + objDistance) / Smooth, 0.0, 1.0);
resultDistance = lerp(resultDistance, -objDistance, h) + Smooth * h * (1.0 - h);
}

void Oper_SmoothMask_f4015abe8c464f9aae1b8dfb6eb494f7(inout float resultDistance, inout float4 resultColour, float objDistance, float4 objColour, float Smooth)
{
float h = clamp(0.5 - 0.5 * (resultDistance - objDistance) / Smooth, 0.0, 1.0);
resultDistance = lerp(resultDistance, objDistance, h) + Smooth * h * (1.0 - h);
}

#endif // OPERATIONFUNCTIONS_HLSL