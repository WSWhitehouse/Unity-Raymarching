using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : RaymarchBase
{
  [SerializeField] public ShaderFeatureImpl<RaymarchSDF> raymarchSDF;
  [SerializeField] public ShaderFeatureImpl<RaymarchMaterial> raymarchMat;

  public override void Awake()
  {
    raymarchSDF.OnAwake(GUID);
    raymarchMat.OnAwake(GUID);

    InitShaderIDs();
    base.Awake();
  }

  protected override void OnDestroy()
  {
    raymarchSDF.OnDestroy();
    raymarchMat.OnDestroy();

    base.OnDestroy();
  }

  private struct ShaderIDs
  {
    public int Position;
    public int Rotation;
    public int Scale;
    public int Colour;
  }

  private ShaderIDs shaderIDs;

  private void InitShaderIDs()
  {
    string guid = GUID.ToShaderSafeString();

    shaderIDs.Position = Shader.PropertyToID($"_Position{guid}");
    shaderIDs.Rotation = Shader.PropertyToID($"_Rotation{guid}");
    shaderIDs.Scale = Shader.PropertyToID($"_Scale{guid}");
    shaderIDs.Colour = Shader.PropertyToID($"_Colour{guid}");
  }

  public override bool IsValid()
  {
    return raymarchSDF.ShaderFeature != null;
  }

  public Vector3 Position => transform.position;

  private Vector3 Rotation =>
    new(transform.eulerAngles.x * Mathf.Deg2Rad,
      transform.eulerAngles.y * Mathf.Deg2Rad,
      transform.eulerAngles.z * Mathf.Deg2Rad);

  public Vector3 Scale => transform.lossyScale * 0.5f;

  [SerializeField] private Color colour = Color.white;

  public Color Colour
  {
    get => colour;
    set => DirtyFlag.SetField(ref colour, value);
  }

#if UNITY_EDITOR
  // Shader
  public override string GetShaderCode_Variables()
  {
    string guid = GUID.ToShaderSafeString();

    var code = $"uniform float3 _Position{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float3 _Rotation{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float3 _Scale{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float4 _Colour{guid};{ShaderGen.NewLine}";

    code = string.Concat(code, raymarchSDF.GetShaderVariables(GUID));
    code = string.Concat(code, raymarchMat.GetShaderVariables(GUID));

    return code;
  }

  public string GetShaderCode_CalcDistance()
  {
    return $"{raymarchSDF.ShaderFeature.FunctionNameWithGuid}({GetShaderDistanceParameters()});";
  }

  private string GetShaderDistanceParameters()
  {
    string guid = GUID.ToShaderSafeString();

    string parameters = $"position{guid}, _Scale{guid}";
    for (int i = 0; i < raymarchSDF.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", raymarchSDF.ShaderVariables[i].GetShaderName(GUID));
    }

    return parameters;
  }

  public string GetShaderCode_Material()
  {
    string guid = GUID.ToShaderSafeString();

    if (raymarchMat.ShaderFeature == null)
    {
      return $"resultColour = _Colour{guid}.xyz;{ShaderGen.NewLine}";
    }

    string parameters = $"position{guid}, _Colour{guid}";
    for (int i = 0; i < raymarchMat.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", raymarchMat.ShaderVariables[i].GetShaderName(GUID));
    }

    return $"resultColour = {raymarchMat.ShaderFeature.FunctionNameWithGuid}({parameters});";
  }

#endif

  protected override void UploadShaderData(Material material)
  {
    material.SetVector(shaderIDs.Position, Position);
    material.SetVector(shaderIDs.Rotation, Rotation);
    material.SetVector(shaderIDs.Scale, Scale);
    material.SetVector(shaderIDs.Colour, Colour);

    raymarchSDF.UploadShaderData(material);
    raymarchMat.UploadShaderData(material);
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchObject))]
public class RaymarchObjectEditor : Editor
{
  private RaymarchObject Target => target as RaymarchObject;

  private static bool _materialDropDown = false;

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    EditorGUI.BeginChangeCheck();

    GUIStyle boldStyle = new GUIStyle(GUI.skin.GetStyle("label"))
    {
      fontStyle = FontStyle.Bold
    };

    EditorGUILayout.LabelField("Signed Distance Function", boldStyle);
    Target.raymarchSDF =
      ShaderFeatureImpl<RaymarchSDF>.Editor.ShaderFeatureField(GUIContent.none, Target.raymarchSDF, Target);
    Target.raymarchSDF =
      ShaderFeatureImpl<RaymarchSDF>.Editor.ShaderVariableField(new GUIContent("SDF Variables"), Target.raymarchSDF,
        Target);

    EditorGUILayout.Space();

    _materialDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_materialDropDown, new GUIContent("Material"));
    if (_materialDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);
      
      EditorGUILayout.HelpBox(
        "Materials describe how the Raymarch Object will look. Leave the following material field empty to just use a colour.",
        MessageType.Info, true);

      Target.raymarchMat =
        ShaderFeatureImpl<RaymarchMaterial>.Editor.ShaderFeatureField(new GUIContent("Material"),
          Target.raymarchMat, Target);

      Target.Colour = EditorGUILayout.ColorField(new GUIContent("Colour"), Target.Colour);

      Target.raymarchMat =
        ShaderFeatureImpl<RaymarchMaterial>.Editor.ShaderVariableField(GUIContent.none, Target.raymarchMat, Target);
      
      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();

    if (EditorGUI.EndChangeCheck())
    {
      Target.DirtyFlag.SetDirty();
      EditorUtility.SetDirty(Target);
    }

    serializedObject.ApplyModifiedProperties();
  }
}
#endif