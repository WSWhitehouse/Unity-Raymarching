using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : RaymarchBase
{
  [SerializeField] public ShaderFeatureImpl<SDFShaderFeature> raymarchSDF;
  [SerializeField] public ShaderFeatureImpl<MaterialShaderFeature> raymarchMat;

  [SerializeField] public List<ToggleableShaderFeatureImpl<ModifierShaderFeature>> raymarchMods =
    new List<ToggleableShaderFeatureImpl<ModifierShaderFeature>>();

  public Vector4 Position
  {
    get
    {
      if (!Transform4DEnabled) return transform.position;

      Vector3 pos = transform.position;
      return new Vector4(pos.x, pos.y, pos.z, positionW);
    }
    set
    {
      transform.position = value;
      positionW = value.w;
    }
  }

  public Vector4 RotationRotor3D
  {
    get
    {
      Quaternion rot = transform.rotation;
      return new Vector4(rot.z, -rot.y, rot.x, rot.w);
    }
  }

  public Vector4 Scale
  {
    get
    {
      Vector3 scale = transform.lossyScale;
      return new Vector4(scale.x, scale.y, scale.z, scaleW) * 0.5f;
    }
  }

  [SerializeField] private bool transform4DEnabled = false;

  public bool Transform4DEnabled
  {
    get => transform4DEnabled;
    set => transform4DEnabled = value;
  }

  [SerializeField] public float positionW = 0f;
  [SerializeField] private Vector3 rotation4D = Vector3.zero;
  [SerializeField] private float scaleW = 1f; // global scale

  public Vector3 Rotation4D
  {
    get => rotation4D;
    set => rotation4D = value;
  }

  public float LossyScaleW
  {
    get => scaleW;
    set => scaleW = value;
  }

  [SerializeField] private Color colour = Color.white;

  public Color Colour
  {
    get => colour;
    set => colour = value;
  }

  [SerializeField] private float marchingStepAmount = 1f;

  public float MarchingStepAmount
  {
    get => marchingStepAmount;
    set => marchingStepAmount = value;
  }

  private struct ShaderIDs
  {
    public int Position;
    public int RotationRotor3D;
    public int Scale;
    public int Colour;
    public int MarchingStepAmount;
    public int Transform4DEnabled;
    public int Rotation4D;
  }

  private ShaderIDs _shaderIDs = new ShaderIDs();

  private void InitShaderIDs()
  {
    string guid = GUID.ToShaderSafeString();

    _shaderIDs.Position = Shader.PropertyToID($"_{nameof(Position)}{guid}");
    _shaderIDs.RotationRotor3D = Shader.PropertyToID($"_{nameof(RotationRotor3D)}{guid}");
    _shaderIDs.Rotation4D = Shader.PropertyToID($"_{nameof(Rotation4D)}{guid}");
    _shaderIDs.Scale = Shader.PropertyToID($"_{nameof(Scale)}{guid}");
    _shaderIDs.Colour = Shader.PropertyToID($"_{nameof(Colour)}{guid}");
    _shaderIDs.MarchingStepAmount = Shader.PropertyToID($"_{nameof(MarchingStepAmount)}{guid}");
    _shaderIDs.Transform4DEnabled = Shader.PropertyToID($"_{nameof(Transform4DEnabled)}{guid}");
  }

  public override void Awake()
  {
    raymarchSDF.Awake(GUID);
    raymarchMat.Awake(GUID);

    for (var i = 0; i < raymarchMods.Count; i++)
    {
      var mod = raymarchMods[i];
      mod.Awake(GUID, i.ToString());
    }

    InitShaderIDs();
    Raymarch.UploadShaderDataAddCallback(UploadShaderData);

    base.Awake();
  }

  protected override void OnDestroy()
  {
    raymarchSDF.OnDestroy();
    raymarchMat.OnDestroy();

    foreach (var mod in raymarchMods)
    {
      mod.OnDestroy();
    }

    Raymarch.UploadShaderDataRemoveCallback(UploadShaderData);

    base.OnDestroy();
  }

  public override bool IsValid()
  {
    return raymarchSDF.IsValid();
  }

  private void UploadShaderData(Material material)
  {
    material.SetVector(_shaderIDs.Position, Position);
    material.SetVector(_shaderIDs.RotationRotor3D, RotationRotor3D);
    material.SetVector(_shaderIDs.Rotation4D, Rotation4D);
    material.SetVector(_shaderIDs.Scale, Scale);
    material.SetVector(_shaderIDs.Colour, Colour);
    material.SetFloat(_shaderIDs.MarchingStepAmount, MarchingStepAmount);
    material.SetInteger(_shaderIDs.Transform4DEnabled, Transform4DEnabled ? 1 : 0);
  }

#if UNITY_EDITOR
  protected override string GetShaderVariablesImpl()
  {
    StringBuilder result = new StringBuilder();

    result.AppendLine(raymarchSDF.GetShaderVariables(GUID));
    result.AppendLine(raymarchMat.GetShaderVariables(GUID));

    foreach (var raymarchMod in raymarchMods)
    {
      result.AppendLine(raymarchMod.GetShaderVariables(GUID));
    }

    string guid = GUID.ToShaderSafeString();

    result.AppendLine($"uniform float4 _{nameof(Position)}{guid};");
    result.AppendLine($"uniform float4 _{nameof(RotationRotor3D)}{guid};");
    result.AppendLine($"uniform float3 _{nameof(Rotation4D)}{guid};");
    result.AppendLine($"uniform float4 _{nameof(Scale)}{guid};");
    result.AppendLine($"uniform float4 _{nameof(Colour)}{guid};");
    result.AppendLine($"uniform float _{nameof(MarchingStepAmount)}{guid};");
    result.AppendLine($"uniform int _{nameof(Transform4DEnabled)}{guid};");

    return result.ToString();
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchObject)), CanEditMultipleObjects]
public class RaymarchObjectEditor : RaymarchBaseEditor
{
  private RaymarchObject Target => target as RaymarchObject;

