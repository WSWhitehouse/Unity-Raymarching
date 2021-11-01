using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[Serializable]
public class RaymarchSettings
{
  public float renderDistance = 100.0f;
  public float hitResolution = 0.001f;
  public float relaxation = 1.2f;
  public int maxIterations = 164;
}

[Serializable]
public class LightingSettings
{
  public Color ambientColour = new Color(0.2117f, 0.2274f, 0.2588f, 1);
  public float colourMultiplier = 1f;
}

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchScene : MonoBehaviour
{
  [SerializeField] private RaymarchSettings raymarchSettings = new RaymarchSettings();
  public RaymarchSettings RaymarchSettings => raymarchSettings;

  [SerializeField] private LightingSettings lightingSettings = new LightingSettings();
  public LightingSettings LightingSettings => lightingSettings;

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
    if (ActiveInstance != this)
      return;

    ActiveInstance = null;
    Raymarch.ResetData();
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
    if (scene != gameObject.scene) return; // not saving this scene 

    UpdateRaymarchData();
  }

  private void OnValidate()
  {
    InitSingleton();
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchScene)), CanEditMultipleObjects]
public class RaymarchSceneEditor : Editor
{
  private RaymarchScene Target => target as RaymarchScene;

  // Serialized Properties
  private SerializedProperty _shaderProperty;
  private SerializedProperty _templateShaderProperty;
  private SerializedProperty _raymarchSettingsProperty;
  private SerializedProperty _lightingSettingsProperty;
  private SerializedProperty _skyboxSettingsProperty;

  // Dropdowns
  private static bool _raymarchSettingsDropDown = false;
  private static bool _lightingSettingsDropDown = false;
  private static bool _skyboxSettingsDropDown = false;

  private void OnEnable()
  {
    _shaderProperty = serializedObject.FindProperty("shader");
    _templateShaderProperty = serializedObject.FindProperty("templateShader");
    _raymarchSettingsProperty = serializedObject.FindProperty("raymarchSettings");
    _lightingSettingsProperty = serializedObject.FindProperty("lightingSettings");
    _skyboxSettingsProperty = serializedObject.FindProperty("skyboxSettings");
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

    // EditorGUILayout.PropertyField(_raymarchSettingsProperty, new GUIContent("Raymarch Settings"), true);
    // EditorGUILayout.PropertyField(_lightingSettingsProperty, new GUIContent("Lighting Settings"), true);

    _raymarchSettingsDropDown =
      EditorGUILayout.BeginFoldoutHeaderGroup(_raymarchSettingsDropDown, new GUIContent("Raymarch Settings"));
    if (_raymarchSettingsDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      var raymarchSettings = _raymarchSettingsProperty.GetChildren().ToArray();
      foreach (var raymarchSetting in raymarchSettings)
      {
        EditorGUILayout.PropertyField(raymarchSetting,
          new GUIContent(raymarchSetting.displayName, raymarchSetting.tooltip), true);
      }

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();

    _lightingSettingsDropDown =
      EditorGUILayout.BeginFoldoutHeaderGroup(_lightingSettingsDropDown, new GUIContent("Lighting Settings"));
    if (_lightingSettingsDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      var lightingSettings = _lightingSettingsProperty.GetChildren().ToArray();
      foreach (var lightingSetting in lightingSettings)
      {
        EditorGUILayout.PropertyField(lightingSetting,
          new GUIContent(lightingSetting.displayName, lightingSetting.tooltip), true);
      }

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();

    if (EditorGUI.EndChangeCheck())
    {
      EditorUtility.SetDirty(Target);
    }

    serializedObject.ApplyModifiedProperties();
  }
}
#endif