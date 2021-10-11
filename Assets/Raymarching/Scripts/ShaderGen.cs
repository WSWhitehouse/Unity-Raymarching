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

public class ShaderGen
{
  #region Common Shader Code

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

  #endregion // Common Shader Code

  #region Distance Functions

  private const string DistanceFunctionShaderName = "DistanceFunctions";

  public static void GenerateDistanceFunctionsShader()
  {
    string[] guids = AssetDatabase.FindAssets(string.Concat("t:", nameof(RaymarchSDF)), null);

    List<RaymarchSDF> sdfAssets = new List<RaymarchSDF>(guids.Length);
    sdfAssets.AddRange(guids.Select(guid =>
      AssetDatabase.LoadAssetAtPath<RaymarchSDF>(AssetDatabase.GUIDToAssetPath(guid))));

    var functions = sdfAssets.Aggregate(String.Empty,
      (current, sdf) => string.Concat(current, NewLine, sdf.FunctionPrototypeWithGuid, NewLine, "{",
        NewLine, sdf.FunctionBody, NewLine, "}", NewLine));

    GenerateUtilShader(DistanceFunctionShaderName, functions);

    // maybe detect what assets have already been generated and skip them?
    // instead of regenerating the entire shader every time one is created/destroyed
  }

  #endregion // Distance Functions

  #region Material Functions

  private const string MaterialFunctionShaderName = "MaterialFunctions";

  public static void GenerateMaterialFunctionsShader()
  {
    string[] guids = AssetDatabase.FindAssets(string.Concat("t:", nameof(RaymarchMaterial)), null);

    List<RaymarchMaterial> materials = new List<RaymarchMaterial>(guids.Length);
    materials.AddRange(guids.Select(guid =>
      AssetDatabase.LoadAssetAtPath<RaymarchMaterial>(AssetDatabase.GUIDToAssetPath(guid))));

    var functions = materials.Aggregate(String.Empty,
      (current, mat) => string.Concat(current, NewLine, mat.FunctionPrototypeWithGuid, NewLine, "{",
        NewLine, mat.FunctionBody, NewLine, "}", NewLine));

    GenerateUtilShader(MaterialFunctionShaderName, functions);

    // maybe detect what assets have already been generated and skip them?
    // instead of regenerating the entire shader every time one is created/destroyed
  }

  #endregion // Material Functions

  #region Scene Raymarch Shader

  private const string RaymarchTemplateShaderTitle = "RaymarchTemplateShader";
  private const string RaymarchVars = "// RAYMARCH VARS //";
  private const string RaymarchCalcDistance = "// RAYMARCH CALC DISTANCE //";
  private const string RaymarchCalcLights = "// RAYMARCH CALC LIGHT //";

  private static string CalculateRotationString(string positionName, string rotationName)
  {
    return string.Format("{0}.xz = mul({0}.xz, float2x2(cos({1}.y), sin({1}.y), -sin({1}.y), cos({1}.y)));" + NewLine +
                         "{0}.yz = mul({0}.yz, float2x2(cos({1}.x), -sin({1}.x), sin({1}.x), cos({1}.x)));" + NewLine +
                         "{0}.xy = mul({0}.xy, float2x2(cos({1}.z), -sin({1}.z), sin({1}.z), cos({1}.z)));" + NewLine,
      positionName, rotationName);
  }

  public static Shader GenerateSceneRaymarchShader(Scene scene, Shader template, List<RaymarchBase> raymarchBase)
  {
    string currentDir = Directory.GetCurrentDirectory();

    // Finding path of template
    string templatePath = string.Concat(currentDir, "/", AssetDatabase.GetAssetPath(template));

    // Find/Create path for Generated Shader
    string scenePath = string.Concat(currentDir, "/", scene.path);
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

    List<RaymarchObject> _objects = new List<RaymarchObject>();
    List<RaymarchLight> _lights = new List<RaymarchLight>();

    foreach (var rmBase in raymarchBase)
    {
      var rmObj = rmBase.GetComponent<RaymarchObject>();
      if (rmObj != null)
      {
        _objects.Add(rmObj);
      }

      var rmlight = rmBase.GetComponent<RaymarchLight>();
      if (rmlight != null)
      {
        _lights.Add(rmlight);
      }
    }

    string raymarchVars = raymarchBase.Aggregate($"// Raymarch Variables{NewLine}",
      (current, rmBase) => string.Concat(current, rmBase.GetShaderCode_Variables(), NewLine));

    string raymarchDistance = string.Empty;

    foreach (var rmObject in _objects)
    {
      string guid = rmObject.GUID.ToShaderSafeString();

      string positionName = $"_Position{guid}";
      string rotationName = $"_Rotation{guid}";
      string distanceName = $"distance{guid}";
      string colourName = $"_Colour{guid}";
      string localPosName = $"position{guid}";

      raymarchDistance = $"{raymarchDistance}{NewLine}float3 {localPosName} = rayPos - {positionName};{NewLine}";
      raymarchDistance = $"{raymarchDistance}{NewLine}{CalculateRotationString(localPosName, rotationName)};{NewLine}";
      raymarchDistance = $"{raymarchDistance}{NewLine}float {distanceName} = {rmObject.GetShaderCode_CalcDistance()}{NewLine}";

      raymarchDistance = $"{raymarchDistance}{NewLine}if ({distanceName} < resultDistance){NewLine} " +
                         $"{{ {NewLine}" +
                         $"resultDistance = {distanceName};{NewLine}" +
                         $"{rmObject.GetShaderCode_Material()}{NewLine}" +
                         $"}} {NewLine}";
    }

    string raymarchLight = _lights.Aggregate(string.Empty,
      (current, light) => string.Concat(current, light.GetShaderCode_CalcLight()));

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

    return AssetDatabase.LoadAssetAtPath<Shader>(filePath);
  }

  #endregion // Scene Raymarch Shader

  #region Util Shader

  private const string UtilShaderPath = "Assets/Raymarching/Shaders/Generated/";
  private const string UtilShaderExtension = "hlsl";

  private static string UnityShaderIncludes = $"{NewLine}// Unity Includes {NewLine}" +
                                              $"#include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl\"{NewLine}" +
                                              $"#include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl\"{NewLine}";

  private static void GenerateUtilShader(string shaderName, string shaderContents)
  {
    GenerateUtilShader(shaderName, Encoding.ASCII.GetBytes(shaderContents));
  }

  private static void GenerateUtilShader(string shaderName, byte[] shaderContents)
  {
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

    file.Write(shaderContents);

    file.Write(Encoding.ASCII.GetBytes(NewLine));
    file.Write(Encoding.ASCII.GetBytes(HeaderGuardEnd(headerGuardName)));

    file.Close();
    AssetDatabase.Refresh();
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

  #endregion // Util Shader
}

#endif