﻿#if UNITY_EDITOR

using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class ShaderGen
{
  #region Shader Code

  public const string Comment = "// ";
  public const string NewLine = "\r\n";

  public const string IfnDef = "#ifndef ";
  public const string Define = "#define ";
  public const string EndIf = "#endif ";

  private static string HeaderGuardStart(string name)
  {
    var nameUpper = name.ToUpper().Replace(' ', '_');
    return string.Concat(IfnDef, nameUpper, NewLine, Define, nameUpper, NewLine);
  }

  private static string HeaderGuardEnd(string name)
  {
    var nameUpper = name.ToUpper();
    return string.Concat(EndIf, Comment, nameUpper, NewLine);
  }

  #endregion Shader Code

  #region Scene Raymarch Shader

  private const string RaymarchTemplateShaderTitle = "RaymarchTemplateShader";
  private const string RaymarchVars = "// RAYMARCH VARS //";
  private const string RaymarchCalcDistance = "// RAYMARCH CALC DISTANCE //";
  private const string RaymarchCalcLights = "// RAYMARCH CALC LIGHT //";

  // NOTE(WSWhitehouse): string capacity that will be multiplied per object
  private const int StringCapacityPerObject = 512;

  public static void GenerateRaymarchShader()
  {
    if (Application.isPlaying)
    {
      Debug.LogWarning("Generate Raymarch Shader called during runtime!");
      return;
    }

    RaymarchScene rmScene = RaymarchScene.ActiveInstance;

    // Raymarch Scene Sanity Checks
    if (rmScene == null) return;
    if (rmScene.templateShader == null) return;

    Scene activeScene = rmScene.gameObject.scene;
    var raymarchBases = GenerateRaymarchSceneHierarchy(activeScene);

    string currentDir = Directory.GetCurrentDirectory();

    // Finding path of template
    string templatePath = string.Concat(currentDir, "/", AssetDatabase.GetAssetPath(rmScene.templateShader));

    // Find/Create path for Generated Shader
    string scenePath = string.Concat(currentDir, "/", activeScene.path);
    string sceneName = Path.GetFileNameWithoutExtension(scenePath);
    scenePath = string.Concat(Path.GetDirectoryName(scenePath), "/");

    string shaderName = string.Concat(sceneName, "_RaymarchShader");

    string filePath = string.Concat(scenePath, sceneName);
    Directory.CreateDirectory(filePath);
    filePath = string.Concat(filePath, "/", shaderName, ".shader");

    if (File.Exists(filePath))
    {
      File.Delete(filePath);
    }

    // NOTE(WSWhitehouse): Creating strings to hold shader code that will be inserted into the shader
    string raymarchVars = raymarchBases.Aggregate($"// Raymarch Variables{NewLine}",
      (current, rmBase) => string.Concat(current, rmBase.GetShaderCode_Variables(), NewLine));

    int capacity = StringCapacityPerObject * raymarchBases.Count;
    
    StringBuilder raymarchDistance = new StringBuilder(capacity);
    StringBuilder raymarchLight = new StringBuilder(capacity);

    List<RaymarchOperation> operations = new List<RaymarchOperation>();

    for (int i = 0; i < raymarchBases.Count; i++)
    {
      string guid = raymarchBases[i].GUID.ToShaderSafeString();

      if (raymarchBases[i] is RaymarchOperation rmOperation)
      {
        operations.Add(rmOperation);

        var opGuid = rmOperation.GUID.ToShaderSafeString();

        raymarchDistance.AppendLine($"// Operation Start {rmOperation.operation.ShaderFeature.FunctionName} {opGuid}");
        raymarchDistance.AppendLine($"float distance{opGuid} = _RenderDistance;");
        raymarchDistance.AppendLine($"float4 colour{opGuid} = float4(1,1,1,1);");
        raymarchDistance.AppendLine(); // NOTE(WSWhitehouse): New line
      }

      if (raymarchBases[i] is RaymarchObject rmObject)
      {
        string positionName = $"_Position{guid}";
        string rotationName = $"_Rotation{guid}";
        string isActiveName = $"_IsActive{guid}";

        string localDistName = $"distance{guid}";
        string localPosName = $"position{guid}";

        raymarchDistance.AppendLine(
          $"float3 {localPosName} = Rotate3D(rayPos - {positionName}, {rotationName});");
        raymarchDistance.AppendLine($"float {localDistName} = _RenderDistance;");

        raymarchDistance.AppendLine($"if ({isActiveName} > 0)");
        raymarchDistance.AppendLine($"{{");
        raymarchDistance.AppendLine(rmObject.GetShaderCode_CalcDistance());
        raymarchDistance.AppendLine($"}}");

        raymarchDistance.AppendLine(); // NOTE(WSWhitehouse): New line

        if (operations.Count > 0)
        {
          var operation = operations[^1];
          var opGuid = operation.GUID.ToShaderSafeString();

          if (i > operation.StartIndex)
          {
            raymarchDistance.AppendLine(
              operation.GetShaderCode_CalcOperation(localDistName, rmObject.GetShaderCode_Material()));
          }
          else
          {
            raymarchDistance.AppendLine($"distance{opGuid} = {localDistName};");
            raymarchDistance.AppendLine($"colour{opGuid} = {rmObject.GetShaderCode_Material()};");
          }
        }
        else
        {
          raymarchDistance.AppendLine($"if ({localDistName} < resultDistance)");
          raymarchDistance.AppendLine($"{{");
          raymarchDistance.AppendLine($"resultDistance = {localDistName};");
          raymarchDistance.AppendLine($"resultColour   = {rmObject.GetShaderCode_Material()};");
          raymarchDistance.AppendLine($"}}");
        }

        raymarchDistance.AppendLine(); // NOTE(WSWhitehouse): New line
      }

      if (raymarchBases[i] is RaymarchLight rmLight)
      {
        raymarchLight.AppendLine(rmLight.GetShaderCode_CalcLight());
      }

      // End any operations that end on this index and remove them from the list
      for (var j = operations.Count - 1; j >= 0; j--)
      {
        var operation = operations[j];
        if (operation.EndIndex != i) continue;

        string opGuid = operation.GUID.ToShaderSafeString();

        raymarchDistance.AppendLine($"// Operation End {opGuid}");

        if (j > 0)
        {
          raymarchDistance.AppendLine(
            $"{operations[j - 1].GetShaderCode_CalcOperation($"distance{opGuid}", $"colour{opGuid}")}");
        }
        else
        {
          raymarchDistance.AppendLine($"if (distance{opGuid} < resultDistance)");
          raymarchDistance.AppendLine($"{{");
          raymarchDistance.AppendLine($"resultDistance = distance{opGuid};");
          raymarchDistance.AppendLine($"resultColour   = colour{opGuid};");
          raymarchDistance.AppendLine($"}}");
        }

        raymarchDistance.AppendLine(); // NOTE(WSWhitehouse): New line
      }

      operations.RemoveAll(x => x.EndIndex == i);
    }

    // Read template shader and replace placeholders
    string shader = File.ReadAllText(templatePath);
    shader = shader.Replace(RaymarchTemplateShaderTitle, shaderName)
      .Replace(RaymarchVars, raymarchVars)
      .Replace(RaymarchCalcDistance, raymarchDistance.ToString())
      .Replace(RaymarchCalcLights, raymarchLight.ToString());

    // Create shader
    FileStream file = File.Open(filePath, FileMode.OpenOrCreate);

    file.Write(Encoding.ASCII.GetBytes(AutoGeneratedComment()));
    file.Write(Encoding.ASCII.GetBytes(shader));

    file.Close();
    AssetDatabase.Refresh();

    int assetsIndex = filePath.IndexOf("\\Assets\\", StringComparison.Ordinal);
    filePath = filePath.Remove(0, assetsIndex + 1);

    rmScene.Shader = AssetDatabase.LoadAssetAtPath<Shader>(filePath);

    if (rmScene.Shader != null)
    {
      EditorUtility.SetDirty(rmScene.Shader);
    }
    else
    {
      Debug.LogError("Generated shader is null!");
    }

    // NOTE(WSWhitehouse): Enable all objects after generating shader.
    foreach (RaymarchBase raymarchBase in raymarchBases)
    {
      raymarchBase.Awake();
    }
  }

  private static List<RaymarchBase> GenerateRaymarchSceneHierarchy(Scene scene)
  {
    List<RaymarchBase> rmBases = new List<RaymarchBase>();

    if (!scene.isLoaded || !scene.IsValid())
    {
      return rmBases;
    }

    var rootGameObjects = new List<GameObject>(scene.rootCount);
    scene.GetRootGameObjects(rootGameObjects);

    foreach (var rootGameObject in rootGameObjects)
    {
      CheckObjectForRaymarchBase(ref rmBases, rootGameObject);
    }

    return rmBases;
  }

  private static void CheckObjectForRaymarchBase(ref List<RaymarchBase> rmBases, GameObject gameObject)
  {
    int currentIndex = rmBases.Count;

    RaymarchBase rmBase = gameObject.GetComponent<RaymarchBase>();
    if (rmBase != null && rmBase.IsValid())
    {
      // NOTE(WSWhitehouse): Checks if this GUID already exists, if so generate a new one!
      if (rmBases.FindIndex(x => x.GUID.ToShaderSafeString() == rmBase.GUID.ToShaderSafeString()) != -1)
      {
        rmBase.GUID.ResetGUID();
      }

      rmBases.Add(rmBase);
    }

    for (int i = 0; i < gameObject.transform.childCount; i++)
    {
      Transform child = gameObject.transform.GetChild(i);
      CheckObjectForRaymarchBase(ref rmBases, child.gameObject);
    }

    if (rmBase is RaymarchOperation rmOperation && rmBase.IsValid())
    {
      rmOperation.StartIndex = currentIndex + 1;
      rmOperation.EndIndex = rmBases.Count - 1;
    }
  }

  #endregion Scene Raymarch Shader

  #region Util

  private const string UtilShaderPath = "Assets/Raymarching/Shaders/Generated/";
  private const string UtilShaderExtension = "hlsl";

  private static readonly string UnityShaderIncludes = $"{NewLine}// Unity Includes {NewLine}" +
                                                       $"#include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl\"{NewLine}" +
                                                       $"#include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl\"{NewLine}";

  public static void GenerateUtilShader<T>(string shaderName) where T : ShaderFeature
  {
    string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).FullName}", null);

    List<T> shaderFeatures = new List<T>(guids.Length);
    shaderFeatures.AddRange(guids.Select(guid =>
      AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))));

    var functions = shaderFeatures.Aggregate(String.Empty,
      (current, feature) => string.Concat(current, NewLine, feature.FunctionPrototypeWithGuid, NewLine, "{",
        NewLine, feature.FunctionBody, NewLine, "}", NewLine));

    string filePath = string.Concat(UtilShaderPath, shaderName, ".", UtilShaderExtension);
    string headerGuardName = string.Concat(shaderName.ToUpper().Replace(' ', '_'), "_", UtilShaderExtension.ToUpper());

    // NOTE(WSWhitehouse): Should use AssetDatabase functions here, but they don't work reliably
    Directory.CreateDirectory(UtilShaderPath);

    if (File.Exists(filePath))
    {
      File.Delete(filePath);
    }

    FileStream file = File.Open(filePath, FileMode.OpenOrCreate);

    file.Write(Encoding.ASCII.GetBytes(AutoGeneratedComment()));
    file.Write(Encoding.ASCII.GetBytes(HeaderGuardStart(headerGuardName)));

    file.Write(Encoding.ASCII.GetBytes(UnityShaderIncludes));
    file.Write(Encoding.ASCII.GetBytes(functions));

    file.Write(Encoding.ASCII.GetBytes(NewLine));
    file.Write(Encoding.ASCII.GetBytes(HeaderGuardEnd(headerGuardName)));

    file.Close();
    AssetDatabase.Refresh();

    var shader = AssetDatabase.LoadAssetAtPath<Object>(filePath);
    EditorUtility.SetDirty(shader);
  }

  private static string AutoGeneratedComment()
  {
    DateTime now = DateTime.Now;

    StringBuilder result = new StringBuilder();
    result.AppendLine($"//---------------------------------------------------------------------{NewLine}");
    result.AppendLine($"//    This code was generated by a tool.                               {NewLine}");
    result.AppendLine($"//                                                                     {NewLine}");
    result.AppendLine($"//    Changes to this file may cause incorrect behavior and will be    {NewLine}");
    result.AppendLine($"//    lost if the code is regenerated.                                 {NewLine}");
    result.AppendLine($"//                                                                     {NewLine}");
    result.AppendLine($"//    Time Generated: {now.ToString(CultureInfo.InvariantCulture)}     {NewLine}");
    result.AppendLine($"//---------------------------------------------------------------------{NewLine}");

    return result.ToString();
  }

  #endregion Util
}

#endif