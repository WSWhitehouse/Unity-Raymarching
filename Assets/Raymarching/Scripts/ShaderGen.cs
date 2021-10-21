#if UNITY_EDITOR

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

  #endregion // Shader Code

  #region Scene Raymarch Shader

  private const string RaymarchTemplateShaderTitle = "RaymarchTemplateShader";
  private const string RaymarchVars = "// RAYMARCH VARS //";
  private const string RaymarchCalcDistance = "// RAYMARCH CALC DISTANCE //";
  private const string RaymarchCalcLights = "// RAYMARCH CALC LIGHT //";

  private static void CheckForDuplicateGUIDS(ref List<RaymarchBase> rmBases)
  {
    for (int i = 0; i < rmBases.Count; i++)
    {
      for (int j = i + 1; j < rmBases.Count; j++)
      {
        if (rmBases[i].GUID.GUID == rmBases[j].GUID.GUID)
        {
          rmBases[i].GUID.ResetGUID();
        }
      }
    }
  }

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

    CheckForDuplicateGUIDS(ref raymarchBases);

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

    string raymarchVars = raymarchBases.Aggregate($"// Raymarch Variables{NewLine}",
      (current, rmBase) => string.Concat(current, rmBase.GetShaderCode_Variables(), NewLine));

    string raymarchDistance = string.Empty;
    string raymarchLight = string.Empty;

    List<RaymarchOperation> operations = new List<RaymarchOperation>();

    for (var i = 0; i < raymarchBases.Count; i++)
    {
      string guid = raymarchBases[i].GUID.ToShaderSafeString();

      if (raymarchBases[i] is RaymarchOperation rmOperation)
      {
        operations.Add(rmOperation);
      }

      if (raymarchBases[i] is RaymarchObject rmObject)
      {
        string positionName = $"_Position{guid}";
        string rotationName = $"_Rotation{guid}";
        string isActiveName = $"_IsActive{guid}";

        string localDistName = $"distance{guid}";
        string localPosName = $"position{guid}";

        raymarchDistance =
          $"{raymarchDistance}{NewLine}float3 {localPosName} = Rotate3D(rayPos - {positionName}, {rotationName});{NewLine}";
        raymarchDistance = $"{raymarchDistance}float {localDistName} = _RenderDistance;{NewLine}";
        raymarchDistance = $"{raymarchDistance}if ({isActiveName} > 0){NewLine}{{{NewLine}";

        raymarchDistance =
          $"{raymarchDistance}{rmObject.GetShaderCode_CalcDistance()}";
        
        raymarchDistance = $"{raymarchDistance}}}{NewLine}";

        if (operations.Count > 0)
        {
          var operation = operations[^1];
          var opGuid = operation.GUID.ToShaderSafeString();

          if (i > operation.StartIndex)
          {
            raymarchDistance =
              $"{raymarchDistance}{NewLine}{operation.GetShaderCode_CalcOperation(localDistName, rmObject.GetShaderCode_Material())}{NewLine}";
          }
          else
          {
            raymarchDistance = $"{raymarchDistance}{NewLine}float distance{opGuid} = {localDistName};";
            raymarchDistance = $"{raymarchDistance}{NewLine}float4 colour{opGuid} = {rmObject.GetShaderCode_Material()};{NewLine}";
          }
        }
        else
        {
          raymarchDistance = $"{raymarchDistance}{NewLine}if ({localDistName} < resultDistance){NewLine} " +
                             $"{{ {NewLine}" +
                             $"resultDistance = {localDistName};{NewLine}" +
                             $"resultColour = {rmObject.GetShaderCode_Material()};{NewLine}" +
                             $"}} {NewLine}";
        }
      }

      if (raymarchBases[i] is RaymarchLight rmLight)
      {
        raymarchLight = string.Concat(raymarchLight, rmLight.GetShaderCode_CalcLight());
      }

      // End any operations that end on this index and remove them from the list
      for (var j = operations.Count - 1; j >= 0; j--)
      {
        var operation = operations[j];
        if (operation.EndIndex != i) continue;

        string opGuid = operation.GUID.ToShaderSafeString();

        if (j > 0)
        {
          raymarchDistance =
            $"{raymarchDistance}{operations[j - 1].GetShaderCode_CalcOperation($"distance{opGuid}", $"colour{opGuid}")};{NewLine}";
        }
        else
        {
          raymarchDistance = $"{raymarchDistance}{NewLine}if (distance{opGuid} < resultDistance){NewLine} " +
                             $"{{ {NewLine}" +
                             $"resultDistance = distance{opGuid};{NewLine}" +
                             $"resultColour = colour{opGuid};{NewLine}" +
                             $"}} {NewLine}";
        }
      }

      operations.RemoveAll(x => x.EndIndex == i);
    }

    // Read template shader and replace placeholders
    string shader = File.ReadAllText(templatePath);
    shader = shader.Replace(RaymarchTemplateShaderTitle, shaderName)
      .Replace(RaymarchVars, raymarchVars)
      .Replace(RaymarchCalcDistance, raymarchDistance)
      .Replace(RaymarchCalcLights, raymarchLight);

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
      CheckObjectForRmBase(ref rmBases, rootGameObject);
    }

    return rmBases;
  }

  private static void CheckObjectForRmBase(ref List<RaymarchBase> rmBases, GameObject gameObject)
  {
    int currentIndex = rmBases.Count;

    RaymarchBase rmBase = gameObject.GetComponent<RaymarchBase>();
    if (rmBase != null && rmBase.IsValid())
    {
      rmBases.Add(rmBase);
      rmBase.Awake();
    }

    for (int i = 0; i < gameObject.transform.childCount; i++)
    {
      Transform child = gameObject.transform.GetChild(i);
      CheckObjectForRmBase(ref rmBases, child.gameObject);
    }

    if (rmBase is RaymarchOperation rmOperation && rmBase.IsValid())
    {
      rmOperation.StartIndex = currentIndex + 1;
      rmOperation.EndIndex = rmBases.Count - 1;
    }
  }

  #endregion // Scene Raymarch Shader

  #region Util

  private const string UtilShaderPath = "Assets/Raymarching/Shaders/Generated/";
  private const string UtilShaderExtension = "hlsl";

  private static string UnityShaderIncludes = $"{NewLine}// Unity Includes {NewLine}" +
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

    // Should use AssetDatabase functions here, but they don't work reliably
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

    string result = string.Empty;
    result = string.Concat(result, "//---------------------------------------------------------------------", NewLine);
    result = string.Concat(result, "//    This code was generated by a tool.                               ", NewLine);
    result = string.Concat(result, "//                                                                     ", NewLine);
    result = string.Concat(result, "//    Changes to this file may cause incorrect behavior and will be    ", NewLine);
    result = string.Concat(result, "//    lost if the code is regenerated.                                 ", NewLine);
    result = string.Concat(result, "//                                                                     ", NewLine);
    result = string.Concat(result, "//    Time Generated: ", now.ToString(CultureInfo.InvariantCulture), NewLine);
    result = string.Concat(result, "//---------------------------------------------------------------------", NewLine,
      NewLine);

    return result;
  }

  #endregion // Util
}

#endif