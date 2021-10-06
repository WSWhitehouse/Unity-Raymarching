using System;
using UnityEngine;

[DisallowMultipleComponent, ExecuteAlways]
public abstract class RaymarchBase : MonoBehaviour
{
  // Dirty Flag
  private DirtyFlag _dirtyFlag;
  public DirtyFlag DirtyFlag => _dirtyFlag ??= new DirtyFlag(transform);

  // GUID
  [SerializeField] private SerializableGuid guid;

  public SerializableGuid GUID => guid;

  public abstract bool IsValid();

  protected virtual void Awake()
  {
    Raymarch.OnUploadShaderData += UploadShaderData;
  }

  protected virtual void OnDestroy()
  {
    Raymarch.OnUploadShaderData -= UploadShaderData;
  }

  protected virtual void OnEnable()
  {
    DirtyFlag.SetDirty();
  }

  protected virtual void OnDisable()
  {
    DirtyFlag.SetDirty();
  }

  protected abstract void UploadShaderData(Material material);

  public virtual string GetShaderVariablesCode()
  {
    return string.Empty;
  }
  
  public virtual string GetShaderDistanceCode()
  {
    return string.Empty;
  }
}