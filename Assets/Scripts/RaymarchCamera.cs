using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace WSWhitehouse
{
    [RequireComponent(typeof(Camera)), ExecuteInEditMode]
    public class RaymarchCamera : SceneViewFilter
    {
        [SerializeField] private Shader shader;

        [SerializeField] private Light directionalLight;

        [Header("Shader Variables")] [SerializeField]
        private float maxDistance;

        // Raymarch Objects
        private List<RaymarchObject> _raymarchObjects = new List<RaymarchObject>();
        public List<RaymarchObject> RaymarchObjects => _raymarchObjects;

        private ComputeBuffer _objectInfoBuffer;

        private Material _raymarchMaterial;

        public Material RaymarchMaterial
        {
            get
            {
                if (_raymarchMaterial == null && shader != null)
                {
                    _raymarchMaterial = new Material(shader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }

                return _raymarchMaterial;
            }
        }

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
        private static readonly int shader_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int shader_CamFrustum = Shader.PropertyToID("_CamFrustum");
        private static readonly int shader_CamToWorld = Shader.PropertyToID("_CamToWorld");
        private static readonly int shader_MaxDistance = Shader.PropertyToID("_MaxDistance");
        private static readonly int shader_LightDirection = Shader.PropertyToID("_LightDirection");
        private static readonly int shader_ObjectInfoCount = Shader.PropertyToID("_ObjectInfoCount");
        private static readonly int shader_ObjectInfo = Shader.PropertyToID("_ObjectInfo");
        
        [ImageEffectUsesCommandBuffer]
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (RaymarchMaterial == null || RaymarchObjects.Count <= 0)
            {
                Graphics.Blit(src, dest);
                return;
            }

            SetShaderProperties();
            SetObjectInfoBuffer();

            RenderTexture.active = dest;
            _raymarchMaterial.SetTexture(shader_MainTex, src);
            GL.PushMatrix();
            GL.LoadOrtho();
            RaymarchMaterial.SetPass(0);
            GL.Begin(GL.QUADS);

            // Bottom Left
            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 3.0f);

            // Bottom Right
            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 2.0f);

            // Top Right
            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);

            // Top Left
            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);

            GL.End();
            GL.PopMatrix();
        }

        private void SetShaderProperties()
        {
            RaymarchMaterial.SetMatrix(shader_CamFrustum, CameraFrustum());
            RaymarchMaterial.SetMatrix(shader_CamToWorld, Camera.cameraToWorldMatrix);
            RaymarchMaterial.SetFloat(shader_MaxDistance, maxDistance);
            RaymarchMaterial.SetVector(shader_LightDirection,
                directionalLight != null ? directionalLight.transform.forward : Vector3.down);
        }

        private void SetObjectInfoBuffer()
        {
            int objectInfoCount = RaymarchObjects.Count;

            if (objectInfoCount <= 0)
            {
                RaymarchMaterial.SetInt(shader_ObjectInfoCount, 0);
                return;
            }

            RaymarchObjectInfo[] objectInfo = new RaymarchObjectInfo[objectInfoCount];
            for (int i = 0; i < objectInfoCount; i++)
            {
                objectInfo[i] = new RaymarchObjectInfo(RaymarchObjects[i]);
                Debug.Log(objectInfo[i].Position);
            }

            _objectInfoBuffer =
                new ComputeBuffer(objectInfoCount,
                    RaymarchObjectInfo.GetSize(),
                    ComputeBufferType.Default);
            
            _objectInfoBuffer.SetData(objectInfo);

            RaymarchMaterial.SetBuffer(shader_ObjectInfo, _objectInfoBuffer);
            RaymarchMaterial.SetInt(shader_ObjectInfoCount, objectInfoCount);

            _objectInfoBuffer.Release();
        }

        private Matrix4x4 CameraFrustum()
        {
            Matrix4x4 frustum = Matrix4x4.identity;
            float fov = Mathf.Tan((Camera.fieldOfView * 0.5f) * Mathf.Deg2Rad);

            Vector3 goUp = Vector3.up * fov;
            Vector3 goRight = Vector3.right * fov * Camera.aspect;

            Vector3 topLeft = (-Vector3.forward - goRight + goUp);
            Vector3 topRight = (-Vector3.forward + goRight + goUp);
            Vector3 bottomRight = (-Vector3.forward + goRight - goUp);
            Vector3 bottomLeft = (-Vector3.forward - goRight - goUp);

            frustum.SetRow(0, topLeft);
            frustum.SetRow(1, topRight);
            frustum.SetRow(2, bottomRight);
            frustum.SetRow(3, bottomLeft);

            return frustum;
        }
    }
}