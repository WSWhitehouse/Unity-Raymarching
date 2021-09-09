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
        // Compute Shader
        [SerializeField] private ComputeShader shader;
        private int _kernelIndex = 0;
        private RenderTexture _source;
        private RenderTexture _destination;
        private int _rtResX;
        private int _rtResY;

        // Raymarching
        [SerializeField] private float renderDistance = 100.0f;
        [SerializeField] [Range(0.1f, 1f)] private float imageResolution = 0.7f;
        [SerializeField] private int maxIterations = 164;
        [SerializeField] private float hitResolution = 0.001f;

        // Lighting & Shadows
        [SerializeField] private Color ambientColour = new Color(54, 58, 66, 255);
        [SerializeField] private ShadowType shadowType = ShadowType.SoftShadows;
        [SerializeField] private float shadowIntensity = 1.0f;
        [SerializeField] private int shadowSteps = 10;
        [SerializeField] private Vector2 shadowDistance = new Vector2(0.05f, 50.0f);
        [SerializeField] private float shadowPenumbra;

        private enum ShadowType
        {
            NoShadows,
            HardShadows,
            SoftShadows
        }

        // Raymarch Objects
        public List<RaymarchObject> RaymarchObjects { get; private set; } = new List<RaymarchObject>();
        private List<RaymarchObjectInfo> _objectInfos = new List<RaymarchObjectInfo>();
        private ComputeBuffer _objectsBuffer;

        // RaymarchLights
        public List<RaymarchLight> RaymarchLights { get; private set; } = new List<RaymarchLight>();
        private List<RaymarchLightInfo> _lightInfos = new List<RaymarchLightInfo>();
        private ComputeBuffer _lightsBuffer;

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
        private static readonly int shader_CamResolution = Shader.PropertyToID("_CamResolution");
        private static readonly int shader_RenderDistance = Shader.PropertyToID("_RenderDistance");

        private static readonly int shader_MaxIterations = Shader.PropertyToID("_MaxIterations");
        private static readonly int shader_HitResolution = Shader.PropertyToID("_HitResolution");

        private static readonly int shader_AmbientColour = Shader.PropertyToID("_AmbientColour");
        private static readonly string shader_NO_SHADOWS = "NO_SHADOWS";
        private static readonly string shader_HARD_SHADOWS = "HARD_SHADOWS";
        private static readonly string shader_SOFT_SHADOWS = "SOFT_SHADOWS";
        private static readonly int shader_ShadowIntensity = Shader.PropertyToID("_ShadowIntensity");
        private static readonly int shader_ShadowSteps = Shader.PropertyToID("_ShadowSteps");
        private static readonly int shader_ShadowDistance = Shader.PropertyToID("_ShadowDistance");
        private static readonly int shader_ShadowPenumbra = Shader.PropertyToID("_ShadowPenumbra");

        private static readonly int shader_ObjectInfo = Shader.PropertyToID("_ObjectInfo");
        private static readonly int shader_ObjectInfoCount = Shader.PropertyToID("_ObjectInfoCount");

        private static readonly int shader_LightInfo = Shader.PropertyToID("_LightInfo");
        private static readonly int shader_LightInfoCount = Shader.PropertyToID("_LightInfoCount");


        [ImageEffectUsesCommandBuffer]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
#if UNITY_EDITOR
            // Only find objects in editor. Objects will automatically add
            // themselves to the list in play mode.
            RaymarchObjects = FindObjectsOfType<RaymarchObject>().ToList();
            RaymarchLights = FindObjectsOfType<RaymarchLight>().ToList();
