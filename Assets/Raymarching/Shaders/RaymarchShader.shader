Shader "Raymarch/RaymarchShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        // Unity Includes
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        // Includes
        #include "DistanceFunctions.hlsl"
        #include "InfoStructs.hlsl"
        #include "Ray.hlsl"

        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        // Camera
        uniform float4x4 _CamToWorldMatrix;

        // Raymarching
        uniform float _RenderDistance;
        uniform float _HitResolution;
        uniform float _Relaxation;
        uniform int _MaxIterations;

        // Lighting & Shadows
        uniform float3 _AmbientColour;

        // Ambient Occlusion
        uniform float _AoStepSize;
        uniform float _AoIntensity;
        uniform int _AoIterations;

        // Shadows
        #pragma multi_compile NO_SHADOWS HARD_SHADOWS SOFT_SHADOWS

        #if NO_SHADOWS
        #else
        uniform float _ShadowIntensity;
        uniform int _ShadowSteps;
        uniform float2 _ShadowDistance;
        #endif

        #if SOFT_SHADOWS
        uniform float _ShadowPenumbra;
        #endif

        // Raymarch Object Info
        uniform StructuredBuffer<RaymarchObjectInfo> _ObjectInfo;
        uniform int _ObjectInfoCount;

        // Raymarch Modifier Info
        uniform StructuredBuffer<RaymarchModifierInfo> _ModifierInfo;
        uniform int _ModifierInfoCount;

        // Raymarch Light Info
        uniform StructuredBuffer<RaymarchLightInfo> _LightInfo;
        uniform int _LightInfoCount;

        float PerformSDF(float3 origin, RaymarchObjectInfo object)
        {
            // Position
            object.Position = origin - object.Position;

            // Rotation
            object.Position.xz = mul(object.Position.xz,
                                     float2x2(cos(object.Rotation.y), sin(object.Rotation.y),
                                              -sin(object.Rotation.y), cos(object.Rotation.y)));
            object.Position.yz = mul(object.Position.yz,
                                     float2x2(cos(object.Rotation.x), -sin(object.Rotation.x),
                                              sin(object.Rotation.x), cos(object.Rotation.x)));
            object.Position.xy = mul(object.Position.xy,
                                     float2x2(cos(object.Rotation.z), -sin(object.Rotation.z),
                                              sin(object.Rotation.z), cos(object.Rotation.z)));

            // Scale
            object.Scale *= 0.5;
            // object.Roundness *= 0.5;
            object.Scale -= object.Roundness;

            float dist = _RenderDistance;

            switch (object.SdfShape)
            {
            case 0:
                {
                    dist = sdf_sphere(object);
                    break;
                }
            case 1:
                {
                    dist = sdf_box(object);
                    break;
                }
            default: return _RenderDistance;
            }

            return (abs(dist - object.Roundness + object.WallThickness) - object.WallThickness) / object.
                MarchingStepAmount;
        }

        float4 PerformModifier(float globalDistance, float3 globalColour, float objectDistance,
                               RaymarchObjectInfo object, RaymarchModifierInfo modifier)
        {
            float distance = globalDistance;
            float3 colour = globalColour;

            // https://www.iquilezles.org/www/articles/smin/smin.htm

            switch (modifier.Operation)
            {
            case 1: // blend
                {
                    float h = clamp(0.5 + 0.5 * (objectDistance - distance) / modifier.OperationMod, 0.0, 1.0);
                    distance = lerp(objectDistance, distance, h) - modifier.OperationMod * h * (1.0 - h);
                    colour = lerp(object.Colour, colour, h);
                    break;
                }
            case 2: // cut
                {
                    if (modifier.OperationSmooth)
                    {
                        float h = clamp(0.5 - 0.5 * (distance + objectDistance) / modifier.OperationMod, 0.0, 1.0);
                        distance = lerp(distance, -objectDistance, h) + modifier.OperationMod * h * (1.0 - h);
                        break;
                    }

                    if (-objectDistance > distance)
                    {
                        distance = -objectDistance;
                    }
                    break;
                }
            case 3: // mask
                {
                    if (modifier.OperationSmooth)
                    {
                        float h = clamp(0.5 - 0.5 * (distance - objectDistance) / modifier.OperationMod, 0.0, 1.0);
                        distance = lerp(distance, objectDistance, h) + modifier.OperationMod * h * (1.0 - h);
                        break;
                    }

                    if (objectDistance > distance)
                    {
                        distance = objectDistance;
                    }
                    break;
                }
            default:
                {
                    if (objectDistance < distance)
                    {
                        distance = objectDistance;
                        colour = object.Colour;
                    }
                    break;
                }
            }

            return float4(colour.xyz, distance);
        }


        float4 GetDistanceFromObjects(float3 origin)
        {
            float resultDistance = _RenderDistance;
            float3 resultColour = float3(1, 1, 1);

            for (int i = 0; i < _ObjectInfoCount; i++)
            {
                float distance = _RenderDistance;
                float3 colour = float3(1, 1, 1);

                if (_ObjectInfo[i].ModifierIndex < 0) // no modifier
                {
                    if (_ObjectInfo[i].IsVisible == 0) continue;

                    distance = PerformSDF(origin, _ObjectInfo[i]);
                    colour = _ObjectInfo[i].Colour;
                }
                else
                {
                    int modIndex = _ObjectInfo[i].ModifierIndex;
                    int endIndex = i + _ModifierInfo[modIndex].NumOfObjects;

                    distance = PerformSDF(origin, _ObjectInfo[i]);
                    colour = _ObjectInfo[i].Colour;

                    for (int j = i + 1; j < endIndex && j < _ObjectInfoCount; j++)
                    {
                        if (_ObjectInfo[j].IsVisible == 0) continue;

                        float objectDistance = PerformSDF(origin, _ObjectInfo[j]);
                        float4 modifier = PerformModifier(distance, colour, objectDistance, _ObjectInfo[j],
                                                          _ModifierInfo[modIndex]);

                        distance = modifier.w;
                        colour = modifier.xyz;
                    }

                    i = endIndex - 1;
                }

                if (distance < resultDistance)
                {
                    resultDistance = distance;
                    resultColour = colour;
                }
            }

            return float4(resultColour.xyz, resultDistance);
        }

        #if HARD_SHADOWS
        float HardShadow(float3 pos, float3 dir)
        {
            float t = _ShadowDistance.x;

            for (int i = 0; i < _ShadowSteps; i++)
            {
                if (t >= _ShadowDistance.y) break;

                const float h = GetDistanceFromObjects(pos + dir * t);
                if (h < _HitResolution)
                {
                    return 0.0;
                }
                t += h;
            }
            return 1.0;
        }
        #endif

        #if SOFT_SHADOWS
        float SoftShadow(float3 pos, float3 dir, float k)
        {
            float res = 1.0;
            float t = _ShadowDistance.x;
            for (int i = 0; i < _ShadowSteps; ++i)
            {
                if (t >= _ShadowDistance.y) break;

                const float h = GetDistanceFromObjects(pos + dir * t);
                if (h < _HitResolution)
                {
                    return 0.0;
                }

                res = min(res, k * h / t);
                t += h;
            }
            return res;
        }
        #endif

        float3 GetLight(float3 pos, float3 normal)
        {
            float3 light = float3(0, 0, 0);

            for (int i = 0; i < _LightInfoCount; i++)
            {
                switch (_LightInfo[i].LightType)
                {
                case 0: // directional
                    {
                        light += _LightInfo[i].Colour * max(0.0, dot(-normal, _LightInfo[i].Direction)) * _LightInfo[i].
                            Intensity;

                        #if HARD_SHADOWS
                        light *= max(0.0, pow(HardShadow(pos, -_LightInfo[i].Direction) * 0.5 + 0.5, _ShadowIntensity));
                        #endif

                        #if SOFT_SHADOWS
                        light *= max(0.0, pow(SoftShadow(pos, -_LightInfo[i].Direction, _ShadowPenumbra) * 0.5 + 0.5, _ShadowIntensity));
                        #endif

                        break;
                    }
                case 1: // point
                default:
                    {
                        //http://forum.unity3d.com/threads/light-attentuation-equation.16006/
                        float3 toLight = pos - _LightInfo[i].Position;
                        float range = clamp(length(toLight) / _LightInfo[i].Range, 0., 1.);
                        float attenuation = 1.0 / (1.0 + 256.0 * range * range);

                        light += max(0.0, _LightInfo[i].Colour * dot(-normal, normalize(toLight.xyz))) *
                            _LightInfo[i].Intensity * attenuation;
                        break;
                    }
                }
            }

            return light;
        }

        float GetAmbientOcclusion(float3 pos, float3 normal)
        {
            float step = _AoStepSize;
            float ao = 0.0;
            float dist;

            for (int i = 1; i <= _AoIterations; i++)
            {
                dist = step * i;
                ao += max(0.0, (dist - GetDistanceFromObjects(pos + normal * dist)) / dist);
            }

            return 1.0 - ao * _AoIntensity;
        }

        float3 GetObjectNormal(float3 pos)
        {
            float2 offset = float2(0.01f, 0.0f);
            float3 normal = float3(
                GetDistanceFromObjects(pos + offset.xyy).w - GetDistanceFromObjects(pos - offset.xyy).w,
                GetDistanceFromObjects(pos + offset.yxy).w - GetDistanceFromObjects(pos - offset.yxy).w,
                GetDistanceFromObjects(pos + offset.yyx).w - GetDistanceFromObjects(pos - offset.yyx).w
            );

            return normalize(normal);
        }

        float3 CalculateLighting(Ray ray, float3 colour, float distance)
        {
            // Object Shading
            float3 pos = ray.Origin + ray.Direction * distance;
            float3 normal = GetObjectNormal(pos);

            // Adding Light
            float3 combinedColour = colour * _AmbientColour;
            combinedColour *= GetAmbientOcclusion(pos, normal);
            combinedColour += GetLight(pos, normal) * colour;

            return combinedColour;
        }

        half4 RaymarchSimple(Ray ray, float depth)
        {
            float distanceTraveled = _ProjectionParams.y; // near clip plane

            [loop]
            for (int i = 0; i < _MaxIterations; i++)
            {
                if (distanceTraveled > _RenderDistance || distanceTraveled >= depth)
                {
                    // Environment
                    return half4(ray.Direction, 0);
                }

                float3 pos = ray.Origin + ray.Direction * distanceTraveled;
                float4 combined = GetDistanceFromObjects(pos);
                float3 colour = combined.xyz;
                float distance = combined.w;

                if (distance < _HitResolution) // Hit something
                {
                    return half4(CalculateLighting(ray, colour, distanceTraveled), 1);
                }

                distanceTraveled += distance;
            }

            return half4(ray.Direction, 0);
        }

        half4 Raymarch(Ray ray, float depth)
        {
            float relaxOmega = _Relaxation;
            float distanceTraveled = _ProjectionParams.y; // near clip plane
            float candidateError = _RenderDistance;
            float candidateDistanceTraveled = distanceTraveled;
            float3 candidateColour = float3(0, 0, 0);
            float prevRadius = 0;
            float stepLength = 0;

            float funcSign = GetDistanceFromObjects(ray.Origin).w < 0 ? -1 : +1;

            [loop]
            for (int i = 0; i < _MaxIterations; i++)
            {
                float3 pos = ray.Origin + ray.Direction * distanceTraveled;
                float4 combined = GetDistanceFromObjects(pos);
                float3 colour = combined.xyz;
                float distance = combined.w;

                float signedRadius = funcSign * distance;
                float radius = abs(signedRadius);

                bool sorFail = relaxOmega > 1 && (radius + prevRadius) < stepLength;

                [branch]
                if (sorFail)
                {
                    stepLength -= relaxOmega * stepLength;
                    relaxOmega = 1;
                }
                else
                {
                    stepLength = signedRadius * relaxOmega;
                }

                prevRadius = radius;

                [branch]
                if (sorFail)
                {
                    distanceTraveled += stepLength;
                    continue;
                }

                if (distanceTraveled > _RenderDistance || distanceTraveled >= depth) // Environment
                {
                    return half4(ray.Direction, 0);
                }

                float error = radius / distanceTraveled;

                if (error < candidateError)
                {
                    candidateDistanceTraveled = distanceTraveled;
                    candidateColour = colour;
                    candidateError = error;

                    if (error < _HitResolution) break; // Hit Something
                }

                distanceTraveled += stepLength;
            }

            return half4(CalculateLighting(ray, candidateColour, candidateDistanceTraveled), 1);
        }
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                #ifdef UNITY_UV_STARTS_AT_TOP
                // v.uv.y = 1 - v.uv.y;
                #endif

                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = UnityStereoTransformScreenSpaceTex(v.uv);
                return o;
            }

            half4 frag(v2f i) : SV_Target0
            {
                Ray ray = CreateCameraRay(i.uv, _CamToWorldMatrix);

                #if UNITY_REVERSED_Z
                float depth = SampleSceneDepth(i.uv);
                #else
                // Adjust z to match NDC for OpenGL
                float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
                #endif

                depth = LinearEyeDepth(depth, _ZBufferParams) * ray.Length;

                half4 result = Raymarch(ray, depth);
                return half4(tex2D(_MainTex, i.uv).xyz * (1.0 - result.w) + result.xyz * result.w, 1.0);
            }
            ENDHLSL
        }
    }
}