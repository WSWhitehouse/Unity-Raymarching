using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ShaderFeatureImpl<T> where T : ShaderFeature
{
  #region Shader Feature

  [SerializeField] private T shaderFeature;

  public T ShaderFeature
  {
    get => shaderFeature;
#if UNITY_EDITOR
    // NOTE(WSWhitehouse): Cannot change the shader feature during runtime, for editor only
    set
    {
      if (Application.isPlaying) return;
      if (EqualityComparer<T>.Default.Equals(shaderFeature, value)) return;

      if (shaderFeature != null)
      {
        shaderFeature.OnShaderVariablesChanged -= OnShaderVariablesChanged;
      }

      if (value == null)
      {
        shaderFeature = null;
        shaderVariables.Clear();
      }
      else
      {
        shaderFeature = value;
        OnShaderVariablesChanged();
      }

      if (shaderFeature != null)
      {
        shaderFeature.OnShaderVariablesChanged += OnShaderVariablesChanged;
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
    int count = ShaderFeature.ShaderVariables.Count;

    var newVariables = new List<ShaderVariable>(count);

    for (int i = 0; i < count; i++)
    {
      int index = ShaderVariables.FindIndex(x =>
        x.Name == ShaderFeature.ShaderVariables[i].Name);

      if (index < 0) // variable not found
      {
        newVariables.Add(ShaderFeature.ShaderVariables[i]);
        continue;
      }

      newVariables.Add(shaderVariables[index].ShaderType != ShaderFeature.ShaderVariables[i].ShaderType
        ? ShaderFeature.ShaderVariables[i]
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

  private void InitShaderIDs(SerializableGuid guid)
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

  private string postfix = string.Empty;

  public void OnAwake(SerializableGuid guid, string postfix = "")
  {
#if UNITY_EDITOR
    if (ShaderFeature != null)
    {
      ShaderFeature.OnShaderVariablesChanged += OnShaderVariablesChanged;
    }
#endif

    this.postfix = postfix;

    InitShaderIDs(guid);
  }

  public bool IsValid()
  {
    return ShaderFeature != null;
  }

  public void OnDestroy()
  {
#if UNITY_EDITOR
    if (ShaderFeature != null)
    {
      ShaderFeature.OnShaderVariablesChanged -= OnShaderVariablesChanged;
    }
#endif
  }

  public void UploadShaderData(Material material)
  {
    if (ShaderFeature == null) return;

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
  public string GetShaderVariables(SerializableGuid guid)
  {
    if (ShaderFeature == null) return string.Empty;

    string code = string.Empty;

    for (int i = 0; i < ShaderVariables.Count; i++)
    {
      code =
        $"{code}uniform {ShaderVariables[i].GetShaderType()} {GetShaderVariableName(i, guid)};{ShaderGen.NewLine}";
    }

    return code;
  }
#endif

#if UNITY_EDITOR
  public static class Editor
  {
    public static ShaderFeatureImpl<T> ShaderFeatureField(GUIContent guiContent, ShaderFeatureImpl<T> shaderFeature,
      UnityEngine.Object target)
    {
      EditorGUI.BeginChangeCheck();
      shaderFeature.ShaderFeature =
        (T) EditorGUILayout.ObjectField(guiContent, shaderFeature.ShaderFeature, typeof(T), false);
      if (EditorGUI.EndChangeCheck())
      {
        EditorUtility.SetDirty(target);
      }

      return shaderFeature;
    }

    public static ShaderFeatureImpl<T> ShaderVariableField(GUIContent guiContent, ShaderFeatureImpl<T> shaderFeature,
      UnityEngine.Object target)
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
          EditorUtility.SetDirty(target);
        }
      }

      return shaderFeature;
    }
  }
#endif
}