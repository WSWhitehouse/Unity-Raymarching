using System.Collections.Generic;
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
  private bool _lightDirty = true;
  public bool IsDirty => _lightDirty || transform.hasChanged;

  public void SetDirty()
  {
    _lightDirty = true;
  }

  public void ResetDirtyFlag()
  {
    _lightDirty = false;
    transform.hasChanged = false;
  }

  private void SetField<T>(ref T field, T value)
  {
    if (EqualityComparer<T>.Default.Equals(field, value)) return;

    field = value;
    SetDirty();
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

  private void Awake()
  {
    Raymarch.AddLight(this);
  }

  private void OnDestroy()
  {
    Raymarch.RemoveLight(this);
  }

#if UNITY_EDITOR
  // Allows lights to be added to list while not playing or in scene view
  private void OnValidate()
  {
    Awake();
  }
#endif
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
      Target.SetDirty();
    }
  }
}
#endif