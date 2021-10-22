using System;
using UnityEngine;

public enum ShaderType
{
  Void = -1,
  Float,
  Int,
  Bool,
  Vector2,
  Vector3,
  Vector4,
  Colour,
  Texture2D
}

public static class ShaderTypeExtensions
{
  public static string ToShaderString(this ShaderType shaderType)
  {
    return shaderType switch
    {
      ShaderType.Void => "void",
      ShaderType.Float => "float",
      ShaderType.Int => "int",
      ShaderType.Bool => "int",
      ShaderType.Vector2 => "float2",
      ShaderType.Vector3 => "float3",
      ShaderType.Vector4 => "float4",
      ShaderType.Colour => "float4",
      ShaderType.Texture2D => "sampler2D",
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public static ShaderType ToShaderType(this Type type)
  {
    if (type == typeof(void))
    {
      return ShaderType.Void;
    }
    
    if (type == typeof(float))
    {
      return ShaderType.Float;
    }

    if (type == typeof(int))
    {
      return ShaderType.Int;
    }

    if (type == typeof(bool))
    {
      return ShaderType.Bool;
    }

    if (type == typeof(Vector2))
    {
      return ShaderType.Vector2;
    }

    if (type == typeof(Vector3))
    {
      return ShaderType.Vector3;
    }

    if (type == typeof(Vector4))
    {
      return ShaderType.Vector4;
    }

    if (type == typeof(Color))
    {
      return ShaderType.Colour;
    }

    if (type == typeof(Texture2D))
    {
      return ShaderType.Texture2D;
    }

    throw new NotSupportedException($"{type.Name} is not a supported Shader Type.");
  }
}