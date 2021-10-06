using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct AnyValue
{
  public enum Type
  {
    Float,
    Int,
    Bool,
    Vector2,
    Vector3,
    Vector4,
    Colour
  }

  [SerializeField] public Type type;

  [SerializeField] public float floatValue;
  [SerializeField] public int intValue;
  [SerializeField] public Vector4 vectorValue;

  public string TypeToShaderType()
  {
    return type switch
    {
      Type.Float => "float",
      Type.Int => "int",
      Type.Bool => "int",
      Type.Vector2 => "float2",
      Type.Vector3 => "float3",
      Type.Vector4 => "float4",
      Type.Colour => "float4",
      _ => throw new ArgumentOutOfRangeException()
    };
  }
  
  #if UNITY_EDITOR
  public static void EDITOR_DrawInspectorValue(ref AnyValue value, GUIContent guiContent)
  {
    switch (value.type)
    {
      case Type.Float:
        value.floatValue = EditorGUILayout.FloatField(guiContent, value.floatValue);
        break;
      case Type.Int:
        value.intValue = EditorGUILayout.IntField(guiContent, value.intValue);
        break;
      case Type.Bool:
        value.intValue = EditorGUILayout.Toggle(guiContent, value.intValue > 0) ? 1 : 0;
        break;
      case Type.Vector2:
        value.vectorValue = EditorGUILayout.Vector2Field(guiContent, value.vectorValue);
        break;
      case Type.Vector3:
        value.vectorValue = EditorGUILayout.Vector3Field(guiContent, value.vectorValue);
        break;
      case Type.Vector4:
        value.vectorValue = EditorGUILayout.Vector4Field(guiContent, value.vectorValue);
        break;
      case Type.Colour:
        value.vectorValue = EditorGUILayout.ColorField(guiContent, value.vectorValue);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  } 
  #endif
}

[Serializable]
public class ShaderValues
{
  [SerializeField] public List<string> _keys = new List<string>();
  [SerializeField] public List<AnyValue> _values = new List<AnyValue>();

  public void InitValues(ShaderValues other)
  {
    var newKeys = new List<string>(other._keys.Count);
    var newValues = new List<AnyValue>(other._keys.Count);

    for (int i = 0; i < other._keys.Count; i++)
    {
      int index = _keys.FindIndex(x => x == other._keys[i]);

      if (index < 0) // key not found
      {
        newKeys.Add(other._keys[i]);
        newValues.Add(other._values[i]);
        continue;
      }

      newKeys.Add(_keys[index]);
      newValues.Add(_values[index].type != other._values[i].type ? other._values[i] : _values[index]);
    }

    _keys = newKeys;
    _values = newValues;
  }

  public void ClearValues()
  {
    _keys.Clear();
    _values.Clear();
  }

  public void AddValue(string name, AnyValue value)
  {
    _keys.Add(name.Replace(' ', '_'));
    _values.Add(value);
  }

  public void RemoveValue(string name)
  {
    RemoveValue(PropertyToID(name));
  }

  public void RemoveValue(int index)
  {
    _keys.RemoveAt(index);
    _values.RemoveAt(index);
  }

  public int Count => _keys.Count;

  public int PropertyToID(string name)
  {
    return _keys.FindIndex(x => x == name);
  }

  public string ToShaderParameter(int id)
  {
    return string.Concat(_values[id].TypeToShaderType(), " ", _keys[id]);
  }

  // Getters
  public float GetFloat(int id)
  {
    Assert.AreEqual(AnyValue.Type.Float, _values[id].type);
    return _values[id].floatValue;
  }

  public int GetInt(int id)
  {
    Assert.AreEqual(AnyValue.Type.Int, _values[id].type);
    return _values[id].intValue;
  }

  public bool GetBool(int id)
  {
    Assert.AreEqual(AnyValue.Type.Bool, _values[id].type);
    return _values[id].intValue > 0;
  }

  public Vector2 GetVector2(int id)
  {
    Assert.AreEqual(AnyValue.Type.Vector2, _values[id].type);
    return _values[id].vectorValue;
  }

  public Vector3 GetVector3(int id)
  {
    Assert.AreEqual(AnyValue.Type.Vector3, _values[id].type);
    return _values[id].vectorValue;
  }

  public Vector4 GetVector4(int id)
  {
    Assert.AreEqual(AnyValue.Type.Vector4, _values[id].type);
    return _values[id].vectorValue;
  }

  public Color GetColour(int id)
  {
    Assert.AreEqual(AnyValue.Type.Colour, _values[id].type);
    return _values[id].vectorValue;
  }

  // Setters
  public void SetFloat(int id, float value)
  {
    Assert.AreEqual(AnyValue.Type.Float, _values[id].type);

    var values = _values[id];
    values.floatValue = value;
    _values[id] = values;
  }

  public void SetInt(int id, int value)
  {
    Assert.AreEqual(AnyValue.Type.Int, _values[id].type);

    var values = _values[id];
    values.intValue = value;
    _values[id] = values;
  }

  public void SetBool(int id, bool value)
  {
    Assert.AreEqual(AnyValue.Type.Bool, _values[id].type);

    var values = _values[id];
    values.intValue = value ? 1 : 0;
    _values[id] = values;
  }

  public void SetVector2(int id, Vector2 value)
  {
    Assert.AreEqual(AnyValue.Type.Vector2, _values[id].type);

    var values = _values[id];
    values.vectorValue = value;
    _values[id] = values;
  }

  public void SetVector3(int id, Vector3 value)
  {
    Assert.AreEqual(AnyValue.Type.Vector3, _values[id].type);

    var values = _values[id];
    values.vectorValue = value;
    _values[id] = values;
  }

  public void SetVector4(int id, Vector4 value)
  {
    Assert.AreEqual(AnyValue.Type.Vector4, _values[id].type);

    var values = _values[id];
    values.vectorValue = value;
    _values[id] = values;
  }

  public void SetColour(int id, Color value)
  {
    Assert.AreEqual(AnyValue.Type.Colour, _values[id].type);

    var values = _values[id];
    values.vectorValue = value;
    _values[id] = values;
  }
  
  #if UNITY_EDITOR
  public static ShaderValues EDITOR_DrawValuesInspector(ShaderValues values)
  {
    for (int i = 0; i < values.Count; i++)
    {
      AnyValue value = values._values[i];
      GUIContent label = new GUIContent(values._keys[i]);

      AnyValue.EDITOR_DrawInspectorValue(ref value, label);
    }

    return values;
  }
  
  #endif
}