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

    private ComputeBuffer _objectsBuffer;
    private ComputeBuffer _lightsBuffer;

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

    private RenderTextureDescriptor _descriptor;

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
      _width = cameraTextureDescriptor.width;
      _height = cameraTextureDescriptor.height;

      RenderTextureDescriptor descriptor = cameraTextureDescriptor;
      descriptor.enableRandomWrite = true;

      cmd.GetTemporaryRT(ShaderPropertyID.Destination, descriptor);
      _destination = new RenderTargetIdentifier(ShaderPropertyID.Destination);

      _descriptor = descriptor;

      // ConfigureTarget(_destination);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      var camera = renderingData.cameraData.camera;

      // if (camera.cameraType is not (CameraType.SceneView or CameraType.Game))
      // {
      //   return;
      // }
      
      if (_settings.shader == null || Material == null || Raymarch.Objects.Count == 0 || Raymarch.Lights.Count == 0)
      {
        return;
      }

      var cameraColourTexture = renderingData.cameraData.renderer.cameraColorTarget;
      var cameraDepthTexture = renderingData.cameraData.renderer.cameraDepthTarget;

      CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

      SetShaderProperties(cmd, camera);
      
      cmd.Blit(cameraColourTexture, _destination, Material);
      cmd.Blit(_destination, cameraColourTexture);

      // cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
      // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, Material);

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
      Material.SetMatrix("_CamToWorld", camera.cameraToWorldMatrix);

      // Raymarching
      Material.SetFloat(ShaderPropertyID.RenderDistance, _settings.renderDistance - camera.nearClipPlane);
      Material.SetFloat(ShaderPropertyID.HitResolution, _settings.hitResolution);
      Material.SetFloat(ShaderPropertyID.Relaxation, _settings.relaxation);
      Material.SetInt(ShaderPropertyID.MaxIterations, _settings.maxIterations);

      // Lighting & Shadows
      Material.SetVector(ShaderPropertyID.AmbientColour, _settings.ambientColour);

      // Compute Buffer
      _objectsBuffer = Raymarch.CreateObjectInfoBuffer();
      Material.SetBuffer(ShaderPropertyID.ObjectInfo, _objectsBuffer);
      Material.SetInt(ShaderPropertyID.ObjectInfoCount, _objectsBuffer.count);

      _lightsBuffer = Raymarch.CreateLightInfoBuffer();
      Material.SetBuffer(ShaderPropertyID.LightInfo, _lightsBuffer);
      Material.SetInt(ShaderPropertyID.LightInfoCount, _lightsBuffer.count);

      /*// Camera
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
      cmd.SetComputeIntParam(_settings.shader, ShaderID.LightInfoCount, _lightsBuffer.count);*/
    }

    private Matrix4x4 GetCamFrustum(Camera camera)
    {
      Matrix4x4 frustum = Matrix4x4.identity;
      float fov = Mathf.Tan((camera.fieldOfView * 0.5f) * Mathf.Deg2Rad);

      Vector3 goUp = Vector3.up * fov;
      Vector3 goRight = Vector3.right * fov * camera.aspect;

      Vector3 TopLeft = (-Vector3.forward - goRight + goUp);
      Vector3 TopRight = (-Vector3.forward + goRight + goUp);
      Vector3 BottomRight = (-Vector3.forward + goRight - goUp);
      Vector3 BottomLeft = (-Vector3.forward - goRight - goUp);

      frustum.SetRow(0, TopLeft);
      frustum.SetRow(0, TopRight);
      frustum.SetRow(0, BottomRight);
      frustum.SetRow(0, BottomLeft);

      return frustum;
    }
  }
}