#endif

            if (shader == null || RaymarchObjects.Count == 0 || RaymarchLights.Count == 0)
            {
                Graphics.Blit(src, dest);
                return;
            }

            // Render Texture
            InitRenderTexture();
            // _kernelIndex = shader.FindKernel("CSMain");
            Graphics.Blit(src, _source);
            shader.SetTexture(_kernelIndex, shader_Source, _source);
            shader.SetTexture(_kernelIndex, shader_Destination, _destination);

            // Shader Properties
            CreateObjectInfoBuffer();
            CreateLightInfoBuffer();
            SetShaderProperties();

            int threadGroupsX = Mathf.CeilToInt(Camera.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(Camera.pixelHeight / 8.0f);
            shader.Dispatch(_kernelIndex, threadGroupsX, threadGroupsY, 1);

            Graphics.Blit(_destination, dest);
            DisposeBuffers();
        }

        private void InitRenderTexture()
        {
            _rtResX = (int) (Camera.pixelWidth * imageResolution);
            _rtResY = (int) (Camera.pixelHeight * imageResolution);

            CreateRenderTexture(ref _source);
            CreateRenderTexture(ref _destination);
        }

        private void CreateRenderTexture(ref RenderTexture renderTexture)
        {
            if (renderTexture != null && renderTexture.width == _rtResX && renderTexture.height == _rtResY)
            {
                return;
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
            }

            renderTexture = new RenderTexture(_rtResX, _rtResY, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };

            renderTexture.Create();
        }

        private void SetShaderProperties()
        {
            // Camera
            shader.SetTextureFromGlobal(_kernelIndex, shader_CamDepthTexture, shader_CameraDepthTexture);
            shader.SetMatrix(shader_CamInverseProjection, Camera.projectionMatrix.inverse);
            shader.SetMatrix(shader_CamToWorld, Camera.cameraToWorldMatrix);
            shader.SetFloat(shader_CamResolution, imageResolution);
            shader.SetFloat(shader_RenderDistance, renderDistance);

            // Lighting & Shadows
            shader.SetVector(shader_AmbientColour, ambientColour);

            shader.DisableKeyword(shader_HARD_SHADOWS);
            shader.DisableKeyword(shader_SOFT_SHADOWS);
            shader.DisableKeyword(shader_NO_SHADOWS);

            switch (shadowType)
            {
                case ShadowType.HardShadows:
                    shader.EnableKeyword(shader_HARD_SHADOWS);
                    break;
                case ShadowType.SoftShadows:
                    shader.EnableKeyword(shader_SOFT_SHADOWS);
                    break;
                default:
                    shader.EnableKeyword(shader_NO_SHADOWS);
                    break;
            }

            if (shadowType != ShadowType.NoShadows)
            {
                shader.SetFloat(shader_ShadowIntensity, shadowIntensity);
                shader.SetInt(shader_ShadowSteps, shadowSteps);
                shader.SetVector(shader_ShadowDistance, shadowDistance);

                if (shadowType == ShadowType.SoftShadows)
                {
                    shader.SetFloat(shader_ShadowPenumbra, shadowPenumbra);
                }
            }

            // Raymarching
            shader.SetInt(shader_MaxIterations, maxIterations);
            shader.SetFloat(shader_HitResolution, hitResolution);

            // Compute Buffer
            shader.SetBuffer(_kernelIndex, shader_ObjectInfo, _objectsBuffer);
            shader.SetInt(shader_ObjectInfoCount, _objectsBuffer.count);

            shader.SetBuffer(_kernelIndex, shader_LightInfo, _lightsBuffer);
            shader.SetInt(shader_LightInfoCount, _lightsBuffer.count);
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

            _objectsBuffer = new ComputeBuffer(count, RaymarchObjectInfo.GetSize(), ComputeBufferType.Default);
            _objectsBuffer.SetData(_objectInfos);
        }

        private void CreateLightInfoBuffer()
        {
            int count = RaymarchLights.Count;

            if (_lightInfos.Count != count)
            {
                _lightInfos = new List<RaymarchLightInfo>(count);
            }

            for (int i = 0; i < count; i++)
            {
                if (_lightInfos.Count <= i)
                {
                    _lightInfos.Add(new RaymarchLightInfo(RaymarchLights[i]));
                }
                else
                {
                    _lightInfos[i] = new RaymarchLightInfo(RaymarchLights[i]);
                }
            }

            _lightsBuffer = new ComputeBuffer(count, RaymarchLightInfo.GetSize(), ComputeBufferType.Default);
            _lightsBuffer.SetData(_lightInfos);
        }

        private void DisposeBuffers()
        {
            _objectsBuffer.Dispose();
            _lightsBuffer.Dispose();
        }
    }
}