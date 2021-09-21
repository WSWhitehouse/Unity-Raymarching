using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WSWhitehouse
{
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

    private int _width;
    private int _height;

    private RenderTargetIdentifier _destination;

    private ComputeBuffer _objectsBuffer;
    private ComputeBuffer _lightsBuffer;

    public RaymarchRenderPass(string profilerTag, RaymarchRenderSettings settings)
    {
      _profilerTag = profilerTag;
      _settings = settings.RaymarchSettings;
      renderPassEvent = settings.PassEvent;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
      _width = cameraTextureDescriptor.width;
      _height = cameraTextureDescriptor.height;

      RenderTextureDescriptor descriptor = cameraTextureDescriptor;
      descriptor.enableRandomWrite = true;

      cmd.GetTemporaryRT(ShaderID.Destination, descriptor);

      _destination = new RenderTargetIdentifier(ShaderID.Destination);

      ConfigureTarget(_destination);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      if (Raymarch.Objects.Count == 0 || Raymarch.Lights.Count == 0)
      {
        return;
      }

      var cameraColourTexture = renderingData.cameraData.renderer.cameraColorTarget;
      var cameraDepthTexture = renderingData.cameraData.renderer.cameraDepthTarget;

      CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

      cmd.SetComputeTextureParam(_settings.shader, _settings.KernelIndex, ShaderID.Source, cameraColourTexture);
      cmd.SetComputeTextureParam(_settings.shader, _settings.KernelIndex, ShaderID.Destination, _destination);
      cmd.SetComputeTextureParam(_settings.shader, _settings.KernelIndex, ShaderID.DepthTexture, cameraDepthTexture);

      SetShaderProperties(cmd, renderingData.cameraData.camera);

      int threadGroupsX = Mathf.CeilToInt(_width / 8.0f);
      int threadGroupsY = Mathf.CeilToInt(_height / 8.0f);
      cmd.DispatchCompute(_settings.shader, _settings.KernelIndex, threadGroupsX, threadGroupsY, 1);

      cmd.Blit(_destination, cameraColourTexture);
      cmd.ReleaseTemporaryRT(ShaderID.Destination);

      context.ExecuteCommandBuffer(cmd);
      cmd.Clear();
      context.Submit();

      CommandBufferPool.Release(cmd);
      _objectsBuffer.Release();
      _lightsBuffer.Release();
    }

    private void SetShaderProperties(CommandBuffer cmd, Camera camera)
    {
      // Camera
      cmd.SetComputeMatrixParam(_settings.shader, ShaderID.CamInverseProjection, camera.projectionMatrix.inverse);
      cmd.SetComputeMatrixParam(_settings.shader, ShaderID.CamToWorld, camera.cameraToWorldMatrix);
      cmd.SetComputeFloatParam(_settings.shader, ShaderID.CamNearClipPlane, camera.nearClipPlane);

      // Raymarching
      cmd.SetComputeFloatParam(_settings.shader, ShaderID.RenderDistance,
        _settings.renderDistance - camera.nearClipPlane);
      cmd.SetComputeFloatParam(_settings.shader, ShaderID.HitResolution, _settings.hitResolution);
      cmd.SetComputeFloatParam(_settings.shader, ShaderID.Relaxation, _settings.relaxation);
      cmd.SetComputeIntParam(_settings.shader, ShaderID.MaxIterations, _settings.maxIterations);

      // Lighting & Shadows
      cmd.SetComputeVectorParam(_settings.shader, ShaderID.AmbientColour, _settings.ambientColour);

      // Compute Buffer
      _objectsBuffer = Raymarch.CreateObjectInfoBuffer();
      cmd.SetComputeBufferParam(_settings.shader, _settings.KernelIndex, ShaderID.ObjectInfo, _objectsBuffer);
      cmd.SetComputeIntParam(_settings.shader, ShaderID.ObjectInfoCount, _objectsBuffer.count);

      _lightsBuffer = Raymarch.CreateLightInfoBuffer();
      cmd.SetComputeBufferParam(_settings.shader, _settings.KernelIndex, ShaderID.LightInfo, _lightsBuffer);
      cmd.SetComputeIntParam(_settings.shader, ShaderID.LightInfoCount, _lightsBuffer.count);
    }
  }
}