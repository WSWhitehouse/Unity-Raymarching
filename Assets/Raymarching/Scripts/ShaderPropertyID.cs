using UnityEngine;

public static class ShaderPropertyID
{
  public static readonly int Destination = Shader.PropertyToID("_Destination");

  public static readonly int CamToWorldMatrix = Shader.PropertyToID("_CamToWorldMatrix");

  public static readonly int RenderDistance = Shader.PropertyToID("_RenderDistance");
  public static readonly int HitResolution = Shader.PropertyToID("_HitResolution");
  public static readonly int Relaxation = Shader.PropertyToID("_Relaxation");
  public static readonly int MaxIterations = Shader.PropertyToID("_MaxIterations");

  public static readonly int AmbientColour = Shader.PropertyToID("_AmbientColour");

  public static readonly int AoStepSize = Shader.PropertyToID("_AoStepSize");
  public static readonly int AoIntensity = Shader.PropertyToID("_AoIntensity");
  public static readonly int AoIterations = Shader.PropertyToID("_AoIterations");

  public static readonly string NoShadows = "NO_SHADOWS";
  public static readonly string HardShadows = "HARD_SHADOWS";
  public static readonly string SoftShadows = "SOFT_SHADOWS";
  public static readonly int ShadowIntensity = Shader.PropertyToID("_ShadowIntensity");
  public static readonly int ShadowSteps = Shader.PropertyToID("_ShadowSteps");
  public static readonly int ShadowDistance = Shader.PropertyToID("_ShadowDistance");
  public static readonly int ShadowPenumbra = Shader.PropertyToID("_ShadowPenumbra");

  public static readonly int ObjectInfo = Shader.PropertyToID("_ObjectInfo");
  public static readonly int ObjectInfoCount = Shader.PropertyToID("_ObjectInfoCount");
  
  public static readonly int ModifierInfo = Shader.PropertyToID("_ModifierInfo");
  public static readonly int ModifierInfoCount = Shader.PropertyToID("_ModifierInfoCount");

  public static readonly int LightInfo = Shader.PropertyToID("_LightInfo");
  public static readonly int LightInfoCount = Shader.PropertyToID("_LightInfoCount");
}