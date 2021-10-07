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

  protected override string GetDefaultParameters()
  {
    return "float3 pos, float3 scale";
  }

  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    ShaderGen.GenerateDistanceFunctionsShader();
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchSDF))]
public class RaymarchSDFEditor : ShaderFeatureEditor
{
  private RaymarchSDF Target => target as RaymarchSDF;

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
  }
}
#endif