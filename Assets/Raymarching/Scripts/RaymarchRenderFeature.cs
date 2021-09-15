using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WSWhitehouse
{
    public class RaymarchRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private RaymarchRenderSettings settings = new();

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
        public RenderPassEvent PassEvent = RenderPassEvent.AfterRenderingOpaques;
        public RaymarchSettings RaymarchSettings;
    }

    public class RaymarchRenderPass : ScriptableRenderPass
    {
        private readonly string _name;
        private RaymarchSettings _settings;

        private RenderTargetIdentifier _source;
        private RenderTargetIdentifier _destination;
        private RenderTargetIdentifier _depth;
        private Camera _camera;

        private RenderTexture _sourceRT;
        private RenderTexture _destinationRT;
        private RenderTexture _depthRT;
        private int renderTexSizeX;
        private int renderTexSizeY;

        // Raymarch Objects
        public static List<RaymarchObject> RaymarchObjects { get; private set; } = new();
        private List<RaymarchObjectInfo> _objectInfos = new();
        private ComputeBuffer _objectsBuffer;

        // RaymarchLights
        public static List<RaymarchLight> RaymarchLights { get; private set; } = new();
        private List<RaymarchLightInfo> _lightInfos = new();
        private ComputeBuffer _lightsBuffer;

        // Shader IDs
        private readonly LocalKeyword keyword_NoShadows;
        private readonly LocalKeyword keyword_HardShadows;
        private readonly LocalKeyword keyword_SoftShadows;

        public RaymarchRenderPass(string name, RaymarchRenderSettings settings)
        {
            _name = name;
            _settings = settings.RaymarchSettings;
            renderPassEvent = settings.PassEvent;

            // Shader Keywords
            keyword_NoShadows = new LocalKeyword(_settings.shader, ShaderID.NoShadows);
            keyword_HardShadows = new LocalKeyword(_settings.shader, ShaderID.HardShadows);
            keyword_SoftShadows = new LocalKeyword(_settings.shader, ShaderID.SoftShadows);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // _source = renderingData.cameraData.renderer.cameraColorTarget;
            // _depth = renderingData.cameraData.renderer.cameraDepthTarget;
            // _camera = renderingData.cameraData.camera;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var source = renderingData.cameraData.renderer.cameraColorTarget;
            var depth = renderingData.cameraData.renderer.cameraDepthTarget;

            CommandBuffer cmd = CommandBufferPool.Get();

            CreateRenderTexture(ref _sourceRT, camera.pixelWidth, camera.pixelHeight);
            CreateRenderTexture(ref _destinationRT, camera.pixelWidth, camera.pixelHeight);
            CreateRenderTexture(ref _depthRT, camera.pixelWidth, camera.pixelHeight);

            cmd.Blit(source, _sourceRT);
            cmd.Blit(depth, _depthRT);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            Raymarch.Compute(_sourceRT, _destinationRT, camera, _settings, _depthRT);

            cmd.Blit(_destinationRT, colorAttachment);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            // if (RaymarchObjects.Count == 0)
            // {
            //     Debug.Log("Objs Empty");
            //     return;
            // }
            //
            // if (RaymarchLights.Count == 0)
            // {
            //     Debug.Log("Lights Empty");
            //     return;
            // }
            //
            // CommandBuffer cmd = CommandBufferPool.Get();
            //
            // InitRenderTextures(cmd);
            //
            // cmd.SetComputeTextureParam(_settings.shader, _settings.KernelIndex, ShaderID.Source, _source);
            // cmd.SetComputeTextureParam(_settings.shader, _settings.KernelIndex, ShaderID.Destination, _destinationRT);
            // cmd.SetComputeTextureParam(_settings.shader, _settings.KernelIndex, ShaderID.DepthTexture, _depth);
            //
            // CreateObjectInfoBuffer(cmd);
            // CreateLightInfoBuffer(cmd);
            // SetShaderProperties(cmd);
            //
            // int threadGroupsX = Mathf.CeilToInt(_camera.pixelWidth / 8.0f);
            // int threadGroupsY = Mathf.CeilToInt(_camera.pixelHeight / 8.0f);
            // cmd.DispatchCompute(_settings.shader, _settings.KernelIndex, threadGroupsX, threadGroupsY, 1);
            //
            // cmd.Blit(_destinationRT, colorAttachment);
            //
            // context.ExecuteCommandBuffer(cmd);
            // CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // _objectsBuffer.Dispose();
            // _lightsBuffer.Dispose();
        }

        private void InitRenderTextures(CommandBuffer cmd)
        {
            renderTexSizeX = _camera.pixelWidth;
            renderTexSizeY = _camera.pixelHeight;

            CreateRenderTexture(ref _destinationRT, renderTexSizeX, renderTexSizeY);
        }

        private void CreateRenderTexture(ref RenderTexture renderTexture, int width, int height)
        {
            if (renderTexture != null && renderTexture.width == width &&
                renderTexture.height == height)
            {
                return;
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
            }

            renderTexture = new RenderTexture(width, height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };

            renderTexture.Create();
        }

        private void SetShaderProperties(CommandBuffer cmd)
        {
            // Camera
            cmd.SetComputeMatrixParam(_settings.shader, ShaderID.CamInverseProjection,
                _camera.projectionMatrix.inverse);
            cmd.SetComputeMatrixParam(_settings.shader, ShaderID.CamToWorld, _camera.cameraToWorldMatrix);
            cmd.SetComputeFloatParam(_settings.shader, ShaderID.RenderDistance, _settings.renderDistance);

            // Lighting & Shadows
            cmd.SetComputeVectorParam(_settings.shader, ShaderID.AmbientColour, _settings.ambientColour);

            cmd.DisableKeyword(_settings.shader, keyword_HardShadows);
            cmd.DisableKeyword(_settings.shader, keyword_SoftShadows);
            cmd.DisableKeyword(_settings.shader, keyword_NoShadows);

            switch (_settings.shadowType)
            {
                case RaymarchSettings.ShadowType.HardShadows:
                    cmd.EnableKeyword(_settings.shader, keyword_HardShadows);
                    break;
                case RaymarchSettings.ShadowType.SoftShadows:
                    cmd.EnableKeyword(_settings.shader, keyword_SoftShadows);
                    break;
                default:
                    cmd.EnableKeyword(_settings.shader, keyword_NoShadows);
                    break;
            }

            if (_settings.shadowType != RaymarchSettings.ShadowType.NoShadows)
            {
                cmd.SetComputeFloatParam(_settings.shader, ShaderID.ShadowIntensity,
                    _settings.shadowIntensity);
                cmd.SetComputeIntParam(_settings.shader, ShaderID.ShadowSteps,
                    _settings.shadowSteps);
                cmd.SetComputeVectorParam(_settings.shader, ShaderID.ShadowDistance,
                    _settings.shadowDistance);

                if (_settings.shadowType == RaymarchSettings.ShadowType.SoftShadows)
                {
                    cmd.SetComputeFloatParam(_settings.shader, ShaderID.ShadowPenumbra,
                        _settings.shadowPenumbra);
                }
            }

            // Raymarching
            cmd.SetComputeIntParam(_settings.shader, ShaderID.MaxIterations, _settings.maxIterations);
            cmd.SetComputeFloatParam(_settings.shader, ShaderID.HitResolution, _settings.hitResolution);

            // Compute Buffer
            cmd.SetComputeBufferParam(_settings.shader, _settings.KernelIndex, ShaderID.ObjectInfo, _objectsBuffer);
            cmd.SetComputeIntParam(_settings.shader, ShaderID.ObjectInfoCount, _objectInfos.Count);

            cmd.SetComputeBufferParam(_settings.shader, _settings.KernelIndex, ShaderID.LightInfo, _lightsBuffer);
            cmd.SetComputeIntParam(_settings.shader, ShaderID.LightInfoCount, _lightInfos.Count);
        }

        private void CreateObjectInfoBuffer(CommandBuffer cmd)
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
            cmd.SetBufferData(_objectsBuffer, _objectInfos);
        }

        private void CreateLightInfoBuffer(CommandBuffer cmd)
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
            cmd.SetBufferData(_lightsBuffer, _lightInfos);
        }
    }
}