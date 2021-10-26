using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR
using System.Text;
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

  private ShaderIDs shaderIDs;

  private void InitShaderIDs()
  {
    string guid = GUID.ToShaderSafeString();

    shaderIDs.Position = Shader.PropertyToID($"_Position{guid}");
    shaderIDs.Direction = Shader.PropertyToID($"_Direction{guid}");
    shaderIDs.Colour = Shader.PropertyToID($"_Colour{guid}");
    shaderIDs.Range = Shader.PropertyToID($"_Range{guid}");
    shaderIDs.Intensity = Shader.PropertyToID($"_Intensity{guid}");
    shaderIDs.SpotAngle = Shader.PropertyToID($"_SpotAngle{guid}");
    shaderIDs.InnerSpotAngle = Shader.PropertyToID($"_InnerSpotAngle{guid}");
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
    material.SetFloat(shaderIDs.SpotAngle, SpotAngle);
    material.SetFloat(shaderIDs.InnerSpotAngle, InnerSpotAngle);

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
    code = $"{code}uniform float _SpotAngle{guid};{ShaderGen.NewLine}";
    code = $"{code}uniform float _InnerSpotAngle{guid};{ShaderGen.NewLine}";

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
    string spotAngle = $"_SpotAngle{guid}";
    string innerSpotAngle = $"_InnerSpotAngle{guid}";
    string isActive = $"_IsActive{guid}";

    if (LightMode == LightMode.Baked)
    {
      if (!IsActive) return $"// Light{guid} (baked) is not active in scene";

      position = ToShaderFloat3(Position);
      direction = ToShaderFloat3(Direction);
      colour = ToShaderFloat4(Colour);
      range = Range.ToString(CultureInfo.InvariantCulture);
      intensity = Intensity.ToString(CultureInfo.InvariantCulture);
      spotAngle = SpotAngle.ToString(CultureInfo.InvariantCulture);
      innerSpotAngle = InnerSpotAngle.ToString(CultureInfo.InvariantCulture);
    }

    StringBuilder result = new StringBuilder();

    if (LightMode != LightMode.Baked)
    {
      result.AppendLine($"if ({isActive} > 0)");
      result.AppendLine($"{{");
    }

    switch (LightType)
    {
      case LightType.Directional:
        result.AppendLine($"light += GetDirectionalLight(pos, normal, {colour}, {direction}, {intensity});");
        break;
      case LightType.Point:
        result.AppendLine($"light += GetPointLight(pos, normal, {position}, {colour}, {range}, {intensity});");
        break;
      case LightType.Spot:
        result.AppendLine(
          $"light += GetSpotLight(pos, normal, {position}, {colour}, {direction}, {range}, {intensity}, {spotAngle}, {innerSpotAngle});");
        break;
      default:
        Debug.Log($"{LightType.ToString()} is currently not supported!");
        return string.Empty;
    }

    if (LightMode != LightMode.Baked)
    {
      result.AppendLine($"}}");
    }

    return result.ToString();
  }

  private string ToShaderFloat3(Vector3 vec)
  {
    return
      $"float3({vec.x.ToString(CultureInfo.InvariantCulture)}, " +
      $"{vec.y.ToString(CultureInfo.InvariantCulture)}, " +
      $"{vec.z.ToString(CultureInfo.InvariantCulture)})";
  }
  
  private string ToShaderFloat4(Vector4 vec)
  {
    return
      $"float4({vec.x.ToString(CultureInfo.InvariantCulture)}, " +
      $"{vec.y.ToString(CultureInfo.InvariantCulture)}, " +
      $"{vec.z.ToString(CultureInfo.InvariantCulture)}, " +
      $"{vec.w.ToString(CultureInfo.InvariantCulture)})";
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