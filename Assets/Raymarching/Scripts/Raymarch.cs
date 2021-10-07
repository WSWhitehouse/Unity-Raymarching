using System;
using UnityEngine;

public static class Raymarch
{
  private static Shader _shader;

  public static Shader Shader
  {
    get => _shader;
    set
    {
      _shader = value;

      if (_shader == null)
      {
        Material = null;
      }
      else
      {
        Material = new Material(_shader)
        {
          hideFlags = HideFlags.HideAndDontSave
        };
      }
    }
  }

  public static RaymarchSettings Settings { get; set; }

  public static Material Material { get; private set; }

  public static Action<Material> OnUploadShaderData;

  public static void InvokeUploadShaderData()
  {
    OnUploadShaderData?.Invoke(Material);
    Settings.UploadShaderData(Material);
  }

  public static bool ShouldRender()
  {
    return Material != null && Settings != null;
  }
}