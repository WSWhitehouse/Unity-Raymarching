using System.Collections.Generic;
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

  protected override List<ShaderVariable> GetDefaultParameters()
  {
    var list = new List<ShaderVariable>
    {
      new ShaderVariable("pos", ShaderType.Vector3),
      new ShaderVariable("colour", ShaderType.Vector4),
      new ShaderVariable("normal", ShaderType.Vector3)
    };
    
    return list;
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