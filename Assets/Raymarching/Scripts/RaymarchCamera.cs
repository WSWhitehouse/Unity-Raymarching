using UnityEngine;

[ExecuteAlways]
public class RaymarchCamera : MonoBehaviour
{
  [SerializeField] private float cameraPositionW = 0.0f;

  public float CameraPositionW
  {
    get => cameraPositionW;
    set => cameraPositionW = value;
  }
  
  [SerializeField] private Vector3 cameraRotation4D = Vector3.zero;

  public Vector3 CameraRotation4D
  {
    get => cameraRotation4D;
    set => cameraRotation4D = value;
  }
  
  struct ShaderIDs
  {
    public int CamPositionW;
    public int CamRotation4D;
  }

  private ShaderIDs _shaderIDs = new ShaderIDs();
  
  public void Awake()
  {
    Raymarch.OnUploadShaderData += UploadShaderData;

    _shaderIDs.CamPositionW = Shader.PropertyToID("_CamPositionW");
    _shaderIDs.CamRotation4D = Shader.PropertyToID("_CamRotation4D");
  }

  private void OnDestroy()
  {
    Raymarch.OnUploadShaderData -= UploadShaderData;
  }

  private void UploadShaderData(Material material)
  {
    material.SetFloat(_shaderIDs.CamPositionW, CameraPositionW);
    material.SetVector(_shaderIDs.CamRotation4D, CameraRotation4D);
  }
}
