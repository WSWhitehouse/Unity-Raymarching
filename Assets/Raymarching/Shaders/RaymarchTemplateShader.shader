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

    // DEBUG SETTINGS //

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

    // RAYMARCH SETTINGS START //
    static const float _RenderDistance = 100;
    static const float _HitResolution = 0.001;
    static const float _Relaxation = 1.2;
    static const int _MaxIterations = 164;
    // RAYMARCH SETTINGS END //

    // LIGHTING SETTINGS START //
    static const float4 _AmbientColour = float4(0.2117, 0.2274, 0.2588, 1);
    static const float _ColourMultiplier = 1;
    // LIGHTING SETTINGS END //

    // Camera Settings
    uniform float4x4 _CamToWorldMatrix;
    uniform float _CamPositionW;
    uniform float3 _CamRotation4D;

    // RAYMARCH VARS //

    inline float3 Rotate3D(in float3 pos, in float4 rotor)
    {
      /*
       * bi-vector components of the rotor
       * rotor.x = bivector xy = b01
       * rotor.y = bivector xz = b02
       * rotor.z = bivector yz = b12
       */

      // NOTE(zack): v = basis vectors in 3 dimensions
      float3 v;
      v.x = rotor.a * pos.x + pos.y * rotor.x + pos.z * rotor.y;
      v.y = rotor.a * pos.y - pos.x * rotor.x + pos.z * rotor.z;
      v.z = rotor.a * pos.z - pos.x * rotor.y - pos.y * rotor.z;

      float triVec = pos.x * rotor.z - pos.y * rotor.y + pos.z * rotor.x;

      // NOTE(zack): Reflection formula vector and bivector multiplication table
      float3 result;
      result.x = rotor.a * v.x + v.y * rotor.x + v.z * rotor.y + triVec * rotor.z;
      result.y = rotor.a * v.y - v.x * rotor.x - triVec * rotor.y + v.z * rotor.z;
      result.z = rotor.a * v.z + triVec * rotor.x - v.x * rotor.y - v.y * rotor.z;

      return result;
    }

    inline float4 Rotate4D(float4 pos, float3 rot)
    {
      pos.xw = mul(pos.xw, float2x2(cos(rot.x), sin(rot.x), -sin(rot.x), cos(rot.x)));
      pos.yw = mul(pos.yw, float2x2(cos(rot.y), -sin(rot.y), sin(rot.y), cos(rot.y)));
      pos.zw = mul(pos.zw, float2x2(cos(rot.z), -sin(rot.z), sin(rot.z), cos(rot.z)));

      return pos;
    }

    RaymarchMapResult RaymarchMap(float3 rayPos)
    {
      float4 rayPos4D = float4(rayPos, _CamPositionW);

      /* NOTE(WSWhitehouse): 
       * 
       */

      // if (length(_CamRotation4D) != 0)
      // {
      //   rayPos = Rotate4D(rayPos4D, _CamRotation4D);
      // }

      int CamRot = length(_CamRotation4D) != 0;
      rayPos4D = (Rotate4D(rayPos4D, _CamRotation4D) * CamRot) +
        (rayPos4D * !CamRot);

      float resultDistance = _RenderDistance;
      float4 resultColour = float4(1, 1, 1, 1);

      // RAYMARCH CALC DISTANCE //

      return CreateRaymarchMapResult(resultDistance, resultColour);
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
        RaymarchMap(pos + offset.xyy).Distance - RaymarchMap(pos - offset.xyy).Distance,
        RaymarchMap(pos + offset.yxy).Distance - RaymarchMap(pos - offset.yxy).Distance,
        RaymarchMap(pos + offset.yyx).Distance - RaymarchMap(pos - offset.yyx).Distance
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

      float funcSign = RaymarchMap(ray.Origin).Distance < 0 ? +1 : +1;

      [loop]
      for (int i = 0; i < _MaxIterations; i++)
      {
        float3 pos = ray.Origin + ray.Direction * distanceTraveled;
        RaymarchMapResult objData = RaymarchMap(pos);

        float signedRadius = funcSign * objData.Distance;
        float radius = abs(signedRadius);

        int sorFail = relaxOmega > 1 && (radius + prevRadius) < stepLength;

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
        float sceneDepth = SampleSceneDepth(i.uv);
        #else
        // Adjust z to match NDC for OpenGL
        float sceneDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(i.uv));
        #endif

        float depth = LinearEyeDepth(sceneDepth, _ZBufferParams) * ray.Length;

        RaymarchResult raymarchResult = Raymarch(ray, depth);
        return (half4(raymarchResult.Colour * _ColourMultiplier) * raymarchResult.Succeeded) +
               (half4(tex2D(_MainTex, i.uv)) * !raymarchResult.Succeeded);
      }
      ENDHLSL
    }
  }
}