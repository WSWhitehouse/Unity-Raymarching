#ifndef RAYMARCHING_RAY_HLSL
#define RAYMARCHING_RAY_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct Ray
{
    float3 Origin;
    float3 Direction;
    float Length;
};

inline Ray CreateRay(float3 origin, float3 direction, float dirLength)
{
    Ray ray;
    ray.Origin = origin;
    ray.Direction = direction;
    ray.Length = dirLength;
    return ray;
}

inline Ray CreateCameraRay(float2 uv, float4x4 camToWorld)
{
    float3 origin = GetCameraPositionWS();
    float3 direction = mul(unity_CameraInvProjection, float4(uv * 2 - 1, 0, 1)).xyz;
    direction = mul(camToWorld, float4(direction, 0)).xyz;
    float dirLength = length(direction);
    direction = normalize(direction);

    return CreateRay(origin, direction, dirLength);
}

#endif // RAYMARCHING_RAY_HLSL
