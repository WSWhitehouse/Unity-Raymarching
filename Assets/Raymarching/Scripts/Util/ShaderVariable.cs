using System;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct ShaderVariable
{
  public enum Type
  {
    Float,
    Int,
    Bool,
    Vector2,
    Vector3,
    Vector4,
    Colour,
    Texture2D
  }

  [SerializeField] private string name;
  [SerializeField] private Type type;

  [SerializeField] private float floatValue;
  [SerializeField] private int intValue;
  [SerializeField] private Vector4 vectorValue;
  [SerializeField] private Texture2D textureValue;

  // Constructor
  public ShaderVariable(string _name)
  {
    name = _name;
    type = Type.Float;

    floatValue = 0f;
    intValue = 0;
    vectorValue = Vector4.zero;
    textureValue = null;
  }

  public ShaderVariable(ShaderVariable other)
  {
    name = other.name;
    type = other.type;

    floatValue = other.floatValue;
    intValue = other.intValue;
    vectorValue = other.vectorValue;
    textureValue = other.textureValue;
  }

  // Shader
  public string GetShaderType()
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
      Type.Texture2D => "sampler2D",
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public string GetShaderName(SerializableGuid guid)
  {
    return $"_{GetVariableName()}{guid.ToShaderSafeString()}";
  }

  public string ToShaderVariable(SerializableGuid guid)
  {
    return $"uniform {GetShaderType()} {GetShaderName(guid)};";
  }

  public string ToShaderParameter()
  {
    return $"{GetShaderType()} {GetVariableName()}";
  }

  public void UploadToShader(Material material, int shaderID)
  {
    switch (type)
    {
      case Type.Float:
        material.SetFloat(shaderID, floatValue);
        break;
      case Type.Int:
      case Type.Bool:
        material.SetInt(shaderID, intValue);
        break;
      case Type.Vector2:
      case Type.Vector3:
      case Type.Vector4:
      case Type.Colour:
        material.SetVector(shaderID, vectorValue);
        break;
      case Type.Texture2D:
        material.SetTexture(shaderID, textureValue);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  // Getters
  public Type GetVariableType()
  {
    return type;
  }

  public string GetVariableName()
  {
    return name;
  }

  public float GetFloat()
  {
    Assert.AreEqual(Type.Float, type);
    return floatValue;
  }

  public int GetInt()
  {
    Assert.AreEqual(Type.Int, type);
    return intValue;
  }

  public bool GetBool()
  {
    Assert.AreEqual(Type.Bool, type);
    return intValue > 0;
  }

  public Vector2 GetVector2()
  {
    Assert.AreEqual(Type.Vector2, type);
    return vectorValue;
  }

  public Vector3 GetVector3()
  {
    Assert.AreEqual(Type.Vector3, type);
    return vectorValue;
  }

  public Vector4 GetVector4()
  {
    Assert.AreEqual(Type.Vector4, type);
    return vectorValue;
  }

  public Color GetColour()
  {
    Assert.AreEqual(Type.Colour, type);
    return vectorValue;
  }

  public Texture2D GetTexture2D()
  {
    Assert.AreEqual(Type.Texture2D, type);
    return textureValue;
  }

  // Setters
  public void SetVariableType(Type type)
  {
    this.type = type;
  }

  public void SetVariableName(string name)
  {
    this.name = name.Replace(' ', '_');
  }

  public void SetFloat(float value)
  {
    Assert.AreEqual(Type.Float, type);
    floatValue = value;
  }

  public void SetInt(int value)
  {
    Assert.AreEqual(Type.Int, type);
    intValue = value;
  }

  public void SetBool(bool value)
  {
    Assert.AreEqual(Type.Bool, type);
    intValue = value ? 1 : 0;
  }

  public void SetVector2(Vector2 value)
  {
    Assert.AreEqual(Type.Vector2, type);
    vectorValue = value;
  }

  public void SetVector3(Vector3 value)
  {
    Assert.AreEqual(Type.Vector3, type);
    vectorValue = value;
  }

  public void SetVector4(Vector4 value)
  {
    Assert.AreEqual(Type.Vector4, type);
    vectorValue = value;
  }

  public void SetColour(Color value)
  {
    Assert.AreEqual(Type.Colour, type);
    vectorValue = value;
  }
  
  public void SetTexture2D(Texture2D value)
  {
    Assert.AreEqual(Type.Texture2D, type);
    textureValue = value;
  }

#if UNITY_EDITOR
  public sealed class Editor
  {
    public static ShaderVariable EditableVariableField(ShaderVariable variable)
    {
      EditorGUILayout.BeginHorizontal();
      variable.SetVariableName(EditorGUILayout.TextField(GUIContent.none, variable.GetVariableName()));
      variable.SetVariableType((Type) EditorGUILayout.EnumPopup(GUIContent.none, variable.GetVariableType()));
      EditorGUILayout.EndHorizontal();

      switch (variable.GetVariableType())
      {
        case Type.Float:
          variable.SetFloat(EditorGUILayout.FloatField(GUIContent.none, variable.GetFloat()));
          break;
        case Type.Int:
          variable.SetInt(EditorGUILayout.IntField(GUIContent.none, variable.GetInt()));
          break;
        case Type.Bool:
          variable.SetBool(EditorGUILayout.Toggle(GUIContent.none, variable.GetBool()));
          break;
        case Type.Vector2:
          variable.SetVector2(EditorGUILayout.Vector2Field(GUIContent.none, variable.GetVector2()));
          break;
        case Type.Vector3:
          variable.SetVector3(EditorGUILayout.Vector3Field(GUIContent.none, variable.GetVector3()));
          break;
        case Type.Vector4:
          variable.SetVector4(EditorGUILayout.Vector4Field(GUIContent.none, variable.GetVector4()));
          break;
        case Type.Colour:
          variable.SetColour(EditorGUILayout.ColorField(GUIContent.none, variable.GetColour()));
          break;
        case Type.Texture2D:
          variable.SetTexture2D((Texture2D) EditorGUILayout.ObjectField(GUIContent.none, variable.GetTexture2D(), 
            typeof(Texture2D), false));
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return variable;
    }

    public static ShaderVariable VariableField(ShaderVariable variable)
    {
      var guiContent = new GUIContent(variable.GetVariableName());

      switch (variable.GetVariableType())
      {
        case Type.Float:
          variable.SetFloat(EditorGUILayout.FloatField(guiContent, variable.GetFloat()));
          break;
        case Type.Int:
          variable.SetInt(EditorGUILayout.IntField(guiContent, variable.GetInt()));
          break;
        case Type.Bool:
          variable.SetBool(EditorGUILayout.Toggle(guiContent, variable.GetBool()));
          break;
        case Type.Vector2:
          variable.SetVector2(EditorGUILayout.Vector2Field(guiContent, variable.GetVector2()));
          break;
        case Type.Vector3:
          variable.SetVector3(EditorGUILayout.Vector3Field(guiContent, variable.GetVector3()));
          break;
        case Type.Vector4:
          variable.SetVector4(EditorGUILayout.Vector4Field(guiContent, variable.GetVector4()));
          break;
        case Type.Colour:
          variable.SetColour(EditorGUILayout.ColorField(guiContent, variable.GetColour()));
          break;
        case Type.Texture2D:
          variable.SetTexture2D((Texture2D) EditorGUILayout.ObjectField(guiContent, variable.GetTexture2D(), 
            typeof(Texture2D), false));
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return variable;
    }
  }
#endif
}