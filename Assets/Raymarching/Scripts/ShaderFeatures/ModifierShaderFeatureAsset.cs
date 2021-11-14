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
public class ModifierShaderFeatureAsset : ShaderFeatureAsset
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

  protected override ShaderType GetReturnType()
  {
    return ModifierType switch
    {
      ModifierType.PreSDF => ShaderType.Vector4,
      ModifierType.PostSDF => ShaderType.Float,
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  protected override ShaderVariable[] GetDefaultParameters()
  {
    return ModifierType switch
    {
      ModifierType.PreSDF => new ShaderVariable[]
      {
        new ShaderVariable("pos", ShaderType.Vector4)
      },
      ModifierType.PostSDF => new ShaderVariable[]
      {
        new ShaderVariable("pos", ShaderType.Vector4),
        new ShaderVariable("objDistance", ShaderType.Float)
      },
      _ => throw new ArgumentOutOfRangeException()
    };
  }

#if UNITY_EDITOR
  public override void SignalShaderFeatureUpdated()
  {
    base.SignalShaderFeatureUpdated();
    RaymarchShaderGen.GenerateUtilShader<ModifierShaderFeatureAsset>("ModifierFunctions");
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ModifierShaderFeatureAsset))]
public class ModifierShaderFeatureEditor : ShaderFeatureEditor
{
  private ModifierShaderFeatureAsset Target => target as ModifierShaderFeatureAsset;

  private readonly string[] modifierTypeStrings = Enum.GetNames(typeof(ModifierType));

  protected override void DrawInspector()
  {
    EditorGUI.BeginChangeCheck();
    Target.ModifierType = (ModifierType) GUILayout.Toolbar((int) Target.ModifierType, modifierTypeStrings);
    if (EditorGUI.EndChangeCheck())
    {
      Target.SignalShaderFeatureUpdated();
    }

    base.DrawInspector();
  }
}
#endif