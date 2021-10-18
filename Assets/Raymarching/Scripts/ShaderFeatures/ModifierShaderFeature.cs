using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ModifierType
{
  PreSDF = 0,
  PostSDF
}

[CreateAssetMenu(menuName = "Raymarching/Modifier")]
public class ModifierShaderFeature : ShaderFeature
{
  [SerializeField] private ModifierType modifierType;

  public ModifierType ModifierType
  {
    get => modifierType;
    set => modifierType = value;
  }

  protected override string GetFunctionPrefix()
  {
    return "Mod";
  }

  protected override string GetReturnType()
  {
    return ModifierType switch
    {
      ModifierType.PreSDF => "float3",
      ModifierType.PostSDF => "float",
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  protected override ShaderVariable[] GetDefaultParameters()
  {
    return ModifierType switch
    {
      ModifierType.PreSDF => new ShaderVariable[]
      {
        new ShaderVariable("pos", ShaderType.Vector3),
        new ShaderVariable("scale", ShaderType.Vector3)
      },
      ModifierType.PostSDF => new ShaderVariable[]
      {
        new ShaderVariable("pos", ShaderType.Vector3),
        new ShaderVariable("objDistance", ShaderType.Float)
      },
      _ => throw new ArgumentOutOfRangeException()
    };
  }

#if UNITY_EDITOR
  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    ShaderGen.GenerateUtilShader<ModifierShaderFeature>("ModifierFunctions");
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ModifierShaderFeature))]
public class ModifierShaderFeatureEditor : ShaderFeatureEditor
{
  private ModifierShaderFeature Target => target as ModifierShaderFeature;

  private readonly string[] modifierTypeStrings = Enum.GetNames(typeof(ModifierType));

  protected override void DrawShaderFeatureInspector()
  {
    EditorGUI.BeginChangeCheck();
    Target.ModifierType = (ModifierType) GUILayout.Toolbar((int) Target.ModifierType, modifierTypeStrings);
    if (EditorGUI.EndChangeCheck())
    {
      Target.SignalShaderFeatureUpdated();
    }

    base.DrawShaderFeatureInspector();
  }
}
#endif