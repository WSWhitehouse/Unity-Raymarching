using System.Collections.Generic;
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

  protected override List<ShaderVariable> GetDefaultParameters()
  {
    var list = new List<ShaderVariable>
    {
      new ShaderVariable("pos", ShaderType.Vector3),
      new ShaderVariable("scale", ShaderType.Vector3)
    };

    return list;
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