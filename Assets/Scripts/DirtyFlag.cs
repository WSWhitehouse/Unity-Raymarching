using System.Collections.Generic;
using UnityEngine;

public class DirtyFlag
{
  public DirtyFlag(Transform transform)
  {
    _transform = transform;
    _objDirty = true;
  }
  
  private readonly Transform _transform;
  private bool _objDirty;

  public bool IsDirty => _objDirty || _transform.hasChanged;

  public void SetDirty()
  {
    _objDirty = true;
  }

  public void ResetDirtyFlag()
  {
    _objDirty = false;
    _transform.hasChanged = false;
  }
  
  public void SetField<T>(ref T field, T value)
  {
    if (EqualityComparer<T>.Default.Equals(field, value)) return;

    field = value;
    SetDirty();
  }
}