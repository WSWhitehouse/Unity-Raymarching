using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Signed Distance Function")]
public class RaymarchSDF : ShaderFeature
{
  protected override string GetFunctionPrefix()
  {
    return "SDF";
  }

  protected override string GetReturnType()
  {
    return "float";
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
    ShaderGen.GenerateDistanceFunctionsShader();
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchSDF))]
public class RaymarchSDFEditor : ShaderFeatureEditor
{
  private RaymarchSDF Target => target as RaymarchSDF;

  protected override void DrawShaderFeatureInspector()
  {
    base.DrawShaderFeatureInspector();
  }
}
#endif