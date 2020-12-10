#ifndef WSW_MATH_CGINC
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
#define WSW_MATH_CGINC

// DEFINES
#define PI 3.14159265358979

// USEFUL MATHS FUNCTIONS
inline float min(float3 a)
{
    float val = min(a.x, a.y);
    return min(val, a.z);
}

float rand(float2 seed)
{
    return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
}

inline float mod(float a, float b)
{
    return frac(abs(a / b)) * abs(b);
}

inline float2 mod(float2 a, float2 b)
{
    return frac(abs(a / b)) * abs(b);
}

inline float3 mod(float3 a, float3 b)
{
    return frac(abs(a / b)) * abs(b);
}

// ROTATION
// https://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToMatrix/index.htm
inline float3x3 RotateMatrix(float3 rotation)
{
    // Bank (X Rot)
    float cb = cos(rotation.x);
    float sb = sin(rotation.x);
    // Heading (Y Rot)
    float ch = cos(rotation.y);
    float sh = sin(rotation.y);
    // Attitude (Z Rot)
    float ca = cos(rotation.z);
    float sa = sin(rotation.z);

    /* --------------------------- */
    /* Matrix row column ordering: */
    /*   [m00 m01 m02]             */
    /*   [m10 m11 m12]             */
    /*   [m20 m21 m22]             */
    /* --------------------------- */
    float m00 = ch * ca;
    float m01 = sh * sb - ch * sa * cb;
    float m02 = ch * sa * sb + sh * cb;
    float m10 = sa;
    float m11 = ca * cb;
    float m12 = -ca * sb;
    float m20 = -sh * ca;
    float m21 = sh * sa * cb + ch * sb;
    float m22 = -sh * sa * sb + ch * cb;

    return float3x3(m00, m01, m02, m10, m11, m12, m20, m21, m22);
}

// TWIST
inline float3 TwistX(float3 p, float power)
{
    float s = sin(power * p.y);
    float c = cos(power * p.y);
    float3x3 m = float3x3(
        1.0, 0.0, 0.0,
        0.0, c, s,
        0.0, -s, c
    );
    return mul(m, p);
}

inline float3 TwistY(float3 p, float power)
{
    float s = sin(power * p.y);
    float c = cos(power * p.y);
    float3x3 m = float3x3(
        c, 0.0, -s,
        0.0, 1.0, 0.0,
        s, 0.0, c
    );
    return mul(m, p);
}

inline float3 TwistZ(float3 p, float power)
{
    float s = sin(power * p.y);
    float c = cos(power * p.y);
    float3x3 m = float3x3(
        c, s, 0.0,
        -s, c, 0.0,
        0.0, 0.0, 1.0
    );
    return mul(m, p);
}

inline float3 Twist(float3 pos, float3 power)
{
    pos = TwistX(pos, power.x);
    pos = TwistY(pos, power.y);
    pos = TwistZ(pos, power.z);
}

#endif
