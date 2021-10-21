using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : RaymarchBase
{
  [SerializeField] public ShaderFeatureImpl<SDFShaderFeature> raymarchSDF;
  [SerializeField] public ShaderFeatureImpl<MaterialShaderFeature> raymarchMat;
  [SerializeField] public List<ShaderFeatureImpl<ModifierShaderFeature>> raymarchMods =
    new List<ShaderFeatureImpl<ModifierShaderFeature>>();

  public override void Awake()
  {
    raymarchSDF.OnAwake(GUID);
    raymarchMat.OnAwake(GUID);

    for (var i = 0; i < raymarchMods.Count; i++)
    {
      var mod = raymarchMods[i];
      mod.OnAwake(GUID, i.ToString());
    }

    base.Awake();
  }

  protected override void OnDestroy()
  {
    raymarchSDF.OnDestroy();
    raymarchMat.OnDestroy();

    foreach (var mod in raymarchMods)
    {
      mod.OnDestroy();
    }

    base.OnDestroy();
  }

  public override bool IsValid()
  {
    return raymarchSDF.IsValid();
  }

  [UploadToShader] public Vector3 Position => transform.position;
  [UploadToShader] public Vector3 Rotation => transform.eulerAngles * Mathf.Deg2Rad;
  [UploadToShader] public Vector3 Scale => transform.lossyScale * 0.5f;


  [SerializeField] private Color colour = Color.white;

  [UploadToShader]
  public Color Colour
  {
    get => colour;
    set => colour = value;
  }

  [SerializeField] private float marchingStepAmount = 1f;

  [UploadToShader]
  public float MarchingStepAmount
  {
    get => marchingStepAmount;
    set => marchingStepAmount = value;
  }

  protected override void UploadShaderData(Material material)
  {
    raymarchSDF.UploadShaderData(material);
    raymarchMat.UploadShaderData(material);

    foreach (var mod in raymarchMods)
    {
      mod.UploadShaderData(material);
    }

    base.UploadShaderData(material);
  }

#if UNITY_EDITOR
  public override string GetShaderCode_Variables()
  {
    var code = base.GetShaderCode_Variables();
    code = string.Concat(code, raymarchSDF.GetShaderVariables(GUID));
    code = string.Concat(code, raymarchMat.GetShaderVariables(GUID));
    code = raymarchMods.Aggregate(code, (current, mod) =>
      string.Concat(current, mod.GetShaderVariables(GUID)));

    return code;
  }

  public string GetShaderCode_CalcDistance()
  {
    string guid = GUID.ToShaderSafeString();

    string localDistName = $"distance{guid}";
    string localPosName = $"position{guid}";

    string marchingStepAmountName = $"_MarchingStepAmount{guid}";

    string result = string.Empty;

    for (var i = 0; i < raymarchMods.Count; i++)
    {
      var modifier = raymarchMods[i];
      if (modifier.ShaderFeature.ModifierType != ModifierType.PreSDF) continue;

      result =
        $"{result}{localPosName} = {modifier.ShaderFeature.FunctionNameWithGuid}({GetModifierParameters(i)});{ShaderGen.NewLine}";
    }

    result =
      $"{result}{localDistName} = {raymarchSDF.ShaderFeature.FunctionNameWithGuid}({GetShaderDistanceParameters()});{ShaderGen.NewLine}";

    for (var i = 0; i < raymarchMods.Count; i++)
    {
      var modifier = raymarchMods[i];
      if (modifier.ShaderFeature.ModifierType != ModifierType.PostSDF) continue;

      result =
        $"{result}{localDistName} = {modifier.ShaderFeature.FunctionNameWithGuid}({GetModifierParameters(i)});{ShaderGen.NewLine}";
    }

    result = $"{result}{localDistName} /= {marchingStepAmountName};{ShaderGen.NewLine}";

    return result;
  }

  private string GetShaderDistanceParameters()
  {
    string guid = GUID.ToShaderSafeString();

    string parameters = $"position{guid}, _Scale{guid}";
    for (int i = 0; i < raymarchSDF.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", raymarchSDF.GetShaderVariableName(i, GUID));
    }

    return parameters;
  }

  private string GetModifierParameters(int index)
  {
    var modifier = raymarchMods[index];

    string guid = GUID.ToShaderSafeString();
    string localDistName = $"distance{guid}";

    string parameters;

    switch (modifier.ShaderFeature.ModifierType)
    {
      case ModifierType.PreSDF:
        parameters = $"position{guid}, _Scale{guid}";
        break;
      case ModifierType.PostSDF:
        parameters = $"position{guid}, {localDistName}";
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    for (int i = 0; i < modifier.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", modifier.GetShaderVariableName(i, GUID));
    }

    return parameters;
  }

  public string GetShaderCode_Material()
  {
    string guid = GUID.ToShaderSafeString();

    if (raymarchMat.ShaderFeature == null)
    {
      return $"_Colour{guid}";
    }

    string parameters = $"position{guid}, _Colour{guid}";
    for (int i = 0; i < raymarchMat.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", raymarchMat.GetShaderVariableName(i, GUID));
    }

    return $"{raymarchMat.ShaderFeature.FunctionNameWithGuid}({parameters})";
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchObject))]
public class RaymarchObjectEditor : RaymarchBaseEditor
{
  private RaymarchObject Target => target as RaymarchObject;

