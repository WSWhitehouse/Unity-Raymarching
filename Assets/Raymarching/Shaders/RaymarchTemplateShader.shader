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
    #include "Assets/Raymarching/Shaders/Generated/SDFFunctions.hlsl"
    #include "Assets/Raymarching/Shaders/Generated/MaterialFunctions.hlsl"
    #include "Assets/Raymarching/Shaders/Generated/ModifierFunctions.hlsl"
    #include "Assets/Raymarching/Shaders/Generated/OperationFunctions.hlsl"
    #include "Assets/Raymarching/Shaders/Structs.hlsl"
    #include "Assets/Raymarching/Shaders/Light.hlsl"

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
    uniform float4 _AmbientColour;
    uniform float _ColourMultiplier;

    // RAYMARCH VARS //

    float3 Rotate3D(float3 pos, float3 rot)
    {
      pos.xz = mul(pos.xz, float2x2(cos(rot.y), sin(rot.y), -sin(rot.y), cos(rot.y)));
      pos.yz = mul(pos.yz, float2x2(cos(rot.x), -sin(rot.x), sin(rot.x), cos(rot.x)));
      pos.xy = mul(pos.xy, float2x2(cos(rot.z), -sin(rot.z), sin(rot.z), cos(rot.z)));
      return pos;
    }

    ObjectDistanceResult GetDistanceFromObjects(float3 rayPos)
    {
      float resultDistance = _RenderDistance;
      float4 resultColour = float4(1, 1, 1, 1);

      // RAYMARCH CALC DISTANCE //

      return CreateObjectDistanceResult(resultDistance, resultColour);
    }

    float4 GetLight(float3 pos, float3 normal)
    {
      float3 light = float3(0, 0, 0);

      // RAYMARCH CALC LIGHT //

      return float4(light.xyz, 1.0);
    }

    float3 GetObjectNormal(float3 pos)
    {
      float2 offset = float2(0.01, 0.0);
      float3 normal = float3(
        GetDistanceFromObjects(pos + offset.xyy).Distance - GetDistanceFromObjects(pos - offset.xyy).Distance,
        GetDistanceFromObjects(pos + offset.yxy).Distance - GetDistanceFromObjects(pos - offset.yxy).Distance,
        GetDistanceFromObjects(pos + offset.yyx).Distance - GetDistanceFromObjects(pos - offset.yyx).Distance
      );

      return normalize(normal);
    }

    half4 CalculateLighting(Ray ray, half4 colour, float distance)
    {
      // Object Shading
      float3 pos = ray.Origin + ray.Direction * distance;
      float3 normal = GetObjectNormal(pos);

      // Adding Light
      half4 combinedColour = colour * _AmbientColour;
      combinedColour += half4(GetLight(pos, normal).xyz, 1.0) * colour;

      return combinedColour;
    }

    RaymarchResult Raymarch(Ray ray, float depth)
    {
      float relaxOmega = _Relaxation;
      float distanceTraveled = _ProjectionParams.y; // near clip plane
      float candidateError = _RenderDistance;
      float candidateDistanceTraveled = distanceTraveled;
      half4 candidateColour = half4(0, 0, 0, 0);
      float prevRadius = 0;
      float stepLength = 0;

      float funcSign = GetDistanceFromObjects(ray.Origin).Distance < 0 ? +1 : +1;

      [loop]
      for (int i = 0; i < _MaxIterations; i++)
      {
        float3 pos = ray.Origin + ray.Direction * distanceTraveled;
        ObjectDistanceResult objData = GetDistanceFromObjects(pos);

        float signedRadius = funcSign * objData.Distance;
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
          return CreateRaymarchResult(0, half4(0, 0, 0, 0));
        }

        float error = radius / distanceTraveled;

        if (error < candidateError)
        {
          candidateDistanceTraveled = distanceTraveled;
          candidateColour = objData.Colour;
          candidateError = error;

          if (error < _HitResolution) break; // Hit Something
        }

        distanceTraveled += stepLength;
      }

      return CreateRaymarchResult(1, CalculateLighting(ray, candidateColour, candidateDistanceTraveled));
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

        RaymarchResult raymarchResult = Raymarch(ray, depth);

        return half4(
          tex2D(_MainTex, i.uv) * (1.0 - raymarchResult.Succeeded) +
          (raymarchResult.Colour * _ColourMultiplier) * raymarchResult.Succeeded);
      }
      ENDHLSL
    }
  }
}