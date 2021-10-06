using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Raymarching/Signed Distance Function")]
public class RaymarchSDF : ScriptableObject
{
  [SerializeField] private SerializableGuid guid;
  public Guid GUID => guid.GUID;

  [SerializeField] private string _functionBody = "return 0;";

  public string Name => name.Replace(' ', '_');
  public string FunctionName => string.Concat("SDF_", Name);
  public string FunctionNameWithGUID => string.Concat(FunctionName, "_", guid.ToShaderSafeString());

  public string FunctionParameters
  {
    get
    {
      string parameters = "float3 pos, float3 scale";

      for (int i = 0; i < shaderValues.Count; i++)
      {
        parameters = string.Concat(parameters, ", ", shaderValues.ToShaderParameter(i));
      }

      return parameters;
    }
  }

  public string FunctionPrototype => string.Concat("float ", FunctionName, "(", FunctionParameters, ")");

  public string FunctionPrototypeWithGUID =>
    string.Concat("float ", FunctionNameWithGUID, "(", FunctionParameters, ")");

  public string FunctionBody
  {
    get => _functionBody;
    set => _functionBody = value;
  }

  [SerializeField] public ShaderValues shaderValues;
  public Action OnShaderValuesChanged;

#if UNITY_EDITOR
  private void Awake()
  {
    SignalSdfUpdated();
  }

  public void SignalSdfUpdated()
  {
    OnShaderValuesChanged?.Invoke();
    EditorUtility.SetDirty(this);
    ShaderGen.GenerateDistanceFunctionsShader();
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchSDF))]
public class RaymarchSDFEditor : Editor
{
  private RaymarchSDF Target => target as RaymarchSDF;

  private bool _valuesDropDown = true;

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    GUIStyle wordWrapStyle = EditorStyles.wordWrappedLabel;
    wordWrapStyle.fontStyle = FontStyle.Bold;

    EditorGUILayout.LabelField("Signed Distance Function", wordWrapStyle);
    EditorGUILayout.BeginVertical(GUI.skin.box);
    EditorGUILayout.LabelField(Target.FunctionPrototype, wordWrapStyle);
    EditorGUILayout.LabelField(ShaderGen.SquigglyBracketOpen);

    EditorGUI.BeginChangeCheck();

    Target.FunctionBody = EditorGUILayout.TextArea(Target.FunctionBody);

    if (EditorGUI.EndChangeCheck())
    {
      Target.SignalSdfUpdated();
    }

    EditorGUILayout.LabelField(ShaderGen.SquigglyBracketClose);
    EditorGUILayout.EndVertical();

    EditorGUILayout.Space();

    _valuesDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_valuesDropDown, "Values");

    if (_valuesDropDown)
    {
      EditorGUI.indentLevel++;

      for (int i = 0; i < Target.shaderValues.Count; i++)
      {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();

        string key = EditorGUILayout.TextField(GUIContent.none, Target.shaderValues._keys[i]);

        AnyValue value = Target.shaderValues._values[i];
        value.type = (AnyValue.Type) EditorGUILayout.EnumPopup(GUIContent.none, Target.shaderValues._values[i].type);

        if (EditorGUI.EndChangeCheck())
        {
          Target.shaderValues._keys[i] = key.Replace(' ', '_');
          Target.shaderValues._values[i] = value;
          Target.SignalSdfUpdated();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();

        AnyValue.EDITOR_DrawInspectorValue(ref value, GUIContent.none);

        if (EditorGUI.EndChangeCheck())
        {
          Target.shaderValues._values[i] = value;
          EditorUtility.SetDirty(Target);
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

    serializedObject.ApplyModifiedProperties();
  }

  private void RemoveValue(int index)
  {
    Target.shaderValues.RemoveValue(index);
    Target.SignalSdfUpdated();
  }

  private void AddValue()
  {
    Target.shaderValues.AddValue(string.Concat("Value ", Target.shaderValues.Count.ToString()), new AnyValue());
    Target.SignalSdfUpdated();
  }
}
#endif