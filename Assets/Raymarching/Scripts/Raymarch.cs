using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace WSWhitehouse
{
    public static class Raymarch
    {
        // Raymarch Objects
        public static List<RaymarchObject> Objects { get; private set; } = new();
        private static List<RaymarchObjectInfo> _objectInfos = new();

        // RaymarchLights
        public static List<RaymarchLight> Lights { get; private set; } = new();
        private static List<RaymarchLightInfo> _lightInfos = new();

        public static void Compute(RenderTexture src, RenderTexture dest, Camera camera, RaymarchSettings settings,
            [CanBeNull] RenderTexture depthRT = null)
        {
            if (settings.shader == null || Objects.Count == 0 || Lights.Count == 0)
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

            if (depthRT == null)
            {
                settings.shader.SetTextureFromGlobal(settings.KernelIndex, ShaderID.DepthTexture,
                    ShaderID.CameraDepthTexture);
            }
            else
            {
                settings.shader.SetTexture(settings.KernelIndex, ShaderID.DepthTexture, depthRT);
            }

            // Set Shader Properties
            SetShaderProperties(camera, settings);

            // Create & Set Buffers
            var objectsBuffer = CreateObjectInfoBuffer();
            var lightsBuffer = CreateLightInfoBuffer();

            settings.shader.SetBuffer(settings.KernelIndex, ShaderID.ObjectInfo, objectsBuffer);
            settings.shader.SetInt(ShaderID.ObjectInfoCount, objectsBuffer.count);

            settings.shader.SetBuffer(settings.KernelIndex, ShaderID.LightInfo, lightsBuffer);
            settings.shader.SetInt(ShaderID.LightInfoCount, lightsBuffer.count);

            // Dispatch Shader
            int threadGroupsX = Mathf.CeilToInt(camera.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(camera.pixelHeight / 8.0f);
            settings.shader.Dispatch(settings.KernelIndex, threadGroupsX, threadGroupsY, 1);

            // Blit to Final Destination
            Graphics.Blit(destination, dest);

            // Cleanup
            RenderTexture.ReleaseTemporary(destination);
            objectsBuffer.Dispose();
            lightsBuffer.Dispose();
        }

        private static void SetShaderProperties(Camera camera, RaymarchSettings settings)
        {
            // Camera
            settings.shader.SetMatrix(ShaderID.CamInverseProjection, camera.projectionMatrix.inverse);
            settings.shader.SetMatrix(ShaderID.CamToWorld, camera.cameraToWorldMatrix);
            settings.shader.SetFloat(ShaderID.CamNearClipPlane, camera.nearClipPlane);

            // Raymarching
            settings.shader.SetFloat(ShaderID.RenderDistance, settings.renderDistance - camera.nearClipPlane);
            settings.shader.SetFloat(ShaderID.HitResolution, settings.hitResolution);
            settings.shader.SetFloat(ShaderID.Relaxation, settings.relaxation);
            settings.shader.SetInt(ShaderID.MaxIterations, settings.maxIterations);

            // Lighting & Shadows
            settings.shader.SetVector(ShaderID.AmbientColour, settings.ambientColour);

            settings.shader.DisableKeyword(ShaderID.HardShadows);
            settings.shader.DisableKeyword(ShaderID.SoftShadows);
            settings.shader.DisableKeyword(ShaderID.NoShadows);

            switch (settings.shadowType)
            {
                case RaymarchSettings.ShadowType.HardShadows:
                    settings.shader.EnableKeyword(ShaderID.HardShadows);
                    break;
                case RaymarchSettings.ShadowType.SoftShadows:
                    settings.shader.EnableKeyword(ShaderID.SoftShadows);
                    break;
                default:
                    settings.shader.EnableKeyword(ShaderID.NoShadows);
                    break;
            }

            if (settings.shadowType != RaymarchSettings.ShadowType.NoShadows)
            {
                settings.shader.SetFloat(ShaderID.ShadowIntensity, settings.shadowIntensity);
                settings.shader.SetInt(ShaderID.ShadowSteps, settings.shadowSteps);
                settings.shader.SetVector(ShaderID.ShadowDistance, settings.shadowDistance);

                if (settings.shadowType == RaymarchSettings.ShadowType.SoftShadows)
                {
                    settings.shader.SetFloat(ShaderID.ShadowPenumbra, settings.shadowPenumbra);
                }
            }
        }

        private static ComputeBuffer CreateObjectInfoBuffer()
        {
            int count = Objects.Count;
            
            CheckListCapacity(ref _objectInfos, count);

            Objects = Objects
                .OrderBy(x => x.OperationLayer)
                .ThenBy(x => x.Operation)
                .ToList();

            for (int i = 0; i < count; i++)
            {
                if (_objectInfos.Count <= i)
                {
                    _objectInfos.Add(new RaymarchObjectInfo(Objects[i]));
                }
                else
                {
                    _objectInfos[i] = new RaymarchObjectInfo(Objects[i]);
                }
            }

            var buffer = new ComputeBuffer(count, RaymarchObjectInfo.GetSize(), ComputeBufferType.Default);
            buffer.SetData(_objectInfos);

            return buffer;
        }

        private static ComputeBuffer CreateLightInfoBuffer()
        {
            int count = Lights.Count;

            CheckListCapacity(ref _lightInfos, count);

            for (int i = 0; i < count; i++)
            {
                if (_lightInfos.Count <= i)
                {
                    _lightInfos.Add(new RaymarchLightInfo(Lights[i]));
                }
                else
                {
                    _lightInfos[i] = new RaymarchLightInfo(Lights[i]);
                }
            }

            var buffer = new ComputeBuffer(count, RaymarchLightInfo.GetSize(), ComputeBufferType.Default);
            buffer.SetData(_lightInfos);

            return buffer;
        }

        private static void CheckListCapacity<T>(ref List<T> list, int count)
        {
            if (list.Capacity < count)
            {
                // Unavoidable Heap Alloc
                list = new List<T>(count);
            }
            
            if (list.Count > count)
            {
                list.RemoveRange(0, list.Count - count);
            }
        }
    }
}