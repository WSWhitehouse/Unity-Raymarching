using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LightType
{
  Directional,
  Point,
  Spot
}

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

  public LightType LightType
  {
    get
    {
      switch (Light.type)
      {
        case UnityEngine.LightType.Spot: return LightType.Spot;
        case UnityEngine.LightType.Point: return LightType.Point;
        case UnityEngine.LightType.Directional:
        case UnityEngine.LightType.Area:
        case UnityEngine.LightType.Disc:
        default: return LightType.Directional;
      }
    }
  }

  public Vector3 Position => transform.position;
  public Vector3 Direction => transform.forward;

  public Color Colour => Light.color;
  public float Range => Light.range;
  public float Intensity => Light.intensity;

  private struct ShaderIDs
  {
    public int Position;
    public int Direction;
    public int Colour;
    public int Range;
    public int Intensity;
  }

  private ShaderIDs shaderIDs;

  private void InitShaderIDs()
  {
    string guid = GUID.ToShaderSafeString();

    shaderIDs.Position = Shader.PropertyToID($"_Position{guid}");
    shaderIDs.Direction = Shader.PropertyToID($"_Direction{guid}");
    shaderIDs.Colour = Shader.PropertyToID($"_Colour{guid}");
    shaderIDs.Range = Shader.PropertyToID($"_Range{guid}");
    shaderIDs.Intensity = Shader.PropertyToID($"_Intensity{guid}");
  }

  public override void Awake()
  {
    if (LightMode == LightMode.Realtime)
    {
      InitShaderIDs();
      base.Awake();
    }
  }

  protected override void OnDestroy()
  {
    if (LightMode == LightMode.Realtime)
      base.OnDestroy();
  }

  public override bool IsValid()
  {
    return true;
  }

  protected override void UploadShaderData(Material material)
  {
    material.SetVector(shaderIDs.Position, Position);
    material.SetVector(shaderIDs.Direction, Direction);
    material.SetVector(shaderIDs.Colour, Colour);
    material.SetFloat(shaderIDs.Range, Range);
    material.SetFloat(shaderIDs.Intensity, Intensity);

    base.UploadShaderData(material);
  }

#if UNITY_EDITOR
  public override string GetShaderCode_Variables()
  {
    if (LightMode == LightMode.Baked)
      return string.Empty;

    string guid = GUID.ToShaderSafeString();

    var code = $"uniform float3 _Position{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float3 _Direction{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float4 _Colour{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float _Range{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float _Intensity{guid};{ShaderGen.NewLine}";

    return string.Concat(code, base.GetShaderCode_Variables());
  }

  public string GetShaderCode_CalcLight()
  {
    string guid = GUID.ToShaderSafeString();

    string position = $"_Position{guid}";
    string direction = $"_Direction{guid}";
    string colour = $"_Colour{guid}";
    string range = $"_Range{guid}";
    string intensity = $"_Intensity{guid}";

    if (LightMode == LightMode.Baked)
    {
      position = ToShaderFloat3(Position);
      direction = ToShaderFloat3(Direction);
      colour = ToShaderFloat3(new Vector3(Colour.r, Colour.g, Colour.b));
      range = Range.ToString(CultureInfo.InvariantCulture);
      intensity = Intensity.ToString(CultureInfo.InvariantCulture);

      return GetLightTypeShaderCode(position, direction, colour, range, intensity);
    }

    string isActive = $"_IsActive{guid}";

    return
      $"if ({isActive} > 0){ShaderGen.NewLine}{{{ShaderGen.NewLine}{GetLightTypeShaderCode(position, direction, colour, range, intensity)}{ShaderGen.NewLine}}}{ShaderGen.NewLine}";
  }

  private string GetLightTypeShaderCode(string position, string direction,
    string colour, string range, string intensity)
  {
    string guid = GUID.ToShaderSafeString();

    switch (LightType)
    {
      case LightType.Point: //http://forum.unity3d.com/threads/light-attentuation-equation.16006/
        return
          $"float3 toLight{guid} = pos - {position}; {ShaderGen.NewLine}" +
          $"float range{guid} = clamp(length(toLight{guid}) / {range}, 0., 1.); {ShaderGen.NewLine}" +
          $"float attenuation{guid} = 1.0 / (1.0 + 256.0 * range{guid} * range{guid}); {ShaderGen.NewLine}" +
          $"light += max(0.0, {colour} * dot(-normal, normalize(toLight{guid}.xyz))) * {intensity} * attenuation{guid}; {ShaderGen.NewLine}";
      case LightType.Spot:
        return $"";
      case LightType.Directional:
      default:
        return $"light += {colour} * max(0.0, dot(-normal, {direction})) * {intensity}; {ShaderGen.NewLine}";
    }
  }

  private string ToShaderFloat3(Vector3 vec)
  {
    return
      $"float3({vec.x.ToString(CultureInfo.InvariantCulture)}, " +
      $"{vec.y.ToString(CultureInfo.InvariantCulture)}, " +
      $"{vec.z.ToString(CultureInfo.InvariantCulture)})";
  }

#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchLight))]
public class RaymarchLightEditor : RaymarchBaseEditor
{
  private RaymarchLight Target => target as RaymarchLight;

  protected override void DrawInspector()
  {
    DrawDefaultInspector();
  }
}
#endif