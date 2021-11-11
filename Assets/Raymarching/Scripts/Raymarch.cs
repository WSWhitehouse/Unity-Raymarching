using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

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

  #region Upload Shader Data Event

  public delegate void UploadShaderData(Material material);

  // NOTE(WSWhitehouse): This event is private so scripts can only add/remove callbacks through 
  // the functions below. Meaning this script can perform any checks on the callbacks.
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

#if UNITY_EDITOR
  /* NOTE(WSWhitehouse):
   * This function performs the appropriate renderer checks depending on the current pipeline.
   * This should only be called in the editor as it is quite expensive (uses a lot of reflection) and
   * can make changes to the renderer which isn't something that should be allowed during runtime.
   *
   * URP - Checks if scriptable render pipeline renderer has a RaymarchRenderFeature attached
   *
   * Currently no other render pipelines are supported
   */
  public static bool PerformRendererChecks()
  {
    if (Application.isPlaying)
    {
      // NOTE(WSWhitehouse): callingMethodName is the name of the function that called this function
      string callingMethodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
      Debug.LogError($"Raymarch: {callingMethodName} called PerformRendererChecks during runtime, this is not supported!");
      return false;
    }

#if UNITY_PIPELINE_URP

    if (GraphicsSettings.currentRenderPipeline is not UniversalRenderPipelineAsset)
    {
      Debug.LogError("Raymarch: URP Pipeline is active but the current render pipeline is not URP");
      return false;
    }

    var urpPipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

    // NOTE(WSWhitehouse): IDE is screaming about possible null reference exception here.
    // Ignore it, the null check exists in the if statement above.
    Type type = urpPipeline.GetType();
    FieldInfo propertyInfo = type.GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);

    if (propertyInfo == null)
    {
      Debug.LogError(
        $"Raymarch: URP Asset ({urpPipeline.name}) doesnt include 'm_RendererDataList'. Cannot check for render feature");
      return false;
    }

    var scriptableRenderData = (ScriptableRendererData[]) propertyInfo.GetValue(urpPipeline);

    if (scriptableRenderData == null)
    {
      Debug.LogError($"Raymarch: {urpPipeline.name} ScriptableRenderData equals null");
      return false;
    }

    if (scriptableRenderData.Length == 0)
    {
      Debug.LogError($"Raymarch: {urpPipeline.name} doesn't contain any ScriptableRenderData");
      return false;
    }

    foreach (var renderData in scriptableRenderData)
    {
      foreach (var rendererFeature in renderData.rendererFeatures)
      {
        if (rendererFeature is RaymarchRenderFeature)
        {
          return true;
        }
      }
    }
    
    Debug.LogError(
      "HEY BUDDY! fuck you, we dont have one");
    return false;


#else
    Debug.LogError("Raymarch: Current Unity Pipeline is not supported!");
#endif

    return false;
  }
#endif
}