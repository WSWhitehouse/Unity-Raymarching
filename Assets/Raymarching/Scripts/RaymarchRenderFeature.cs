using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RaymarchRenderFeature : ScriptableRendererFeature
{
  [SerializeField] private RaymarchRenderSettings settings = new RaymarchRenderSettings();

  private RaymarchRenderPass _renderPass;

  public override void Create()
  {
    if (settings.RaymarchSettings == null) return;

    _renderPass = new RaymarchRenderPass(name, settings);
  }

  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
  {
    if (settings.RaymarchSettings == null) return;

    renderer.EnqueuePass(_renderPass);
  }
}

[Serializable]
public class RaymarchRenderSettings
{
  public RenderPassEvent PassEvent = RenderPassEvent.AfterRenderingSkybox;
  public RaymarchSettings RaymarchSettings;
}

public class RaymarchRenderPass : ScriptableRenderPass
{
  private readonly string _profilerTag;
  private RaymarchSettings _settings;

  private RenderTargetIdentifier _destination;

  private Material _material;

  private Material Material
  {
    get
    {
      if (_material == null && _settings.shader != null)
      {
        _material = new Material(_settings.shader)
        {
          hideFlags = HideFlags.HideAndDontSave
        };
      }

      return _material;
    }
  }

  public RaymarchRenderPass(string profilerTag, RaymarchRenderSettings settings)
  {
    _profilerTag = profilerTag;
    _settings = settings.RaymarchSettings;
    renderPassEvent = settings.PassEvent;
  }

  public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
  {
    RenderTextureDescriptor descriptor = cameraTextureDescriptor;
    descriptor.enableRandomWrite = true;

    cmd.GetTemporaryRT(ShaderPropertyID.Destination, descriptor);
    _destination = new RenderTargetIdentifier(ShaderPropertyID.Destination);
  }

  public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
  {
    var camera = renderingData.cameraData.camera;
    var cameraColourTexture = renderingData.cameraData.renderer.cameraColorTarget;

#if UNITY_EDITOR
    if (camera.cameraType is not (CameraType.SceneView or CameraType.Game))
    {
      return;
    }
#endif

    if (_settings.shader == null || Material == null || !Raymarch.ShouldRender())
    {
      return;
    }

    CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

    SetShaderProperties(camera);

    cmd.Blit(cameraColourTexture, _destination, Material);
    cmd.Blit(_destination, cameraColourTexture);

    context.ExecuteCommandBuffer(cmd);
    cmd.Clear();
    context.Submit();
    CommandBufferPool.Release(cmd);
  }

  private void SetShaderProperties(Camera camera)
  {
    // Camera
    Material.SetMatrix(ShaderPropertyID.CamToWorldMatrix, camera.cameraToWorldMatrix);

    // Raymarching
    Material.SetFloat(ShaderPropertyID.RenderDistance, _settings.renderDistance - camera.nearClipPlane);
    Material.SetFloat(ShaderPropertyID.HitResolution, _settings.hitResolution);
    Material.SetFloat(ShaderPropertyID.Relaxation, _settings.relaxation);
    Material.SetInt(ShaderPropertyID.MaxIterations, _settings.maxIterations);

    // Lighting
    Material.SetVector(ShaderPropertyID.AmbientColour, _settings.ambientColour);

    // Ambient Occlusion
    Material.SetFloat(ShaderPropertyID.AoStepSize, _settings.aoStepSize);
    Material.SetFloat(ShaderPropertyID.AoIntensity, _settings.aoIntensity);
    Material.SetInt(ShaderPropertyID.AoIterations, _settings.aoIterations);

    // Shadows
    Material.DisableKeyword(ShaderPropertyID.HardShadows);
    Material.DisableKeyword(ShaderPropertyID.SoftShadows);
    Material.DisableKeyword(ShaderPropertyID.NoShadows);

    switch (_settings.shadowType)
    {
      case RaymarchSettings.ShadowType.HardShadows:
        Material.EnableKeyword(ShaderPropertyID.HardShadows);
        break;
      case RaymarchSettings.ShadowType.SoftShadows:
        Material.EnableKeyword(ShaderPropertyID.SoftShadows);
        break;
      default:
        Material.EnableKeyword(ShaderPropertyID.NoShadows);
        break;
    }

    if (_settings.shadowType != RaymarchSettings.ShadowType.NoShadows)
    {
      Material.SetFloat(ShaderPropertyID.ShadowIntensity, _settings.shadowIntensity);
      Material.SetInt(ShaderPropertyID.ShadowSteps, _settings.shadowSteps);
      Material.SetVector(ShaderPropertyID.ShadowDistance, _settings.shadowDistance);

      if (_settings.shadowType == RaymarchSettings.ShadowType.SoftShadows)
      {
        Material.SetFloat(ShaderPropertyID.ShadowPenumbra, _settings.shadowPenumbra);
      }
    }

    // Compute Buffer
    Material.SetBuffer(ShaderPropertyID.ObjectInfo, Raymarch.ObjectComputeBuffer);
    Material.SetInt(ShaderPropertyID.ObjectInfoCount, Raymarch.ObjectComputeBuffer.count); 
    
    Material.SetBuffer(ShaderPropertyID.ModifierInfo, Raymarch.ModifierComputeBuffer);
    Material.SetInt(ShaderPropertyID.ModifierInfoCount, Raymarch.ModifierComputeBuffer.count);

    Material.SetBuffer(ShaderPropertyID.LightInfo, Raymarch.LightComputeBuffer);
    Material.SetInt(ShaderPropertyID.LightInfoCount, Raymarch.LightComputeBuffer.count);
  }
}