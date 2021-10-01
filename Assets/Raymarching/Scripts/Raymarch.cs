using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Raymarch
{
  // Raymarch Objects
  private static List<RaymarchObject> _objects = new List<RaymarchObject>();
  private static List<RaymarchObjectInfo> _objectInfos = new List<RaymarchObjectInfo>();
  private static ComputeBuffer _objectComputeBuffer;
  private static bool _objectsDirty = true;

  public static ComputeBuffer ObjectComputeBuffer
  {
    get
    {
      if (_objectComputeBuffer == null || _objectsDirty ||
          _objects.Any(x => x.DirtyFlag.IsDirty))
      {
        _objectComputeBuffer?.Release();
        _objectComputeBuffer = CreateObjectComputeBuffer();

        // Reset dirty flags
        _objectsDirty = false;
      }

      return _objectComputeBuffer;
    }
  }

  public static void SetObjects(List<RaymarchObject> _raymarchObjects)
  {
    _objects = _raymarchObjects;
    _objectsDirty = true;
    _modifiersDirty = true;
  }

  private static ComputeBuffer CreateObjectComputeBuffer()
  {
    int count = _objects.Count;

    CheckListCapacity(ref _objectInfos, count);

    for (int i = 0; i < count; i++)
    {
      if (!_objectsDirty && !_objects[i].DirtyFlag.IsDirty)
      {
        continue;
      }

      int modifierIndex = -1;

      RaymarchModifier modifier;
      if (_modifiers.TryGetValue(i, out modifier))
      {
        modifierIndex = modifier.Index;
      }

      // Set Object Info
      if (_objectInfos.Count <= i)
      {
        _objectInfos.Add(new RaymarchObjectInfo(_objects[i], modifierIndex));
      }
      else
      {
        _objectInfos[i] = new RaymarchObjectInfo(_objects[i], modifierIndex);
      }

      // Reset object dirty flag
      _objects[i].DirtyFlag.ResetDirtyFlag();
    }

    var buffer = new ComputeBuffer(count, RaymarchObjectInfo.GetSize(), ComputeBufferType.Default);
    buffer.SetData(_objectInfos);

    return buffer;
  }

  // Raymarch Modifiers
  private static Dictionary<int, RaymarchModifier> _modifiers = new Dictionary<int, RaymarchModifier>();
  private static List<RaymarchModifierInfo> _modifierInfos = new List<RaymarchModifierInfo>();
  private static ComputeBuffer _modifierComputeBuffer;
  private static bool _modifiersDirty = true;

  public static ComputeBuffer ModifierComputeBuffer
  {
    get
    {
#if UNITY_EDITOR
      if (_modifiers.Any(x => x.Value == null))
      {
        if (Application.isPlaying)
        {
          Debug.LogError("A Raymarch Modifier has been deleted during play mode!");
        }
      }
#endif

      if (_modifierComputeBuffer == null || _modifiersDirty ||
          _modifiers.Any(x => x.Value.DirtyFlag.IsDirty))
      {
        _modifierComputeBuffer?.Release();
        _modifierComputeBuffer = CreateModifierComputeBuffer();

        // Reset dirty flags
        _modifiersDirty = false;
      }

      return _modifierComputeBuffer;
    }
  }

  public static void SetModifiers(Dictionary<int, RaymarchModifier> modifiers)
  {
    _modifiers = modifiers;
    _objectsDirty = true;
    _modifiersDirty = true;
  }

  private static ComputeBuffer CreateModifierComputeBuffer()
  {
    int count = _modifiers.Count;

    CheckListCapacity(ref _modifierInfos, count);

    var modifiers = _modifiers.Values.ToList();

    for (int i = 0; i < count; i++)
    {
      if (!_modifiersDirty && !modifiers[i].DirtyFlag.IsDirty)
      {
        continue;
      }

      // Set Object Info
      if (_modifierInfos.Count <= i)
      {
        _modifierInfos.Add(new RaymarchModifierInfo(modifiers[i]));
      }
      else
      {
        _modifierInfos[i] = new RaymarchModifierInfo(modifiers[i]);
      }

      // Reset object dirty flag
      modifiers[i].DirtyFlag.ResetDirtyFlag();
    }

    var buffer = new ComputeBuffer(count, RaymarchModifierInfo.GetSize(), ComputeBufferType.Default);
    buffer.SetData(_modifierInfos);

    return buffer;
  }

  // Raymarch Lights
  private static List<RaymarchLight> _lights = new List<RaymarchLight>();
  private static List<RaymarchLightInfo> _lightInfos = new List<RaymarchLightInfo>();
  private static ComputeBuffer _lightComputeBuffer;
  private static bool _lightsDirty = true;

  public static ComputeBuffer LightComputeBuffer
  {
    get
    {
      if (_lightComputeBuffer == null || _lightsDirty ||
          _lights.Any(x => x.DirtyFlag.IsDirty))
      {
        _lightComputeBuffer?.Release();
        _lightComputeBuffer = CreateLightComputeBuffer();

        // Reset dirty flags
        _lightsDirty = false;
      }

      return _lightComputeBuffer;
    }
  }

  public static void AddLight(RaymarchLight light)
  {
    if (_lights.Contains(light)) return;

    _lights.Add(light);
    _lightsDirty = true;
  }

  public static void RemoveLight(RaymarchLight light)
  {
    if (!_lights.Contains(light)) return;

    _lights.Remove(light);
    _lightsDirty = true;

    if (_lights.Count == 0) _lightComputeBuffer?.Dispose();
  }

  private static ComputeBuffer CreateLightComputeBuffer()
  {
    int count = _lights.Count;

    CheckListCapacity(ref _lightInfos, count);

    for (int i = 0; i < count; i++)
    {
      if (!_lightsDirty && !_lights[i].DirtyFlag.IsDirty)
      {
        continue;
      }

      // Set Light Info
      if (_lightInfos.Count <= i)
      {
        _lightInfos.Add(new RaymarchLightInfo(_lights[i]));
      }
      else
      {
        _lightInfos[i] = new RaymarchLightInfo(_lights[i]);
      }

      // Reset object dirty flag
      _lights[i].DirtyFlag.ResetDirtyFlag();
    }

    var buffer = new ComputeBuffer(count, RaymarchLightInfo.GetSize(), ComputeBufferType.Default);
    buffer.SetData(_lightInfos);

    return buffer;
  }

  // Util
  public static bool ShouldRender()
  {
    return _lights.Count != 0 && _objects.Count != 0
# if UNITY_EDITOR
                              && _objects.All(x => x != null) && _modifiers.All(x => x.Value != null)
#endif
      ;
  }

  private static void CheckListCapacity<T>(ref List<T> list, int count)
  {
    if (list.Capacity < count)
    {
      // Unavoidable Heap Alloc
      list = new List<T>(count);
    }

    if (list.Count > count)
    {
      list.RemoveRange(0, list.Count - count);
    }
  }
}