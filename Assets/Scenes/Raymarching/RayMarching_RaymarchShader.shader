//---------------------------------------------------------------------
//    This code was generated by a tool.                               
//                                                                     
//    Changes to this file may cause incorrect behavior and will be    
//    lost if the code is regenerated.                                 
//                                                                     
//    Time Generated: 10/21/2021 16:48:22
//---------------------------------------------------------------------

Shader "Raymarch/RayMarching_RaymarchShader"
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
    uniform float _ColourScalar;

    // Raymarch Variables

uniform float3 _Position065aa5c7754e4ee3bf1117f267e253f0;
uniform float3 _Rotation065aa5c7754e4ee3bf1117f267e253f0;
uniform float3 _Scale065aa5c7754e4ee3bf1117f267e253f0;
uniform float4 _Colour065aa5c7754e4ee3bf1117f267e253f0;
uniform float _MarchingStepAmount065aa5c7754e4ee3bf1117f267e253f0;
uniform int _IsActive065aa5c7754e4ee3bf1117f267e253f0;
uniform sampler2D _Texture065aa5c7754e4ee3bf1117f267e253f0;
uniform float _Displacement065aa5c7754e4ee3bf1117f267e253f00;
uniform float _Freq065aa5c7754e4ee3bf1117f267e253f01;
uniform float _Amplitude065aa5c7754e4ee3bf1117f267e253f01;
uniform float _Speed065aa5c7754e4ee3bf1117f267e253f01;
uniform float3 _Dir065aa5c7754e4ee3bf1117f267e253f01;
uniform float _Displacement065aa5c7754e4ee3bf1117f267e253f02;
uniform float _Freq065aa5c7754e4ee3bf1117f267e253f03;
uniform float _Amplitude065aa5c7754e4ee3bf1117f267e253f03;
uniform float _Speed065aa5c7754e4ee3bf1117f267e253f03;
uniform float3 _Dir065aa5c7754e4ee3bf1117f267e253f03;

uniform float _Smooth10fc656da492424e9757568d34136d0e;
uniform int _IsActive10fc656da492424e9757568d34136d0e;

uniform float3 _Position94a3c657384a49dcae537aad3cfd17ad;
uniform float3 _Rotation94a3c657384a49dcae537aad3cfd17ad;
uniform float3 _Scale94a3c657384a49dcae537aad3cfd17ad;
uniform float4 _Colour94a3c657384a49dcae537aad3cfd17ad;
uniform float _MarchingStepAmount94a3c657384a49dcae537aad3cfd17ad;
uniform int _IsActive94a3c657384a49dcae537aad3cfd17ad;
uniform float _TwistAmountX94a3c657384a49dcae537aad3cfd17ad0;

uniform float3 _Position176c1ec3a1c64bb79ab34788ceb863d5;
uniform float3 _Rotation176c1ec3a1c64bb79ab34788ceb863d5;
uniform float3 _Scale176c1ec3a1c64bb79ab34788ceb863d5;
uniform float4 _Colour176c1ec3a1c64bb79ab34788ceb863d5;
uniform float _MarchingStepAmount176c1ec3a1c64bb79ab34788ceb863d5;
uniform int _IsActive176c1ec3a1c64bb79ab34788ceb863d5;

