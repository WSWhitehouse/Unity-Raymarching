#ifndef SDF_SHAPE
#define SDF_SHAPE

struct Shape
{
    // Translation
    float3 Position;
    float3 Rotation;
    float3 Scale;

    // Object Properties
    float3 Colour;
    int ShapeType;
    float3 Modifier;
    float Roundness;
    float WallThickness;

    // RayMarch
    float MarchingStepAmount;
    int Operation;
    float BlendStrength;

    // Sine Wave
    int EnableSineWave;
    float3 SineWaveDirection;
    float SineWaveFreq;
    float SineWaveSpeed;
    float SineWaveAmp;

    // Num of Children
    int NumOfChildren;
};

#endif
