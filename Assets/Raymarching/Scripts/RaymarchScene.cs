using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchScene : MonoBehaviour
{
  [SerializeField] private RaymarchSettings settings;

  public RaymarchSettings Settings
  {
    get => settings;
    set
    {
      settings = value;
      Raymarch.Settings = settings;
    }
  }

  [SerializeField] private Shader shader;

  private void Awake()
  {
#if UNITY_EDITOR
    if (!Application.isPlaying)
    {
      BuildTree();
    }
#endif

    Raymarch.Settings = settings;
    Raymarch.Shader = shader;
  }

#if UNITY_EDITOR

  [SerializeField] private Shader templateShader;

  private void OnEnable()
  {
    if (Application.isPlaying) return;
    EditorSceneManager.sceneSaving += OnSceneSaving;

    Raymarch.Settings = settings;
    Raymarch.Shader = shader;
  }

  private void OnDisable()
  {
    if (Application.isPlaying) return;
    EditorSceneManager.sceneSaving -= OnSceneSaving;
  }

  private void OnSceneSaving(Scene scene, string path)
  {
    if (scene != gameObject.scene) return; // not saving *this* scene 

    Awake();
  }

  private void OnValidate()
  {
    Awake();
  }

  private List<RaymarchObject> _objects = new List<RaymarchObject>();
  private List<RaymarchBase> _bases = new List<RaymarchBase>();

  public void BuildTree()
  {
    _objects.Clear();
    _bases.Clear();

    Scene activeScene = SceneManager.GetActiveScene();

    if (!activeScene.isLoaded || !activeScene.IsValid() || templateShader == null)
    {
      return;
    }

    List<GameObject> rootGameObjects = new List<GameObject>(activeScene.rootCount);
    activeScene.GetRootGameObjects(rootGameObjects);

    foreach (var rootGameObject in rootGameObjects)
    {
      AddObjToTree(rootGameObject);
    }

    shader = ShaderGen.GenerateSceneRaymarchShader(activeScene, templateShader, _bases);

    if (shader != null)
    {
      Raymarch.Settings = settings;
      Raymarch.Shader = shader;
      return;
    }

    Debug.LogError("Generated shader is null!");
  }

  private void AddObjToTree(GameObject gameObject)
  {
    var rmBase = gameObject.GetComponent<RaymarchBase>();
    if (rmBase != null && rmBase.IsValid())
    {
      _bases.Add(rmBase);

      var rmObject = rmBase.GetComponent<RaymarchObject>();
      if (rmObject != null)
      {
        _objects.Add(rmObject);
      }

      if (!Application.isPlaying)
        rmBase.Awake();
    }

    /*// var rmMod = gameObject.GetComponent<RaymarchModifier>();
    int index = _objects.Count;

    // if (rmMod != null)
    // _bases.Add(rmMod);

    foreach (Transform child in gameObject.transform)
    {
      AddObjToTree(child.gameObject);
    }

    if (rmMod != null)
    {
      rmMod.NumOfObjects = _objects.Count - index;
      rmMod.Index = _modifiers.Count;

      _modifiers.Add(index, rmMod);
    }*/
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

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("Template Shader", wordWrapStyle);
    EditorGUILayout.PropertyField(_templateShaderProperty, GUIContent.none, true);

    if (GUILayout.Button("Regenerate Shader"))
    {
      Target.BuildTree();
    }

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("Raymarch Settings", wordWrapStyle);
    Target.Settings = (RaymarchSettings) EditorGUILayout.ObjectField(Target.Settings, typeof(RaymarchSettings), false);

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