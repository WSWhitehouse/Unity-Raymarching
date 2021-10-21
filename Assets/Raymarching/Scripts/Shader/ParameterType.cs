using System;

public enum ParameterType
{
  None,
  In,
  Out,
  InOut
}

public static class ParameterTypeExtensions
{
  public static string ToShaderString(this ParameterType parameterType)
  {
    return parameterType switch
    {
      ParameterType.None => string.Empty,
      ParameterType.In => "in ",
      ParameterType.Out => "out ",
      ParameterType.InOut => "inout ",
      _ => throw new ArgumentOutOfRangeException()
    };
  }
}