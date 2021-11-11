using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct ShaderVariable
{
  [SerializeField] private string name;
  [SerializeField] private ShaderType shaderType;
  [SerializeField] private ParameterType parameterType;

  [SerializeField] private float floatValue;
  [SerializeField] private int intValue;
  [SerializeField] private Vector4 vectorValue;
  [SerializeField] private Texture2D textureValue;

  public string Name
  {
    get => name;
    set => name = value;
  }

  public ShaderType ShaderType
  {
    get => shaderType;
    set => shaderType = value;
  }

  // Constructor
  public ShaderVariable(string _name, ShaderType _type, ParameterType _parameterType = ParameterType.None)
  {
    name = _name;
    shaderType = _type;
    parameterType = _parameterType;

    floatValue = 0f;
    intValue = 0;
    vectorValue = Vector4.zero;
    textureValue = null;
  }

  public ShaderVariable(ShaderVariable other)
  {
    name = other.name;
    shaderType = other.shaderType;
    parameterType = other.parameterType;

    floatValue = other.floatValue;
    intValue = other.intValue;
    vectorValue = other.vectorValue;
    textureValue = other.textureValue;
  }
  
  // Shader
  public string GetShaderType()
  {
    return shaderType.ToShaderString();
  }

  public string GetShaderName(SerializableGuid guid)
  {
    return $"_{Name}{guid.ToShaderSafeString()}";
  }

  public string ToShaderParameter()
  {
    return $"{parameterType.ToShaderString()}{GetShaderType()} {Name}";
  }

  public void UploadToShader(Material material, int shaderID)
  {
    switch (shaderType)
    {
      case ShaderType.Float:
        material.SetFloat(shaderID, floatValue);
        break;
      case ShaderType.Int:
      case ShaderType.Bool:
        material.SetInt(shaderID, intValue);
        break;
      case ShaderType.Vector2:
      case ShaderType.Vector3:
      case ShaderType.Vector4:
      case ShaderType.Colour:
        material.SetVector(shaderID, vectorValue);
        break;
      case ShaderType.Texture2D:
        material.SetTexture(shaderID, textureValue);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }
  
  public string ValueToShaderString()
  {
    // NOTE(WSWhitehouse):
    // Local function to convert float to string as it requires string culture to be explicitly specified
    string _FloatToString(float value)
    {
      return value.ToString(NumberFormatInfo.CurrentInfo);
    }
    
    switch (shaderType)
    {
      
      case ShaderType.Float: return _FloatToString(floatValue);
      case ShaderType.Int: return intValue.ToString();
      case ShaderType.Bool: return intValue > 0 ? "true" : "false";
      case ShaderType.Vector2: return $"float2({_FloatToString(vectorValue.x)}, {_FloatToString(vectorValue.y)})";
      case ShaderType.Vector3: return $"float3({_FloatToString(vectorValue.x)}, {_FloatToString(vectorValue.y)}, {_FloatToString(vectorValue.z)})";
      case ShaderType.Vector4: return $"float4({_FloatToString(vectorValue.x)}, {_FloatToString(vectorValue.y)}, {_FloatToString(vectorValue.z)}, {_FloatToString(vectorValue.w)})";
      case ShaderType.Colour:  return $"float4({_FloatToString(vectorValue.x)}, {_FloatToString(vectorValue.y)}, {_FloatToString(vectorValue.z)}, {_FloatToString(vectorValue.w)})";
      
      // NOTE(WSWhitehouse): Cannot convert these types to a string
      case ShaderType.Texture2D:
      case ShaderType.Void: return "";
      
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  // Getters
  public float GetFloat()
  {
    Assert.AreEqual(ShaderType.Float, shaderType);
    return floatValue;
  }

  public int GetInt()
  {
    Assert.AreEqual(ShaderType.Int, shaderType);
    return intValue;
  }

  public bool GetBool()
  {
    Assert.AreEqual(ShaderType.Bool, shaderType);
    return intValue > 0;
  }

  public Vector2 GetVector2()
  {
    Assert.AreEqual(ShaderType.Vector2, shaderType);
    return vectorValue;
  }

  public Vector3 GetVector3()
  {
    Assert.AreEqual(ShaderType.Vector3, shaderType);
    return vectorValue;
  }

  public Vector4 GetVector4()
  {
    Assert.AreEqual(ShaderType.Vector4, shaderType);
    return vectorValue;
  }

  public Color GetColour()
  {
    Assert.AreEqual(ShaderType.Colour, shaderType);
    return vectorValue;
  }

  public Texture2D GetTexture2D()
  {
    Assert.AreEqual(ShaderType.Texture2D, shaderType);
    return textureValue;
  }

  // Setters
  public void SetFloat(float value)
  {
    Assert.AreEqual(ShaderType.Float, shaderType);
    floatValue = value;
  }

  public void SetInt(int value)
  {
    Assert.AreEqual(ShaderType.Int, shaderType);
    intValue = value;
  }

  public void SetBool(bool value)
  {
    Assert.AreEqual(ShaderType.Bool, shaderType);
    intValue = value ? 1 : 0;
  }

  public void SetVector2(Vector2 value)
  {
    Assert.AreEqual(ShaderType.Vector2, shaderType);
    vectorValue = value;
  }

  public void SetVector3(Vector3 value)
  {
    Assert.AreEqual(ShaderType.Vector3, shaderType);
    vectorValue = value;
  }

  public void SetVector4(Vector4 value)
  {
    Assert.AreEqual(ShaderType.Vector4, shaderType);
    vectorValue = value;
  }

  public void SetColour(Color value)
  {
    Assert.AreEqual(ShaderType.Colour, shaderType);
    vectorValue = value;
  }

  public void SetTexture2D(Texture2D value)
  {
    Assert.AreEqual(ShaderType.Texture2D, shaderType);
    textureValue = value;
  }

#if UNITY_EDITOR
  public sealed class Editor
  {
    public static ShaderVariable EditableVariableField(ShaderVariable variable)
    {
      EditorGUILayout.BeginHorizontal();
      
      EditorGUI.BeginChangeCheck();
      string name = EditorGUILayout.TextField(GUIContent.none, variable.Name);
      if (EditorGUI.EndChangeCheck())
      {
        variable.Name = name.Replace(' ', '_');
      }
      
      variable.ShaderType = (ShaderType) EditorGUILayout.EnumPopup(GUIContent.none, variable.ShaderType);
      EditorGUILayout.EndHorizontal();

      switch (variable.ShaderType)
      {
        case ShaderType.Float:
          variable.SetFloat(EditorGUILayout.FloatField(GUIContent.none, variable.GetFloat()));
          break;
        case ShaderType.Int:
          variable.SetInt(EditorGUILayout.IntField(GUIContent.none, variable.GetInt()));
          break;
        case ShaderType.Bool:
          variable.SetBool(EditorGUILayout.Toggle(GUIContent.none, variable.GetBool()));
          break;
        case ShaderType.Vector2:
          variable.SetVector2(EditorGUILayout.Vector2Field(GUIContent.none, variable.GetVector2()));
          break;
        case ShaderType.Vector3:
          variable.SetVector3(EditorGUILayout.Vector3Field(GUIContent.none, variable.GetVector3()));
          break;
        case ShaderType.Vector4:
          variable.SetVector4(EditorGUILayout.Vector4Field(GUIContent.none, variable.GetVector4()));
          break;
        case ShaderType.Colour:
          variable.SetColour(EditorGUILayout.ColorField(GUIContent.none, variable.GetColour()));
          break;
        case ShaderType.Texture2D:
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
      var guiContent = new GUIContent(variable.Name);

      switch (variable.ShaderType)
      {
        case ShaderType.Float:
          variable.SetFloat(EditorGUILayout.FloatField(guiContent, variable.GetFloat()));
          break;
        case ShaderType.Int:
          variable.SetInt(EditorGUILayout.IntField(guiContent, variable.GetInt()));
          break;
        case ShaderType.Bool:
          variable.SetBool(EditorGUILayout.Toggle(guiContent, variable.GetBool()));
          break;
        case ShaderType.Vector2:
          variable.SetVector2(EditorGUILayout.Vector2Field(guiContent, variable.GetVector2()));
          break;
        case ShaderType.Vector3:
          variable.SetVector3(EditorGUILayout.Vector3Field(guiContent, variable.GetVector3()));
          break;
        case ShaderType.Vector4:
          variable.SetVector4(EditorGUILayout.Vector4Field(guiContent, variable.GetVector4()));
          break;
        case ShaderType.Colour:
          variable.SetColour(EditorGUILayout.ColorField(guiContent, variable.GetColour()));
          break;
        case ShaderType.Texture2D:
          variable.SetTexture2D((Texture2D) EditorGUILayout.ObjectField(guiContent, variable.GetTexture2D(),
            typeof(Texture2D), false));
          break;
        case ShaderType.Void:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return variable;
    }
  }
#endif
}