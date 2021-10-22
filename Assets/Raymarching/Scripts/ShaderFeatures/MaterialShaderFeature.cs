using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Material")]
public class MaterialShaderFeature : ShaderFeature
{
  protected override string GetFunctionPrefix()
  {
    return "Mat";
  }

  protected override ShaderType GetReturnType()
  {
    return ShaderType.Vector4;
  }

  protected override ShaderVariable[] GetDefaultParameters()
  {
    return new ShaderVariable[]
    {
      new ShaderVariable("pos", ShaderType.Vector3),
      new ShaderVariable("colour", ShaderType.Vector4),
      // new ShaderVariable("normal", ShaderType.Vector3)
    };
  }

#if UNITY_EDITOR
  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    ShaderGen.GenerateUtilShader<MaterialShaderFeature>("MaterialFunctions");
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(MaterialShaderFeature))]
public class MaterialShaderFeatureEditor : ShaderFeatureEditor
{
  private MaterialShaderFeature Target => target as MaterialShaderFeature;
  
  protected override void DrawInspector()
  {
    base.DrawInspector();
  }
}
#endif