//---------------------------------------------------------------------
//    This code was generated by a tool.                               
//                                                                     
//    Changes to this file may cause incorrect behavior and will be    
//    lost if the code is regenerated.                                 
//                                                                     
//    Time Generated: 10/14/2021 13:01:33
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
        #include "Assets/Raymarching/Shaders/Generated/DistanceFunctions.hlsl"
        #include "Assets/Raymarching/Shaders/Generated/MaterialFunctions.hlsl"
        #include "Assets/Raymarching/Shaders/Generated/ModifierFunctions.hlsl"
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

        // Raymarch Variables

uniform float3 _Position065aa5c7754e4ee3bf1117f267e253f0;
uniform float3 _Rotation065aa5c7754e4ee3bf1117f267e253f0;
uniform float3 _Scale065aa5c7754e4ee3bf1117f267e253f0;
uniform float4 _Colour065aa5c7754e4ee3bf1117f267e253f0;
uniform float _MarchingStepAmount065aa5c7754e4ee3bf1117f267e253f0;
uniform sampler2D _Texture065aa5c7754e4ee3bf1117f267e253f0;  
uniform float _Freq065aa5c7754e4ee3bf1117f267e253f00;  
uniform float _Amplitude065aa5c7754e4ee3bf1117f267e253f00;  
uniform float _Speed065aa5c7754e4ee3bf1117f267e253f00;  
uniform float3 _Dir065aa5c7754e4ee3bf1117f267e253f00;  
uniform float _Displacement065aa5c7754e4ee3bf1117f267e253f01;  
uniform float _Freq065aa5c7754e4ee3bf1117f267e253f02;  
uniform float _Amplitude065aa5c7754e4ee3bf1117f267e253f02;  
uniform float _Speed065aa5c7754e4ee3bf1117f267e253f02;  
uniform float3 _Dir065aa5c7754e4ee3bf1117f267e253f02;  
uniform float _Displacement065aa5c7754e4ee3bf1117f267e253f03;  
uniform float _Freq065aa5c7754e4ee3bf1117f267e253f04;  
uniform float _Amplitude065aa5c7754e4ee3bf1117f267e253f04;  
uniform float _Speed065aa5c7754e4ee3bf1117f267e253f04;  
uniform float3 _Dir065aa5c7754e4ee3bf1117f267e253f04;  
uniform float _Displacement065aa5c7754e4ee3bf1117f267e253f05;  

uniform float3 _Positionc8a47c22b18e4affbb588ce1fb9a6c49;
uniform float3 _Rotationc8a47c22b18e4affbb588ce1fb9a6c49;
uniform float3 _Scalec8a47c22b18e4affbb588ce1fb9a6c49;
uniform float4 _Colourc8a47c22b18e4affbb588ce1fb9a6c49;
uniform float _MarchingStepAmountc8a47c22b18e4affbb588ce1fb9a6c49;
uniform sampler2D _Texturec8a47c22b18e4affbb588ce1fb9a6c49;  

uniform float3 _Position18af99f408b34fefbfdd01bdbad93604;
uniform float3 _Rotation18af99f408b34fefbfdd01bdbad93604;
uniform float3 _Scale18af99f408b34fefbfdd01bdbad93604;
uniform float4 _Colour18af99f408b34fefbfdd01bdbad93604;
uniform float _MarchingStepAmount18af99f408b34fefbfdd01bdbad93604;
uniform float _Displacement18af99f408b34fefbfdd01bdbad936040;  
uniform float _Displacement18af99f408b34fefbfdd01bdbad936041;  
uniform float _Displacement18af99f408b34fefbfdd01bdbad936042;  
uniform float _Displacement18af99f408b34fefbfdd01bdbad936043;  
uniform float _Freq18af99f408b34fefbfdd01bdbad936044;  
uniform float _Amplitude18af99f408b34fefbfdd01bdbad936044;  
uniform float _Speed18af99f408b34fefbfdd01bdbad936044;  
uniform float3 _Dir18af99f408b34fefbfdd01bdbad936044;  

uniform float3 _Positiond43ec28988614de994c897c65f232b53;
uniform float3 _Rotationd43ec28988614de994c897c65f232b53;
uniform float3 _Scaled43ec28988614de994c897c65f232b53;
uniform float4 _Colourd43ec28988614de994c897c65f232b53;
uniform float _MarchingStepAmountd43ec28988614de994c897c65f232b53;
uniform float _TwistAmountYd43ec28988614de994c897c65f232b530;  
uniform float _TwistAmountXd43ec28988614de994c897c65f232b531;  
uniform float _TwistAmountZd43ec28988614de994c897c65f232b532;  

