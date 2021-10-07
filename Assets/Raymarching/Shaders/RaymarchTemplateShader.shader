Shader "Raymarch/RaymarchTemplateShader"
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
        #include "Assets/Raymarching/Shaders/Generated/DistanceFunctions.hlsl"
        #include "Assets/Raymarching/Shaders/Generated/MaterialFunctions.hlsl"
        #include "Assets/Raymarching/Shaders/Ray.hlsl"

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

        // RAYMARCH VARS //

        float4 GetDistanceFromObjects(float3 origin)
        {
            float resultDistance = _RenderDistance;
            float3 resultColour = float3(1, 1, 1);

            // RAYMARCH CALC DISTANCE //


            // if (distance < resultDistance)
            // {
            //     resultDistance = distance;
            //     resultColour = colour;
            // }


            return float4(resultColour.xyz, resultDistance);
        }

        float3 GetLight(float3 pos, float3 normal)
        {
            float3 light = float3(0, 0, 0);
            
            // RAYMARCH CALC LIGHT //

            return light;
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
            combinedColour += GetLight(pos, normal) * colour;

            return combinedColour;
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

            float funcSign = GetDistanceFromObjects(ray.Origin).w < 0 ? +1 : +1;

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