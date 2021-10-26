//---------------------------------------------------------------------

//    This code was generated by a tool.                               

//                                                                     

//    Changes to this file may cause incorrect behavior and will be    

//    lost if the code is regenerated.                                 

//                                                                     

//    Time Generated: 10/25/2021 15:26:08     

//---------------------------------------------------------------------

Shader "Raymarch/Manifold_RaymarchShader"
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
    uniform float _ColourMultiplier;

    // Raymarch Variables

uniform float _Smooth5f6229b977e74e23a6188ffac9ca59bd;
uniform int _IsActive5f6229b977e74e23a6188ffac9ca59bd;

uniform float3 _Position76a474ff47de44e18867d4da2622a330;
uniform float3 _Rotation76a474ff47de44e18867d4da2622a330;
uniform float3 _Scale76a474ff47de44e18867d4da2622a330;
uniform float4 _Colour76a474ff47de44e18867d4da2622a330;
uniform float _MarchingStepAmount76a474ff47de44e18867d4da2622a330;
uniform int _IsActive76a474ff47de44e18867d4da2622a330;

uniform float3 _Position10518fb76e9e4d378c4899d9f6c3e2c9;
uniform float3 _Rotation10518fb76e9e4d378c4899d9f6c3e2c9;
uniform float3 _Scale10518fb76e9e4d378c4899d9f6c3e2c9;
uniform float4 _Colour10518fb76e9e4d378c4899d9f6c3e2c9;
uniform float _MarchingStepAmount10518fb76e9e4d378c4899d9f6c3e2c9;
uniform int _IsActive10518fb76e9e4d378c4899d9f6c3e2c9;

uniform float3 _Position11c01ce4dcfe4716876d3f4e6b5e7037;
uniform float3 _Rotation11c01ce4dcfe4716876d3f4e6b5e7037;
uniform float3 _Scale11c01ce4dcfe4716876d3f4e6b5e7037;
uniform float4 _Colour11c01ce4dcfe4716876d3f4e6b5e7037;
uniform float _MarchingStepAmount11c01ce4dcfe4716876d3f4e6b5e7037;
uniform int _IsActive11c01ce4dcfe4716876d3f4e6b5e7037;
uniform float3 _a11c01ce4dcfe4716876d3f4e6b5e7037;
uniform float3 _b11c01ce4dcfe4716876d3f4e6b5e7037;
uniform float _r11c01ce4dcfe4716876d3f4e6b5e7037;

uniform float _Smoothba84b4c5b4fc440ba4e9604d566be8dc;
uniform int _IsActiveba84b4c5b4fc440ba4e9604d566be8dc;

uniform float3 _Positionef0db21fb328435ebac0fb203090cbd7;
uniform float3 _Rotationef0db21fb328435ebac0fb203090cbd7;
uniform float3 _Scaleef0db21fb328435ebac0fb203090cbd7;
uniform float4 _Colouref0db21fb328435ebac0fb203090cbd7;
uniform float _MarchingStepAmountef0db21fb328435ebac0fb203090cbd7;
uniform int _IsActiveef0db21fb328435ebac0fb203090cbd7;
uniform float3 _aef0db21fb328435ebac0fb203090cbd7;
uniform float3 _bef0db21fb328435ebac0fb203090cbd7;
uniform float _ref0db21fb328435ebac0fb203090cbd7;

uniform int _IsActiveab63694457d54834b7ea550dfcc959dd;

uniform float3 _Positiond223265320814ba299e3ea035de051f4;
uniform float3 _Rotationd223265320814ba299e3ea035de051f4;
uniform float3 _Scaled223265320814ba299e3ea035de051f4;
uniform float4 _Colourd223265320814ba299e3ea035de051f4;
uniform float _MarchingStepAmountd223265320814ba299e3ea035de051f4;
uniform int _IsActived223265320814ba299e3ea035de051f4;
uniform float3 _ad223265320814ba299e3ea035de051f4;
uniform float3 _bd223265320814ba299e3ea035de051f4;
uniform float _rd223265320814ba299e3ea035de051f4;

uniform float3 _Position43ed3c7c5c124db3bbf62a5add8b1d48;
uniform float3 _Rotation43ed3c7c5c124db3bbf62a5add8b1d48;
uniform float3 _Scale43ed3c7c5c124db3bbf62a5add8b1d48;
uniform float4 _Colour43ed3c7c5c124db3bbf62a5add8b1d48;
uniform float _MarchingStepAmount43ed3c7c5c124db3bbf62a5add8b1d48;
uniform int _IsActive43ed3c7c5c124db3bbf62a5add8b1d48;

