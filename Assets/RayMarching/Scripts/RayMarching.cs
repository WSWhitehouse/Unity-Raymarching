using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WSWhitehouse.RayMarching.Enums;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace WSWhitehouse.RayMarching
{
    [ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera)),
     ExecuteAlways, DisallowMultipleComponent]
    public class RayMarching : MonoBehaviour
    {
        #region VARIABLES

        // Shader
        [SerializeField] private ComputeShader rayMarchingShader;
        private List<ComputeBuffer> _computeBuffer = new List<ComputeBuffer>();

        // RayMarching
        [SerializeField] private float maxDistance = 200f;
        [SerializeField] private int maxStepCount = 250;

        public float MaxDistance
        {
            get => maxDistance;
            set => maxDistance = value;
        }

        public int MaxStepCount
        {
            get => maxStepCount;
            set => maxStepCount = value;
        }

        // Components
        [SerializeField] private Light mainLight;
        private Camera _camera;

        // Background
        [SerializeField] private bool enableSkyBoxCol = false;

        public bool EnableSkyBoxCol
        {
            get => enableSkyBoxCol;
            set => enableSkyBoxCol = value;
        }

        [SerializeField] private ColourType skyBoxType = ColourType.Gradient;

        public ColourType SkyBoxType
        {
            get => skyBoxType;
            set => skyBoxType = value;
        }

        [SerializeField] private Color skyBoxCol = new Color(0.2f, 0.0117647059f, 0.0784313725f, 1);
        [SerializeField] private Color skyBoxTopCol = new Color(0.2f, 0.0117647059f, 0.0784313725f, 1);
        [SerializeField] private Color skyBoxBottomCol = new Color(0.0627450980f, 0.0235294118f, 0.1098039216f, 1);

        // Private Variables
        private RenderTexture _target;

        // Shape List
        public List<SDFShape> Shapes { get; private set; } = new List<SDFShape>();
        public int NumOfShapes => Shapes.Count;

        #endregion VARIABLES

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (rayMarchingShader == null || mainLight == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            _computeBuffer = new List<ComputeBuffer>();

            FindShapesInScene();

            if (Shapes.Count == 0)
            {
                Graphics.Blit(src, dest);
                return; 
            }
            
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

            // RayMarching
            rayMarchingShader.SetFloat("_MaxDst", MaxDistance);
            rayMarchingShader.SetInt("_MaxStepCount", MaxStepCount);

            // SkyBox
            if (rayMarchingShader.IsKeywordEnabled("ENABLE_SKY_BOX") != EnableSkyBoxCol)
            {
                if (EnableSkyBoxCol)
                {
                    rayMarchingShader.EnableKeyword("ENABLE_SKY_BOX");
                }
                else
                {
                    rayMarchingShader.DisableKeyword("ENABLE_SKY_BOX");
                }
            }

            if (EnableSkyBoxCol)
            {
                int skyType;

                switch (SkyBoxType)
                {
                    case ColourType.Colour:
                        skyType = 1;
                        break;
                    case ColourType.Gradient:
                        skyType = 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                rayMarchingShader.SetInt("_SkyBoxType", skyType);
                rayMarchingShader.SetVector("_SkyBoxCol", skyBoxCol);
                rayMarchingShader.SetVector("_SkyBoxTopCol", skyBoxTopCol);
                rayMarchingShader.SetVector("_SkyBoxBottomCol", skyBoxBottomCol);
            }
        }

        private void FindShapesInScene()
        {
            Shapes = FindObjectsOfType<SDFShape>().ToList();
            Shapes.Sort((a, b) => a.Operation.CompareTo(b.Operation));
        }

        private void InitSceneShapes()
        {
            List<SDFShape> orderedShapes = new List<SDFShape>();

            foreach (SDFShape shape in Shapes)
            {
                if (shape.transform.parent != null) continue;

                Transform parentShape = shape.transform;
                orderedShapes.Add(shape);
                shape.NumOfChildren = parentShape.childCount;

                for (int j = 0; j < parentShape.childCount; j++)
                {
                    if (parentShape.GetChild(j).GetComponent<SDFShape>() == null) continue;

                    orderedShapes.Add(parentShape.GetChild(j).GetComponent<SDFShape>());
                    orderedShapes[orderedShapes.Count - 1].NumOfChildren = 0;
                }
            }


            ShapeData[] shapeData = new ShapeData[NumOfShapes];

            for (int i = 0; i < NumOfShapes; i++)
            {
                SDFShape sdfShape = orderedShapes[i];
                Vector3 colour = new Vector3(sdfShape.Colour.r, sdfShape.Colour.g, sdfShape.Colour.b);

                shapeData[i] = new ShapeData
                {
                    // Translation
                    Position = sdfShape.Position,
                    Rotation = sdfShape.Rotation,
                    Scale = sdfShape.Scale,

                    // Object Properties
                    Colour = colour,
                    ShapeType = (int) sdfShape.ShapeType,
                    Modifier = sdfShape.Modifier,
                    Roundness = sdfShape.Roundness,
                    WallThickness = sdfShape.WallThickness,

                    // RayMarch
                    MarchingStepAmount = sdfShape.MarchingStepAmount,
                    Operation = (int) sdfShape.Operation,
                    BlendStrength = sdfShape.BlendStrength,

                    // Sine Wave
                    EnableSineWave = sdfShape.EnableSineWave ? 1 : 0,
                    SineWaveDirection = sdfShape.SineWaveDirection,
                    SineWaveFreq = sdfShape.SineWaveFrequency,
                    SineWaveSpeed = sdfShape.SineWaveSpeed,
                    SineWaveAmp = sdfShape.SineWaveAmplitude,

                    // Num of Children
                    NumOfChildren = sdfShape.NumOfChildren
                };
            }

            ComputeBuffer shapeBuffer = new ComputeBuffer(NumOfShapes, ShapeData.GetSize());
            shapeBuffer.SetData(shapeData);

            rayMarchingShader.SetBuffer(0, "Shapes", shapeBuffer);
            rayMarchingShader.SetInt("NumShapes", NumOfShapes);

            _computeBuffer.Add(shapeBuffer);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(RayMarching))]
    public class RayMarchingEditor : Editor
    {
        // Target
        private RayMarching _rayMarching;

        // Serialized Properties
        private SerializedProperty _rayMarchingShader;
        private SerializedProperty _maxDistance;
        private SerializedProperty _maxStepCount;
        private SerializedProperty _mainLight;
        private SerializedProperty _skyBoxCol;
        private SerializedProperty _skyBoxTopCol;
        private SerializedProperty _skyBoxBottomCol;

        // Dropdown
        private static bool _rayMarchingDropdown = false;
        private static bool _backgroundPropertiesDropdown = false;

        private void OnEnable()
        {
            // Target
            _rayMarching = (RayMarching) target;

            // Serialized Properties
            _rayMarchingShader = serializedObject.FindProperty("rayMarchingShader");
            _maxDistance = serializedObject.FindProperty("maxDistance");
            _maxStepCount = serializedObject.FindProperty("maxStepCount");
            _mainLight = serializedObject.FindProperty("mainLight");
            _skyBoxCol = serializedObject.FindProperty("skyBoxCol");
            _skyBoxTopCol = serializedObject.FindProperty("skyBoxTopCol");
            _skyBoxBottomCol = serializedObject.FindProperty("skyBoxCol");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Shader", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_rayMarchingShader);
            EditorGUILayout.Space();
            DrawRayMarchingProperties();
            DrawBackgroundProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRayMarchingProperties()
        {
            _rayMarchingDropdown = EditorGUILayout.BeginFoldoutHeaderGroup(_rayMarchingDropdown, "Ray Marching");

            if (_rayMarchingDropdown)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(_maxDistance);
                EditorGUILayout.PropertyField(_maxStepCount);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_mainLight);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawBackgroundProperties()
        {
            _backgroundPropertiesDropdown = EditorGUILayout.BeginFoldoutHeaderGroup(_backgroundPropertiesDropdown,
                "Sky Box");

            if (_backgroundPropertiesDropdown)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("box");

                string buttonText = _rayMarching.EnableSkyBoxCol
                    ? "Disable Custom Sky Box"
                    : "Enable Custom Sky Box";

                if (GUILayout.Button(buttonText))
                {
                    _rayMarching.EnableSkyBoxCol = !_rayMarching.EnableSkyBoxCol;
                }

                if (_rayMarching.EnableSkyBoxCol)
                {
                    EditorGUILayout.LabelField("Sky Box Properties", EditorStyles.boldLabel);
                    _rayMarching.SkyBoxType =
                        (ColourType) EditorGUILayout.EnumPopup("Sky Box Type", _rayMarching.SkyBoxType);

                    switch (_rayMarching.SkyBoxType)
                    {
                        case ColourType.Colour:
                        {
                            EditorGUILayout.PropertyField(_skyBoxCol);
                            break;
                        }
                        case ColourType.Gradient:
                        {
                            EditorGUILayout.PropertyField(_skyBoxTopCol);
                            EditorGUILayout.PropertyField(_skyBoxBottomCol);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

#endif
}