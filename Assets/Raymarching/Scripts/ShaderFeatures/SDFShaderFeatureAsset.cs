using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Signed Distance Function (SDF)")]
public class SDFShaderFeatureAsset : ShaderFeatureAsset
{
  protected override string GetFunctionPrefix()
  {
    return "SDF";
  }

  protected override ShaderType GetReturnType()
  {
    return ShaderType.Float;
  }

  protected override ShaderVariable[] GetDefaultParameters()
  {
    return new ShaderVariable[]
    {
      new ShaderVariable("pos", ShaderType.Vector4)
    };
  }

#if UNITY_EDITOR
  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    RaymarchShaderGen.GenerateUtilShader<SDFShaderFeatureAsset>("SDFFunctions");
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SDFShaderFeatureAsset))]
public class SDFShaderFeatureEditor : ShaderFeatureEditor
{
  private SDFShaderFeatureAsset Target => target as SDFShaderFeatureAsset;

  protected override void DrawInspector()
  {
    base.DrawInspector();
  }
}
#endif