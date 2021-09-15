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
        
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Raymarch.Compute(src,dest, Camera, settings, null);
        }
    }
}