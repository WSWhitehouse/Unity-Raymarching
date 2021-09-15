using UnityEngine;
using UnityEngine.Rendering;

namespace WSWhitehouse
{
    [RequireComponent(typeof(Camera)), ExecuteAlways, DisallowMultipleComponent]
    public class RaymarchCameraURP : MonoBehaviour
    {
        [SerializeField] private RaymarchSettings settings;

        private void OnEnable()
        {
            if (settings == null) return;

            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            if (cam.cameraType is CameraType.Preview or CameraType.SceneView) return;

            RenderTexture tmp = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            cam.targetTexture = tmp;
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            if (cam.cameraType is CameraType.Preview) return;

#if UNITY_EDITOR
            if (cam.cameraType is CameraType.SceneView)
            {
                Raymarch.Compute(cam.targetTexture, cam.targetTexture, cam, settings, null);
                return;
            }
#endif

            RenderTexture source = cam.targetTexture;
            cam.targetTexture = null;

            Raymarch.Compute(source, null, cam, settings, null);

            RenderTexture.ReleaseTemporary(source);
        }
    }
}