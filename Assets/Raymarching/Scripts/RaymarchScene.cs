using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchScene : MonoBehaviour
{
  // Raymarch Settings
  [SerializeField] private RaymarchSettings settings;

  public RaymarchSettings Settings
  {
    get => settings;
    set
    {
      settings = value;
      UpdateRaymarchData();

#if UNITY_EDITOR
      EditorUtility.SetDirty(this);
#endif
    }
  }

  // Active Raymarch Shader
  [SerializeField] private Shader shader;

  public Shader Shader
  {
    get => shader;
    set
    {
      shader = value;
      UpdateRaymarchData();

#if UNITY_EDITOR
      EditorUtility.SetDirty(this);
#endif
    }
  }

  private void UpdateRaymarchData()
  {
    Raymarch.Settings = settings;
    Raymarch.Shader = shader;
  }

  /// <summary>
  /// Using a singleton here because there MUST be one per scene, the RaymarchScene object
  /// must be gotten in a static instance when generating the shader. It is *very* strongly
  /// recommended to not use this singleton outside the editor - if you need access to this
  /// object during runtime, use GameObject.Find, GetComponent, or even cache it! If
  /// ActiveInstance is null then there isn't a RaymarchScene in the active scene.
  /// </summary>
  public static RaymarchScene ActiveInstance { get; private set; } = null;

  private void Awake()
  {
    InitSingleton();
    UpdateRaymarchData();
  }

  private void InitSingleton()
  {
    // Set up Singleton
    if (ActiveInstance != null && ActiveInstance != this)
    {
      Debug.LogError(
        $"There are multiple RaymarchScenes in the current scene ({gameObject.scene.name}) - there must only be 1!");
#if UNITY_EDITOR
      EditorGUIUtility.PingObject(this);
#endif

      return;
    }

    ActiveInstance = this;
  }

  private void OnDestroy()
  {
    if (ActiveInstance == this)
      ActiveInstance = null;
  }


#if UNITY_EDITOR

  [SerializeField] public Shader templateShader;

  private void OnEnable()
  {
    if (Application.isPlaying) return;
    EditorSceneManager.sceneSaving += OnSceneSaving;

    UpdateRaymarchData();
  }

  private void OnDisable()
  {
    if (Application.isPlaying) return;
    EditorSceneManager.sceneSaving -= OnSceneSaving;
  }

  private void OnSceneSaving(Scene scene, string path)
  {
    if (scene != gameObject.scene) return; // not saving *this* scene 

    UpdateRaymarchData();
  }

  private void OnValidate()
  {
    InitSingleton();
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchScene))]
public class RaymarchSceneEditor : Editor
{
  private RaymarchScene Target => target as RaymarchScene;

  private RaymarchSettingsEditor _settingsEditor;

  // Serialized Properties
  private SerializedProperty _shaderProperty;
  private SerializedProperty _templateShaderProperty;

  private void OnEnable()
  {
    UpdateSettingsEditor();

    _shaderProperty = serializedObject.FindProperty("shader");
    _templateShaderProperty = serializedObject.FindProperty("templateShader");
  }

  private void OnDisable()
  {
    if (_settingsEditor != null)
    {
      DestroyImmediate(_settingsEditor);
    }
  }

  private void UpdateSettingsEditor()
  {
    if (_settingsEditor != null)
    {
      DestroyImmediate(_settingsEditor);
    }

    if (Target.Settings != null)
    {
      _settingsEditor = (RaymarchSettingsEditor) CreateEditor(Target.Settings);
    }
  }

  private void DrawSettingsEditor()
  {
    UpdateSettingsEditor();

    if (_settingsEditor == null) return;

    EditorGUILayout.Space();

    EditorGUILayout.BeginVertical(GUI.skin.box);
    _settingsEditor.OnInspectorGUI();
    EditorGUILayout.EndVertical();
  }

  public override void OnInspectorGUI()
  {
    GUIStyle wordWrapStyle = EditorStyles.wordWrappedLabel;
    wordWrapStyle.fontStyle = FontStyle.Bold;

    serializedObject.Update();

    EditorGUI.BeginChangeCheck();

    EditorGUILayout.LabelField("Active Shader", wordWrapStyle);

    bool cachedGUIEnabled = GUI.enabled;
    GUI.enabled = false;

    EditorGUILayout.PropertyField(_shaderProperty, GUIContent.none, true);

    GUI.enabled = cachedGUIEnabled;

    if (GUILayout.Button("Regenerate Shader"))
    {
      ShaderGen.GenerateRaymarchShader();
    }

    if (GUILayout.Button(new GUIContent("Force Render Scene",
      "If the objects aren't rendering in the scene, press this button.")))
    {
      var objects = FindObjectsOfType<RaymarchBase>();

      foreach (var rmBase in objects)
      {
        rmBase.Awake();
      }
    }

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("Template Shader", wordWrapStyle);
    EditorGUILayout.PropertyField(_templateShaderProperty, GUIContent.none, true);

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("Raymarch Settings", wordWrapStyle);
    EditorGUI.BeginChangeCheck();
    var settings = (RaymarchSettings) EditorGUILayout.ObjectField(Target.Settings, typeof(RaymarchSettings), false);
    if (EditorGUI.EndChangeCheck())
    {
      Target.Settings = settings;
    }

    DrawSettingsEditor();

    EditorGUILayout.Space();

    if (EditorGUI.EndChangeCheck())
    {
      EditorUtility.SetDirty(Target);
    }

    serializedObject.ApplyModifiedProperties();
  }
}
#endif