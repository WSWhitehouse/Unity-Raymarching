#ifndef WSWHITEHOUSE_DISTANCE_FUNCTIONS_CGINC
#define WSWHITEHOUSE_DISTANCE_FUNCTIONS_CGINC

#include "RaymarchObjectInfo.cginc"

float sdf_sphere(in RaymarchObjectInfo object)
{
    return length(object.Position) - object.Scale.x;
}

float sdf_box(in RaymarchObjectInfo object)
{
    float3 o = abs(object.Position) - object.Scale;
    return length(max(o, 0.0)) + min(max(o.x, max(o.y, o.z)), 0.0);
}

#endif // WSWHITEHOUSE_DISTANCE_FUNCTIONS_CGINC
