using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : RaymarchBase
{
  [SerializeField] public ShaderFeature<SDFShaderFeatureAsset> raymarchSDF;
  [SerializeField] public ShaderFeature<MaterialShaderFeatureAsset> raymarchMat;

  [SerializeField] public List<ToggleableShaderFeature<ModifierShaderFeatureAsset>> raymarchMods =
    new List<ToggleableShaderFeature<ModifierShaderFeatureAsset>>();

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
    base.Awake();
    
    raymarchSDF?.Awake(GUID);
    raymarchMat?.Awake(GUID);

    for (var i = 0; i < raymarchMods.Count; i++)
    {
      var mod = raymarchMods[i];
      mod?.Awake(GUID, i.ToString());
    }

    InitShaderIDs();
    Raymarch.OnUploadShaderData += UploadShaderData;
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();
    
    raymarchSDF?.OnDestroy();
    raymarchMat?.OnDestroy();

    foreach (var mod in raymarchMods)
    {
      mod?.OnDestroy();
    }

    Raymarch.OnUploadShaderData -= UploadShaderData;
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
    result.AppendLine($"uniform float  _{nameof(MarchingStepAmount)}{guid};");
    result.AppendLine($"uniform int    _{nameof(Transform4DEnabled)}{guid};");

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

  private readonly string[] _transformTypeNames = {"3D Transform", "4D Transform"};

  protected override void DrawInspector()
  {
    /* NOTE(WSWhitehouse):
     * Each section is split up into functions below, this should help organise and
     * help you follow the code as this can look quite a mess in some places.
     * But that's mainly because Unity's custom editors normally do...
     */
    
    DrawTransformInspector();

    EditorGUILayout.Space();

    DrawSDFInspector();

    EditorGUILayout.Space();

    DrawMaterialInspector();
    DrawModifierInspector();
  }

  private void DrawTransformInspector()
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
  }

  private void DrawSDFInspector()
  {
    EditorGUILayout.LabelField("Signed Distance Function", BoldLabelStyle);

    EditorGUI.BeginChangeCheck();
    Target.raymarchSDF.ShaderFeatureAsset =
      (SDFShaderFeatureAsset) EditorGUILayout.ObjectField(GUIContent.none, Target.raymarchSDF.ShaderFeatureAsset,
        typeof(SDFShaderFeatureAsset), false);
    if (EditorGUI.EndChangeCheck())
    {
      RaymarchShaderGen.GenerateRaymarchShader();
    }

    Target.raymarchSDF =
      ShaderFeature<SDFShaderFeatureAsset>.Editor.ShaderVariableField(new GUIContent("SDF Variables"),
        Target.raymarchSDF);

    Target.MarchingStepAmount =
      EditorGUILayout.FloatField(new GUIContent("Marching Step Amount",
          "Increase this value to reduce visual glitches (especially useful when using modifiers). " +
          "However, increasing this value also reduces the performance - so it's a fine balance between what looks good and the performance."),
        Target.MarchingStepAmount);
  }

  private void DrawMaterialInspector()
  {
    _materialDropDown = EditorGUILayout.BeginFoldoutHeaderGroup(_materialDropDown, new GUIContent("Material"));
    if (_materialDropDown)
    {
      EditorGUILayout.BeginVertical(GUI.skin.box);

      EditorGUILayout.HelpBox(
        "Materials describe how the Raymarch Object will look. Leave the following material field empty to just use a colour.",
        MessageType.Info, true);

      EditorGUI.BeginChangeCheck();
      Target.raymarchMat.ShaderFeatureAsset =
        (MaterialShaderFeatureAsset) EditorGUILayout.ObjectField(new GUIContent("Material"),
          Target.raymarchMat.ShaderFeatureAsset,
          typeof(MaterialShaderFeatureAsset), false);
      if (EditorGUI.EndChangeCheck())
      {
        RaymarchShaderGen.GenerateRaymarchShader();
      }

      Target.Colour = EditorGUILayout.ColorField(new GUIContent("Colour"), Target.Colour);

      Target.raymarchMat =
        ShaderFeature<MaterialShaderFeatureAsset>.Editor.ShaderVariableField(GUIContent.none, Target.raymarchMat);

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();
  }

  private void DrawModifierInspector()
  {
    void MoveRaymarchMods(int oldIndex, int newIndex)
    {
      var item = Target.raymarchMods[oldIndex];
      Target.raymarchMods.RemoveAt(oldIndex);
      Target.raymarchMods.Insert(newIndex, item);
      RaymarchShaderGen.GenerateRaymarchShader();
    }
    
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

        string label = $"[{i.ToString()}] ";
        if (Target.raymarchMods[i].ShaderFeatureAsset != null)
        {
          label = string.Concat(label, Target.raymarchMods[i].ShaderFeatureAsset.name);
        }

        EditorGUILayout.BeginHorizontal();

        Rect toolbarRect = EditorGUILayout.GetControlRect();
        Rect toggleAndNameRect = toolbarRect;
        Rect nameRect = toolbarRect;
        Rect menuButtonRect = toolbarRect;

        toggleAndNameRect.width -= toolbarRect.height;
        nameRect.x += 15f; // NOTE(WSWhitehouse): Magic Number - width of toggle box!

        menuButtonRect.width = toolbarRect.height;
        menuButtonRect.x += toggleAndNameRect.width;

        if (Target.raymarchMods[i].hardcodedShaderFeature)
        {
          bool guiEnabledCached = GUI.enabled;
          GUI.enabled = !Target.raymarchMods[i].hardcodedShaderFeature;

          EditorGUI.Toggle(toggleAndNameRect, true);

          GUI.enabled = guiEnabledCached;
        }
        else
        {
          Target.raymarchMods[i].IsEnabled = EditorGUI.Toggle(toggleAndNameRect, Target.raymarchMods[i].IsEnabled);
        }

        GUI.Label(nameRect, new GUIContent(label), BoldLabelStyle);

        if (Target.raymarchMods[i].hardcodedShaderFeature)
        {
          Rect hardcodedRect = nameRect;
          hardcodedRect.width = 80f; // NOTE(WSWhitehouse): Magic Number - width of "HARDCODED" (see code below)!
          hardcodedRect.x = menuButtonRect.x - hardcodedRect.width;

          GUI.Label(hardcodedRect, new GUIContent("HARDCODED"));
        }

        if (GUI.Button(menuButtonRect, new GUIContent(EditorGUIUtility.FindTexture("_Menu")), GUIStyle.none))
        {
          // NOTE(WSWhitehouse): Local functions to be used in the Generic Menu below
          void onToggleHardcodedModifier(object modifier)
          {
            if (modifier is not ToggleableShaderFeature<ModifierShaderFeatureAsset> modifierAs) return;
            modifierAs.hardcodedShaderFeature = !modifierAs.hardcodedShaderFeature;
            modifierAs.IsEnabled = true;
            RaymarchShaderGen.GenerateRaymarchShader();
          }

          // NOTE(WSWhitehouse): Setting up modifier Options Menu
          GenericMenu menu = new GenericMenu();
          menu.AddItem(new GUIContent("Hardcoded Modifier"), Target.raymarchMods[i].hardcodedShaderFeature,
            onToggleHardcodedModifier, Target.raymarchMods[i]);

          menu.ShowAsContext();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        Target.raymarchMods[i].ShaderFeatureAsset =
          (ModifierShaderFeatureAsset) EditorGUILayout.ObjectField(GUIContent.none, Target.raymarchMods[i].ShaderFeatureAsset,
            typeof(ModifierShaderFeatureAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
          RaymarchShaderGen.GenerateRaymarchShader();
        }

        Target.raymarchMods[i] =
          ShaderFeature<ModifierShaderFeatureAsset>.Editor.ShaderVariableField(GUIContent.none, Target.raymarchMods[i])
            as ToggleableShaderFeature<ModifierShaderFeatureAsset>;

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
          RaymarchShaderGen.GenerateRaymarchShader();
          break; // break out of loop so iter doesnt get messed up!
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
      }

      if (GUILayout.Button("Add New Modifier"))
      {
        Target.raymarchMods.Add(new ToggleableShaderFeature<ModifierShaderFeatureAsset>());
      }

      EditorGUILayout.EndVertical();
    }

    EditorGUILayout.EndFoldoutHeaderGroup();
  }
}
#endif