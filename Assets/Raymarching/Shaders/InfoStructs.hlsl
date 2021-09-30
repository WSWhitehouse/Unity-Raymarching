#ifndef RAYMARCHING_INFO_STRUCTS_HLSL
#define RAYMARCHING_INFO_STRUCTS_HLSL

struct RaymarchObjectInfo
{
    int IsVisible;
    
    int SdfShape;
    float MarchingStepAmount;

    float3 Position;
    float4 Rotation;
    float3 Scale;

    float3 Colour;

    int Operation;
    int OperationSmooth;
    float OperationMod;
    int OperationLayer;

    float Roundness;
    float WallThickness;
};

struct RaymarchLightInfo
{
    int LightType;
    
    float3 Position;
    float3 Direction;

    float3 Colour;
    float Range;
    float Intensity;
};

#endif // RAYMARCHING_INFO_STRUCTS_HLSL
