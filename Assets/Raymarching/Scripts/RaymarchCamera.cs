using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        private ComputeBuffer _computeBuffer;

        // Lights
        [SerializeField] private Light directionalLight;

        [Header("Raymarching")] [SerializeField]
        private float renderDistance = 100.0f;

        [SerializeField] private int maxIterations = 164;

        [SerializeField] private float hitResolution = 0.001f;

        // Raymarch Objects
        private List<RaymarchObjectInfo> _objectInfos = new List<RaymarchObjectInfo>();

        private List<RaymarchObject> _raymarchObjects = new List<RaymarchObject>();

        public List<RaymarchObject> RaymarchObjects
        {
            get => _raymarchObjects;
            private set => _raymarchObjects = value;
        }

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
        private static readonly int shader_MaxIterations = Shader.PropertyToID("_MaxIterations");
        private static readonly int shader_HitResolution = Shader.PropertyToID("_HitResolution");
        private static readonly int shader_CamDepthTexture = Shader.PropertyToID("_CamDepthTexture");
        private static readonly int shader_CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        private static readonly int shader_CamInverseProjection = Shader.PropertyToID("_CamInverseProjection");
        private static readonly int shader_CamToWorld = Shader.PropertyToID("_CamToWorld");
        private static readonly int shader_RenderDistance = Shader.PropertyToID("_RenderDistance");
        private static readonly int shader_LightDirection = Shader.PropertyToID("_LightDirection");
        private static readonly int shader_ObjectInfo = Shader.PropertyToID("_ObjectInfo");
        private static readonly int shader_ObjectInfoCount = Shader.PropertyToID("_ObjectInfoCount");


        [ImageEffectUsesCommandBuffer]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
#if UNITY_EDITOR
            // Only find objects in editor. Objects will automatically add
            // themselves to the list in play mode.
            RaymarchObjects = FindObjectsOfType<RaymarchObject>().ToList();
#endif

            if (shader == null || RaymarchObjects.Count == 0)
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
            CreateObjectInfoBuffer();
            SetShaderProperties();

            int threadGroupsX = Mathf.CeilToInt(Camera.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(Camera.pixelHeight / 8.0f);
            shader.Dispatch(_kernelIndex, threadGroupsX, threadGroupsY, 1);

            Graphics.Blit(_target, dest);
            _computeBuffer.Dispose();
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
            shader.SetFloat(shader_RenderDistance, renderDistance);

            // Raymarching
            shader.SetInt(shader_MaxIterations, maxIterations);
            shader.SetFloat(shader_HitResolution, hitResolution);

            // Lighting
            shader.SetVector(shader_LightDirection,
                directionalLight != null ? directionalLight.transform.forward : Vector3.down);

            // Compute Buffer
            shader.SetBuffer(_kernelIndex, shader_ObjectInfo, _computeBuffer);
            shader.SetInt(shader_ObjectInfoCount, _computeBuffer.count);
        }

        private void CreateObjectInfoBuffer()
        {
            int count = RaymarchObjects.Count;

            if (_objectInfos.Count != count)
            {
                _objectInfos = new List<RaymarchObjectInfo>(count);
            }

            // RaymarchObjects.Sort((a, b) => a.OperationLayer.CompareTo(b.OperationLayer));

            RaymarchObjects = RaymarchObjects
                .OrderBy(x => x.OperationLayer)
                .ThenBy(x => x.Operation)
                .ToList();

            for (int i = 0; i < count; i++)
            {
                if (_objectInfos.Count <= i)
                {
                    _objectInfos.Add(new RaymarchObjectInfo(RaymarchObjects[i]));
                }
                else
                {
                    _objectInfos[i] = new RaymarchObjectInfo(RaymarchObjects[i]);
                }
            }

            _computeBuffer = new ComputeBuffer(count, RaymarchObjectInfo.GetSize(), ComputeBufferType.Default);
            _computeBuffer.SetData(_objectInfos);
        }
    }
}