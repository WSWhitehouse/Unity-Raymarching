﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
#endif

/*
 * NOTE(WSWhitehouse):
 * This script is used to implement Shader Features Assets on scene objects. It creates local copies
 * of variables so they can be updated on a per-object basis by responding to the
 * OnShaderVariablesChanged event in the Shader Feature Asset. It also handles getting shader
 * IDs and uploading the local Shader Feature Asset variables to the shader. Also includes an editor
 * class for useful OnInspectorGUI functions.
 */

[Serializable]
public class ShaderFeature<T> where T : ShaderFeatureAsset
{
  #region Shader Feature

  [SerializeField, FormerlySerializedAs("shaderFeature")] 
  private T shaderFeatureAsset;

  public T ShaderFeatureAsset
  {
    get => shaderFeatureAsset;
#if UNITY_EDITOR
    // NOTE(WSWhitehouse): Cannot change the shader feature asset during runtime, for editor only
    set
    {
      if (Application.isPlaying) return;
      if (EqualityComparer<T>.Default.Equals(shaderFeatureAsset, value)) return;

      if (shaderFeatureAsset != null)
      {
        shaderFeatureAsset.OnShaderVariablesChanged -= OnShaderVariablesChanged;
      }

      if (value == null)
      {
        shaderFeatureAsset = null;
        shaderVariables.Clear();
      }
      else
      {
        shaderFeatureAsset = value;
        OnShaderVariablesChanged();
        shaderFeatureAsset.OnShaderVariablesChanged += OnShaderVariablesChanged;
      }
    }
#endif
  }

  #endregion Shader Feature

  #region Shader Variables

  [SerializeField] private List<ShaderVariable> shaderVariables = new List<ShaderVariable>();

  public List<ShaderVariable> ShaderVariables => shaderVariables;

  public string GetShaderVariableName(int index, SerializableGuid guid)
  {
    return $"{ShaderVariables[index].GetShaderName(guid)}{postfix}";
  }

  /// <summary>
  /// Get index of variable from its name.
  /// </summary>
  /// <param name="name">name of shader variable</param>
  /// <returns>index of shader variable in <see cref="ShaderVariables"/> list. Returns -1 if variable cannot be found</returns>
  public int PropertyToID(string name)
  {
    return ShaderVariables.FindIndex(x => x.Name == name);
  }

  /// <summary>
  /// Returns the shader variable at the ID
  /// </summary>
  /// <param name="id">ID of shader variable - use <see cref="PropertyToID"/> to get ID</param>
  /// <seealso cref="PropertyToID"/>
  public ShaderVariable GetShaderVariable(int id)
  {
    return ShaderVariables[id];
  }

  /// <summary>
  /// Returns the shader variable with the given name - this performs no error checking, meaning if the variable
  /// cannot be found an exception could be thrown. Use <see cref="GetShaderVariable(int)"/> instead.
  /// </summary>
  /// <param name="name">Name of shader variable</param>
  public ShaderVariable GetShaderVariable(string name)
  {
    return ShaderVariables[PropertyToID(name)];
  }

#if UNITY_EDITOR
  private void OnShaderVariablesChanged()
  {
    int count = ShaderFeatureAsset.ShaderVariables.Count;

    var newVariables = new List<ShaderVariable>(count);

    for (int i = 0; i < count; i++)
    {
      int index = ShaderVariables.FindIndex(x =>
        x.Name == ShaderFeatureAsset.ShaderVariables[i].Name);

      if (index < 0) // variable not found
      {
        newVariables.Add(ShaderFeatureAsset.ShaderVariables[i]);
        continue;
      }

      newVariables.Add(shaderVariables[index].ShaderType != ShaderFeatureAsset.ShaderVariables[i].ShaderType
        ? ShaderFeatureAsset.ShaderVariables[i]
        : shaderVariables[index]);
    }

    shaderVariables = newVariables;
  }
#endif

  #endregion Shader Variables

  #region Shader IDs

  private int[] _shaderIDs;

#if UNITY_EDITOR
  // NOTE(WSWhitehouse): Storing the guid of the object that is initialising the ShaderFeature, just in case
  // the variables get updated and the IDs need to be regenerated - this only happens in the editor
  private SerializableGuid guid;
#endif

  protected virtual void InitShaderIDs(SerializableGuid guid)
  {
#if UNITY_EDITOR
    this.guid = guid;
#endif

    _shaderIDs = new int[ShaderVariables.Count];
    for (int i = 0; i < _shaderIDs.Length; i++)
    {
      _shaderIDs[i] = Shader.PropertyToID(GetShaderVariableName(i, guid));
    }
  }

  #endregion Shader IDs

  protected string postfix = string.Empty;

  /// <param name="postfix">
  /// This string will be appended to the names of any shader variables. Used to
  /// distinguish between multiple of the same shader feature (i.e. a list/array of shader features)
  /// </param>
  public void Awake(SerializableGuid guid, string postfix = "")
  {
#if UNITY_EDITOR
    if (ShaderFeatureAsset != null)
    {
      ShaderFeatureAsset.OnShaderVariablesChanged += OnShaderVariablesChanged;
    }
#endif

    this.postfix = postfix;

    InitShaderIDs(guid);
    Raymarch.OnUploadShaderData += UploadShaderData;
  }

  public bool IsValid()
  {
    return ShaderFeatureAsset != null;
  }

  public void OnDestroy()
  {
#if UNITY_EDITOR
    if (ShaderFeatureAsset != null)
    {
      ShaderFeatureAsset.OnShaderVariablesChanged -= OnShaderVariablesChanged;
    }
#endif
    
    Raymarch.OnUploadShaderData -= UploadShaderData;
  }

  protected virtual void UploadShaderData(Material material)
  {
    if (ShaderFeatureAsset == null) return;

#if UNITY_EDITOR
    // NOTE(WSWhitehouse): This can happen when the scene is reloaded in edit mode
    if (ShaderVariables.Count != _shaderIDs.Length)
    {
      InitShaderIDs(guid);
    }
#endif

    for (int i = 0; i < ShaderVariables.Count; i++)
    {
      ShaderVariables[i].UploadToShader(material, _shaderIDs[i]);
    }
  }

#if UNITY_EDITOR
  public virtual string GetShaderVariables(SerializableGuid guid)
  {
    if (ShaderFeatureAsset == null) return string.Empty;

    StringBuilder result = new StringBuilder();

    for (int i = 0; i < ShaderVariables.Count; i++)
    {
      result.AppendLine($"uniform {ShaderVariables[i].GetShaderType()} {GetShaderVariableName(i, guid)};");
    }

    return result.ToString();
  }
#endif

#if UNITY_EDITOR
  public static class Editor
  {
    public static ShaderFeature<T> ShaderVariableField(GUIContent guiContent, ShaderFeature<T> shaderFeature)
    {
      if (shaderFeature.shaderVariables.Count <= 0)
      {
        return shaderFeature;
      }

      GUIStyle boldStyle = new GUIStyle(GUI.skin.GetStyle("label"))
      {
        fontStyle = FontStyle.Bold
      };

      if (guiContent != GUIContent.none)
        EditorGUILayout.LabelField(guiContent, boldStyle);

      for (int i = 0; i < shaderFeature.ShaderVariables.Count; i++)
      {
        EditorGUI.BeginChangeCheck();

        var variable = ShaderVariable.Editor.VariableField(shaderFeature.ShaderVariables[i]);

        if (EditorGUI.EndChangeCheck())
        {
          shaderFeature.ShaderVariables[i] = variable;
        }
      }

      return shaderFeature;
    }
  }
#endif
}