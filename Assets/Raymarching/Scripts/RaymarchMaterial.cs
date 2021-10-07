using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Material")]
public class RaymarchMaterial : ShaderFeature
{
  protected override string GetFunctionPrefix()
  {
    return "Mat";
  }

  protected override string GetReturnType()
  {
    return "float4";
  }

  protected override string GetDefaultParameters()
  {
    return "float3 pos, float4 colour, float3 normal";
  }

  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    ShaderGen.GenerateMaterialFunctionsShader();
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchMaterial))]
public class RaymarchMaterialEditor : ShaderFeatureEditor
{
  private RaymarchMaterial Target => target as RaymarchMaterial;

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
  }
}
#endif