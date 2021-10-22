using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Operation")]
public class OperationShaderFeature : ShaderFeature
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
    ShaderGen.GenerateUtilShader<OperationShaderFeature>("OperationFunctions");
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(OperationShaderFeature))]
public class OperationShaderFeatureEditor : ShaderFeatureEditor
{
  private MaterialShaderFeature Target => target as MaterialShaderFeature;
  
  protected override void DrawInspector()
  {
    base.DrawInspector();
  }
}
#endif