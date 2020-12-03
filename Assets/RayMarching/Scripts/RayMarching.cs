using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WSWhitehouse.RayMarching.Structs;

namespace WSWhitehouse.RayMarching
{
    [ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera)), ExecuteAlways, DisallowMultipleComponent]
    public class RayMarching : MonoBehaviour
    {
        // Shader
        [SerializeField] private ComputeShader rayMarchingShader;
        private List<ComputeBuffer> _computeBuffer = new List<ComputeBuffer>();

        // Components
        [SerializeField] private Light mainLight;
        private Camera _camera;

        // Private Variables
        private RenderTexture _target;

        // Shape List
        [SerializeField] private List<SDFShape> _shapes = new List<SDFShape>();
        public int NumOfShapes => _shapes.Count;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (rayMarchingShader == null || _camera == null || mainLight == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            _computeBuffer = new List<ComputeBuffer>();

            InitRenderTexture();
            InitSceneShapes();
            SetShaderParams();

            rayMarchingShader.SetTexture(0, "Source", src);
            rayMarchingShader.SetTexture(0, "Destination", _target);

            int threadGroupsX = Mathf.CeilToInt(_camera.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(_camera.pixelHeight / 8.0f);
            rayMarchingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            Graphics.Blit(_target, dest);

            foreach (var buffer in _computeBuffer)
            {
                buffer.Dispose();
            }
        }

        private void InitRenderTexture()
        {
            if (_target != null && _target.width == _camera.pixelWidth && _target.height == _camera.pixelHeight)
            {
                return;
            }

            if (_target != null)
            {
                _target.Release();
            }

            _target = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };

            _target.Create();
        }

        private void SetShaderParams()
        {
            // Camera
            rayMarchingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
            rayMarchingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);

            // Light
            bool lightIsDirectional = mainLight.type == LightType.Directional;
            var mainLightTransform = mainLight.transform;
            rayMarchingShader.SetVector("_Light",
                lightIsDirectional ? mainLightTransform.forward : mainLightTransform.position);
            rayMarchingShader.SetBool("_PositionLight", !lightIsDirectional);

            // Time
            rayMarchingShader.SetFloat("_Time", Application.isPlaying ? Time.time : Time.realtimeSinceStartup);
        }

        private void InitSceneShapes()
        {
            _shapes = FindObjectsOfType<SDFShape>().ToList();

            _shapes.Sort((a, b) => a.Operation.CompareTo(b.Operation));

            ShapeData[] shapeData = new ShapeData[NumOfShapes];

            for (int i = 0; i < NumOfShapes; i++)
            {
                SDFShape sdfShape = _shapes[i];
                Vector3 colour = new Vector3(sdfShape.Colour.r, sdfShape.Colour.g, sdfShape.Colour.b);

                shapeData[i] = new ShapeData
                {
                    Position = sdfShape.Position,
                    Rotation = sdfShape.Rotation,
                    Scale = sdfShape.Scale,
                    Colour = colour,
                    ShapeType = (int) sdfShape.ShapeType,
                    Operation = (int) sdfShape.Operation
                };
            }

            ComputeBuffer shapeBuffer = new ComputeBuffer(NumOfShapes, ShapeData.GetSize());
            shapeBuffer.SetData(shapeData);

            rayMarchingShader.SetBuffer(0, "Shapes", shapeBuffer);
            rayMarchingShader.SetInt("NumShapes", NumOfShapes);

            _computeBuffer.Add(shapeBuffer);
        }
    }
}