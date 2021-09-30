using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Raymarch
{
  // Raymarch Objects
  private static List<RaymarchObject> _objects = new List<RaymarchObject>();
  private static List<RaymarchObjectInfo> _objectInfos = new List<RaymarchObjectInfo>();
  private static ComputeBuffer _objectComputeBuffer;
  private static bool _objectListDirty = true;

  public static ComputeBuffer ObjectComputeBuffer
  {
    get
    {
      if (_objectComputeBuffer == null || _objectListDirty ||
          _objects.Any(x => x.IsDirty))
      {
        _objectComputeBuffer?.Release();
        _objectComputeBuffer = CreateObjectComputeBuffer();

        // Reset dirty flags
        _objectListDirty = false;
      }

      return _objectComputeBuffer;
    }
  }

  public static void AddObject(RaymarchObject obj)
  {
    if (_objects.Contains(obj)) return;

    _objects.Add(obj);
    _objectListDirty = true;
  }

  public static void RemoveObject(RaymarchObject obj)
  {
    if (!_objects.Contains(obj)) return;

    _objects.Remove(obj);
    _objectListDirty = true;

    if (_objects.Count == 0) _objectComputeBuffer?.Dispose();
  }

  private static ComputeBuffer CreateObjectComputeBuffer()
  {
    int count = _objects.Count;

    CheckListCapacity(ref _objectInfos, count);

    // Sort objects
    _objects = _objects
      .OrderBy(x => x.OperationLayer)
      .ThenBy(x => x.Operation)
      .ToList();

    for (int i = 0; i < count; i++)
    {
      if (!_objectListDirty && !_objects[i].IsDirty)
      {
        continue;
      }

      // Set Object Info
      if (_objectInfos.Count <= i)
      {
        _objectInfos.Add(new RaymarchObjectInfo(_objects[i]));
      }
      else
      {
        _objectInfos[i] = new RaymarchObjectInfo(_objects[i]);
      }

      // Reset object dirty flag
      _objects[i].ResetDirtyFlag();
    }

    var buffer = new ComputeBuffer(count, RaymarchObjectInfo.GetSize(), ComputeBufferType.Default);
    buffer.SetData(_objectInfos);

    return buffer;
  }

  // Raymarch Lights
  private static List<RaymarchLight> _lights = new List<RaymarchLight>();
  private static List<RaymarchLightInfo> _lightInfos = new List<RaymarchLightInfo>();
  private static ComputeBuffer _lightComputeBuffer;
  private static bool _lightListDirty = true;

  public static ComputeBuffer LightComputeBuffer
  {
    get
    {
      if (_lightComputeBuffer == null || _lightListDirty ||
          _lights.Any(x => x.IsDirty))
      {
        _lightComputeBuffer?.Release();
        _lightComputeBuffer = CreateLightComputeBuffer();

        // Reset dirty flags
        _lightListDirty = false;
      }

      return _lightComputeBuffer;
    }
  }

  public static void AddLight(RaymarchLight light)
  {
    if (_lights.Contains(light)) return;
    
    _lights.Add(light);
    _lightListDirty = true;
  }

  public static void RemoveLight(RaymarchLight light)
  {
    if (!_lights.Contains(light)) return;
    
    _lights.Remove(light);
    _lightListDirty = true; 
      
    if (_lights.Count == 0) _lightComputeBuffer?.Dispose();
  }

  private static ComputeBuffer CreateLightComputeBuffer()
  {
    int count = _lights.Count;

    CheckListCapacity(ref _lightInfos, count);

    for (int i = 0; i < count; i++)
    {
      if (!_lightListDirty && !_lights[i].IsDirty)
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
      _lights[i].ResetDirtyFlag();
    }

    var buffer = new ComputeBuffer(count, RaymarchLightInfo.GetSize(), ComputeBufferType.Default);
    buffer.SetData(_lightInfos);

    return buffer;
  }

  // Util
  public static bool ShouldRender()
  {
    return _lights.Count != 0 && _objects.Count != 0;
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