uniform float3 _Position9909da39672d4c0a9ddbeec2369d040d;
uniform float3 _Rotation9909da39672d4c0a9ddbeec2369d040d;
uniform float3 _Scale9909da39672d4c0a9ddbeec2369d040d;
uniform float4 _Colour9909da39672d4c0a9ddbeec2369d040d;
uniform float _MarchingStepAmount9909da39672d4c0a9ddbeec2369d040d;
uniform int _IsActive9909da39672d4c0a9ddbeec2369d040d;
uniform float _Displacement9909da39672d4c0a9ddbeec2369d040d0;
uniform float _Freq9909da39672d4c0a9ddbeec2369d040d1;
uniform float _Amplitude9909da39672d4c0a9ddbeec2369d040d1;
uniform float _Speed9909da39672d4c0a9ddbeec2369d040d1;
uniform float3 _Dir9909da39672d4c0a9ddbeec2369d040d1;



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

      
float3 position065aa5c7754e4ee3bf1117f267e253f0 = Rotate3D(rayPos - _Position065aa5c7754e4ee3bf1117f267e253f0, _Rotation065aa5c7754e4ee3bf1117f267e253f0);
float distance065aa5c7754e4ee3bf1117f267e253f0 = _RenderDistance;
if (_IsActive065aa5c7754e4ee3bf1117f267e253f0 > 0)
{
distance065aa5c7754e4ee3bf1117f267e253f0 = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position065aa5c7754e4ee3bf1117f267e253f0, _Scale065aa5c7754e4ee3bf1117f267e253f0);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Displacement065aa5c7754e4ee3bf1117f267e253f00);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Freq065aa5c7754e4ee3bf1117f267e253f01, _Amplitude065aa5c7754e4ee3bf1117f267e253f01, _Speed065aa5c7754e4ee3bf1117f267e253f01, _Dir065aa5c7754e4ee3bf1117f267e253f01);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Displacement065aa5c7754e4ee3bf1117f267e253f02);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Freq065aa5c7754e4ee3bf1117f267e253f03, _Amplitude065aa5c7754e4ee3bf1117f267e253f03, _Speed065aa5c7754e4ee3bf1117f267e253f03, _Dir065aa5c7754e4ee3bf1117f267e253f03);
distance065aa5c7754e4ee3bf1117f267e253f0 /= _MarchingStepAmount065aa5c7754e4ee3bf1117f267e253f0;
}

if (distance065aa5c7754e4ee3bf1117f267e253f0 < resultDistance)
 { 
resultDistance = distance065aa5c7754e4ee3bf1117f267e253f0;
resultColour = Mat_TextureMaterial_c3735437331f4f80a12534d02a465e6a(position065aa5c7754e4ee3bf1117f267e253f0, _Colour065aa5c7754e4ee3bf1117f267e253f0, _Texture065aa5c7754e4ee3bf1117f267e253f0);
} 

float3 position94a3c657384a49dcae537aad3cfd17ad = Rotate3D(rayPos - _Position94a3c657384a49dcae537aad3cfd17ad, _Rotation94a3c657384a49dcae537aad3cfd17ad);
float distance94a3c657384a49dcae537aad3cfd17ad = _RenderDistance;
if (_IsActive94a3c657384a49dcae537aad3cfd17ad > 0)
{
position94a3c657384a49dcae537aad3cfd17ad = Mod_TwistX_a2afad70a366443ead7b8bf1ce7c82fc(position94a3c657384a49dcae537aad3cfd17ad, _Scale94a3c657384a49dcae537aad3cfd17ad, _TwistAmountX94a3c657384a49dcae537aad3cfd17ad0);
distance94a3c657384a49dcae537aad3cfd17ad = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position94a3c657384a49dcae537aad3cfd17ad, _Scale94a3c657384a49dcae537aad3cfd17ad);
distance94a3c657384a49dcae537aad3cfd17ad /= _MarchingStepAmount94a3c657384a49dcae537aad3cfd17ad;
}

float distance10fc656da492424e9757568d34136d0e = distance94a3c657384a49dcae537aad3cfd17ad;
float4 colour10fc656da492424e9757568d34136d0e = _Colour94a3c657384a49dcae537aad3cfd17ad;

