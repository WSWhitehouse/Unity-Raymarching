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

  public int Operation;
  public int OperationSmooth;
  public float OperationMod;
  public int Operationlayer;

  public float Roundness;
  public float WallThickness;

  public RaymarchObjectInfo(RaymarchObject _raymarchObject)
  {
    // Fill in Info Struct
    IsVisible = _raymarchObject.isActiveAndEnabled ? 1 : 0;
    SdfShape = (int) _raymarchObject.SdfShape;
    MarchingStepAmount = _raymarchObject.MarchingStepAmount;
    Position = _raymarchObject.Position;
    Rotation = _raymarchObject.Rotation;
    Scale = _raymarchObject.Scale;
    Colour = new Vector3(_raymarchObject.Colour.r, _raymarchObject.Colour.g, _raymarchObject.Colour.b);
    Operation = (int) _raymarchObject.Operation;
    OperationSmooth = _raymarchObject.OperationSmooth ? 1 : 0;
    OperationMod = _raymarchObject.OperationMod;
    Operationlayer = _raymarchObject.OperationLayer;
    Roundness = _raymarchObject.Roundness;
    WallThickness = _raymarchObject.WallThickness;
  }

  public static int GetSize()
  {
    return (sizeof(float) * 17) + (sizeof(int) * 5);
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