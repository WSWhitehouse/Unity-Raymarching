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

public static class ShaderGen
{
  #region Scene Raymarch Shader

  private const string RaymarchTemplateShaderTitle = "RaymarchTemplateShader";

  private const string RaymarchVars = "// RAYMARCH VARS //";
  private const string RaymarchCalcDistance = "// RAYMARCH CALC DISTANCE //";
  private const string RaymarchCalcLights = "// RAYMARCH CALC LIGHT //";

  private const string RaymarchSettingsStart = "// RAYMARCH SETTINGS START //";
  private const string RaymarchSettingsEnd = "// RAYMARCH SETTINGS END //";

  private const string LightingSettingsStart = "// LIGHTING SETTINGS START //";
  private const string LightingSettingsEnd = "// LIGHTING SETTINGS END //";
  
  // NOTE(WSWhitehouse): string capacity that will be multiplied per object
  private const int StringCapacityPerObject = 512;

  public static void GenerateRaymarchShader()
  {
    Debug.Log("Generate Raymarch Shader called");

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
    int capacity = StringCapacityPerObject * raymarchBases.Count;
    StringBuilder raymarchVars = new StringBuilder(capacity);
    StringBuilder raymarchDistance = new StringBuilder(capacity);
    StringBuilder raymarchLight = new StringBuilder(capacity);

    raymarchVars.AppendLine("// Raymarch Variables");
    foreach (RaymarchBase raymarchBase in raymarchBases)
    {
      raymarchVars.AppendLine(raymarchBase.GetShaderVariables());
    }

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
        raymarchDistance.AppendLine();
      }

      if (raymarchBases[i] is RaymarchObject rmObject)
      {
        string positionName = $"_{nameof(rmObject.Position)}{guid}";
        string rotationName = $"_{nameof(rmObject.RotationRotor3D)}{guid}";
        string rotation4DName = $"_{nameof(rmObject.Rotation4D)}{guid}";
        string scaleName = $"_{nameof(rmObject.Scale)}{guid}";
        string transform4DEnabledName = $"_{nameof(rmObject.Transform4DEnabled)}{guid}";
        string isActiveName = $"_IsActive{guid}";

        string localDistName = $"distance{guid}";
        string localPosName = $"position{guid}";

        raymarchDistance.AppendLine(
          $"float4 {localPosName} = (rayPos4D - {positionName}) / {scaleName};");
        raymarchDistance.AppendLine(
          $"{localPosName} = float4(Rotate3D({localPosName}.xyz, {rotationName}), {localPosName}.w);");

        raymarchDistance.AppendLine($"if ({transform4DEnabledName} > 0)");
        raymarchDistance.AppendLine("{");
        raymarchDistance.AppendLine($"{localPosName} = Rotate4D({localPosName}, {rotation4DName});");
        raymarchDistance.AppendLine("}");

        raymarchDistance.AppendLine($"float {localDistName} = _RenderDistance;");

        raymarchDistance.AppendLine($"if ({isActiveName} > 0)");
        raymarchDistance.AppendLine($"{{");
        raymarchDistance.Append(RaymarchObjectDistanceShaderCode(rmObject));
        raymarchDistance.AppendLine($"}}");

        raymarchDistance.AppendLine();

        if (operations.Count > 0)
        {
          var operation = operations[^1];
          var opGuid = operation.GUID.ToShaderSafeString();

          if (i > operation.StartIndex)
          {
            raymarchDistance.AppendLine(
              RaymarchOperationShaderCode(operation, localDistName, RaymarchObjectMaterialShaderCode(rmObject)));
          }
          else
          {
            raymarchDistance.AppendLine($"distance{opGuid} = {localDistName};");
            raymarchDistance.AppendLine($"colour{opGuid} = {RaymarchObjectMaterialShaderCode(rmObject)};");
          }
        }
        else
        {
          raymarchDistance.AppendLine($"if ({localDistName} < resultDistance)");
          raymarchDistance.AppendLine($"{{");
          raymarchDistance.AppendLine($"resultDistance = {localDistName};");
          raymarchDistance.AppendLine($"resultColour   = {RaymarchObjectMaterialShaderCode(rmObject)};");
          raymarchDistance.AppendLine($"}}");
        }

        raymarchDistance.AppendLine();
      }