  private static bool _materialDropDown = false;
  private static bool _modifierDropDown = false;

  private readonly string[] _transformTypeNames = new[] {"3D Transform", "4D Transform"};

  protected override void DrawInspector()
  {
    EditorGUILayout.LabelField("Transform", BoldLabelStyle);
    Target.Transform4DEnabled = GUILayout.Toolbar(Target.Transform4DEnabled ? 1 : 0, _transformTypeNames) == 1;

    if (Target.Transform4DEnabled)
    {
      Target.positionW = EditorGUILayout.FloatField(
        new GUIContent("4D Position", "This is the 'w' position"), Target.positionW);
      Target.Rotation4D = EditorGUILayout.Vector3Field(new GUIContent("4D Rotation"), Target.Rotation4D);
      Target.LossyScaleW = EditorGUILayout.FloatField(
        new GUIContent("4D Scale", "This is the 'w' scale"), Target.LossyScaleW);
    }

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("Signed Distance Function", BoldLabelStyle);

    EditorGUI.BeginChangeCheck();
    Target.raymarchSDF.ShaderFeature =
      (SDFShaderFeature) EditorGUILayout.ObjectField(GUIContent.none, Target.raymarchSDF.ShaderFeature,
        typeof(SDFShaderFeature), false);
    if (EditorGUI.EndChangeCheck())
    {
      ShaderGen.GenerateRaymarchShader();
    }

    Target.raymarchSDF =
      ShaderFeatureImpl<SDFShaderFeature>.Editor.ShaderVariableField(new GUIContent("SDF Variables"),
        Target.raymarchSDF);

    Target.MarchingStepAmount =
      EditorGUILayout.FloatField(new GUIContent("Marching Step Amount",
          "Increase this value to reduce visual glitches (especially useful when using modifiers). " +
          "However, increasing this value also reduces the performance - so it's a fine balance between what looks good and the performance."),
        Target.MarchingStepAmount);

    EditorGUILayout.Space();

    _materialDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_materialDropDown, new GUIContent("Material"));
    if (_materialDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      EditorGUILayout.HelpBox(
        "Materials describe how the Raymarch Object will look. Leave the following material field empty to just use a colour.",
        MessageType.Info, true);

      EditorGUI.BeginChangeCheck();
      Target.raymarchMat.ShaderFeature =
        (MaterialShaderFeature) EditorGUILayout.ObjectField(new GUIContent("Material"),
          Target.raymarchMat.ShaderFeature,
          typeof(MaterialShaderFeature), false);
      if (EditorGUI.EndChangeCheck())
      {
        ShaderGen.GenerateRaymarchShader();
      }

      Target.Colour = EditorGUILayout.ColorField(new GUIContent("Colour"), Target.Colour);

      Target.raymarchMat =
        ShaderFeatureImpl<MaterialShaderFeature>.Editor.ShaderVariableField(GUIContent.none, Target.raymarchMat);

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();

    _modifierDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_modifierDropDown, new GUIContent("Modifiers"));
    if (_modifierDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      if (Target.raymarchMods.Count <= 0)
      {
        EditorGUILayout.HelpBox("There are no modifiers on this object!", MessageType.Info, true);
      }

      for (int i = 0; i < Target.raymarchMods.Count; i++)
      {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // Target.raymarchMods[i].IsEnabled = EditorGUILayout.Toggle(Target.raymarchMods[i].IsEnabled);

        string label = $"[{i.ToString()}] ";
        if (Target.raymarchMods[i].ShaderFeature != null)
        {
          label += Target.raymarchMods[i].ShaderFeature.name;
        }

        // EditorGUILayout.LabelField(label, BoldLabelStyle);

        Target.raymarchMods[i].IsEnabled =
          EditorGUILayout.ToggleLeft(new GUIContent(label), Target.raymarchMods[i].IsEnabled, BoldLabelStyle);

        EditorGUI.BeginChangeCheck();
        Target.raymarchMods[i].ShaderFeature =
          (ModifierShaderFeature) EditorGUILayout.ObjectField(GUIContent.none, Target.raymarchMods[i].ShaderFeature,
            typeof(ModifierShaderFeature), false);
        if (EditorGUI.EndChangeCheck())
        {
          ShaderGen.GenerateRaymarchShader();
        }

        Target.raymarchMods[i] =
          ShaderFeatureImpl<ModifierShaderFeature>.Editor.ShaderVariableField(GUIContent.none, Target.raymarchMods[i])
            as ToggleableShaderFeatureImpl<ModifierShaderFeature>;

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.25f);

        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;

        Rect arrowButtonsRect = rect;
        arrowButtonsRect.width *= 0.5f;

        bool guiEnabledCache = GUI.enabled;
        if (i <= 0)
        {
          GUI.enabled = false;
        }

        GUIStyle leftStyle = GUI.skin.FindStyle($"{GUI.skin.button.name}left");
        GUIStyle rightStyle = GUI.skin.FindStyle($"{GUI.skin.button.name}right");
        
        if (GUI.Button(arrowButtonsRect, "▲", leftStyle))
        {
          MoveRaymarchMods(i, i - 1);
        }

        GUI.enabled = guiEnabledCache;

        if (i >= Target.raymarchMods.Count - 1)
        {
          GUI.enabled = false;
        }

        arrowButtonsRect.x += arrowButtonsRect.width;

        if (GUI.Button(arrowButtonsRect, "▼", rightStyle))
        {
          MoveRaymarchMods(i, i + 1);
        }

        GUI.enabled = guiEnabledCache;

        rect.x += rect.width;

        if (GUI.Button(rect, "Remove Modifier"))
        {
          Target.raymarchMods.RemoveAt(i);
          ShaderGen.GenerateRaymarchShader();
          break; // break out of loop so iter doesnt get messed up!
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
      }

      if (GUILayout.Button("Add New Modifier"))
      {
        Target.raymarchMods.Add(new ToggleableShaderFeatureImpl<ModifierShaderFeature>());
      }

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();
  }

  private void MoveRaymarchMods(int oldIndex, int newIndex)
  {
    var item = Target.raymarchMods[oldIndex];
    Target.raymarchMods.RemoveAt(oldIndex);
    Target.raymarchMods.Insert(newIndex, item);
    ShaderGen.GenerateRaymarchShader();
  }
}
#endif