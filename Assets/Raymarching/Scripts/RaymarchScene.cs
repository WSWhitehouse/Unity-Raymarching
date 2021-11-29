using System;
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
  [Header("Colour")]
  public Color ambientColour = new Color(0.2117f, 0.2274f, 0.2588f, 1);
  public float colourMultiplier = 1f;

  [Header("Ambient Occlusion")] 
  public bool aoEnabled = true;
  public float aoStepSize = 0.2f;
  public float aoIntensity = 0.25f;
  public int aoIterations = 3;
}

[Serializable]
public class DebugSettings
{
  public bool enableDebugSymbols = false;
}

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchScene : MonoBehaviour
{
  /// <summary>
  /// Get currently active RM Scene. Returns null if one cannot be found.
  /// This function does NOT cache the result, so do not call in Update, etc.
  /// </summary>
  /// <returns>Currently active Raymarch Scene - can be null!</returns>
  public static RaymarchScene Get() => FindObjectOfType<RaymarchScene>();

  [SerializeField] private RaymarchSettings raymarchSettings = new RaymarchSettings();
  public RaymarchSettings RaymarchSettings => raymarchSettings;

  [SerializeField] private LightingSettings lightingSettings = new LightingSettings();
  public LightingSettings LightingSettings => lightingSettings;

  [SerializeField] private DebugSettings debugSettings = new DebugSettings();
  public DebugSettings DebugSettings => debugSettings;

  // Active Raymarch Shader
  [SerializeField] private Shader raymarchShader;

  public Shader RaymarchShader
  {
    get => raymarchShader;
    set
    {
      raymarchShader = value;
      UpdateRaymarchData();

#if UNITY_EDITOR
      EditorUtility.SetDirty(this);
#endif
    }
  }

  private void UpdateRaymarchData()
  {
    Raymarch.Shader = RaymarchShader;
  }
  
  private void Awake()
  {
    UpdateRaymarchData();
  }

  private void OnDestroy()
  {
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
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchScene)), CanEditMultipleObjects]
public class RaymarchSceneEditor : Editor
{
  private RaymarchScene Target => target as RaymarchScene;

  // Serialized Properties
  private SerializedProperty _raymarchShaderProperty;
  private SerializedProperty _templateShaderProperty;
  private SerializedProperty _raymarchSettingsProperty;
  private SerializedProperty _lightingSettingsProperty;
  private SerializedProperty _debugSettingsProperty;

  // Dropdowns
  private static bool _raymarchSettingsDropDown = false;
  private static bool _lightingSettingsDropDown = false;
  private static bool _debugSettingsDropDown    = false;
  
  // Renderer Check
  private static bool? _rendererOkay = null;

  private void OnEnable()
  {
    _raymarchShaderProperty   = serializedObject.FindProperty("raymarchShader");
    _templateShaderProperty   = serializedObject.FindProperty("templateShader");
    _raymarchSettingsProperty = serializedObject.FindProperty("raymarchSettings");
    _lightingSettingsProperty = serializedObject.FindProperty("lightingSettings");
    _debugSettingsProperty    = serializedObject.FindProperty("debugSettings");
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

    EditorGUILayout.PropertyField(_raymarchShaderProperty, GUIContent.none, true);

    GUI.enabled = cachedGUIEnabled;

    if (GUILayout.Button("Regenerate Shader"))
    {
      RaymarchShaderGen.GenerateRaymarchShader();
    }

    if (GUILayout.Button(new GUIContent("Force Render Scene",
      "If the objects aren't rendering in the scene, press this button.")))
    {
     RaymarchShaderGen.ForceRenderScene();
    }
    
    if (GUILayout.Button(new GUIContent("Perform Renderer Checks",
      "Checks to see if the renderer is set up correctly")))
    {
      _rendererOkay = Raymarch.PerformRendererChecks();
    }

    string rendererOkayLabel = "Not performed renderer checks";
    if (_rendererOkay.HasValue)
    {
      rendererOkayLabel = _rendererOkay.Value ? "Renderer okay" : "RaymarchRenderFeature cannot be found. Please add one.";
    }

    EditorGUILayout.LabelField($"Renderer Status: {rendererOkayLabel}");

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("Template Shader", wordWrapStyle);
    EditorGUILayout.PropertyField(_templateShaderProperty, GUIContent.none, true);

    EditorGUILayout.Space();

    _raymarchSettingsDropDown =
      EditorGUILayout.BeginFoldoutHeaderGroup(_raymarchSettingsDropDown, new GUIContent("Raymarch Settings"));
    if (_raymarchSettingsDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      var raymarchSettings = Util.Editor.GetSerializedPropertyChildren(_raymarchSettingsProperty).ToArray();
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

      var lightingSettings = Util.Editor.GetSerializedPropertyChildren(_lightingSettingsProperty).ToArray();
      foreach (var lightingSetting in lightingSettings)
      {
        EditorGUILayout.PropertyField(lightingSetting,
          new GUIContent(lightingSetting.displayName, lightingSetting.tooltip), true);
      }

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();
    
    _debugSettingsDropDown =
      EditorGUILayout.BeginFoldoutHeaderGroup(_debugSettingsDropDown, new GUIContent("Debug Settings"));
    if (_debugSettingsDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      var debugSettings = Util.Editor.GetSerializedPropertyChildren(_debugSettingsProperty).ToArray();
      foreach (var debugSetting in debugSettings)
      {
        EditorGUILayout.PropertyField(debugSetting,
          new GUIContent(debugSetting.displayName, debugSetting.tooltip), true);
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