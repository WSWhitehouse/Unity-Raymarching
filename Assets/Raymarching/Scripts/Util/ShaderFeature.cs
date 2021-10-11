using System;
using System.Collections.Generic;
using UnityEngine;
using Action = System.Action;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class ShaderFeature : ScriptableObject
{
  // GUID
  [SerializeField] private SerializableGuid guid;
  public SerializableGuid GUID => guid;

  public string FunctionName => $"{GetFunctionPrefix()}_{name.Replace(' ', '_')}";
  public string FunctionNameWithGuid => $"{FunctionName}_{GUID.ToShaderSafeString()}";

  public string FunctionParameters
  {
    get
    {
      string parameters = String.Empty;
      var defaultParams = GetDefaultParameters();

      for (int i = 0; i < defaultParams.Count; i++)
      {
        if (i == 0)
        {
          parameters = $"{defaultParams[i].ToShaderParameter()}";
          continue;
        }
        
        parameters = $"{parameters}, {defaultParams[i].ToShaderParameter()}";
      }

      for (int i = 0; i < shaderVariables.Count; i++)
      {
        parameters = string.Concat(parameters, ", ", shaderVariables[i].ToShaderParameter());
      }

      return parameters;
    }
  }

  public string FunctionPrototype => $"{GetReturnType()} {FunctionName}({FunctionParameters})";

  public string FunctionPrototypeWithGuid => $"{GetReturnType()} {FunctionNameWithGuid}({FunctionParameters})";

  [SerializeField] private string functionBody = "return 0;";

  public string FunctionBody
  {
    get => functionBody;
    set => functionBody = value;
  }

  [SerializeField] public List<ShaderVariable> shaderVariables;

  protected virtual string GetFunctionPrefix()
  {
    return string.Empty;
  }

  protected virtual string GetReturnType()
  {
    return string.Empty;
  }

  protected virtual List<ShaderVariable> GetDefaultParameters()
  {
    return new List<ShaderVariable>();
  }

#if UNITY_EDITOR
  public Action OnShaderValuesChanged;
  
  protected void Awake()
  {
    SignalShaderFeatureUpdated();
  }

  public virtual void SignalShaderFeatureUpdated()
  {
    OnShaderValuesChanged?.Invoke();
    EditorUtility.SetDirty(this);
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ShaderFeature))]
public class ShaderFeatureEditor : Editor
{
  private ShaderFeature Target => target as ShaderFeature;

  private bool _valuesDropDown = true;

  public override void OnInspectorGUI()
  {
    if (Application.isPlaying)
      EditorGUILayout.HelpBox("You cannot edit Shader Features during play mode.", MessageType.Info, true);

    bool cachedGUIEnabled = GUI.enabled;
    GUI.enabled = !Application.isPlaying;

    serializedObject.Update();
    DrawShaderFeatureInspector();
    serializedObject.ApplyModifiedProperties();

    GUI.enabled = cachedGUIEnabled;
  }

  private void DrawShaderFeatureInspector()
  {
    GUIStyle wordWrapStyle = EditorStyles.wordWrappedLabel;
    wordWrapStyle.fontStyle = FontStyle.Bold;

    EditorGUILayout.BeginVertical(GUI.skin.box);
    EditorGUILayout.LabelField(Target.FunctionPrototype, wordWrapStyle);
    EditorGUILayout.LabelField(ShaderGen.SquigglyBracketOpen);

    EditorGUI.BeginChangeCheck();

    Target.FunctionBody = EditorGUILayout.TextArea(Target.FunctionBody);

    if (EditorGUI.EndChangeCheck())
    {
      Target.SignalShaderFeatureUpdated();
    }

    EditorGUILayout.LabelField(ShaderGen.SquigglyBracketClose);
    EditorGUILayout.EndVertical();

    EditorGUILayout.Space();

    _valuesDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_valuesDropDown, "Values");

    if (_valuesDropDown)
    {
      EditorGUI.indentLevel++;

      for (int i = 0; i < Target.shaderVariables.Count; i++)
      {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUI.BeginChangeCheck();

        var value = ShaderVariable.Editor.EditableVariableField(Target.shaderVariables[i]);

        if (EditorGUI.EndChangeCheck())
        {
          Target.shaderVariables[i] = value;
          Target.SignalShaderFeatureUpdated();
        }

        if (GUILayout.Button("Remove Value"))
        {
          RemoveValue(i);
          EditorGUILayout.EndVertical();
          break; // leave loop so iterator doesn't get messed up!
        }

        EditorGUILayout.EndVertical();
      }

      if (GUILayout.Button("Add New Value"))
      {
        AddValue();
      }

      EditorGUI.indentLevel--;
    }

    EditorGUILayout.EndFoldoutHeaderGroup();
  }

  private void RemoveValue(int index)
  {
    Target.shaderVariables.RemoveAt(index);
    Target.SignalShaderFeatureUpdated();
  }

  private void AddValue()
  {
    Target.shaderVariables.Add(new ShaderVariable($"Value {Target.shaderVariables.Count.ToString()}"));
    Target.SignalShaderFeatureUpdated();
  }
}
#endif