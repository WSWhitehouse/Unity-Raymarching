using UnityEngine;

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public abstract class RaymarchBase : MonoBehaviour
{
  #region GUID

  [SerializeField, HideInInspector] private SerializableGuid guid;
  public SerializableGuid GUID => guid;

  #endregion GUID

  public bool IsActive => gameObject.activeInHierarchy /*&& enabled*/;

  private struct ShaderIDs
  {
    public int IsActive;
  }

  private ShaderIDs _shaderIDs = new ShaderIDs();

  public abstract bool IsValid();

  public virtual void Awake()
  {
    Raymarch.UploadShaderDataAddCallback(UploadShaderData);

    string guid = GUID.ToShaderSafeString();
    _shaderIDs.IsActive = Shader.PropertyToID($"_{nameof(IsActive)}{guid}");
  }

  protected virtual void OnDestroy()
  {
    Raymarch.UploadShaderDataRemoveCallback(UploadShaderData);
  }

  private void UploadShaderData(Material material)
  {
    material.SetInteger(_shaderIDs.IsActive, IsActive ? 1 : 0);
  }

#if UNITY_EDITOR
  public string GetShaderVariables()
  {
    string guid = GUID.ToShaderSafeString();
    StringBuilder result = new StringBuilder();

    result.AppendLine($"uniform int _{nameof(IsActive)}{guid};");

    return string.Concat(result.ToString(), GetShaderVariablesImpl());
  }

  protected virtual string GetShaderVariablesImpl()
  {
    return string.Empty;
  }

  [MenuItem("CONTEXT/RaymarchBase/Force Render Scene")]
  private static void ForceRenderScene()
  {
    RaymarchScene.ForceRenderScene();
  }

  [MenuItem("CONTEXT/RaymarchBase/Reset GUID")]
  private static void ResetGUID(MenuCommand command)
  {
    if (command.context is RaymarchBase rmBase)
      rmBase.guid.ResetGUIDWithShaderGen();
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchBase)), CanEditMultipleObjects]
public class RaymarchBaseEditor : Editor
{
  private RaymarchBase Target => target as RaymarchBase;

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
    GUI.enabled = false;
    EditorGUILayout.TextField(
      new GUIContent("GUID", "You cannot change the GUID manually, this is only here for debug purposes."),
      Target.GUID.ToShaderSafeString());
    GUI.enabled = guiEnabledCache;

    EditorGUILayout.Space();

    DrawInspector();

    if (EditorGUI.EndChangeCheck()) // global change check
    {
      Undo.RecordObject(Target, Target.name);
      EditorUtility.SetDirty(Target);
      // NOTE(WSWhitehouse): Don't generate shader here, only on some select variable fields (e.g. SDF, material and modifiers)!
    }

    serializedObject.ApplyModifiedProperties();
  }

  protected virtual void DrawInspector()
  {
  }
}
#endif