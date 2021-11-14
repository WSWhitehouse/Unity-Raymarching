using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Operation")]
public class OperationShaderFeatureAsset : ShaderFeatureAsset
{
  protected override string GetFunctionPrefix()
  {
    return "Oper";
  }

  protected override ShaderType GetReturnType()
  {
    return ShaderType.Void;
  }

  protected override ShaderVariable[] GetDefaultParameters()
  {
    return new ShaderVariable[]
    {
      new ShaderVariable("resultDistance", ShaderType.Float, ParameterType.InOut),
      new ShaderVariable("resultColour", ShaderType.Vector4, ParameterType.InOut),
      new ShaderVariable("objDistance", ShaderType.Float),
      new ShaderVariable("objColour", ShaderType.Vector4)
    };
  }

#if UNITY_EDITOR
  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    RaymarchShaderGen.GenerateUtilShader<OperationShaderFeatureAsset>("OperationFunctions");
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(OperationShaderFeatureAsset))]
public class OperationShaderFeatureEditor : ShaderFeatureEditor
{
  private MaterialShaderFeatureAsset Target => target as MaterialShaderFeatureAsset;
  
  protected override void DrawInspector()
  {
    base.DrawInspector();
  }
}
#endif