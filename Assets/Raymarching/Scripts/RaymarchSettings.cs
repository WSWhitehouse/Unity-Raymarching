using UnityEngine;

[CreateAssetMenu(fileName = "RaymarchingSettings", menuName = "Raymarhcing/Settings", order = 0)]
public class RaymarchSettings : ScriptableObject
{
  [Header("Shader")] public Shader shader;

  [Space] [Header("Raymarching")] public float renderDistance = 100.0f;
  public float hitResolution = 0.001f;
  public float relaxation = 1.0f;
  public int maxIterations = 164;

  [Space] [Header("Lighting")] public Color ambientColour = new Color(0.2117f, 0.2274f, 0.2588f, 1);

  [Space] [Header("Ambient Occlusion")] [Range(0.01f, 10.0f)]
  public float aoStepSize;

  [Range(1, 5)] public int aoIterations;
  [Range(0, 1)] public float aoIntensity;

  [Space] [Header("Shadows")] public ShadowType shadowType = ShadowType.SoftShadows;
  public float shadowIntensity = 1.0f;
  public int shadowSteps = 10;
  public Vector2 shadowDistance = new Vector2(0.05f, 50.0f);
  public float shadowPenumbra;

  public enum ShadowType
  {
    NoShadows,
    HardShadows,
    SoftShadows
  }
}