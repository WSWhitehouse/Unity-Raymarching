using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum _Operation
{
  None = 0,
  Blend = 1,
  Cut = 2,
  Mask = 3
}

[DisallowMultipleComponent, ExecuteAlways]
public class RaymarchModifier : MonoBehaviour
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

  [SerializeField] private _Operation operation = _Operation.None;

  public _Operation Operation
  {
    get => operation;
    set => DirtyFlag.SetField(ref operation, value);
  }

  [SerializeField] private bool operationSmooth = true;

  public bool OperationSmooth
  {
    get => operationSmooth;
    set => DirtyFlag.SetField(ref operationSmooth, value);
  }

  [SerializeField] private float operationMod = 1.0f;

  public float OperationMod
  {
    get => operationMod;
    set => DirtyFlag.SetField(ref operationMod, value);
  }

  public int NumOfObjects { get; set; } = -1;
  public int Index { get; set; } = -1;
  
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
[CustomEditor(typeof(RaymarchModifier))]
public class RaymarchModifierEditor : Editor
{
  private RaymarchModifier Target => target as RaymarchModifier;

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