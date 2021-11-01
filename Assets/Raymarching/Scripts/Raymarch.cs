using UnityEngine;
using Object = UnityEngine.Object;

public static class Raymarch
{
  #region Raymarch Shader

  private static Shader _shader;

  public static Shader Shader
  {
    get => _shader;
    set
    {
      _shader = value;

      if (_shader == null)
      {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
          Object.Destroy(Material);
        }
        else
        {
          Object.DestroyImmediate(Material);
        }
#else
        Object.Destroy(Material);
#endif

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

  public static Material Material { get; private set; }

  public static void ResetData()
  {
    Shader = null;
  }

  public static bool ShouldRender()
  {
    return Material != null;
  }

  #endregion Raymarch Shader

  #region Camera

  public static float CameraPositionW { get; set; } = 0.0f;
  public static Vector3 CameraRotation4D { get; set; } = Vector3.zero;

  #endregion

  #region Upload Shader Data Event

  public delegate void UploadShaderData(Material material);

  private static event UploadShaderData OnUploadShaderData;

  public static void UploadShaderDataAddCallback(UploadShaderData func)
  {
#if UNITY_EDITOR
    UploadShaderDataRemoveCallback(func);
#endif
    OnUploadShaderData += func;
  }

  public static void UploadShaderDataRemoveCallback(UploadShaderData func)
  {
    OnUploadShaderData -= func;
  }

  public static void UploadShaderDataInvoke()
  {
    OnUploadShaderData?.Invoke(Material);
  }

  #endregion Upload Shader Data Event
}