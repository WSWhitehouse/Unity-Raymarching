using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchOperation : RaymarchBase
{
  [SerializeField] public ShaderFeatureImpl<OperationShaderFeature> operation;

  public override void Awake()
  {
    operation.OnAwake(GUID);

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
    base.UploadShaderData(material);
  }

#if UNITY_EDITOR
  public int StartIndex { get; set; }
  public int EndIndex { get; set; }

  public override string GetShaderCode_Variables()
  {
    // string guid = GUID.ToShaderSafeString();

    var code = operation.GetShaderVariables(GUID);
    return string.Concat(code, base.GetShaderCode_Variables());
  }

  public string GetShaderCode_CalcOperation(string objDistance, string objColour)
  {
    string guid = GUID.ToShaderSafeString();
    string opDistance = $"distance{guid}";
    string opColour = $"colour{guid}";
    string opIsActive = $"_IsActive{guid}";

    string parameters = $"{opDistance}, {opColour}, {objDistance}, {objColour}";
    for (int i = 0; i < operation.ShaderVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", operation.GetShaderVariableName(i, GUID));
    }

    return $"if ({opIsActive} > 0){ShaderGen.NewLine}{{{ShaderGen.NewLine}" +
           $"{operation.ShaderFeature.FunctionNameWithGuid}({parameters});{ShaderGen.NewLine}" +
           $"}}{ShaderGen.NewLine}else{ShaderGen.NewLine}{{{ShaderGen.NewLine}" +
           $"if ({objDistance} < {opDistance}){ShaderGen.NewLine}" +
           $"{{ {ShaderGen.NewLine}" +
           $"{opDistance} = {objDistance};{ShaderGen.NewLine}" +
           $"{opColour} = {objColour};{ShaderGen.NewLine}" +
           $"}} {ShaderGen.NewLine}}}{ShaderGen.NewLine}";
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchOperation))]
public class RaymarchOperationEditor : RaymarchBaseEditor
{
  private RaymarchOperation Target => target as RaymarchOperation;

  protected override void DrawInspector()
  {
    EditorGUILayout.LabelField("Operation Function", BoldLabelStyle);
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
  }
}
#endif