float3 position176c1ec3a1c64bb79ab34788ceb863d5 = Rotate3D(rayPos - _Position176c1ec3a1c64bb79ab34788ceb863d5, _Rotation176c1ec3a1c64bb79ab34788ceb863d5);
float distance176c1ec3a1c64bb79ab34788ceb863d5 = _RenderDistance;
if (_IsActive176c1ec3a1c64bb79ab34788ceb863d5 > 0)
{
distance176c1ec3a1c64bb79ab34788ceb863d5 = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position176c1ec3a1c64bb79ab34788ceb863d5, _Scale176c1ec3a1c64bb79ab34788ceb863d5);
distance176c1ec3a1c64bb79ab34788ceb863d5 /= _MarchingStepAmount176c1ec3a1c64bb79ab34788ceb863d5;
}

if (_IsActive10fc656da492424e9757568d34136d0e > 0)
{
Oper_Blend_c08c11b6fc54453486aa264d1da70b87(distance10fc656da492424e9757568d34136d0e, colour10fc656da492424e9757568d34136d0e, distance176c1ec3a1c64bb79ab34788ceb863d5, _Colour176c1ec3a1c64bb79ab34788ceb863d5, _Smooth10fc656da492424e9757568d34136d0e);
}
else
{
if (distance176c1ec3a1c64bb79ab34788ceb863d5 < distance10fc656da492424e9757568d34136d0e)
{ 
distance10fc656da492424e9757568d34136d0e = distance176c1ec3a1c64bb79ab34788ceb863d5;
colour10fc656da492424e9757568d34136d0e = _Colour176c1ec3a1c64bb79ab34788ceb863d5;
} 
}


if (distance10fc656da492424e9757568d34136d0e < resultDistance)
 { 
resultDistance = distance10fc656da492424e9757568d34136d0e;
resultColour = colour10fc656da492424e9757568d34136d0e;
} 

float3 position9909da39672d4c0a9ddbeec2369d040d = Rotate3D(rayPos - _Position9909da39672d4c0a9ddbeec2369d040d, _Rotation9909da39672d4c0a9ddbeec2369d040d);
float distance9909da39672d4c0a9ddbeec2369d040d = _RenderDistance;
if (_IsActive9909da39672d4c0a9ddbeec2369d040d > 0)
{
distance9909da39672d4c0a9ddbeec2369d040d = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position9909da39672d4c0a9ddbeec2369d040d, _Scale9909da39672d4c0a9ddbeec2369d040d);
distance9909da39672d4c0a9ddbeec2369d040d = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position9909da39672d4c0a9ddbeec2369d040d, distance9909da39672d4c0a9ddbeec2369d040d, _Displacement9909da39672d4c0a9ddbeec2369d040d0);
distance9909da39672d4c0a9ddbeec2369d040d = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position9909da39672d4c0a9ddbeec2369d040d, distance9909da39672d4c0a9ddbeec2369d040d, _Freq9909da39672d4c0a9ddbeec2369d040d1, _Amplitude9909da39672d4c0a9ddbeec2369d040d1, _Speed9909da39672d4c0a9ddbeec2369d040d1, _Dir9909da39672d4c0a9ddbeec2369d040d1);
distance9909da39672d4c0a9ddbeec2369d040d /= _MarchingStepAmount9909da39672d4c0a9ddbeec2369d040d;
}

if (distance9909da39672d4c0a9ddbeec2369d040d < resultDistance)
 { 
resultDistance = distance9909da39672d4c0a9ddbeec2369d040d;
resultColour = _Colour9909da39672d4c0a9ddbeec2369d040d;
} 


      return CreateObjectDistanceResult(resultDistance, resultColour);
    }

    float3 GetLight(float3 pos, float3 normal)
    {
      float3 light = float3(0, 0, 0);

      light += float3(1, 0.9568627, 0.8392157) * max(0.0, dot(-normal, float3(0.5337918, -0.6015774, 0.594282))) * 1; 


      return light;
    }

    float3 GetObjectNormal(float3 pos)
    {
      float2 offset = float2(0.01f, 0.0f);
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
          (raymarchResult.Colour * _ColourScalar) * raymarchResult.Succeeded);
      }
      ENDHLSL
    }
  }
}