using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchOperation : RaymarchBase
{
  public int StartIndex { get; set; }
  public int EndIndex { get; set; }

  [SerializeField] public ShaderFeatureImpl<OperationShaderFeature> operation;

  private struct ShaderIDs
  {
  }

  private ShaderIDs shaderIDs;

  private void InitShaderIDs()
  {
    string guid = GUID.ToShaderSafeString();

    // shaderIDs.Position = Shader.PropertyToID($"_Position{guid}");
  }

  public override void Awake()
  {
    operation.OnAwake(GUID);

    InitShaderIDs();
    base.Awake();
  }

  protected override void OnDestroy()
  {
    operation.OnDestroy();
    base.OnDestroy();
  }

  public override bool IsValid()
  {
    return operation.IsValid();
  }

  protected override void UploadShaderData(Material material)
  {
    operation.UploadShaderData(material);
  }
  
#if UNITY_EDITOR
  public override string GetShaderCode_Variables()
  {
    // string guid = GUID.ToShaderSafeString();

    var code = operation.GetShaderVariables(GUID);
    return code;
  }

  public string GetShaderCode_CalcOperation(string objDistance, string objColour)
  {
    string guid = GUID.ToShaderSafeString();
    string opDistance = $"distance{guid}";
    string opColour = $"colour{guid}";

    string parameters = $"{opDistance}, {opColour}, {objDistance}, {objColour}";
    for (int i = 0; i < operation.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", operation.GetShaderVariableName(i, GUID));
    }

    return
      $"{operation.ShaderFeature.FunctionNameWithGuid}({parameters});";
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchOperation))]
public class RaymarchOperationEditor : Editor
{
  private RaymarchOperation Target => target as RaymarchOperation;

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    EditorGUI.BeginChangeCheck();

    // DrawDefaultInspector();

    EditorGUI.BeginChangeCheck();
    Target.operation.ShaderFeature =
      (OperationShaderFeature) EditorGUILayout.ObjectField(GUIContent.none, Target.operation.ShaderFeature,
        typeof(OperationShaderFeature), false);
    if (EditorGUI.EndChangeCheck())
    {
      ShaderGen.GenerateRaymarchShader();
    }

    Target.operation =
      ShaderFeatureImpl<OperationShaderFeature>.Editor.ShaderVariableField(
        new GUIContent("Operation Variables"), Target.operation, Target);

    if (EditorGUI.EndChangeCheck())
    {
      EditorUtility.SetDirty(Target);
      ShaderGen.GenerateRaymarchShader();
    }

    serializedObject.ApplyModifiedProperties();
  }
}
#endif