using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WSWhitehouse
{
  public static class Raymarch
  {
    // Raymarch Objects
    public static List<RaymarchObject> Objects { get; private set; } = new List<RaymarchObject>();
    private static List<RaymarchObjectInfo> _objectInfos = new List<RaymarchObjectInfo>();

    // RaymarchLights
    public static List<RaymarchLight> Lights { get; private set; } = new List<RaymarchLight>();
    private static List<RaymarchLightInfo> _lightInfos = new List<RaymarchLightInfo>();

    public static ComputeBuffer CreateObjectInfoBuffer()
    {
      int count = Objects.Count;

      CheckListCapacity(ref _objectInfos, count);

      Objects = Objects
        .OrderBy(x => x.OperationLayer)
        .ThenBy(x => x.Operation)
        .ToList();

      for (int i = 0; i < count; i++)
      {
        if (_objectInfos.Count <= i)
        {
          _objectInfos.Add(new RaymarchObjectInfo(Objects[i]));
        }
        else
        {
          _objectInfos[i] = new RaymarchObjectInfo(Objects[i]);
        }
      }

      var buffer = new ComputeBuffer(count, RaymarchObjectInfo.GetSize(), ComputeBufferType.Default);
      buffer.SetData(_objectInfos);

      return buffer;
    }

    public static ComputeBuffer CreateLightInfoBuffer()
    {
      int count = Lights.Count;

      CheckListCapacity(ref _lightInfos, count);

      for (int i = 0; i < count; i++)
      {
        if (_lightInfos.Count <= i)
        {
          _lightInfos.Add(new RaymarchLightInfo(Lights[i]));
        }
        else
        {
          _lightInfos[i] = new RaymarchLightInfo(Lights[i]);
        }
      }

      var buffer = new ComputeBuffer(count, RaymarchLightInfo.GetSize(), ComputeBufferType.Default);
      buffer.SetData(_lightInfos);

      return buffer;
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
}