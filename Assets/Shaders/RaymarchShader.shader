Shader "WSWhitehouse/RaymarchShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "RaymarchObjectInfo.cginc"

            // Environment
            sampler2D _MainTex;

            // Camera
            uniform sampler2D _CameraDepthTexture;
            uniform float4x4 _CamFrustum;
            uniform float4x4 _CamToWorld;

            // Lighting
            uniform float3 _LightDirection;

            // Raymarch Object Info
            uniform StructuredBuffer<RaymarchObjectInfo> _ObjectInfo;
            uniform int _ObjectInfoCount;

            uniform float _MaxDistance;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                const half index = v.vertex.z;
                v.vertex.z = 0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _CamFrustum[(int)index].xyz;
                o.ray /= abs(o.ray.z); // normalise in z direction
                o.ray = mul(_CamToWorld, o.ray);

                return o;
            }

            float sdSphere(float3 pos, float radius)
            {
                return length(pos) - radius;
            }

            float distanceField(float3 pos)
            {
                RaymarchObjectInfo objectInfo = _ObjectInfo[0];
                float sphere1 = sdSphere(pos - objectInfo.Position.xyz, 1);
                return sphere1;
            }

            float3 getNormal(float3 pos)
            {
                const float2 offset = float2(0.01f, 0.0f);
                const float3 normal = float3(
                    distanceField(pos + offset.xyy) - distanceField(pos - offset.xyy),
                    distanceField(pos + offset.yxy) - distanceField(pos - offset.yxy),
                    distanceField(pos + offset.yyx) - distanceField(pos - offset.yyx)
                );

                return normalize(normal);
            }

            fixed4 raymarching(float3 rayOrigin, float3 rayDir, float depth)
            {
                if (_ObjectInfoCount <= 0)
                {
                    return fixed4(rayDir, 0);
                }

                fixed4 result = fixed4(1, 1, 1, 1);

                const int maxIter = 164;
                float distanceTraveled = 0;

                for (int i = 0; i < maxIter; i++)
                {
                    if (distanceTraveled > _MaxDistance || distanceTraveled >= depth)
                    {
                        // Environment
                        result = fixed4(rayDir, 0);
                        break;
                    }

                    const float3 pos = rayOrigin + rayDir * distanceTraveled;
                    // Check for hit in distance field
                    const float distance = distanceField(pos);

                    if (distance < 0.01) // Hit something
                    {
                        // Object Shading
                        const float3 normal = getNormal(pos);
                        const float light = dot(-_LightDirection, normal);

                        result = fixed4(fixed3(1, 1, 1) * light, 1);
                        break;
                    }

                    distanceTraveled += distance;
                }

                return result;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(_ObjectInfo[0].Position.xyz, 1);

                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                depth *= length(i.ray);

                fixed3 colour = tex2D(_MainTex, i.uv);

                float3 rayDir = normalize(i.ray.xyz);
                float3 rayOrigin = _WorldSpaceCameraPos;

                fixed4 result = raymarching(rayOrigin, rayDir, depth);
                return fixed4(colour * (1.0 - result.w) + result.xyz * result.w, 1.0);
            }
            ENDCG
        }
    }
}