uniform float3 _Position656acc72441243959155b9e6545de517;
uniform float3 _Rotation656acc72441243959155b9e6545de517;
uniform float3 _Scale656acc72441243959155b9e6545de517;
uniform float4 _Colour656acc72441243959155b9e6545de517;
uniform float _MarchingStepAmount656acc72441243959155b9e6545de517;
uniform int _IsActive656acc72441243959155b9e6545de517;
uniform float3 _a656acc72441243959155b9e6545de517;
uniform float3 _b656acc72441243959155b9e6545de517;
uniform float _r656acc72441243959155b9e6545de517;



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

      

float3 position76a474ff47de44e18867d4da2622a330 = Rotate3D(rayPos - _Position76a474ff47de44e18867d4da2622a330, _Rotation76a474ff47de44e18867d4da2622a330);

float distance76a474ff47de44e18867d4da2622a330 = _RenderDistance;

if (_IsActive76a474ff47de44e18867d4da2622a330 > 0)

{

distance76a474ff47de44e18867d4da2622a330 = SDF_Sphere_5a5c930dec9347e2970ec043d92e6116(position76a474ff47de44e18867d4da2622a330, _Scale76a474ff47de44e18867d4da2622a330);
distance76a474ff47de44e18867d4da2622a330 /= _MarchingStepAmount76a474ff47de44e18867d4da2622a330;

}



// Operation Start Oper_Blend 5f6229b977e74e23a6188ffac9ca59bd

float distance5f6229b977e74e23a6188ffac9ca59bd = distance76a474ff47de44e18867d4da2622a330;

float4 colour5f6229b977e74e23a6188ffac9ca59bd = _Colour76a474ff47de44e18867d4da2622a330;



float3 position10518fb76e9e4d378c4899d9f6c3e2c9 = Rotate3D(rayPos - _Position10518fb76e9e4d378c4899d9f6c3e2c9, _Rotation10518fb76e9e4d378c4899d9f6c3e2c9);

float distance10518fb76e9e4d378c4899d9f6c3e2c9 = _RenderDistance;

if (_IsActive10518fb76e9e4d378c4899d9f6c3e2c9 > 0)

{

distance10518fb76e9e4d378c4899d9f6c3e2c9 = SDF_Sphere_5a5c930dec9347e2970ec043d92e6116(position10518fb76e9e4d378c4899d9f6c3e2c9, _Scale10518fb76e9e4d378c4899d9f6c3e2c9);
distance10518fb76e9e4d378c4899d9f6c3e2c9 /= _MarchingStepAmount10518fb76e9e4d378c4899d9f6c3e2c9;

}



if (_IsActive5f6229b977e74e23a6188ffac9ca59bd > 0)
{
Oper_Blend_c08c11b6fc54453486aa264d1da70b87(distance5f6229b977e74e23a6188ffac9ca59bd, colour5f6229b977e74e23a6188ffac9ca59bd, distance10518fb76e9e4d378c4899d9f6c3e2c9, _Colour10518fb76e9e4d378c4899d9f6c3e2c9, _Smooth5f6229b977e74e23a6188ffac9ca59bd);
}
else
{
if (distance10518fb76e9e4d378c4899d9f6c3e2c9 < distance5f6229b977e74e23a6188ffac9ca59bd)
{ 
distance5f6229b977e74e23a6188ffac9ca59bd = distance10518fb76e9e4d378c4899d9f6c3e2c9;
colour5f6229b977e74e23a6188ffac9ca59bd = _Colour10518fb76e9e4d378c4899d9f6c3e2c9;
} 
}





float3 position11c01ce4dcfe4716876d3f4e6b5e7037 = Rotate3D(rayPos - _Position11c01ce4dcfe4716876d3f4e6b5e7037, _Rotation11c01ce4dcfe4716876d3f4e6b5e7037);

float distance11c01ce4dcfe4716876d3f4e6b5e7037 = _RenderDistance;

if (_IsActive11c01ce4dcfe4716876d3f4e6b5e7037 > 0)

{

distance11c01ce4dcfe4716876d3f4e6b5e7037 = SDF_Capsule_6dd9ace8ea81468da4fe7ff03e5332bb(position11c01ce4dcfe4716876d3f4e6b5e7037, _Scale11c01ce4dcfe4716876d3f4e6b5e7037, _a11c01ce4dcfe4716876d3f4e6b5e7037, _b11c01ce4dcfe4716876d3f4e6b5e7037, _r11c01ce4dcfe4716876d3f4e6b5e7037);
distance11c01ce4dcfe4716876d3f4e6b5e7037 /= _MarchingStepAmount11c01ce4dcfe4716876d3f4e6b5e7037;

}