  private static bool _materialDropDown = false;
  private static bool _modifierDropDown = false;

  protected override void DrawInspector()
  {
    EditorGUILayout.LabelField("Signed Distance Function", BoldLabelStyle);

    EditorGUI.BeginChangeCheck();
    Target.raymarchSDF.ShaderFeature =
      (SDFShaderFeature) EditorGUILayout.ObjectField(GUIContent.none, Target.raymarchSDF.ShaderFeature,
        typeof(SDFShaderFeature), false);
    if (EditorGUI.EndChangeCheck())
    {
      ShaderGen.GenerateRaymarchShader();
    }

    Target.raymarchSDF =
      ShaderFeatureImpl<SDFShaderFeature>.Editor.ShaderVariableField(new GUIContent("SDF Variables"),
        Target.raymarchSDF,
        Target);

    Target.MarchingStepAmount =
      EditorGUILayout.FloatField(new GUIContent("Marching Step Amount",
          "Increase this value to reduce visual glitches (especially useful when using modifiers). " +
          "However, increasing this value also reduces the performance - so it's a fine balance between what looks good and the performance."),
        Target.MarchingStepAmount);

    EditorGUILayout.Space();

    _materialDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_materialDropDown, new GUIContent("Material"));
    if (_materialDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      EditorGUILayout.HelpBox(
        "Materials describe how the Raymarch Object will look. Leave the following material field empty to just use a colour.",
        MessageType.Info, true);

      EditorGUI.BeginChangeCheck();
      Target.raymarchMat.ShaderFeature =
        (MaterialShaderFeature) EditorGUILayout.ObjectField(new GUIContent("Material"),
          Target.raymarchMat.ShaderFeature,
          typeof(MaterialShaderFeature), false);
      if (EditorGUI.EndChangeCheck())
      {
        ShaderGen.GenerateRaymarchShader();
      }

      Target.Colour = EditorGUILayout.ColorField(new GUIContent("Colour"), Target.Colour);

      Target.raymarchMat =
        ShaderFeatureImpl<MaterialShaderFeature>.Editor.ShaderVariableField(GUIContent.none, Target.raymarchMat,
          Target);

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();

    _modifierDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_modifierDropDown, new GUIContent("Modifiers"));
    if (_modifierDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      for (int i = 0; i < Target.raymarchMods.Count; i++)
      {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUI.BeginChangeCheck();
        Target.raymarchMods[i].ShaderFeature =
          (ModifierShaderFeature) EditorGUILayout.ObjectField(GUIContent.none, Target.raymarchMods[i].ShaderFeature,
            typeof(ModifierShaderFeature), false);

        if (EditorGUI.EndChangeCheck())
        {
          ShaderGen.GenerateRaymarchShader();
        }

        Target.raymarchMods[i] =
          ShaderFeatureImpl<ModifierShaderFeature>.Editor.ShaderVariableField(GUIContent.none,
            Target.raymarchMods[i], Target);

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.25f);

        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;

        Rect arrowButtonsRect = rect;
        arrowButtonsRect.width *= 0.5f;

        bool guiEnabledCache = GUI.enabled;
        if (i <= 0)
        {
          GUI.enabled = false;
        }

        if (GUI.Button(arrowButtonsRect, "▲"))
        {
          MoveRaymarchMods(i, i - 1);
        }

        GUI.enabled = guiEnabledCache;

        if (i >= Target.raymarchMods.Count - 1)
        {
          GUI.enabled = false;
        }

        arrowButtonsRect.x += arrowButtonsRect.width;

        if (GUI.Button(arrowButtonsRect, "▼"))
        {
          MoveRaymarchMods(i, i + 1);
        }

        GUI.enabled = guiEnabledCache;

        rect.x += rect.width;

        if (GUI.Button(rect, "Remove Modifier"))
        {
          Target.raymarchMods.RemoveAt(i);
          ShaderGen.GenerateRaymarchShader();
          break; // break out of loop so iter doesnt get messed up!
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
      }

      if (GUILayout.Button("Add New Modifier"))
      {
        Target.raymarchMods.Add(new ShaderFeatureImpl<ModifierShaderFeature>());
      }

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();
  }

  private void MoveRaymarchMods(int oldIndex, int newIndex)
  {
    var item = Target.raymarchMods[oldIndex];
    Target.raymarchMods.RemoveAt(oldIndex);
    Target.raymarchMods.Insert(newIndex, item);
    ShaderGen.GenerateRaymarchShader();
  }
}
#endif