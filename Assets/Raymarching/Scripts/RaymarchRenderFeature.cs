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
    renderer.EnqueuePass(_renderPass);
  }
}

public class RaymarchRenderPass : ScriptableRenderPass
{
  private readonly string _profilerTag;

  private RenderTargetIdentifier _destination;

  public RaymarchRenderPass(string profilerTag)
  {
    _profilerTag = profilerTag;
  }

  public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
  {
    RenderTextureDescriptor descriptor = cameraTextureDescriptor;
    descriptor.enableRandomWrite = true;

    cmd.GetTemporaryRT(Shader.PropertyToID("_Destination"), descriptor);
    _destination = new RenderTargetIdentifier("_Destination");
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

    if (!Raymarch.ShouldRender())
    {
      return;
    }

    CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

    Raymarch.Material.SetMatrix(Shader.PropertyToID("_CamToWorldMatrix"), camera.cameraToWorldMatrix);
    Raymarch.UploadShaderDataInvoke();

    cmd.Blit(cameraColourTexture, _destination, Raymarch.Material);
    cmd.Blit(_destination, cameraColourTexture);

    context.ExecuteCommandBuffer(cmd);
    cmd.Clear();
    context.Submit();
    CommandBufferPool.Release(cmd);
  }
}