if (_IsActive5f6229b977e74e23a6188ffac9ca59bd > 0)
{
Oper_Blend_c08c11b6fc54453486aa264d1da70b87(distance5f6229b977e74e23a6188ffac9ca59bd, colour5f6229b977e74e23a6188ffac9ca59bd, distance11c01ce4dcfe4716876d3f4e6b5e7037, _Colour11c01ce4dcfe4716876d3f4e6b5e7037, _Smooth5f6229b977e74e23a6188ffac9ca59bd);
}
else
{
if (distance11c01ce4dcfe4716876d3f4e6b5e7037 < distance5f6229b977e74e23a6188ffac9ca59bd)
{ 
distance5f6229b977e74e23a6188ffac9ca59bd = distance11c01ce4dcfe4716876d3f4e6b5e7037;
colour5f6229b977e74e23a6188ffac9ca59bd = _Colour11c01ce4dcfe4716876d3f4e6b5e7037;
} 
}





// Operation End 5f6229b977e74e23a6188ffac9ca59bd

if (distance5f6229b977e74e23a6188ffac9ca59bd < resultDistance)

{

resultDistance = distance5f6229b977e74e23a6188ffac9ca59bd;

resultColour   = colour5f6229b977e74e23a6188ffac9ca59bd;

}



float3 positionef0db21fb328435ebac0fb203090cbd7 = Rotate3D(rayPos - _Positionef0db21fb328435ebac0fb203090cbd7, _Rotationef0db21fb328435ebac0fb203090cbd7);

float distanceef0db21fb328435ebac0fb203090cbd7 = _RenderDistance;

if (_IsActiveef0db21fb328435ebac0fb203090cbd7 > 0)

{

distanceef0db21fb328435ebac0fb203090cbd7 = SDF_Capsule_6dd9ace8ea81468da4fe7ff03e5332bb(positionef0db21fb328435ebac0fb203090cbd7, _Scaleef0db21fb328435ebac0fb203090cbd7, _aef0db21fb328435ebac0fb203090cbd7, _bef0db21fb328435ebac0fb203090cbd7, _ref0db21fb328435ebac0fb203090cbd7);
distanceef0db21fb328435ebac0fb203090cbd7 /= _MarchingStepAmountef0db21fb328435ebac0fb203090cbd7;

}



// Operation Start Oper_Blend ba84b4c5b4fc440ba4e9604d566be8dc

float distanceba84b4c5b4fc440ba4e9604d566be8dc = distanceef0db21fb328435ebac0fb203090cbd7;

float4 colourba84b4c5b4fc440ba4e9604d566be8dc = _Colouref0db21fb328435ebac0fb203090cbd7;



float3 positiond223265320814ba299e3ea035de051f4 = Rotate3D(rayPos - _Positiond223265320814ba299e3ea035de051f4, _Rotationd223265320814ba299e3ea035de051f4);

float distanced223265320814ba299e3ea035de051f4 = _RenderDistance;

if (_IsActived223265320814ba299e3ea035de051f4 > 0)

{

distanced223265320814ba299e3ea035de051f4 = SDF_Capsule_6dd9ace8ea81468da4fe7ff03e5332bb(positiond223265320814ba299e3ea035de051f4, _Scaled223265320814ba299e3ea035de051f4, _ad223265320814ba299e3ea035de051f4, _bd223265320814ba299e3ea035de051f4, _rd223265320814ba299e3ea035de051f4);
distanced223265320814ba299e3ea035de051f4 /= _MarchingStepAmountd223265320814ba299e3ea035de051f4;

}



// Operation Start Oper_Cut ab63694457d54834b7ea550dfcc959dd

float distanceab63694457d54834b7ea550dfcc959dd = distanced223265320814ba299e3ea035de051f4;

float4 colourab63694457d54834b7ea550dfcc959dd = _Colourd223265320814ba299e3ea035de051f4;



float3 position43ed3c7c5c124db3bbf62a5add8b1d48 = Rotate3D(rayPos - _Position43ed3c7c5c124db3bbf62a5add8b1d48, _Rotation43ed3c7c5c124db3bbf62a5add8b1d48);

float distance43ed3c7c5c124db3bbf62a5add8b1d48 = _RenderDistance;

if (_IsActive43ed3c7c5c124db3bbf62a5add8b1d48 > 0)

