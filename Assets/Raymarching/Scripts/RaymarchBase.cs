using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Assertions;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public abstract class RaymarchBase : MonoBehaviour
{
  #region GUID

  [SerializeField, HideInInspector] private SerializableGuid guid;
  public SerializableGuid GUID => guid;

  #endregion GUID
  
  [SerializeField, HideInInspector] private bool isHardcoded = false;

  public bool IsHardcoded
  {
    get => isHardcoded;

#if UNITY_EDITOR
    // NOTE(WSWhitehouse): Should not change this during runtime, for editor only
    set
    {
      Assert.IsFalse(Application.isPlaying);
      isHardcoded = value;
    }
#endif
  }
  
  [UploadToShader] public bool IsActive => gameObject.activeInHierarchy /*&& enabled*/;
  
  private ShaderIDs _shaderIDs = new ShaderIDs();
  
  public abstract bool IsValid();

  public virtual void Awake()
  {
#if UNITY_EDITOR
    Raymarch.OnUploadShaderData -= UploadShaderData;
#endif

    if (IsHardcoded)
      return;

    Raymarch.OnUploadShaderData += UploadShaderData;

    _shaderIDs.Init(this, GUID);
  }

  protected virtual void OnDestroy()
  {
    Raymarch.OnUploadShaderData -= UploadShaderData;
  }

  protected virtual void UploadShaderData(Material material)
  {
    _shaderIDs.UploadShaderData(this, material);
  }

#if UNITY_EDITOR
  public virtual string GetShaderCode_Variables()
  {
    string GetTypeToShaderType(Type type)
    {
      if (type == typeof(float))
      {
        return "float";
      }

      if (type == typeof(int) ||
          type == typeof(bool))
      {
        return "int";
      }

      if (type == typeof(Vector2))
      {
        return "float2";
      }

      if (type == typeof(Vector3))
      {
        return "float3";
      }

      if (type == typeof(Vector4) ||
          type == typeof(Color))
      {
        return "float4";
      }

      return "UNKNOWN_TYPE";
    }

    var properties = GetType().GetProperties();

    string guid = GUID.ToShaderSafeString();
    string variables = string.Empty;
    foreach (var property in properties)
    {
      if (property.GetCustomAttribute(typeof(UploadToShaderAttribute)) is UploadToShaderAttribute)
      {
        variables = String.Concat(variables,
          $"uniform {GetTypeToShaderType(property.PropertyType)} _{property.Name}{guid};{ShaderGen.NewLine}");
      }
    }

    return variables;
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchBase))]
public class RaymarchBaseEditor : Editor
{
  private RaymarchBase Target => target as RaymarchBase;

  private readonly GUIContent[] hardcodedToolbarContents = new[]
  {
    new GUIContent("Runtime Values",
      "Values are uploaded to the shader during runtime, this is more expensive but allows this object to change during runtime."),
    new GUIContent("Hardcoded Values",
      "Values are hardcoded into the shader. This is less expensive but you CANNOT change the values at runtime.")
  };

  protected GUIStyle BoldLabelStyle;

  // NOTE(WSWhitehouse): Do *NOT* override this function as it handles enabling/disabling
  // of the GUI while in play mode. Instead, override "DrawInspector()". The function is
  // marked sealed to ensure it cannot be overriden.
  public sealed override void OnInspectorGUI()
  {
    BoldLabelStyle = new GUIStyle(GUI.skin.GetStyle("label"))
    {
      fontStyle = FontStyle.Bold
    };

    serializedObject.Update();

    EditorGUI.BeginChangeCheck(); // global change check

    bool guiEnabledCache = GUI.enabled;
    if (Target.IsHardcoded && Application.isPlaying)
    {
      EditorGUILayout.HelpBox("You cannot edit hardcoded values during runtime.", MessageType.Info, true);
      GUI.enabled = false;
    }

    // hardcoded toolbar
    {
      EditorGUILayout.LabelField("Raymarch Value Type", BoldLabelStyle);
      bool ishardcoded = GUILayout.Toolbar(Target.IsHardcoded ? 1 : 0, hardcodedToolbarContents) == 1;

      if (ishardcoded != Target.IsHardcoded && !Application.isPlaying)
      {
        Target.IsHardcoded = ishardcoded;
      }
    }

    DrawInspector();

    if (EditorGUI.EndChangeCheck()) // global change check
    {
      EditorUtility.SetDirty(Target);

      if (Target.IsHardcoded)
        ShaderGen.GenerateRaymarchShader();
    }

    GUI.enabled = guiEnabledCache;

    serializedObject.ApplyModifiedProperties();
  }

  protected virtual void DrawInspector()
  {
  }
}
#endif