uniform float3 _Position9909da39672d4c0a9ddbeec2369d040d;
uniform float3 _Rotation9909da39672d4c0a9ddbeec2369d040d;
uniform float3 _Scale9909da39672d4c0a9ddbeec2369d040d;
uniform float4 _Colour9909da39672d4c0a9ddbeec2369d040d;
uniform float _MarchingStepAmount9909da39672d4c0a9ddbeec2369d040d;
uniform float _Displacement9909da39672d4c0a9ddbeec2369d040d0;  
uniform float _Freq9909da39672d4c0a9ddbeec2369d040d1;  
uniform float _Amplitude9909da39672d4c0a9ddbeec2369d040d1;  
uniform float _Speed9909da39672d4c0a9ddbeec2369d040d1;  
uniform float3 _Dir9909da39672d4c0a9ddbeec2369d040d1;  
uniform float _TwistAmountY9909da39672d4c0a9ddbeec2369d040d2;  

uniform float3 _Position2955e4ec50a74819951eedbe070d2998;
uniform float3 _Rotation2955e4ec50a74819951eedbe070d2998;
uniform float3 _Scale2955e4ec50a74819951eedbe070d2998;
uniform float4 _Colour2955e4ec50a74819951eedbe070d2998;
uniform float _MarchingStepAmount2955e4ec50a74819951eedbe070d2998;
uniform float _Freq2955e4ec50a74819951eedbe070d29980;  
uniform float _Amplitude2955e4ec50a74819951eedbe070d29980;  
uniform float _Speed2955e4ec50a74819951eedbe070d29980;  
uniform float3 _Dir2955e4ec50a74819951eedbe070d29980;  

uniform float3 _Position64026a2a5508436d9246e11c95a9502e;
uniform float3 _Rotation64026a2a5508436d9246e11c95a9502e;
uniform float3 _Scale64026a2a5508436d9246e11c95a9502e;
uniform float4 _Colour64026a2a5508436d9246e11c95a9502e;
uniform float _MarchingStepAmount64026a2a5508436d9246e11c95a9502e;



        float3 Rotate3D(float3 pos, float3 rot)
        {
            pos.xz = mul(pos.xz, float2x2(cos(rot.y), sin(rot.y), -sin(rot.y), cos(rot.y)));
            pos.yz = mul(pos.yz, float2x2(cos(rot.x), -sin(rot.x), sin(rot.x), cos(rot.x)));
            pos.xy = mul(pos.xy, float2x2(cos(rot.z), -sin(rot.z), sin(rot.z), cos(rot.z)));
            return pos;
        }

        float4 GetDistanceFromObjects(float3 rayPos)
        {
            float resultDistance = _RenderDistance;
            float3 resultColour = float3(1, 1, 1);

            
float3 position065aa5c7754e4ee3bf1117f267e253f0 = Rotate3D(rayPos - _Position065aa5c7754e4ee3bf1117f267e253f0, _Rotation065aa5c7754e4ee3bf1117f267e253f0);

float distance065aa5c7754e4ee3bf1117f267e253f0 = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position065aa5c7754e4ee3bf1117f267e253f0, _Scale065aa5c7754e4ee3bf1117f267e253f0);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Freq065aa5c7754e4ee3bf1117f267e253f00, _Amplitude065aa5c7754e4ee3bf1117f267e253f00, _Speed065aa5c7754e4ee3bf1117f267e253f00, _Dir065aa5c7754e4ee3bf1117f267e253f00);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Displacement065aa5c7754e4ee3bf1117f267e253f01);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Freq065aa5c7754e4ee3bf1117f267e253f02, _Amplitude065aa5c7754e4ee3bf1117f267e253f02, _Speed065aa5c7754e4ee3bf1117f267e253f02, _Dir065aa5c7754e4ee3bf1117f267e253f02);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Displacement065aa5c7754e4ee3bf1117f267e253f03);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Freq065aa5c7754e4ee3bf1117f267e253f04, _Amplitude065aa5c7754e4ee3bf1117f267e253f04, _Speed065aa5c7754e4ee3bf1117f267e253f04, _Dir065aa5c7754e4ee3bf1117f267e253f04);
distance065aa5c7754e4ee3bf1117f267e253f0 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position065aa5c7754e4ee3bf1117f267e253f0, distance065aa5c7754e4ee3bf1117f267e253f0, _Displacement065aa5c7754e4ee3bf1117f267e253f05);
distance065aa5c7754e4ee3bf1117f267e253f0 /= _MarchingStepAmount065aa5c7754e4ee3bf1117f267e253f0;


if (distance065aa5c7754e4ee3bf1117f267e253f0 < resultDistance)
 { 
resultDistance = distance065aa5c7754e4ee3bf1117f267e253f0;
resultColour = Mat_TextureMaterial_c3735437331f4f80a12534d02a465e6a(position065aa5c7754e4ee3bf1117f267e253f0, _Colour065aa5c7754e4ee3bf1117f267e253f0, _Texture065aa5c7754e4ee3bf1117f267e253f0);
} 