{

distance43ed3c7c5c124db3bbf62a5add8b1d48 = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position43ed3c7c5c124db3bbf62a5add8b1d48, _Scale43ed3c7c5c124db3bbf62a5add8b1d48);
distance43ed3c7c5c124db3bbf62a5add8b1d48 /= _MarchingStepAmount43ed3c7c5c124db3bbf62a5add8b1d48;

}



if (_IsActiveab63694457d54834b7ea550dfcc959dd > 0)
{
Oper_Cut_e668285f5b654bfab03360efeb593db7(distanceab63694457d54834b7ea550dfcc959dd, colourab63694457d54834b7ea550dfcc959dd, distance43ed3c7c5c124db3bbf62a5add8b1d48, _Colour43ed3c7c5c124db3bbf62a5add8b1d48);
}
else
{
if (distance43ed3c7c5c124db3bbf62a5add8b1d48 < distanceab63694457d54834b7ea550dfcc959dd)
{ 
distanceab63694457d54834b7ea550dfcc959dd = distance43ed3c7c5c124db3bbf62a5add8b1d48;
colourab63694457d54834b7ea550dfcc959dd = _Colour43ed3c7c5c124db3bbf62a5add8b1d48;
} 
}





// Operation End ab63694457d54834b7ea550dfcc959dd

if (_IsActiveba84b4c5b4fc440ba4e9604d566be8dc > 0)
{
Oper_Blend_c08c11b6fc54453486aa264d1da70b87(distanceba84b4c5b4fc440ba4e9604d566be8dc, colourba84b4c5b4fc440ba4e9604d566be8dc, distanceab63694457d54834b7ea550dfcc959dd, colourab63694457d54834b7ea550dfcc959dd, _Smoothba84b4c5b4fc440ba4e9604d566be8dc);
}
else
{
if (distanceab63694457d54834b7ea550dfcc959dd < distanceba84b4c5b4fc440ba4e9604d566be8dc)
{ 
distanceba84b4c5b4fc440ba4e9604d566be8dc = distanceab63694457d54834b7ea550dfcc959dd;
colourba84b4c5b4fc440ba4e9604d566be8dc = colourab63694457d54834b7ea550dfcc959dd;
} 
}
;



float3 position656acc72441243959155b9e6545de517 = Rotate3D(rayPos - _Position656acc72441243959155b9e6545de517, _Rotation656acc72441243959155b9e6545de517);

float distance656acc72441243959155b9e6545de517 = _RenderDistance;

if (_IsActive656acc72441243959155b9e6545de517 > 0)

{

distance656acc72441243959155b9e6545de517 = SDF_Capsule_6dd9ace8ea81468da4fe7ff03e5332bb(position656acc72441243959155b9e6545de517, _Scale656acc72441243959155b9e6545de517, _a656acc72441243959155b9e6545de517, _b656acc72441243959155b9e6545de517, _r656acc72441243959155b9e6545de517);
distance656acc72441243959155b9e6545de517 /= _MarchingStepAmount656acc72441243959155b9e6545de517;

}



if (_IsActiveba84b4c5b4fc440ba4e9604d566be8dc > 0)
{
Oper_Blend_c08c11b6fc54453486aa264d1da70b87(distanceba84b4c5b4fc440ba4e9604d566be8dc, colourba84b4c5b4fc440ba4e9604d566be8dc, distance656acc72441243959155b9e6545de517, _Colour656acc72441243959155b9e6545de517, _Smoothba84b4c5b4fc440ba4e9604d566be8dc);
}
else
{
if (distance656acc72441243959155b9e6545de517 < distanceba84b4c5b4fc440ba4e9604d566be8dc)
{ 
distanceba84b4c5b4fc440ba4e9604d566be8dc = distance656acc72441243959155b9e6545de517;
colourba84b4c5b4fc440ba4e9604d566be8dc = _Colour656acc72441243959155b9e6545de517;
} 
}





// Operation End ba84b4c5b4fc440ba4e9604d566be8dc

if (distanceba84b4c5b4fc440ba4e9604d566be8dc < resultDistance)

{

resultDistance = distanceba84b4c5b4fc440ba4e9604d566be8dc;

resultColour   = colourba84b4c5b4fc440ba4e9604d566be8dc;

}



      return CreateObjectDistanceResult(resultDistance, resultColour);
    }

    float3 GetLight(float3 pos, float3 normal)
    {
      float3 light = float3(0, 0, 0);

      light += float3(1, 0.9568627, 0.8392157) * max(0.0, dot(-normal, float3(-0.3213938, -0.7660444, 0.5566705))) * 1; 



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
          (raymarchResult.Colour * _ColourMultiplier) * raymarchResult.Succeeded);
      }
      ENDHLSL
    }
  }
}