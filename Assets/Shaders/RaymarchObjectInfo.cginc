#ifndef WSWHITEHOUSE_RAYMARCH_OBJECT_INFO_CGINC
#define WSWHITEHOUSE_RAYMARCH_OBJECT_INFO_CGINC

struct RaymarchObjectInfo
{
    float3 Position;
    float4 Rotation;
    float3 Scale;
    
    float4 Colour;

    int Operation;
    float OperationMod;
};

#endif // WSWHITEHOUSE_RAYMARCH_OBJECT_INFO_CGINC
