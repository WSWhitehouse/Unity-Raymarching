using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
#endif


[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchOperation : RaymarchBase
{
  [SerializeField] public ShaderFeature<OperationShaderFeatureAsset> operation;

  public override void Awake()
  {
    operation.Awake(GUID);
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

#if UNITY_EDITOR
/*
 * NOTE(WSWhitehouse):
 * The index values are used to keep track of operations during shader generation. They should not be
 * used during runtime.
 */
  public int StartIndex { get; set; }
  public int EndIndex { get; set; }

  protected override string GetShaderVariablesImpl()
  {
    return operation.GetShaderVariables(GUID);
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchOperation)), CanEditMultipleObjects]
public class RaymarchOperationEditor : RaymarchBaseEditor
{
  private RaymarchOperation Target => target as RaymarchOperation;

  protected override void DrawInspector()
  {
    EditorGUILayout.LabelField("Operation Function", BoldLabelStyle);
    EditorGUI.BeginChangeCheck();
    Target.operation.ShaderFeatureAsset =
      (OperationShaderFeatureAsset) EditorGUILayout.ObjectField(GUIContent.none, Target.operation.ShaderFeatureAsset,
        typeof(OperationShaderFeatureAsset), false);
    if (EditorGUI.EndChangeCheck())
    {
      RaymarchShaderGen.GenerateRaymarchShader();
    }

    Target.operation =
      ShaderFeature<OperationShaderFeatureAsset>.Editor.ShaderVariableField(
        new GUIContent("Operation Variables"), Target.operation);
  }
}
#endif