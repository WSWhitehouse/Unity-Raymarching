using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SdfShape
{
  Sphere = 0,
  Cube = 1,
}

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchObject : MonoBehaviour
{
  // Dirty Flag
  private DirtyFlag _dirtyFlag;

  public DirtyFlag DirtyFlag
  {
    get
    {
      if (_dirtyFlag == null)
      {
        _dirtyFlag = new DirtyFlag(transform);
      }

      return _dirtyFlag;
    }
  }
  
  [SerializeField] private SdfShape sdfShape;

  public SdfShape SdfShape
  {
    get => sdfShape;
    set => DirtyFlag.SetField(ref sdfShape, value);
  }

  [SerializeField] private float marchingStepAmount = 1;

  public float MarchingStepAmount
  {
    get => marchingStepAmount;
    set => DirtyFlag.SetField(ref marchingStepAmount, value);
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

  private void OnEnable()
  {
    DirtyFlag.SetDirty();
  }

  private void OnDisable()
  {
    DirtyFlag.SetDirty();
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
  }
}
#endif