float3 positionc8a47c22b18e4affbb588ce1fb9a6c49 = Rotate3D(rayPos - _Positionc8a47c22b18e4affbb588ce1fb9a6c49, _Rotationc8a47c22b18e4affbb588ce1fb9a6c49);

float distancec8a47c22b18e4affbb588ce1fb9a6c49 = SDF_Sphere_5a5c930dec9347e2970ec043d92e6116(positionc8a47c22b18e4affbb588ce1fb9a6c49, _Scalec8a47c22b18e4affbb588ce1fb9a6c49);
distancec8a47c22b18e4affbb588ce1fb9a6c49 /= _MarchingStepAmountc8a47c22b18e4affbb588ce1fb9a6c49;


if (distancec8a47c22b18e4affbb588ce1fb9a6c49 < resultDistance)
 { 
resultDistance = distancec8a47c22b18e4affbb588ce1fb9a6c49;
resultColour = Mat_TextureMaterial_c3735437331f4f80a12534d02a465e6a(positionc8a47c22b18e4affbb588ce1fb9a6c49, _Colourc8a47c22b18e4affbb588ce1fb9a6c49, _Texturec8a47c22b18e4affbb588ce1fb9a6c49);
} 

float3 position18af99f408b34fefbfdd01bdbad93604 = Rotate3D(rayPos - _Position18af99f408b34fefbfdd01bdbad93604, _Rotation18af99f408b34fefbfdd01bdbad93604);

float distance18af99f408b34fefbfdd01bdbad93604 = SDF_Sphere_5a5c930dec9347e2970ec043d92e6116(position18af99f408b34fefbfdd01bdbad93604, _Scale18af99f408b34fefbfdd01bdbad93604);
distance18af99f408b34fefbfdd01bdbad93604 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position18af99f408b34fefbfdd01bdbad93604, distance18af99f408b34fefbfdd01bdbad93604, _Displacement18af99f408b34fefbfdd01bdbad936040);
distance18af99f408b34fefbfdd01bdbad93604 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position18af99f408b34fefbfdd01bdbad93604, distance18af99f408b34fefbfdd01bdbad93604, _Displacement18af99f408b34fefbfdd01bdbad936041);
distance18af99f408b34fefbfdd01bdbad93604 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position18af99f408b34fefbfdd01bdbad93604, distance18af99f408b34fefbfdd01bdbad93604, _Displacement18af99f408b34fefbfdd01bdbad936042);
distance18af99f408b34fefbfdd01bdbad93604 = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position18af99f408b34fefbfdd01bdbad93604, distance18af99f408b34fefbfdd01bdbad93604, _Displacement18af99f408b34fefbfdd01bdbad936043);
distance18af99f408b34fefbfdd01bdbad93604 = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position18af99f408b34fefbfdd01bdbad93604, distance18af99f408b34fefbfdd01bdbad93604, _Freq18af99f408b34fefbfdd01bdbad936044, _Amplitude18af99f408b34fefbfdd01bdbad936044, _Speed18af99f408b34fefbfdd01bdbad936044, _Dir18af99f408b34fefbfdd01bdbad936044);
distance18af99f408b34fefbfdd01bdbad93604 /= _MarchingStepAmount18af99f408b34fefbfdd01bdbad93604;


if (distance18af99f408b34fefbfdd01bdbad93604 < resultDistance)
 { 
resultDistance = distance18af99f408b34fefbfdd01bdbad93604;
resultColour = _Colour18af99f408b34fefbfdd01bdbad93604.xyz;

} 

float3 positiond43ec28988614de994c897c65f232b53 = Rotate3D(rayPos - _Positiond43ec28988614de994c897c65f232b53, _Rotationd43ec28988614de994c897c65f232b53);

positiond43ec28988614de994c897c65f232b53 = Mod_TwistY_84cc6354438645f28030230feaa53e13(positiond43ec28988614de994c897c65f232b53, _Scaled43ec28988614de994c897c65f232b53, _TwistAmountYd43ec28988614de994c897c65f232b530);
positiond43ec28988614de994c897c65f232b53 = Mod_TwistX_a2afad70a366443ead7b8bf1ce7c82fc(positiond43ec28988614de994c897c65f232b53, _Scaled43ec28988614de994c897c65f232b53, _TwistAmountXd43ec28988614de994c897c65f232b531);
positiond43ec28988614de994c897c65f232b53 = Mod_TwistZ_c1d0359417c14757b9996d01b0db1171(positiond43ec28988614de994c897c65f232b53, _Scaled43ec28988614de994c897c65f232b53, _TwistAmountZd43ec28988614de994c897c65f232b532);
float distanced43ec28988614de994c897c65f232b53 = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(positiond43ec28988614de994c897c65f232b53, _Scaled43ec28988614de994c897c65f232b53);
distanced43ec28988614de994c897c65f232b53 /= _MarchingStepAmountd43ec28988614de994c897c65f232b53;


