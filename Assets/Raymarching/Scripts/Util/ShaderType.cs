using System;

public enum ShaderType
{
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
}