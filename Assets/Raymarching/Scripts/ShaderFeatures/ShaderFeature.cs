using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class ShaderFeature : ScriptableObject
{
  #region GUID

  [SerializeField] private SerializableGuid guid;
  public SerializableGuid GUID => guid;

  #endregion GUID

  #region Shader Code

  public string FunctionName => $"{GetFunctionPrefix()}_{name.Replace(' ', '_')}";
  public string FunctionNameWithGuid => $"{FunctionName}_{GUID.ToShaderSafeString()}";

  public string FunctionParameters
  {
    get
    {
      string parameters = String.Empty;
      var defaultParams = GetDefaultParameters();

      for (int i = 0; i < defaultParams.Length; i++)
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

  public string FunctionPrototype => $"{GetReturnType().ToShaderString()} {FunctionName}({FunctionParameters})";

  public string FunctionPrototypeWithGuid =>
    $"{GetReturnType().ToShaderString()} {FunctionNameWithGuid}({FunctionParameters})";

  [SerializeField] private string functionBody = "return 0;";

  public string FunctionBody
  {
    get => functionBody;
    set => functionBody = value;
  }

  /*
   * NOTE(WSWhitehouse):
   * The following abstract functions are used to set up a shader feature.
   * They must all be overwritten but can return nothing - i.e. `string.Empty`,
   * `Array.Empty<>()` or `ShaderType.void`. They are abstract to force inheriting
   * shader features to implement them.
   */

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected abstract string GetFunctionPrefix();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected abstract ShaderType GetReturnType();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected abstract ShaderVariable[] GetDefaultParameters();

  #endregion Shader Code

  [SerializeField] private List<ShaderVariable> shaderVariables = new List<ShaderVariable>();

#if UNITY_EDITOR
  // NOTE(WSWhitehouse): Only allow access in the editor, the ShaderVariables should not be updated at runtime.
  public List<ShaderVariable> ShaderVariables => shaderVariables;

  /*
   * NOTE(WSWhitehouse):
   * This event is invoked when this shader feature gets changed, it ensures that all implementations of
   * a shader feature gets up to date variables and gets notified of a change. The event is only used in
   * debug so don't subscribe to it during runtime.
   */
  public delegate void ShaderVariablesChanged();

  public event ShaderVariablesChanged OnShaderVariablesChanged;

  public virtual void SignalShaderFeatureUpdated()
  {
    OnShaderVariablesChanged?.Invoke();
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

  private string _functionBody;

  protected virtual void OnEnable()
  {
    _functionBody = Target.FunctionBody;
  }

  // NOTE(WSWhitehouse): Do *NOT* override this function as it handles enabling/disabling
  // of the GUI while in play mode. Instead, override "DrawInspector()". The function is
  // marked sealed to ensure it cannot be overriden.
  public sealed override void OnInspectorGUI()
  {
    if (Application.isPlaying)
      EditorGUILayout.HelpBox("You cannot edit Shader Features during play mode.", MessageType.Info, true);

    bool cachedGUIEnabled = GUI.enabled;
    GUI.enabled = !Application.isPlaying;

    serializedObject.Update();
    DrawInspector();
    serializedObject.ApplyModifiedProperties();

    GUI.enabled = cachedGUIEnabled;
  }

  protected virtual void DrawInspector()
  {
    GUIStyle wordWrapStyle = EditorStyles.wordWrappedLabel;
    wordWrapStyle.fontStyle = FontStyle.Bold;

    EditorGUILayout.BeginVertical(GUI.skin.box);
    EditorGUILayout.LabelField(Target.FunctionPrototype, wordWrapStyle);
    EditorGUILayout.LabelField("{");

    EditorGUI.indentLevel++;
    _functionBody = EditorGUILayout.TextArea(_functionBody);
    EditorGUI.indentLevel--;

    EditorGUILayout.LabelField("}");

    EditorGUILayout.BeginHorizontal();

    bool guiEnableCache = GUI.enabled;
    GUI.enabled = !_functionBody.Equals(Target.FunctionBody) && guiEnableCache;

    if (GUILayout.Button("Apply"))
    {
      Target.FunctionBody = _functionBody;
      Target.SignalShaderFeatureUpdated();
    }

    if (GUILayout.Button("Revert"))
    {
      _functionBody = Target.FunctionBody;
    }

    GUI.enabled = guiEnableCache;

    EditorGUILayout.EndHorizontal();

    EditorGUILayout.EndVertical();

    EditorGUILayout.Space();

    _valuesDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_valuesDropDown, "Values");

    if (_valuesDropDown)
    {
      EditorGUI.indentLevel++;

      for (int i = 0; i < Target.ShaderVariables.Count; i++)
      {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUI.BeginChangeCheck();

        var value = ShaderVariable.Editor.EditableVariableField(Target.ShaderVariables[i]);

        if (EditorGUI.EndChangeCheck())
        {
          Target.ShaderVariables[i] = value;
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
    Target.ShaderVariables.RemoveAt(index);
    Target.SignalShaderFeatureUpdated();
  }

  private void AddValue()
  {
    Target.ShaderVariables.Add(
      new ShaderVariable($"Value_{Target.ShaderVariables.Count.ToString()}", ShaderType.Float));
    Target.SignalShaderFeatureUpdated();
  }
}
#endif