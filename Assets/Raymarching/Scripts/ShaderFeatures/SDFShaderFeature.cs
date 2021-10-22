using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Signed Distance Function (SDF)")]
public class SDFShaderFeature : ShaderFeature
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
      new ShaderVariable("pos", ShaderType.Vector3),
      new ShaderVariable("scale", ShaderType.Vector3)
    };
  }

#if UNITY_EDITOR
  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    ShaderGen.GenerateUtilShader<SDFShaderFeature>("SDFFunctions");
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SDFShaderFeature))]
public class SDFShaderFeatureEditor : ShaderFeatureEditor
{
  private SDFShaderFeature Target => target as SDFShaderFeature;

  protected override void DrawInspector()
  {
    base.DrawInspector();
  }
}
#endif