#ifndef WSWHITEHOUSE_RAY_CGINC
#define WSWHITEHOUSE_RAY_CGINC

struct Ray
{
    float3 Origin;
    float3 Direction;
};

Ray CreateCameraRay(float2 uv, float4x4 camToWorld, float4x4 camInvProjection)
{
    float3 origin = mul(camToWorld, float4(0, 0, 0, 1)).xyz;
    float3 direction = mul(camInvProjection, float4(uv, 0, 1)).xyz;
    direction = mul(camToWorld, float4(direction, 0)).xyz;
    //direction = normalize(direction);

    Ray ray;
    ray.Origin = origin;
    ray.Direction = direction;
    return ray;
}

#endif // WSWHITEHOUSE_RAY_CGINC