      if (raymarchBases[i] is RaymarchLight rmLight)
      {
        raymarchLight.AppendLine(RaymarchLightShaderCode(rmLight));
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
            RaymarchOperationShaderCode(operations[j - 1], $"distance{opGuid}", $"colour{opGuid}"));
        }
        else
        {
          raymarchDistance.AppendLine($"if (distance{opGuid} < resultDistance)");
          raymarchDistance.AppendLine($"{{");
          raymarchDistance.AppendLine($"resultDistance = distance{opGuid};");
          raymarchDistance.AppendLine($"resultColour   = colour{opGuid};");
          raymarchDistance.AppendLine($"}}");
        }

        raymarchDistance.AppendLine();
      }

      operations.RemoveAll(x => x.EndIndex == i);
    }

    // Read template shader and replace placeholders
    string shader = File.ReadAllText(templatePath);

    // Raymarch Settings
    string raymarchSettings = RaymarchSettingsShaderCode(rmScene.RaymarchSettings);
    int raymarchSettingsStart = shader.IndexOf(RaymarchSettingsStart, StringComparison.Ordinal);
    int raymarchSettingsEnd = shader.IndexOf(RaymarchSettingsEnd, StringComparison.Ordinal);

    shader = shader.Remove(raymarchSettingsStart, raymarchSettingsEnd - raymarchSettingsStart);
    shader = shader.Insert(raymarchSettingsStart, raymarchSettings);

    // Lighting Settings
    string lightingSettings = LightingSettingsShaderCode(rmScene.LightingSettings);
    int lightingSettingsStart = shader.IndexOf(LightingSettingsStart, StringComparison.Ordinal);
    int lightingSettingsEnd = shader.IndexOf(LightingSettingsEnd, StringComparison.Ordinal);

    shader = shader.Remove(lightingSettingsStart, lightingSettingsEnd - lightingSettingsStart);
    shader = shader.Insert(lightingSettingsStart, lightingSettings);

    // Other
    shader = shader.Replace(RaymarchTemplateShaderTitle, shaderName)
      .Replace(RaymarchVars, raymarchVars.ToString())
      .Replace(RaymarchCalcDistance, raymarchDistance.ToString())
      .Replace(RaymarchCalcLights, raymarchLight.ToString());


    // Create shader
    FileStream file = File.Open(filePath, FileMode.OpenOrCreate);

    file.Write(Encoding.ASCII.GetBytes(Util.AutoGeneratedComment()));
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

  private static string RaymarchObjectDistanceShaderCode(RaymarchObject rmObject)
  {
    string guid = rmObject.GUID.ToShaderSafeString();

    string localDistName = $"distance{guid}";
    string localPosName = $"position{guid}";
    string scaleName = $"_{nameof(rmObject.Scale)}{guid}";

    string marchingStepAmountName = $"_{nameof(rmObject.MarchingStepAmount)}{guid}";

    StringBuilder result = new StringBuilder();

    foreach (ToggleableShaderFeatureImpl<ModifierShaderFeature> modifier in rmObject.raymarchMods)
    {
      if (modifier.ShaderFeature.ModifierType != ModifierType.PreSDF) continue;

      string modifierParams = localPosName;
      for (int i = 0; i < modifier.ShaderVariables.Count; i++)
      {
        modifierParams = string.Concat(modifierParams, ", ", modifier.GetShaderVariableName(i, rmObject.GUID));
      }

      result.AppendLine($"if ({modifier.GetIsEnabledShaderName(rmObject.GUID)} > 0)");
      result.AppendLine("{");
      result.AppendLine($"{localPosName} = {modifier.ShaderFeature.FunctionNameWithGuid}({modifierParams});");
      result.AppendLine("}");
    }

    string parameters = localPosName;
    for (int i = 0; i < rmObject.raymarchSDF.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", rmObject.raymarchSDF.GetShaderVariableName(i, rmObject.GUID));
    }

    result.AppendLine(
      $"{localDistName} = {rmObject.raymarchSDF.ShaderFeature.FunctionNameWithGuid}({parameters}) * {scaleName};");

    foreach (ToggleableShaderFeatureImpl<ModifierShaderFeature> modifier in rmObject.raymarchMods)
    {
      if (modifier.ShaderFeature.ModifierType != ModifierType.PostSDF) continue;

      string modifierParams = $"position{guid}, {localDistName}";
      for (int i = 0; i < modifier.ShaderVariables.Count; i++)
      {
        modifierParams = string.Concat(modifierParams, ", ", modifier.GetShaderVariableName(i, rmObject.GUID));
      }

      result.AppendLine($"if ({modifier.GetIsEnabledShaderName(rmObject.GUID)} > 0)");
      result.AppendLine("{");
      result.AppendLine(
        $"{localDistName} = {modifier.ShaderFeature.FunctionNameWithGuid}({modifierParams});");
      result.AppendLine("}");
    }

    result.AppendLine($"{localDistName} /= {marchingStepAmountName};");

    return result.ToString();
  }

  private static string RaymarchObjectMaterialShaderCode(RaymarchObject rmObject)
  {
    string guid = rmObject.GUID.ToShaderSafeString();

    if (rmObject.raymarchMat.ShaderFeature == null)
    {
      return $"_{nameof(rmObject.Colour)}{guid}";
    }

    string parameters = $"position{guid}, _{nameof(rmObject.Colour)}{guid}";
    for (int i = 0; i < rmObject.raymarchMat.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", rmObject.raymarchMat.GetShaderVariableName(i, rmObject.GUID));
    }

    return $"{rmObject.raymarchMat.ShaderFeature.FunctionNameWithGuid}({parameters})";
  }

  private static string RaymarchOperationShaderCode(RaymarchOperation rmOperation, string objDistance, string objColour)
  {
    string guid = rmOperation.GUID.ToShaderSafeString();
    StringBuilder result = new StringBuilder();

    string opDistance = $"distance{guid}";
    string opColour = $"colour{guid}";
    string opIsActive = $"_IsActive{guid}";

    string parameters = $"{opDistance}, {opColour}, {objDistance}, {objColour}";
    for (int i = 0; i < rmOperation.operation.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", rmOperation.operation.GetShaderVariableName(i, rmOperation.GUID));
    }

    result.AppendLine($"if ({opIsActive} > 0)");
    result.AppendLine($"{{");
    result.AppendLine($"{rmOperation.operation.ShaderFeature.FunctionNameWithGuid}({parameters});");
    result.AppendLine($"}}");
    result.AppendLine($"else");
    result.AppendLine($"{{");
    result.AppendLine($"if ({objDistance} < {opDistance})");
    result.AppendLine($"{{");
    result.AppendLine($"{opDistance} = {objDistance};");
    result.AppendLine($"{opColour} = {objColour};");
    result.AppendLine($"}}");
    result.AppendLine($"}}");

    return result.ToString();
  }

  private static string RaymarchLightShaderCode(RaymarchLight rmLight)
  {
    string guid = rmLight.GUID.ToShaderSafeString();

    string position = $"_Position{guid}";
    string direction = $"_Direction{guid}";
    string colour = $"_Colour{guid}";
    string range = $"_Range{guid}";
    string intensity = $"_Intensity{guid}";
    string spotAngle = $"_SpotAngle{guid}";
    string innerSpotAngle = $"_InnerSpotAngle{guid}";
    string isActive = $"_IsActive{guid}";

    if (rmLight.LightMode == LightMode.Baked)
    {
      if (!rmLight.IsActive) return $"// Light{guid} (baked) is not active in scene";

      position = Util.ToShaderVector(rmLight.Position);
      direction = Util.ToShaderVector(rmLight.Direction);
      colour = Util.ToShaderVector(rmLight.Colour);
      range = rmLight.Range.ToString(CultureInfo.InvariantCulture);
      intensity = rmLight.Intensity.ToString(CultureInfo.InvariantCulture);
      spotAngle = rmLight.SpotAngle.ToString(CultureInfo.InvariantCulture);
      innerSpotAngle = rmLight.InnerSpotAngle.ToString(CultureInfo.InvariantCulture);
    }

    StringBuilder result = new StringBuilder();

    if (rmLight.LightMode != LightMode.Baked)
    {
      result.AppendLine($"if ({isActive} > 0)");
      result.AppendLine($"{{");
    }

    switch (rmLight.LightType)
    {
      case LightType.Directional:
        result.AppendLine($"light += GetDirectionalLight(pos, normal, {colour}, {direction}, {intensity});");
        break;
      case LightType.Point:
        result.AppendLine($"light += GetPointLight(pos, normal, {position}, {colour}, {range}, {intensity});");
        break;
      case LightType.Spot:
        result.AppendLine(
          $"light += GetSpotLight(pos, normal, {position}, {colour}, {direction}, {range}, {intensity}, {spotAngle}, {innerSpotAngle});");
        break;
      case LightType.Area:
      case LightType.Disc:
      default:
        Debug.Log($"{rmLight.LightType.ToString()} is currently not supported!");
        return string.Empty;
    }

    if (rmLight.LightMode != LightMode.Baked)
    {
      result.AppendLine($"}}");
    }

    return result.ToString();
  }

  private static string RaymarchSettingsShaderCode(RaymarchSettings raymarchSettings)
  {
    StringBuilder result = new StringBuilder();
    result.AppendLine("// Raymarch Settings");
    result.AppendLine(
      $"static const float _RenderDistance = {raymarchSettings.renderDistance.ToString(CultureInfo.InvariantCulture)};");
    result.AppendLine(
      $"static const float _HitResolution = {raymarchSettings.hitResolution.ToString(CultureInfo.InvariantCulture)};");
    result.AppendLine(
      $"static const float _Relaxation = {raymarchSettings.relaxation.ToString(CultureInfo.InvariantCulture)};");
    result.AppendLine($"static const int _MaxIterations = {raymarchSettings.maxIterations.ToString()};");

    return result.ToString();
  }

  private static string LightingSettingsShaderCode(LightingSettings lightingSettings)
  {
    StringBuilder result = new StringBuilder();
    result.AppendLine("// Lighting Settings");
    result.AppendLine($"static const float4 _AmbientColour = {Util.ToShaderVector(lightingSettings.ambientColour)};");
    result.AppendLine(
      $"static const float _ColourMultiplier = {lightingSettings.colourMultiplier.ToString(CultureInfo.InvariantCulture)};");

    return result.ToString();
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

  #region Util Shader

  private const string UtilShaderPath = "Assets/Raymarching/Shaders/Generated/";
  private const string UtilShaderExtension = "hlsl";

  public static void GenerateUtilShader<T>(string shaderName) where T : ShaderFeature
  {
    string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).FullName}", null);

    List<T> shaderFeatures = new List<T>(guids.Length);
    shaderFeatures.AddRange(guids.Select(guid =>
      AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))));

    StringBuilder functions = new StringBuilder();
    foreach (T shaderFeature in shaderFeatures)
    {
      functions.AppendLine(shaderFeature.FunctionPrototypeWithGuid);
      functions.AppendLine("{");
      functions.AppendLine(shaderFeature.FunctionBody);
      functions.AppendLine("}");
      functions.AppendLine();
    }

    string filePath = string.Concat(UtilShaderPath, shaderName, ".", UtilShaderExtension);
    string headerGuardName = string.Concat(shaderName.ToUpper().Replace(' ', '_'), "_", UtilShaderExtension.ToUpper());

    // NOTE(WSWhitehouse): Should use AssetDatabase functions here, but they don't work reliably
    Directory.CreateDirectory(UtilShaderPath);

    if (File.Exists(filePath))
    {
      File.Delete(filePath);
    }

    FileStream file = File.Open(filePath, FileMode.OpenOrCreate);

    file.Write(Encoding.ASCII.GetBytes(Util.AutoGeneratedComment()));
    file.Write(Encoding.ASCII.GetBytes(Util.HeaderGuardStart(headerGuardName)));

    file.Write(Encoding.ASCII.GetBytes(Util.UnityShaderIncludes()));
    file.Write(Encoding.ASCII.GetBytes(functions.ToString()));

    file.Write(Encoding.ASCII.GetBytes(Util.HeaderGuardEnd(headerGuardName)));

    file.Close();
    AssetDatabase.Refresh();

    var shader = AssetDatabase.LoadAssetAtPath<Object>(filePath);
    EditorUtility.SetDirty(shader);
  }

  #endregion Util Shader

  private static class Util
  {
    public static string AutoGeneratedComment()
    {
      string now = DateTime.Now.ToString(CultureInfo.InvariantCulture);

      StringBuilder result = new StringBuilder(650);
      result.AppendLine("//---------------------------------------------------------------------");
      result.AppendLine("//    This code was generated by a tool.");
      result.AppendLine("//");
      result.AppendLine("//    Changes to this file may cause incorrect behavior and will be ");
      result.AppendLine("//    lost if the code is regenerated.");
      result.AppendLine("//");
      result.AppendLine($"//    Time Generated: {now}");
      result.AppendLine("//---------------------------------------------------------------------");
      result.AppendLine();

      return result.ToString();
    }

    public static string UnityShaderIncludes()
    {
      StringBuilder result = new StringBuilder(200);
      result.AppendLine("// Unity Includes");

      result.AppendLine(
        "#include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl\"");
      result.AppendLine(
        "#include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl\"");

      result.AppendLine();

      return result.ToString();
    }

    public static string HeaderGuardStart(string name)
    {
      var nameUpper = name.ToUpper().Replace(' ', '_');

      StringBuilder result = new StringBuilder(16 + (nameUpper.Length * 2));
      result.AppendLine($"#ifndef {nameUpper}");
      result.AppendLine($"#define {nameUpper}");
      result.AppendLine();

      return result.ToString();
    }

    public static string HeaderGuardEnd(string name)
    {
      var nameUpper = name.ToUpper().Replace(' ', '_');
      return $"#endif // {nameUpper}";
    }

    public static string ToShaderVector(Vector2 vec2)
    {
      return
        $"float2({vec2.x.ToString(CultureInfo.InvariantCulture)}, {vec2.y.ToString(CultureInfo.InvariantCulture)})";
    }

    public static string ToShaderVector(Vector3 vec3)
    {
      return
        $"float3({vec3.x.ToString(CultureInfo.InvariantCulture)}, {vec3.y.ToString(CultureInfo.InvariantCulture)}, {vec3.z.ToString(CultureInfo.InvariantCulture)})";
    }

    public static string ToShaderVector(Vector4 vec4)
    {
      return
        $"float4({vec4.x.ToString(CultureInfo.InvariantCulture)}, {vec4.y.ToString(CultureInfo.InvariantCulture)}, {vec4.z.ToString(CultureInfo.InvariantCulture)}, {vec4.w.ToString(CultureInfo.InvariantCulture)})";
    }
  }
}

#endif