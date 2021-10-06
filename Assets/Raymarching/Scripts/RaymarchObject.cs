using System;
using System.Collections.Generic;
using UnityEngine;

// Mathematics
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : RaymarchBase
{
  protected override void Awake()
  {
#if UNITY_EDITOR
    if (sdf != null)
    {
      sdf.OnShaderValuesChanged += OnShaderValuesChanged;
    }
#endif

    InitShaderIDs();
    base.Awake();
  }

  protected override void OnDestroy()
  {
#if UNITY_EDITOR
    if (sdf != null)
    {
      sdf.OnShaderValuesChanged -= OnShaderValuesChanged;
    }
#endif

    base.OnDestroy();
  }

#if UNITY_EDITOR
  private void OnShaderValuesChanged()
  {
    sdfValues.InitValues(sdf.shaderValues);
  }
#endif

  [SerializeField] private RaymarchSDF sdf;

  public RaymarchSDF SDF
  {
    get => sdf;
    set
    {
      if (EqualityComparer<RaymarchSDF>.Default.Equals(sdf, value)) return;

#if UNITY_EDITOR
      if (sdf != null)
      {
        sdf.OnShaderValuesChanged -= OnShaderValuesChanged;
      }
#endif

      if (value == null)
      {
        sdf = null;
        sdfValues.ClearValues();
      }
      else
      {
        sdf = value;
        sdfValues.InitValues(sdf.shaderValues);
      }

#if UNITY_EDITOR
      if (sdf != null)
      {
        sdf.OnShaderValuesChanged += OnShaderValuesChanged;
      }

      if (Application.isPlaying) return;
      EditorUtility.SetDirty(this);
#endif
    }
  }

  [SerializeField] public ShaderValues sdfValues;

  private struct ShaderIDs
  {
    public int Position;
    public int Rotation;
    public int Scale;
    public int Colour;

    public int[] SdfValues;
  }

  private ShaderIDs shaderIDs;

  private void InitShaderIDs()
  {
    string guid = GUID.ToShaderSafeString();

    shaderIDs.Position = Shader.PropertyToID(string.Concat("_Position", guid));
    shaderIDs.Rotation = Shader.PropertyToID(string.Concat("_Rotation", guid));
    shaderIDs.Scale = Shader.PropertyToID(string.Concat("_Scale", guid));
    shaderIDs.Colour = Shader.PropertyToID(string.Concat("_Colour", guid));

    shaderIDs.SdfValues = new int[sdfValues.Count];
    for (int i = 0; i < shaderIDs.SdfValues.Length; i++)
    {
      shaderIDs.SdfValues[i] = Shader.PropertyToID(string.Concat("_", sdfValues._keys[i], guid));
    }
  }

  public override bool IsValid()
  {
    return sdf != null;
  }

  public Vector3 Position => transform.position;

  private Vector3 Rotation =>
    new(transform.eulerAngles.x * Mathf.Deg2Rad,
      transform.eulerAngles.y * Mathf.Deg2Rad,
      transform.eulerAngles.z * Mathf.Deg2Rad);

  public Vector3 Scale => transform.lossyScale * 0.5f;

  [SerializeField] private Color colour =
    Color.white;

  public Color Colour
  {
    get => colour;
    set => DirtyFlag.SetField(ref colour, value);
  }

  [SerializeField, Range(0, 1)] private float roundness = 0f;

  public float Roundness
  {
    get
    {
      float minScale = Mathf.Min(Scale.x, Mathf.Min(Scale.y, Scale.z));
      float maxRoundness = minScale * 0.5f;
      return roundness * maxRoundness;
    }
    set => DirtyFlag.SetField(ref roundness, Mathf.Clamp(value, 0f, 1f));
  }

  [SerializeField] private bool hollow = false;

  public bool Hollow
  {
    get => hollow;
    set => DirtyFlag.SetField(ref hollow, value);
  }

  [SerializeField, Range(0, 1)] private float wallThickness;

  public float WallThickness
  {
    get
    {
      float thickness = hollow ? wallThickness : 1.0f;
      float minScale = Mathf.Min(Scale.x, Mathf.Min(Scale.y, Scale.z));
      float maxThickness = minScale * 0.5f;
      return thickness * maxThickness;
    }
    set => DirtyFlag.SetField(ref wallThickness, Mathf.Clamp(value, 0f, 1f));
  }

#if UNITY_EDITOR
  // Shader
  public override string GetShaderVariablesCode()
  {
    string guid = GUID.ToShaderSafeString();

    var code = string.Concat("uniform float3 _Position", guid, ShaderGen.SemiColon, ShaderGen.NewLine);
    code = string.Concat(code, "uniform float3 _Rotation", guid, ShaderGen.SemiColon, ShaderGen.NewLine);
    code = string.Concat(code, "uniform float3 _Scale", guid, ShaderGen.SemiColon, ShaderGen.NewLine);
    code = string.Concat(code, "uniform float4 _Colour", guid, ShaderGen.SemiColon, ShaderGen.NewLine);

    for (int i = 0; i < sdfValues.Count; i++)
    {
      code = string.Concat(code,
        "uniform ", sdfValues._values[i].TypeToShaderType(),
        " _", sdfValues._keys[i], guid,
        ShaderGen.SemiColon, ShaderGen.NewLine);
    }

    return code;
  }

  public override string GetShaderDistanceCode()
  {
    string guid = GUID.ToShaderSafeString();

    return string.Concat("float distance", guid, " = ", sdf.FunctionNameWithGUID,
      "(", GetShaderDistanceParameters(), ")", ShaderGen.SemiColon);
  }

  private string GetShaderDistanceParameters()
  {
    string guid = GUID.ToShaderSafeString();
    string parameters = string.Concat("position", guid, ", ", "_Scale", guid);

    for (int i = 0; i < sdfValues.Count; i++)
    {
      parameters = string.Concat(parameters,
        ", _", sdfValues._keys[i], guid);
    }

    return parameters;
  }

#endif

  protected override void UploadShaderData(Material material)
  {
    material.SetVector(shaderIDs.Position, Position);
    material.SetVector(shaderIDs.Rotation, Rotation);
    material.SetVector(shaderIDs.Scale, Scale);
    material.SetVector(shaderIDs.Colour, Colour);

    for (int i = 0; i < sdfValues.Count; i++)
    {
      int shaderID = shaderIDs.SdfValues[i];

      switch (sdfValues._values[i].type)
      {
        case AnyValue.Type.Float:
          material.SetFloat(shaderID, sdfValues._values[i].floatValue);
          break;
        case AnyValue.Type.Int:
        case AnyValue.Type.Bool:
          material.SetInt(shaderID, sdfValues._values[i].intValue);
          break;
        case AnyValue.Type.Vector2:
        case AnyValue.Type.Vector3:
        case AnyValue.Type.Vector4:
        case AnyValue.Type.Colour:
          material.SetVector(shaderID, sdfValues._values[i].vectorValue);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchObject))]
public class RaymarchObjectEditor : Editor
{
  private RaymarchObject Target => target as RaymarchObject;

  public override void OnInspectorGUI()
  {
    EditorGUI.BeginChangeCheck();

    DrawDefaultInspector();

    if (EditorGUI.EndChangeCheck())
    {
      Target.DirtyFlag.SetDirty();
    }

    EditorGUILayout.Space();

    GUIStyle boldStyle = new GUIStyle(GUI.skin.GetStyle("label"))
    {
      fontStyle = FontStyle.Bold
    };

    serializedObject.Update();

    EditorGUILayout.LabelField("Signed Distance Function", boldStyle);
    Target.SDF = (RaymarchSDF) EditorGUILayout.ObjectField(Target.SDF, typeof(RaymarchSDF), false);

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("SDF Shader Values", boldStyle);

    if (Target.sdfValues.Count <= 0)
    {
      EditorGUILayout.LabelField("There are no values for this SDF");
    }

    for (int i = 0; i < Target.sdfValues.Count; i++)
    {
      EditorGUI.BeginChangeCheck();

      AnyValue value = Target.sdfValues._values[i];
      GUIContent label = new GUIContent(Target.sdfValues._keys[i]);

      AnyValue.EDITOR_DrawInspectorValue(ref value, label);

      if (EditorGUI.EndChangeCheck())
      {
        Target.sdfValues._values[i] = value;
        EditorUtility.SetDirty(Target);
      }
    }

    serializedObject.ApplyModifiedProperties();
  }
}
#endif