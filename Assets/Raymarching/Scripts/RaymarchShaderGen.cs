#if UNITY_EDITOR

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class RaymarchShaderGen
{
  #region Scene Raymarch Shader

  private const string RaymarchTemplateShaderTitle = "RaymarchTemplateShader";

  private const string DebugSettings = "// DEBUG SETTINGS //";

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
    Debug.Log($"Generating Raymarch Shader in '{SceneManager.GetActiveScene().name}'");

    if (Application.isPlaying)
    {
      Debug.LogError($"{nameof(GenerateRaymarchShader)} called during runtime");
      return;
    }

    RaymarchScene rmScene = RaymarchScene.Get();

    // Raymarch Scene Sanity Checks
    if (rmScene == null)
    {
      Debug.LogError($"{nameof(RaymarchShaderGen)}: There is no {nameof(RaymarchScene)} in the active scene");
      return;
    }
    
    if (rmScene.templateShader == null)
    {
      Debug.LogError($"{nameof(RaymarchShaderGen)}: There is no template shader in the {nameof(RaymarchScene)}");
      return;
    }

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

        raymarchDistance.AppendLine($"// Operation Start {rmOperation.operation.ShaderFeatureAsset.FunctionName} {opGuid}");
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

        string localDistName = $"distance{guid}";
        string localPosName = $"position{guid}";

        raymarchDistance.AppendLine(
          $"float4 {localPosName} = (rayPos4D - {positionName}) / {scaleName};");
        raymarchDistance.AppendLine(
          $"{localPosName} = float4(Rotate3D({localPosName}.xyz, {rotationName}), {localPosName}.w);");

        // raymarchDistance.AppendLine($"if ({transform4DEnabledName} > 0)");
        // raymarchDistance.AppendLine("{");
        // raymarchDistance.AppendLine($"{localPosName} = Rotate4D({localPosName}, {rotation4DName});");
        // raymarchDistance.AppendLine("}");
        
        raymarchDistance.AppendLine($"int result{transform4DEnabledName} = {transform4DEnabledName} > 0;");
        raymarchDistance.AppendLine($"{localPosName} = Rotate4D({localPosName}, {rotation4DName}) * result{transform4DEnabledName} + " +
                                    $"{localPosName} * !result{transform4DEnabledName};");

        raymarchDistance.AppendLine();
        raymarchDistance.AppendLine($"float {localDistName} = _RenderDistance;");
        
        raymarchDistance.Append(RaymarchObjectDistanceShaderCode(rmObject));

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
          raymarchDistance.AppendLine(RaymarchMinObjectShaderCode(localDistName, RaymarchObjectMaterialShaderCode(rmObject)));
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
        raymarchDistance.AppendLine(
          j > 0
            ? RaymarchOperationShaderCode(operations[j - 1], $"distance{opGuid}", $"colour{opGuid}")
            : RaymarchMinObjectShaderCode($"distance{opGuid}", $"colour{opGuid}"));

        raymarchDistance.AppendLine();
      }

      operations.RemoveAll(x => x.EndIndex == i);
    }

    // Read template shader and replace placeholders
    string shader = File.ReadAllText(templatePath);

    // Raymarch Settings
    string raymarchSettings = RaymarchSettingsShaderCode(rmScene.RaymarchSettings);
    int raymarchSettingsStart = shader.IndexOf(RaymarchSettingsStart, StringComparison.Ordinal);
    int raymarchSettingsEnd = shader.IndexOf(RaymarchSettingsEnd, StringComparison.Ordinal) + RaymarchSettingsEnd.Length;

    shader = shader.Remove(raymarchSettingsStart, raymarchSettingsEnd - raymarchSettingsStart);
    shader = shader.Insert(raymarchSettingsStart, raymarchSettings);

    // Lighting Settings
    string lightingSettings = LightingSettingsShaderCode(rmScene.LightingSettings);
    int lightingSettingsStart = shader.IndexOf(LightingSettingsStart, StringComparison.Ordinal);
    int lightingSettingsEnd = shader.IndexOf(LightingSettingsEnd, StringComparison.Ordinal) + LightingSettingsEnd.Length;

    shader = shader.Remove(lightingSettingsStart, lightingSettingsEnd - lightingSettingsStart);
    shader = shader.Insert(lightingSettingsStart, lightingSettings);
    
    // Debug Settings
    string debugSettings = DebugSettingsShaderCode(rmScene.DebugSettings);
    shader = shader.Replace(DebugSettings, debugSettings);

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

    rmScene.RaymarchShader = AssetDatabase.LoadAssetAtPath<Shader>(filePath);

    if (rmScene.RaymarchShader != null)
    {
      EditorUtility.SetDirty(rmScene.RaymarchShader);
    }
    else
    {
      Debug.LogError("Generated shader is null!");
    }

    // NOTE(WSWhitehouse): Enable all objects after generating shader.
    ForceRenderScene();
  }

  private static string RaymarchMinObjectShaderCode(string distance, string colour)
  {
    StringBuilder result = new StringBuilder();
    // result.AppendLine($"if ({distance} < resultDistance)");
    // result.AppendLine($"{{");
    // result.AppendLine($"resultDistance = {distance};");
    // result.AppendLine($"resultColour   = {colour};");
    // result.AppendLine($"}}");
                                                   
    result.AppendLine($"int result{distance} = {distance} < resultDistance;");
    result.AppendLine($"resultDistance = ({distance} * result{distance}) + (resultDistance * !result{distance});");
    result.AppendLine($"resultColour   = ({colour} * result{distance}) + (resultColour * !result{distance});");

    return result.ToString();
  }

  private static string RaymarchObjectDistanceShaderCode(RaymarchObject rmObject)
  {
    string guid = rmObject.GUID.ToShaderSafeString();

    string localDistName = $"distance{guid}";
    string localPosName = $"position{guid}";
    string scaleName = $"_{nameof(rmObject.Scale)}{guid}";
    string isActiveName = $"_IsActive{guid}";

    string marchingStepAmountName = $"_{nameof(rmObject.MarchingStepAmount)}{guid}";

    StringBuilder result = new StringBuilder();
    
    result.AppendLine(RaymarchObjectModifiersShaderCode(rmObject, ModifierType.PreSDF));

    // NOTE(WSWhitehouse): Signed Distance Field
    string parameters = localPosName;
    for (int i = 0; i < rmObject.raymarchSDF.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", rmObject.raymarchSDF.GetShaderVariableName(i, rmObject.GUID));
    }

    result.AppendLine(
      $"{localDistName} = {rmObject.raymarchSDF.ShaderFeatureAsset.FunctionNameWithGuid}({parameters}) * {scaleName};");
    result.AppendLine(RaymarchObjectModifiersShaderCode(rmObject, ModifierType.PostSDF));
    result.AppendLine($"{localDistName} /= {marchingStepAmountName};");

    result.AppendLine($"{localDistName} = ({localDistName} * ({isActiveName})) + (_RenderDistance * !({isActiveName}));");
    

    return result.ToString();
  }
  
  private static string RaymarchObjectModifiersShaderCode(RaymarchObject rmObject, ModifierType type)
  {
    string guid = rmObject.GUID.ToShaderSafeString();
    string localDistName = $"distance{guid}";
    string localPosName = $"position{guid}";

    string defaultModifierParams = type switch
    {
      ModifierType.PreSDF => localPosName,
      ModifierType.PostSDF => $"{localPosName}, {localDistName}",
      _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    string modifierOutputVar = type switch
    {
      ModifierType.PreSDF => localPosName,
      ModifierType.PostSDF => localDistName,
      _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
    
    StringBuilder result = new StringBuilder();

    foreach (ToggleableShaderFeature<ModifierShaderFeatureAsset> modifier in rmObject.raymarchMods)
    {
      if (modifier.ShaderFeatureAsset.ModifierType != type) continue;

      string modifierParams = defaultModifierParams;
      for (int i = 0; i < modifier.ShaderVariables.Count; i++)
      {
        ShaderVariable variable = modifier.GetShaderVariable(i);
        
        if (modifier.hardcodedShaderFeature && variable.ShaderType != ShaderType.Texture2D)
        {
          modifierParams = string.Concat(modifierParams, ", ", variable.ValueToShaderString());
        }
        else
        {
          modifierParams = string.Concat(modifierParams, ", ", modifier.GetShaderVariableName(i, rmObject.GUID));
        }
      }

      if (!modifier.hardcodedShaderFeature)
      {
        var modifierName = modifier.GetIsEnabledShaderName(rmObject.GUID);
        result.AppendLine($"{modifierOutputVar} = ({modifier.ShaderFeatureAsset.FunctionNameWithGuid}({modifierParams}) * ({modifierName})) + " +
                          $"({modifierOutputVar} * !({modifierName}));");
      }
      else
      {
        result.AppendLine($"{modifierOutputVar} = {modifier.ShaderFeatureAsset.FunctionNameWithGuid}({modifierParams});");
      }
    }

    return result.ToString();
  }

  private static string RaymarchObjectMaterialShaderCode(RaymarchObject rmObject)
  {
    string guid = rmObject.GUID.ToShaderSafeString();

    if (rmObject.raymarchMat.ShaderFeatureAsset == null)
    {
      return $"_{nameof(rmObject.Colour)}{guid}";
    }

    string parameters = $"position{guid}, _{nameof(rmObject.Colour)}{guid}";
    for (int i = 0; i < rmObject.raymarchMat.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", rmObject.raymarchMat.GetShaderVariableName(i, rmObject.GUID));
    }

    return $"{rmObject.raymarchMat.ShaderFeatureAsset.FunctionNameWithGuid}({parameters})";
  }

  private static string RaymarchOperationShaderCode(RaymarchOperation rmOperation, string objDistance, string objColour)
  {
    string guid = rmOperation.GUID.ToShaderSafeString();
    StringBuilder result = new StringBuilder();

    string opDistance = $"distance{guid}";
    string opColour = $"colour{guid}";
    string opIsActive = $"_IsActive{guid}";

    string distanceBranchlessTestIf = String.Concat("distIf_", GUID.Generate().ToString());
    string colourBranchlessTestIf = String.Concat("colIf_", GUID.Generate().ToString());
    
    string distanceBranchlessTestElse = String.Concat("distanceElse_", GUID.Generate().ToString());
    string colourBranchlessTestElse = String.Concat("colElse_", GUID.Generate().ToString());
    
    string ifResultVarName = $"if_result_{GUID.Generate().ToString()}";
    string elseResultVarName = $"else_result_{GUID.Generate().ToString()}";

    // result.AppendLine($"if ({opIsActive} > 0)");
    // result.AppendLine($"{{");
    // result.AppendLine($"{rmOperation.operation.ShaderFeatureAsset.FunctionNameWithGuid}({parameters});");
    // result.AppendLine($"}}");
    // result.AppendLine($"else");
    // result.AppendLine($"{{");
    // result.AppendLine($"if ({objDistance} < {opDistance})");
    // result.AppendLine($"{{");
    // result.AppendLine($"{opDistance} = {objDistance};");
    // result.AppendLine($"{opColour} = {objColour};");
    // result.AppendLine($"}}");
    // result.AppendLine($"}}");
    
    result.AppendLine($"float {distanceBranchlessTestIf} = {opDistance};");
    result.AppendLine($"float4 {colourBranchlessTestIf} = {opColour};");
    result.AppendLine($"float {distanceBranchlessTestElse} = {opDistance};");
    result.AppendLine($"float4 {colourBranchlessTestElse} = {opColour};");

    string parameters = $"{distanceBranchlessTestIf}, {colourBranchlessTestIf}, {objDistance}, {objColour}";
    for (int i = 0; i < rmOperation.operation.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", rmOperation.operation.GetShaderVariableName(i, rmOperation.GUID));
    }

    result.AppendLine($"{rmOperation.operation.ShaderFeatureAsset.FunctionNameWithGuid}({parameters});");
    
    result.AppendLine($"int {elseResultVarName} = {objDistance} < {opDistance};");
    result.AppendLine($"{distanceBranchlessTestElse} = ({objDistance} * {elseResultVarName}) + ({distanceBranchlessTestElse} * !{elseResultVarName});");
    result.AppendLine($"{colourBranchlessTestElse}   = ({objColour} * {elseResultVarName}) + ({colourBranchlessTestElse} * !{elseResultVarName});");
    
    result.AppendLine($"int {ifResultVarName} = {opIsActive} > 0;");
    result.AppendLine($"{opDistance} = ({distanceBranchlessTestIf} * {ifResultVarName}) + ({distanceBranchlessTestElse} * !{ifResultVarName});");
    result.AppendLine($"{opColour}   = ({colourBranchlessTestIf} * {ifResultVarName}) + ({colourBranchlessTestElse} * !{ifResultVarName});");

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
    
    string lightFunction = rmLight.LightType switch
    {
      LightType.Directional => $"GetDirectionalLight(pos, normal, {colour}, {direction}, {intensity})",
      LightType.Spot        => $"GetSpotLight(pos, normal, {position}, {colour}, {direction}, {range}, {intensity}, {spotAngle}, {innerSpotAngle})",
      LightType.Point       => $"GetPointLight(pos, normal, {position}, {colour}, {range}, {intensity})",
      LightType.Area        => "0",
      LightType.Disc        => "0",
      
      _ => throw new ArgumentOutOfRangeException()
    };
    
    return rmLight.LightMode == LightMode.Baked
      ? $"light += {lightFunction};"
      : $"light += {lightFunction} * {isActive};";
  }

  private static string RaymarchSettingsShaderCode(RaymarchSettings raymarchSettings)
  {
    StringBuilder result = new StringBuilder();
    result.AppendLine("// Raymarch Settings");
    result.AppendLine($"#define _RenderDistance {raymarchSettings.renderDistance.ToString(CultureInfo.InvariantCulture)}");
    result.AppendLine($"#define _HitResolution {raymarchSettings.hitResolution.ToString(CultureInfo.InvariantCulture)}");
    result.AppendLine($"#define _Relaxation {raymarchSettings.relaxation.ToString(CultureInfo.InvariantCulture)}");
    result.AppendLine($"#define _MaxIterations {raymarchSettings.maxIterations.ToString()}");

    return result.ToString();
  }

  private static string LightingSettingsShaderCode(LightingSettings lightingSettings)
  {
    StringBuilder result = new StringBuilder();
    result.AppendLine("// Lighting Settings");
    result.AppendLine($"#define _AmbientColour {Util.ToShaderVector(lightingSettings.ambientColour)}");
    result.AppendLine($"#define _ColourMultiplier {lightingSettings.colourMultiplier.ToString(CultureInfo.InvariantCulture)}");
    
    if (lightingSettings.aoEnabled)
    {
      result.AppendLine($"#define _AOStepSize {lightingSettings.aoStepSize.ToString(CultureInfo.InvariantCulture)}");
      result.AppendLine($"#define _AOIntensity {lightingSettings.aoIntensity.ToString(CultureInfo.InvariantCulture)}");
      result.AppendLine($"#define _AOIterations {lightingSettings.aoIterations.ToString()}");
    }
    else
    {
      result.AppendLine($"#define _AOStepSize 0.0");
      result.AppendLine($"#define _AOIntensity 0.0");
      result.AppendLine($"#define _AOIterations 0");
    }
    return result.ToString();
  }

  private static string DebugSettingsShaderCode(DebugSettings debugSettings)
  {
    StringBuilder result = new StringBuilder();
    result.AppendLine("// Debug Settings");
    if (debugSettings.enableDebugSymbols) result.AppendLine("#pragma enable_d3d11_debug_symbols");
    
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

  public static void GenerateUtilShader<T>(string shaderName) where T : ShaderFeatureAsset
  {
    List<T> shaderFeatures = global::Util.Editor.FindAllAssetsOfType<T>();

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
  
  public static void ForceRenderScene()
  {
    Debug.Log($"{nameof(RaymarchShaderGen)}: Force Rendering Scene ({SceneManager.GetActiveScene().name})");
    
    if (RaymarchScene.Get() == null)
    {
      Debug.LogError($"{nameof(RaymarchShaderGen)}: You don't have an active {nameof(RaymarchScene)} in the scene ({SceneManager.GetActiveScene().name})");
      return;
    }
    
    var rmBases = Object.FindObjectsOfType<RaymarchBase>();
    foreach (var rmBase in rmBases)
    {
      rmBase.Awake();
    }
    
    var rmCameras = Object.FindObjectsOfType<RaymarchCamera>();
    foreach (var rmCamera in rmCameras)
    {
      rmCamera.Awake();
    }
  }

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