if (distanced43ec28988614de994c897c65f232b53 < resultDistance)
 { 
resultDistance = distanced43ec28988614de994c897c65f232b53;
resultColour = _Colourd43ec28988614de994c897c65f232b53.xyz;

} 

float3 position9909da39672d4c0a9ddbeec2369d040d = Rotate3D(rayPos - _Position9909da39672d4c0a9ddbeec2369d040d, _Rotation9909da39672d4c0a9ddbeec2369d040d);

position9909da39672d4c0a9ddbeec2369d040d = Mod_TwistY_84cc6354438645f28030230feaa53e13(position9909da39672d4c0a9ddbeec2369d040d, _Scale9909da39672d4c0a9ddbeec2369d040d, _TwistAmountY9909da39672d4c0a9ddbeec2369d040d2);
float distance9909da39672d4c0a9ddbeec2369d040d = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position9909da39672d4c0a9ddbeec2369d040d, _Scale9909da39672d4c0a9ddbeec2369d040d);
distance9909da39672d4c0a9ddbeec2369d040d = Mod_Displacement_1a61691f0be94ed6b83151f90f2fefb1(position9909da39672d4c0a9ddbeec2369d040d, distance9909da39672d4c0a9ddbeec2369d040d, _Displacement9909da39672d4c0a9ddbeec2369d040d0);
distance9909da39672d4c0a9ddbeec2369d040d = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position9909da39672d4c0a9ddbeec2369d040d, distance9909da39672d4c0a9ddbeec2369d040d, _Freq9909da39672d4c0a9ddbeec2369d040d1, _Amplitude9909da39672d4c0a9ddbeec2369d040d1, _Speed9909da39672d4c0a9ddbeec2369d040d1, _Dir9909da39672d4c0a9ddbeec2369d040d1);
distance9909da39672d4c0a9ddbeec2369d040d /= _MarchingStepAmount9909da39672d4c0a9ddbeec2369d040d;


if (distance9909da39672d4c0a9ddbeec2369d040d < resultDistance)
 { 
resultDistance = distance9909da39672d4c0a9ddbeec2369d040d;
resultColour = _Colour9909da39672d4c0a9ddbeec2369d040d.xyz;

} 

float3 position2955e4ec50a74819951eedbe070d2998 = Rotate3D(rayPos - _Position2955e4ec50a74819951eedbe070d2998, _Rotation2955e4ec50a74819951eedbe070d2998);

float distance2955e4ec50a74819951eedbe070d2998 = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position2955e4ec50a74819951eedbe070d2998, _Scale2955e4ec50a74819951eedbe070d2998);
distance2955e4ec50a74819951eedbe070d2998 = Mod_SineWave_a6bfc751b1354407833fc4a471b08d44(position2955e4ec50a74819951eedbe070d2998, distance2955e4ec50a74819951eedbe070d2998, _Freq2955e4ec50a74819951eedbe070d29980, _Amplitude2955e4ec50a74819951eedbe070d29980, _Speed2955e4ec50a74819951eedbe070d29980, _Dir2955e4ec50a74819951eedbe070d29980);
distance2955e4ec50a74819951eedbe070d2998 /= _MarchingStepAmount2955e4ec50a74819951eedbe070d2998;


if (distance2955e4ec50a74819951eedbe070d2998 < resultDistance)
 { 
resultDistance = distance2955e4ec50a74819951eedbe070d2998;
resultColour = _Colour2955e4ec50a74819951eedbe070d2998.xyz;

} 

float3 position64026a2a5508436d9246e11c95a9502e = Rotate3D(rayPos - _Position64026a2a5508436d9246e11c95a9502e, _Rotation64026a2a5508436d9246e11c95a9502e);

float distance64026a2a5508436d9246e11c95a9502e = SDF_Cube_05845aac9d55425c8e1f8d191d017e1e(position64026a2a5508436d9246e11c95a9502e, _Scale64026a2a5508436d9246e11c95a9502e);
distance64026a2a5508436d9246e11c95a9502e /= _MarchingStepAmount64026a2a5508436d9246e11c95a9502e;


if (distance64026a2a5508436d9246e11c95a9502e < resultDistance)
 { 
resultDistance = distance64026a2a5508436d9246e11c95a9502e;
resultColour = _Colour64026a2a5508436d9246e11c95a9502e.xyz;

} 


            return float4(resultColour.xyz, resultDistance);
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