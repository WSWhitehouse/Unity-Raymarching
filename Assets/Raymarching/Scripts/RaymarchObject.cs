using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : RaymarchBase
{
  public override void Awake()
  {
#if UNITY_EDITOR
    if (raymarchSDF != null)
    {
      raymarchSDF.OnShaderValuesChanged += OnShaderValuesChanged;
    }
#endif

    InitShaderIDs();
    base.Awake();
  }

  protected override void OnDestroy()
  {
#if UNITY_EDITOR
    if (raymarchSDF != null)
    {
      raymarchSDF.OnShaderValuesChanged -= OnShaderValuesChanged;
    }
#endif

    base.OnDestroy();
  }

#if UNITY_EDITOR
  private void OnShaderValuesChanged()
  {
    int count = raymarchSDF.shaderVariables.Count;

    var newVariables = new List<ShaderVariable>(count);

    for (int i = 0; i < count; i++)
    {
      int index = sdfVariables.FindIndex(x =>
        x.Name == raymarchSDF.shaderVariables[i].Name);

      if (index < 0) // variable not found
      {
        newVariables.Add(raymarchSDF.shaderVariables[i]);
        continue;
      }

      newVariables.Add(sdfVariables[index].ShaderType != raymarchSDF.shaderVariables[i].ShaderType
        ? raymarchSDF.shaderVariables[i]
        : sdfVariables[index]);
    }

    sdfVariables = newVariables;
  }
#endif

  [SerializeField] private RaymarchSDF raymarchSDF;

  public RaymarchSDF RaymarchSDF
  {
    get => raymarchSDF;
#if UNITY_EDITOR
    set
    {
      if (EqualityComparer<RaymarchSDF>.Default.Equals(raymarchSDF, value)) return;


      if (raymarchSDF != null)
      {
        raymarchSDF.OnShaderValuesChanged -= OnShaderValuesChanged;
      }

      if (value == null)
      {
        raymarchSDF = null;
        sdfVariables.Clear();
      }
      else
      {
        raymarchSDF = value;
        OnShaderValuesChanged();
      }

      if (raymarchSDF != null)
      {
        raymarchSDF.OnShaderValuesChanged += OnShaderValuesChanged;
      }

      if (Application.isPlaying) return;
      EditorUtility.SetDirty(this);
    }
#endif
  }

  [SerializeField] public List<ShaderVariable> sdfVariables;

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

    shaderIDs.SdfValues = new int[sdfVariables.Count];
    for (int i = 0; i < shaderIDs.SdfValues.Length; i++)
    {
      shaderIDs.SdfValues[i] = Shader.PropertyToID(sdfVariables[i].GetShaderName(GUID));
    }
  }

  public override bool IsValid()
  {
    return raymarchSDF != null;
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
  public override string GetShaderCode_Variables()
  {
    string guid = GUID.ToShaderSafeString();

    var code = string.Concat("uniform float3 _Position", guid, ShaderGen.SemiColon, ShaderGen.NewLine);
    code = string.Concat(code, "uniform float3 _Rotation", guid, ShaderGen.SemiColon, ShaderGen.NewLine);
    code = string.Concat(code, "uniform float3 _Scale", guid, ShaderGen.SemiColon, ShaderGen.NewLine);
    code = string.Concat(code, "uniform float4 _Colour", guid, ShaderGen.SemiColon, ShaderGen.NewLine);

    for (int i = 0; i < sdfVariables.Count; i++)
    {
      code = string.Concat(code, sdfVariables[i].ToShaderVariable(GUID), ShaderGen.NewLine);
    }

    return code;
  }

  public string GetShaderCode_CalcDistance()
  {
    string guid = GUID.ToShaderSafeString();

    return string.Concat("float distance", guid, " = ", raymarchSDF.FunctionNameWithGuid,
      "(", GetShaderDistanceParameters(), ")", ShaderGen.SemiColon);
  }

  private string GetShaderDistanceParameters()
  {
    string guid = GUID.ToShaderSafeString();
    string parameters = string.Concat("position", guid, ", ", "_Scale", guid);

    for (int i = 0; i < sdfVariables.Count; i++)
    {
      parameters = string.Concat(parameters, ", ", sdfVariables[i].GetShaderName(GUID));
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

    for (int i = 0; i < sdfVariables.Count; i++)
    {
      sdfVariables[i].UploadToShader(material, shaderIDs.SdfValues[i]);
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
    Target.RaymarchSDF = (RaymarchSDF) EditorGUILayout.ObjectField(Target.RaymarchSDF, typeof(RaymarchSDF), false);

    EditorGUILayout.Space();

    EditorGUILayout.LabelField("SDF Shader Values", boldStyle);

    if (Target.sdfVariables.Count <= 0)
    {
      EditorGUILayout.LabelField("There are no values for this SDF");
    }

    for (int i = 0; i < Target.sdfVariables.Count; i++)
    {
      EditorGUI.BeginChangeCheck();

      var variable = ShaderVariable.Editor.VariableField(Target.sdfVariables[i]);

      if (EditorGUI.EndChangeCheck())
      {
        Target.sdfVariables[i] = variable;
        EditorUtility.SetDirty(Target);
      }
    }

    serializedObject.ApplyModifiedProperties();
  }
}
#endif