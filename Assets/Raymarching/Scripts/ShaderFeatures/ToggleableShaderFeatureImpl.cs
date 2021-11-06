using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Text;
#endif

/*
 * NOTE(WSWhitehouse):
 * This is an extension class of ShaderFeatureImpl<> to allow the shader feature to be
 * toggled on and off. It generates an enabled boolean that gets sent to the shader.
 * The script directly inherits from ShaderFeatureImpl<>.
 *
 * The beautiful few lines of code below me says this script inherits from ShaderFeatureImpl<>
 * and 'T' must be of type ShaderFeature. Sometimes C# can be truly disgusting to look at :(
 */
[Serializable]
public sealed class ToggleableShaderFeatureImpl<T> : ShaderFeatureImpl<T> where T : ShaderFeature
{
  [SerializeField] private bool isEnabled = true;

  public bool IsEnabled
  {
    get => isEnabled;
    set => isEnabled = value;
  }

  private int isEnabledShaderID;

  protected override void InitShaderIDs(SerializableGuid guid)
  {
    base.InitShaderIDs(guid);
    isEnabledShaderID = Shader.PropertyToID($"_IsEnabled{guid.ToShaderSafeString()}{postfix}");
  }

  protected override void UploadShaderData(Material material)
  {
    if (ShaderFeature == null) return;
    base.UploadShaderData(material);
    material.SetInteger(isEnabledShaderID, IsEnabled ? 1 : 0);
  }
  
#if UNITY_EDITOR
  
  [SerializeField] public bool EDITOR_ToggleHarcodedModifier = false;

  public string GetIsEnabledShaderName(SerializableGuid guid)
  {
   return $"_IsEnabled{guid.ToShaderSafeString()}{postfix}";
  }
  
  public override string GetShaderVariables(SerializableGuid guid)
  {
    if (ShaderFeature == null) return string.Empty;

    StringBuilder result = new StringBuilder();
    result.AppendLine($"uniform int _IsEnabled{guid.ToShaderSafeString()}{postfix};");

    return string.Concat(base.GetShaderVariables(guid), result.ToString());
  }
#endif
}