#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;

[InitializeOnLoad]
public class RenderPipelineDefines
{
  // NOTE(WSWhitehouse): This constructor is called when the Unity engine is loaded due to the 'InitializeOnLoad' attribute 
  static RenderPipelineDefines()
  {
    UpdateDefines();
  }

  private enum PipelineType
  {
    Unsupported,
    BuiltIn,
    URP,
    HDRP
  }

  private static void UpdateDefines()
  {
    var pipeline = GetPipeline();

    if (pipeline == PipelineType.URP)
    {
      AddDefine("UNITY_PIPELINE_URP");
    }
    else
    {
      RemoveDefine("UNITY_PIPELINE_URP");
    }

    if (pipeline == PipelineType.HDRP)
    {
      AddDefine("UNITY_PIPELINE_HDRP");
    }
    else
    {
      RemoveDefine("UNITY_PIPELINE_HDRP");
    }
  }

  private static PipelineType GetPipeline()
  {
    // NOTE(WSWhitehouse): Scriptable Render Pipelines didn't exist before 2019.1.
#if UNITY_2019_1_OR_NEWER
    if (GraphicsSettings.renderPipelineAsset != null)
    {
      string renderPipelineType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

      // NOTE(WSWhitehouse): "Magic" strings below are names of scriptable render pipeline scripts. Cannot use 
      // nameof() or compare types as they might not exist - resulting in a compile error. Therefore, if the
      // scripts get renamed this will stop working!

      if (renderPipelineType.Contains("HDRenderPipelineAsset"))
      {
        return PipelineType.HDRP;
      }

      // NOTE(WSWhitehouse): Check for both versions of URP asset
      if (renderPipelineType.Contains("UniversalRenderPipelineAsset") ||
          renderPipelineType.Contains("LightweightRenderPipelineAsset"))
      {
        return PipelineType.URP;
      }

      return PipelineType.Unsupported;
    }
#endif
    return PipelineType.BuiltIn;
  }

  private static void AddDefine(string define)
  {
    var definesList = GetDefines();
    if (!definesList.Contains(define))
    {
      definesList.Add(define);
      SetDefines(definesList);
    }
  }

  private static void RemoveDefine(string define)
  {
    var definesList = GetDefines();
    if (definesList.Contains(define))
    {
      definesList.Remove(define);
      SetDefines(definesList);
    }
  }

  private static List<string> GetDefines()
  {
    var target = EditorUserBuildSettings.activeBuildTarget;
    var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
    return defines.Split(';').ToList();
  }

  private static void SetDefines(List<string> definesList)
  {
    var target = EditorUserBuildSettings.activeBuildTarget;
    var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
    var defines = string.Join(";", definesList.ToArray());
    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
  }
}
#endif