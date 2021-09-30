using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SdfShape
{
  Sphere = 0,
  Cube = 1,
}

public enum _Operation
{
  None = 0,
  Blend = 1,
  Cut = 2,
  Mask = 3
}

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : MonoBehaviour
{
  // Dirty Flag
  private bool _objectDirty = true;
  public bool IsDirty => _objectDirty || transform.hasChanged;

  public void SetDirty()
  {
    _objectDirty = true;
  }

  public void ResetDirtyFlag()
  {
    _objectDirty = false;
    transform.hasChanged = false;
  }

  private void SetField<T>(ref T field, T value)
  {
    if (EqualityComparer<T>.Default.Equals(field, value)) return;

    field = value;
    SetDirty();
  }

  [SerializeField] private SdfShape sdfShape;

  public SdfShape SdfShape
  {
    get => sdfShape;
    set => SetField(ref sdfShape, value);
  }

  [SerializeField] private float marchingStepAmount = 1;

  public float MarchingStepAmount
  {
    get => marchingStepAmount;
    set => SetField(ref marchingStepAmount, value);
  }

  public Vector3 Position => transform.position;

  public Vector4 Rotation =>
    new Vector4(transform.eulerAngles.x * Mathf.Deg2Rad,
      transform.eulerAngles.y * Mathf.Deg2Rad,
      transform.eulerAngles.z * Mathf.Deg2Rad,
      0);

  public Vector3 Scale => transform.lossyScale;

  [SerializeField] private Color colour = Color.white;

  public Color Colour
  {
    get => colour;
    set => SetField(ref colour, value);
  }

  [SerializeField] private _Operation operation = _Operation.None;

  public _Operation Operation
  {
    get => operation;
    set => SetField(ref operation, value);
  }

  [SerializeField] private bool operationSmooth = true;

  public bool OperationSmooth
  {
    get => operationSmooth;
    set => SetField(ref operationSmooth, value);
  }

  [SerializeField] private float operationMod = 1.0f;

  public float OperationMod
  {
    get => operationMod;
    set => SetField(ref operationMod, value);
  }

  [SerializeField] private int operationLayer = 0;

  public int OperationLayer
  {
    get => operationLayer;
    set => SetField(ref operationLayer, value);
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
    set => SetField(ref roundness, Mathf.Clamp(value, 0f, 1f));
  }

  [SerializeField] private bool hollow = false;

  public bool Hollow
  {
    get => hollow;
    set => SetField(ref hollow, value);
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
    set => SetField(ref wallThickness, Mathf.Clamp(value, 0f, 1f));
  }


  private void Awake()
  {
    Raymarch.AddObject(this);
  }

  private void OnDestroy()
  {
    Raymarch.RemoveObject(this);
  }

#if UNITY_EDITOR
  // Allows objects to be added to list while not playing or in scene view
  private void OnValidate()
  {
    Awake();
  }
#endif
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
      Target.SetDirty();
    }
  }
}
#endif