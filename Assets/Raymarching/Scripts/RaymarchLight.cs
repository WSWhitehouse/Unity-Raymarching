using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using System.Text;
using UnityEditor;
#endif

public enum LightMode
{
  Baked,
  Realtime
}

[DisallowMultipleComponent, ExecuteAlways, RequireComponent(typeof(Light))]
public class RaymarchLight : RaymarchBase
{
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

  [SerializeField] private LightMode lightMode;
  public LightMode LightMode => lightMode;
  public LightType LightType => Light.type;

  public Vector3 Position => transform.position;
  public Vector3 Direction => transform.forward;

  public Color Colour => Light.color;
  public float Range => Light.range;
  public float Intensity => Light.intensity;

  public float SpotAngle => Light.spotAngle * Mathf.Deg2Rad;
  public float InnerSpotAngle => Light.innerSpotAngle * Mathf.Deg2Rad;

  private struct ShaderIDs
  {
    public int Position;
    public int Direction;
    public int Colour;
    public int Range;
    public int Intensity;
    public int SpotAngle;
    public int InnerSpotAngle;
  }

  private ShaderIDs _shaderIDs = new ShaderIDs();

  private void InitShaderIDs()
  {
    string guid = GUID.ToShaderSafeString();

    _shaderIDs.Position = Shader.PropertyToID($"_Position{guid}");
    _shaderIDs.Direction = Shader.PropertyToID($"_Direction{guid}");
    _shaderIDs.Colour = Shader.PropertyToID($"_Colour{guid}");
    _shaderIDs.Range = Shader.PropertyToID($"_Range{guid}");
    _shaderIDs.Intensity = Shader.PropertyToID($"_Intensity{guid}");
    _shaderIDs.SpotAngle = Shader.PropertyToID($"_SpotAngle{guid}");
    _shaderIDs.InnerSpotAngle = Shader.PropertyToID($"_InnerSpotAngle{guid}");
  }

  public override void Awake()
  {
    if (LightMode == LightMode.Realtime)
    {
      InitShaderIDs();
      Raymarch.OnUploadShaderData += UploadShaderData;
      base.Awake();
    }
  }

  protected override void OnDestroy()
  {
    if (LightMode == LightMode.Realtime)
    {
      Raymarch.OnUploadShaderData -= UploadShaderData;
      base.OnDestroy();
    }
  }

  public override bool IsValid()
  {
    return true;
  }

  private void UploadShaderData(Material material)
  {
    material.SetVector(_shaderIDs.Position, Position);
    material.SetVector(_shaderIDs.Direction, Direction);
    material.SetVector(_shaderIDs.Colour, Colour);
    material.SetFloat(_shaderIDs.Range, Range);
    material.SetFloat(_shaderIDs.Intensity, Intensity);
    material.SetFloat(_shaderIDs.SpotAngle, SpotAngle);
    material.SetFloat(_shaderIDs.InnerSpotAngle, InnerSpotAngle);
  }

#if UNITY_EDITOR
  protected override string GetShaderVariablesImpl()
  {
    if (LightMode == LightMode.Baked)
      return string.Empty;

    string guid = GUID.ToShaderSafeString();
    StringBuilder result = new StringBuilder();

    result.AppendLine($"uniform float3 _Position{guid};");
    result.AppendLine($"uniform float3 _Direction{guid};");
    result.AppendLine($"uniform float4 _Colour{guid};");
    result.AppendLine($"uniform float  _Range{guid};");
    result.AppendLine($"uniform float  _Intensity{guid};");
    result.AppendLine($"uniform float  _SpotAngle{guid};");
    result.AppendLine($"uniform float  _InnerSpotAngle{guid};");

    return result.ToString();
  }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchLight)), CanEditMultipleObjects]
public class RaymarchLightEditor : RaymarchBaseEditor
{
  private RaymarchLight Target => target as RaymarchLight;

  protected override void DrawInspector()
  {
    DrawDefaultInspector();
  }
}
#endif