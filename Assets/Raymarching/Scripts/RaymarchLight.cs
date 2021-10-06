using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LightType
{
  Directional = 0,
  Point = 1,
}

[DisallowMultipleComponent, ExecuteAlways, RequireComponent(typeof(Light))]
public class RaymarchLight : MonoBehaviour
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
  
  private Light _light;

  private Light Light
  {
    get
    {
      if (_light == null)
      {
        _light = GetComponent<Light>();
      }

      return _light;
    }
  }

  public LightType LightType =>
    Light.type == UnityEngine.LightType.Directional ? LightType.Directional : LightType.Point;

  public Vector3 Position => transform.position;
  public Vector3 Direction => transform.forward;

  public Color Colour => Light.color;
  public float Range => Light.range;
  public float Intensity => Light.intensity;
  
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
[CustomEditor(typeof(RaymarchLight))]
public class RaymarchLightEditor : Editor
{
  private RaymarchLight Target => target as RaymarchLight;

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