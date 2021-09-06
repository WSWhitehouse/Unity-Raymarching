using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WSWhitehouse
{
    [RequireComponent(typeof(Camera)), ImageEffectAllowedInSceneView, ExecuteAlways, DisallowMultipleComponent]
    public class RaymarchCamera : MonoBehaviour
    {
        // Shader
        [SerializeField] private ComputeShader shader;
        private int _kernelIndex = 0;
        private RenderTexture _target;

        // Lights
        [SerializeField] private Light directionalLight;

        [Header("Shader Variables")] [SerializeField]
        private float maxDistance;

        // Raymarch Objects
        private List<RaymarchObject> _raymarchObjects = new List<RaymarchObject>();
        public List<RaymarchObject> RaymarchObjects => _raymarchObjects;

        // Camera
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

        // Shader IDs
        private static readonly int shader_Source = Shader.PropertyToID("_Source");
        private static readonly int shader_Destination = Shader.PropertyToID("_Destination");
        private static readonly int shader_CamDepthTexture = Shader.PropertyToID("_CamDepthTexture");
        private static readonly int shader_CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        private static readonly int shader_CamInverseProjection = Shader.PropertyToID("_CamInverseProjection");
        private static readonly int shader_CamToWorld = Shader.PropertyToID("_CamToWorld");
        private static readonly int shader_LightDirection = Shader.PropertyToID("_LightDirection");
        private static readonly int shader_MaxDistance = Shader.PropertyToID("_MaxDistance");


        [ImageEffectUsesCommandBuffer]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (shader == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            // Render Texture
            InitRenderTexture();
            // _kernelIndex = shader.FindKernel("CSMain");
            shader.SetTexture(_kernelIndex, shader_Source, src);
            shader.SetTexture(_kernelIndex, shader_Destination, _target);

            // Shader Properties
            SetShaderProperties();

            int threadGroupsX = Mathf.CeilToInt(Camera.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(Camera.pixelHeight / 8.0f);
            shader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            Graphics.Blit(_target, dest);
        }

        private void InitRenderTexture()
        {
            if (_target != null && _target.width == Camera.pixelWidth && _target.height == Camera.pixelHeight)
            {
                return;
            }

            if (_target != null)
            {
                _target.Release();
            }

            _target = new RenderTexture(Camera.pixelWidth, Camera.pixelHeight, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };

            _target.Create();
        }

        private void SetShaderProperties()
        {
            // Camera
            shader.SetTextureFromGlobal(_kernelIndex, shader_CamDepthTexture, shader_CameraDepthTexture);
            shader.SetMatrix(shader_CamInverseProjection, Camera.projectionMatrix.inverse);
            shader.SetMatrix(shader_CamToWorld, Camera.cameraToWorldMatrix);

            // Lighting
            shader.SetVector(shader_LightDirection,
                directionalLight != null ? directionalLight.transform.forward : Vector3.down);

            shader.SetFloat(shader_MaxDistance, maxDistance);
        }
    }
}