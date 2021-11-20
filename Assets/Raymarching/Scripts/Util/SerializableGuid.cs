using System;
using UnityEngine;

// https://github.com/Unity-Technologies/guid-based-reference/blob/master/Assets/CrossSceneReference/Runtime/GuidComponent.cs

[Serializable]
public class SerializableGuid
{
  private Guid guid = Guid.Empty;
  [SerializeField, HideInInspector] private byte[] serializedGuid = Guid.NewGuid().ToByteArray();

  public Guid GUID
  {
    get
    {
      if (guid == Guid.Empty)
      {
        guid = new Guid(serializedGuid);
      }

      return guid;
    }
  }

  public override string ToString()
  {
    return GUID.ToString();
  }

  public string ToShaderSafeString()
  {
    return ToString().Replace("-", "");
  }

#if UNITY_EDITOR
  public void ResetGUID()
  {
    serializedGuid = Guid.NewGuid().ToByteArray();
    guid = Guid.Empty;
  }
#endif
}