using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ShaderFeatureImpl<T> where T : ShaderFeature
{
  [SerializeField] private T shaderFeature;
  [SerializeField] private List<ShaderVariable> shaderVariables = new List<ShaderVariable>();

  private string postfix = string.Empty;

  public T ShaderFeature
  {
    get => shaderFeature;
#if UNITY_EDITOR
    set
    {
      if (EqualityComparer<T>.Default.Equals(shaderFeature, value)) return;

      if (shaderFeature != null)
      {
        shaderFeature.OnShaderValuesChanged -= OnShaderValuesChanged;
      }

      if (value == null)
      {
        shaderFeature = null;
        shaderVariables.Clear();
      }
      else
      {
        shaderFeature = value;
        OnShaderValuesChanged();
      }

      if (shaderFeature != null)
      {
        shaderFeature.OnShaderValuesChanged += OnShaderValuesChanged;
      }
    }
#endif
  }

  public List<ShaderVariable> ShaderVariables => shaderVariables;

  public string GetShaderVariableName(int index, SerializableGuid guid)
  {
    return $"{ShaderVariables[index].GetShaderName(guid)}{postfix}";
  }

  public void OnAwake(SerializableGuid guid, string postfix = "")
  {
#if UNITY_EDITOR
    if (ShaderFeature != null)
    {
      ShaderFeature.OnShaderValuesChanged += OnShaderValuesChanged;
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
      ShaderFeature.OnShaderValuesChanged -= OnShaderValuesChanged;
    }
#endif
  }

#if UNITY_EDITOR
  private void OnShaderValuesChanged()
  {
    int count = ShaderFeature.shaderVariables.Count;

    var newVariables = new List<ShaderVariable>(count);

    for (int i = 0; i < count; i++)
    {
      int index = ShaderVariables.FindIndex(x =>
        x.Name == ShaderFeature.shaderVariables[i].Name);

      if (index < 0) // variable not found
      {
        newVariables.Add(ShaderFeature.shaderVariables[i]);
        continue;
      }

      newVariables.Add(shaderVariables[index].ShaderType != ShaderFeature.shaderVariables[i].ShaderType
        ? ShaderFeature.shaderVariables[i]
        : shaderVariables[index]);
    }

    shaderVariables = newVariables;
  }
#endif

  // Shader
  private int[] _shaderIDs;

#if UNITY_EDITOR
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

#if UNITY_EDITOR
  public string GetShaderVariables(SerializableGuid guid)
  {
    if (ShaderFeature == null) return string.Empty;

    string code = string.Empty;

    for (int i = 0; i < ShaderVariables.Count; i++)
    {
      code =
        $"{code}uniform {ShaderVariables[i].GetShaderType()} {GetShaderVariableName(i, guid)};  {ShaderGen.NewLine}";
    }

    return code;
  }
#endif

  public void UploadShaderData(Material material)
  {
    if (ShaderFeature == null) return;

#if UNITY_EDITOR
    if (ShaderVariables.Count != _shaderIDs.Length)
    {
      // something has seriously gone wrong.
      Debug.LogError("Shader Variable count doesnt equal shader ID count!");
      InitShaderIDs(guid);
    }
#endif

    for (int i = 0; i < ShaderVariables.Count; i++)
    {
      ShaderVariables[i].UploadToShader(material, _shaderIDs[i]);
    }
  }

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