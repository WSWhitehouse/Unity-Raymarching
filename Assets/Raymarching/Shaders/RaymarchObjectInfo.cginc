#ifndef WSWHITEHOUSE_RAYMARCH_OBJECT_INFO_CGINC
#define WSWHITEHOUSE_RAYMARCH_OBJECT_INFO_CGINC

struct RaymarchObjectInfo
{
    int SdfShape;
    float MarchingStepAmount;

    float3 Position;
    float4 Rotation;
    float3 Scale;

    float4 Colour;

    int Operation;
    int OperationSmooth;
    float OperationMod;

    float Roundness;
    float WallThickness;
};

#endif // WSWHITEHOUSE_RAYMARCH_OBJECT_INFO_CGINC
