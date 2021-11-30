using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RaymarchRenderFeature : ScriptableRendererFeature
{
  [SerializeField] private RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;

  private RaymarchRenderPass _renderPass;

  public override void Create()
  {
    _renderPass = new RaymarchRenderPass(name)
    {
      renderPassEvent = passEvent
    };
  }

  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
  {
    ref Camera camera = ref renderingData.cameraData.camera;
    if (camera.cameraType is not (CameraType.SceneView or CameraType.Game))
    {
      // NOTE(WSWhitehouse): Not adding this render pass to cameras that arent for the Game and Scene
      return;
    }

    renderer.EnqueuePass(_renderPass);
  }
}

public class RaymarchRenderPass : ScriptableRenderPass
{
  private RenderTargetIdentifier _destination;
  // private RenderTexture _destinationRT;

  public RaymarchRenderPass(string profilerTag)
  {
    profilingSampler = new ProfilingSampler(profilerTag);
  }

  public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
  {
    RenderTextureDescriptor descriptor = cameraTextureDescriptor;
    // descriptor.enableRandomWrite = true;
    
    // _destinationRT = RenderTexture.GetTemporary(descriptor);
    // _destination = new RenderTargetIdentifier(_destination);
    
    cmd.GetTemporaryRT(Shader.PropertyToID("_Destination"), descriptor);
    _destination = new RenderTargetIdentifier("_Destination");
  }

  public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
  {
    CommandBuffer cmd = CommandBufferPool.Get(profilingSampler.name);
    using (new ProfilingScope(cmd, profilingSampler))
    {
      ref CameraData cameraData = ref renderingData.cameraData;
      ref Camera camera         = ref cameraData.camera;
      
      RenderTargetIdentifier cameraColourTexture = cameraData.renderer.cameraColorTarget;

      if (!Raymarch.ShouldRender())
      {
        return;
      }

      Raymarch.Material.SetMatrix(Shader.PropertyToID("_CamToWorldMatrix"), camera.cameraToWorldMatrix);
      Raymarch.UploadShaderDataInvoke();

      /* NOTE(WSWhitehouse): 
       * Using cmd.Blit in URP XR projects has compatibility issues with the URP XR integration.
       * Using cmd.Blit might implicitly enable or disable XR shader keywords, which breaks XR SPI
       * rendering. Use cmd.DrawMesh instead! Update URP to v13 to use cmd.Blit!
       * https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@13.0/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html
       */

      cmd.Blit(cameraColourTexture, _destination, Raymarch.Material);
      cmd.Blit(_destination, cameraColourTexture);

      // cmd.CopyTexture(cameraColourTexture, _destination);
      // Raymarch.Material.SetTexture(Shader.PropertyToID("_MainTex"), _destinationRT, RenderTextureSubElement.Color);
      // cmd.SetRenderTarget(cameraColourTexture, 0, CubemapFace.Unknown, -1);
      // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, Raymarch.Material);
    }

    context.ExecuteCommandBuffer(cmd);
    cmd.Clear();
    CommandBufferPool.Release(cmd);

    context.Submit();
  }

  public override void OnCameraCleanup(CommandBuffer cmd)
  {
    // _destinationRT.Release();
    cmd.ReleaseTemporaryRT(Shader.PropertyToID("_Destination"));
  }
}