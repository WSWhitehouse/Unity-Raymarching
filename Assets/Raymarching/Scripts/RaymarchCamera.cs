using UnityEngine;

namespace WSWhitehouse
{
  [RequireComponent(typeof(Camera)), ImageEffectAllowedInSceneView, ExecuteAlways, DisallowMultipleComponent]
  public class RaymarchCamera : MonoBehaviour
  {
    [SerializeField] private RaymarchSettings settings;

    private Camera _camera;

    private Camera Camera
    {
      get
      {
        if (_camera == null)
        {
          _camera = GetComponent<Camera>();
        }

        return _camera;
      }
    }

    private ComputeBuffer _objectsBuffer;
    private ComputeBuffer _lightsBuffer;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
      Compute(src, dest);
    }

    private void Compute(RenderTexture src, RenderTexture dest)
    {
      if (settings.shader == null || Raymarch.Objects.Count == 0 || Raymarch.Lights.Count == 0)
      {
        Graphics.Blit(src, dest);
        return;
      }

      // Creating Temp Destination Render Texture
      var descriptor = src.descriptor;
      descriptor.enableRandomWrite = true;
      RenderTexture destination = RenderTexture.GetTemporary(descriptor);

      // Set Render Textures in Shader
      settings.shader.SetTexture(settings.KernelIndex, ShaderID.Source, src);
      settings.shader.SetTexture(settings.KernelIndex, ShaderID.Destination, destination);
      settings.shader.SetTextureFromGlobal(settings.KernelIndex, ShaderID.DepthTexture,
        ShaderID.CameraDepthTexture);

      // Set Shader Properties
      SetShaderProperties();

      // Dispatch Shader
      int threadGroupsX = Mathf.CeilToInt(Camera.pixelWidth / 8.0f);
      int threadGroupsY = Mathf.CeilToInt(Camera.pixelHeight / 8.0f);
      settings.shader.Dispatch(settings.KernelIndex, threadGroupsX, threadGroupsY, 1);

      // Blit to Final Destination
      Graphics.Blit(destination, dest);

      // Cleanup
      RenderTexture.ReleaseTemporary(destination);
      _objectsBuffer.Dispose();
      _lightsBuffer.Dispose();
    }

    private void SetShaderProperties()
    {
      // Camera
      settings.shader.SetMatrix(ShaderID.CamInverseProjection, Camera.projectionMatrix.inverse);
      settings.shader.SetMatrix(ShaderID.CamToWorld, Camera.cameraToWorldMatrix);
      settings.shader.SetFloat(ShaderID.CamNearClipPlane, Camera.nearClipPlane);

      // Raymarching
      settings.shader.SetFloat(ShaderID.RenderDistance, settings.renderDistance - Camera.nearClipPlane);
      settings.shader.SetFloat(ShaderID.HitResolution, settings.hitResolution);
      settings.shader.SetFloat(ShaderID.Relaxation, settings.relaxation);
      settings.shader.SetInt(ShaderID.MaxIterations, settings.maxIterations);

      // Lighting & Shadows
      settings.shader.SetVector(ShaderID.AmbientColour, settings.ambientColour);

      // Create & Set Buffers
      _objectsBuffer = Raymarch.CreateObjectInfoBuffer();
      settings.shader.SetBuffer(settings.KernelIndex, ShaderID.ObjectInfo, _objectsBuffer);
      settings.shader.SetInt(ShaderID.ObjectInfoCount, _objectsBuffer.count);

      _lightsBuffer = Raymarch.CreateLightInfoBuffer();
      settings.shader.SetBuffer(settings.KernelIndex, ShaderID.LightInfo, _lightsBuffer);
      settings.shader.SetInt(ShaderID.LightInfoCount, _lightsBuffer.count);
    }
  }
}