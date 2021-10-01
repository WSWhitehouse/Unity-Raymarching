using System;
using System.Runtime.InteropServices; // Struct layout attribute
using UnityEngine;

[Serializable, StructLayout(LayoutKind.Sequential)]
public struct RaymarchObjectInfo
{
  public int IsVisible;

  public int SdfShape;
  public float MarchingStepAmount;

  public Vector3 Position;
  public Vector4 Rotation;
  public Vector3 Scale;

  public Vector3 Colour;

  public float Roundness;
  public float WallThickness;

  public int ModifierIndex;

  public RaymarchObjectInfo(RaymarchObject _raymarchObject, int modifierIndex)
  {
    // Fill in Info Struct
    IsVisible = _raymarchObject.isActiveAndEnabled ? 1 : 0;
    SdfShape = (int) _raymarchObject.SdfShape;
    MarchingStepAmount = _raymarchObject.MarchingStepAmount;
    Position = _raymarchObject.Position;
    Rotation = _raymarchObject.Rotation;
    Scale = _raymarchObject.Scale;
    Colour = new Vector3(_raymarchObject.Colour.r, _raymarchObject.Colour.g, _raymarchObject.Colour.b);
    Roundness = _raymarchObject.Roundness;
    WallThickness = _raymarchObject.WallThickness;

    ModifierIndex = modifierIndex;
  }

  public static int GetSize()
  {
    return (sizeof(float) * 16) + (sizeof(int) * 3);
  }
}

[Serializable, StructLayout(LayoutKind.Sequential)]
public struct RaymarchModifierInfo
{
  public int NumOfObjects;

  public int Operation;
  public int OperationSmooth;
  public float OperationMod;

  public RaymarchModifierInfo(RaymarchModifier _raymarchModifier)
  {
    NumOfObjects = _raymarchModifier.NumOfObjects;
    Operation = _raymarchModifier.isActiveAndEnabled ? (int) _raymarchModifier.Operation : (int) _Operation.None;
    OperationSmooth = _raymarchModifier.OperationSmooth ? 1 : 0;
    OperationMod = _raymarchModifier.OperationMod;
  }

  public static int GetSize()
  {
    return (sizeof(float) * 1) + (sizeof(int) * 3);
  }
}

[Serializable, StructLayout(LayoutKind.Sequential)]
struct RaymarchLightInfo
{
  int LightType;

  Vector3 Position;
  Vector3 Direction;

  Vector3 Colour;
  float Range;
  float Intensity;

  public RaymarchLightInfo(RaymarchLight _raymarchLight)
  {
    // Fill in Info Struct
    LightType = (int) _raymarchLight.LightType;
    Position = _raymarchLight.Position;
    Direction = _raymarchLight.Direction;
    Colour = new Vector3(_raymarchLight.Colour.r, _raymarchLight.Colour.g, _raymarchLight.Colour.b);
    Range = _raymarchLight.Range;
    Intensity = _raymarchLight.Intensity;
  }

  public static int GetSize()
  {
    return (sizeof(float) * 11) + (sizeof(int) * 1);
  }
}