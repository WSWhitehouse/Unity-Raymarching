using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "RaymarchingSettings", menuName = "Raymarching/Settings", order = 0)]
public class RaymarchSettings : ScriptableObject
{
  [Header("Raymarching")] 
  public float renderDistance = 100.0f;
  public float hitResolution = 0.001f;
  public float relaxation = 1.0f;
  public int maxIterations = 164;

  [Space]
  
  [Header("Lighting")] 
  public Color ambientColour = new Color(0.2117f, 0.2274f, 0.2588f, 1);
  
  [Tooltip("This scalar value is multiplied with the final colour result of all Raymarch Objects in the scene.")]
  public float colourMultiplier = 1;

  public void UploadShaderData(Material material)
  {
    material.SetFloat(RenderDistanceID, renderDistance);
    material.SetFloat(HitResolutionID, hitResolution);
    material.SetFloat(RelaxationID, relaxation);
    material.SetInteger(MaxIterationsID, maxIterations);
    material.SetVector(AmbientColourID, ambientColour);
    material.SetFloat(ColourMultiplierID, colourMultiplier);
  }

  private static readonly int RenderDistanceID = Shader.PropertyToID("_RenderDistance");
  private static readonly int HitResolutionID = Shader.PropertyToID("_HitResolution");
  private static readonly int RelaxationID = Shader.PropertyToID("_Relaxation");
  private static readonly int MaxIterationsID = Shader.PropertyToID("_MaxIterations");
  private static readonly int AmbientColourID = Shader.PropertyToID("_AmbientColour");
  private static readonly int ColourMultiplierID = Shader.PropertyToID("_ColourMultiplier");
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaymarchSettings))]
public class RaymarchSettingsEditor : Editor
{
  private RaymarchSettings Target => target as RaymarchSettings;

  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
  }
}
#endif