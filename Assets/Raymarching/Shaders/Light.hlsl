#ifndef RAYMARCHING_LIGHT_HLSL
#define RAYMARCHING_LIGHT_HLSL

float4 GetDirectionalLight(float3 objPos, float3 normal, float4 lightCol, float3 lightDir, float lightIntensity)
{
    return max(0.0, dot(-normal, lightDir)) * lightCol * lightIntensity;
}

float4 GetPointLight(float3 objPos, float3 normal, float3 lightPos, float4 lightCol, float lightRange,
                     float lightIntensity)
{
    // http://forum.unity3d.com/threads/light-attentuation-equation.16006/
    float3 toLight = objPos - lightPos;
    float range = clamp(length(toLight) / lightRange, 0.0, 1.0);
    float attenuation = 1.0 / (1.0 + 256.0 * range * range);

    return max(0.0, dot(-normal, normalize(toLight.xyz))) * lightCol * lightIntensity * attenuation;
}

float4 GetSpotLight(float3 objPos, float3 normal, float3 lightPos, float4 lightCol, float3 lightDir, float lightRange,
                    float lightIntensity, float spotAngle, float innerSpotAngle)
{
    // http://forum.unity3d.com/threads/light-attentuation-equation.16006/
    float3 toLight = objPos - lightPos;
    float range = clamp(length(toLight) / lightRange, 0.0, 1.0);
    float attenuation = 1.0 / (1.0 + 256.0 * range * range);

    // https://developer.download.nvidia.com/CgTutorial/cg_tutorial_chapter05.html
    float innerCos = cos(innerSpotAngle);
    float outerCos = cos(spotAngle);
    float dirCos = dot(normalize(toLight), normalize(lightDir));
    float spotEffect = smoothstep(outerCos, innerCos, dirCos);

    return max(0.0, dot(-normal, normalize(toLight.xyz))) * lightCol * lightIntensity * attenuation * spotEffect;
}

#endif // RAYMARCHING_LIGHT_HLSL
