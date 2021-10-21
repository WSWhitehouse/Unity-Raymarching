#ifndef RAYMARCHING_STRUCTS_HLSL
#define RAYMARCHING_STRUCTS_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


// RAY
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

// OBJECT DISTANCE RESULT
struct ObjectDistanceResult
{
    float Distance;
    float4 Colour;
};

inline ObjectDistanceResult CreateObjectDistanceResult(float distance, float4 colour)
{
    ObjectDistanceResult objData;
    objData.Distance = distance;
    objData.Colour = colour;
    return objData;
}

// RAYMARCH RESULT
struct RaymarchResult
{
    int Succeeded; // NOTE(WSWhitehouse): Succeeded if greater than 0
    half4 Colour;
};

inline RaymarchResult CreateRaymarchResult(int succeeded, half4 colour)
{
    RaymarchResult raymarchResult;
    raymarchResult.Succeeded = succeeded;
    raymarchResult.Colour = colour;
    return raymarchResult;
}

#endif // RAYMARCHING_STRUCTS_HLSL
