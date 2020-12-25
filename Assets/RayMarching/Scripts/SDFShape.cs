using UnityEngine;
using WSWhitehouse.RayMarching.Enums;
using Math = WSWhitehouse.Util.Math;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace WSWhitehouse.RayMarching
{
    [ExecuteAlways, DisallowMultipleComponent]
    public class SDFShape : MonoBehaviour
    {
        #region TRANSLATION
        public Vector3 Position => transform.position;
        public Vector3 Rotation => -Math.QuaternionToEulerAngles(transform.rotation);

        public Vector3 Scale
        {
            get
            {
                if (transform.parent == null)
                {
                    return transform.localScale / 2f;
                }

                Vector3 parentScale = Vector3.one;
                SDFShape parentSDFShape = transform.parent.GetComponent<SDFShape>();
                if (parentSDFShape != null)
                {
                    parentScale = parentSDFShape.Scale;
                }

                return Vector3.Scale(transform.localScale, parentScale);
            }
        }
        #endregion TRANSLATION

        #region OBJECT PROPERTIES
        [SerializeField] private ShapeType shapeType = ShapeType.Cube;
        public ShapeType ShapeType => shapeType;

        [SerializeField] private Vector3 modifier = Vector3.zero;

        public Vector3 Modifier
        {
            get => modifier;
            set => modifier = value;
        }


        [SerializeField] private Color colour = Color.white;
        public Color Colour => colour;

        [SerializeField, Range(0, 1)] private float roundness = 0f;

        public float Roundness
        {
            get
            {
                float[] scales = {Scale.x, Scale.y, Scale.z};
                float minScale = Mathf.Min(scales);
                float maxRoundness = minScale / 2.0f;
                return roundness * maxRoundness * 2.0f;
            }
            set => roundness = Mathf.Clamp(value, 0f, 1f);
        }

        [SerializeField] private bool hollow = false;

        public bool Hollow
        {
            get => hollow;
            set => hollow = value;
        }

        [SerializeField, Range(0, 1)] private float wallThickness;

        public float WallThickness
        {
            get
            {
                float thickness = wallThickness;
                if (!hollow)
                {
                    thickness = 1;
                }

                float[] scales = {Scale.x, Scale.y, Scale.z};
                float minScale = Mathf.Min(scales);
                float maxThickness = minScale / 2.0f;
                return thickness * maxThickness;
            }
            set => wallThickness = Mathf.Clamp(value, 0f, 1f);
        }
        #endregion OBJECT PROPERTIES

        #region RAYMARCH
        [SerializeField] private float marchingStepAmount = 1;
        public float MarchingStepAmount => marchingStepAmount;

        [SerializeField] private Operation operation = Operation.None;
        public Operation Operation => operation;

        [SerializeField, Range(0, 1)] private float blendStrength = 1;
        public float BlendStrength => blendStrength;
        #endregion RAYMARCH

        #region SINE WAVE
        [SerializeField] private bool enableSineWave = false;

        public bool EnableSineWave
        {
            get => enableSineWave;
            set => enableSineWave = value;
        }

        [SerializeField] private WaveDirectionVector sineWaveDirection;

        public Vector3 SineWaveDirection
        {
            get => sineWaveDirection.GetVector();

            set => sineWaveDirection.SetVector(value);
        }

        [SerializeField] private float sineWaveFrequency = 10f;

        public float SineWaveFrequency
        {
            get => sineWaveFrequency;
            set => sineWaveFrequency = value;
        }

        [SerializeField] private float sineWaveSpeed = 1f;

        public float SineWaveSpeed
        {
            get => sineWaveSpeed;
            set => sineWaveSpeed = value;
        }

        [SerializeField] private float sineWaveAmplitude = 0.25f;

        public float SineWaveAmplitude
        {
            get => sineWaveAmplitude;
            set => sineWaveAmplitude = value;
        }
        #endregion SINE WAVE

        // Num of Children
        public int NumOfChildren { get; set; }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SDFShape))]
    public class SDFShapeEditor : Editor
    {
        #region VARIABLES
        // Target
        private SDFShape _sdfShape;

        // Serialized Properties
        // Object Properties
        private SerializedProperty _shapeType;
        private SerializedProperty _modifier;
        private SerializedProperty _colour;
        private SerializedProperty _roundness;
        private SerializedProperty _wallThickness;

        // RayMarch
        private SerializedProperty _marchingStepAmount;
        private SerializedProperty _operation;
        private SerializedProperty _blendStrength;

        // Sine Wave
        private SerializedProperty _sineWaveFreq;
        private SerializedProperty _sineWaveSpeed;
        private SerializedProperty _sineWaveAmplitude;

        // Dropdowns
        private static bool _objectPropertiesDropdown = false;
        private static bool _rayMarchDropdown = false;
        private static bool _sineWaveDropdown = false;
        #endregion VARIABLES

        private void OnEnable()
        {
            // Target
            _sdfShape = (SDFShape) target;

            // Serialized Properties
            // Object Properties
            _shapeType = serializedObject.FindProperty("shapeType");
            _modifier = serializedObject.FindProperty("modifier");
            _colour = serializedObject.FindProperty("colour");
            _roundness = serializedObject.FindProperty("roundness");
            _wallThickness = serializedObject.FindProperty("wallThickness");

            // RayMarch
            _marchingStepAmount = serializedObject.FindProperty("marchingStepAmount");
            _operation = serializedObject.FindProperty("operation");
            _blendStrength = serializedObject.FindProperty("blendStrength");

            // Sine Wave
            _sineWaveFreq = serializedObject.FindProperty("sineWaveFrequency");
            _sineWaveSpeed = serializedObject.FindProperty("sineWaveSpeed");
            _sineWaveAmplitude = serializedObject.FindProperty("sineWaveAmplitude");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawObjectProperties();
            DrawRayMarchProperties();
            DrawSineWaveProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawObjectProperties()
        {
            _objectPropertiesDropdown =
                EditorGUILayout.BeginFoldoutHeaderGroup(_objectPropertiesDropdown, "Object Properties");

            if (_objectPropertiesDropdown)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(_shapeType);
                EditorGUILayout.PropertyField(_colour);

                // Modifier
                EditorGUI.BeginChangeCheck();
                Vector3 modifier = _sdfShape.Modifier;
                switch (_sdfShape.ShapeType)
                {
                    case ShapeType.BoundingBox:
                    {
                        modifier.x = EditorGUILayout.Slider("Thickness", _sdfShape.Modifier.x,
                            0.0f, 1.0f);
                        // modifier.x = EditorGUILayout.FloatField("Thickness", _sdfShape.Modifier.x);
                        break;
                    }
                    case ShapeType.Torus:
                    {
                        modifier.x = EditorGUILayout.FloatField("Inner Ring Size", _sdfShape.Modifier.x);
                        break;
                    }
                    case ShapeType.Cube:
                    case ShapeType.Sphere:
                    default:
                    {
                        break;
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    _modifier.vector3Value = modifier;
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_roundness);

                EditorGUILayout.Space();

                string buttonText = !_sdfShape.Hollow ? "Enable Hollow Object" : "Disable Hollow Object";

                if (GUILayout.Button(buttonText))
                {
                    _sdfShape.Hollow = !_sdfShape.Hollow;
                }

                if (_sdfShape.Hollow)
                {
                    EditorGUILayout.PropertyField(_wallThickness);
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRayMarchProperties()
        {
            _rayMarchDropdown =
                EditorGUILayout.BeginFoldoutHeaderGroup(_rayMarchDropdown, "Ray Marching Properties");

            if (_rayMarchDropdown)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(_marchingStepAmount);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_operation);

                if (_sdfShape.Operation == Operation.Blend)
                {
                    EditorGUILayout.PropertyField(_blendStrength);
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSineWaveProperties()
        {
            _sineWaveDropdown =
                EditorGUILayout.BeginFoldoutHeaderGroup(_sineWaveDropdown, "Sine Wave Properties");

            if (_sineWaveDropdown)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("box");

                string buttonText = !_sdfShape.EnableSineWave ? "Enable a Sine Wave" : "Disable a Sine Wave";

                if (GUILayout.Button(buttonText))
                {
                    _sdfShape.EnableSineWave = !_sdfShape.EnableSineWave;
                }

                if (_sdfShape.EnableSineWave)
                {
                    EditorGUI.indentLevel++;

                    Vector3 sineWaveDirection = _sdfShape.SineWaveDirection;

                    EditorGUI.BeginChangeCheck();

                    sineWaveDirection.x = (float) (WaveDirection) EditorGUILayout.EnumPopup("Sine Wave Direction X",
                        (WaveDirection) sineWaveDirection.x);
                    sineWaveDirection.y = (float) (WaveDirection) EditorGUILayout.EnumPopup("Sine Wave Direction Y",
                        (WaveDirection) sineWaveDirection.y);
                    sineWaveDirection.z = (float) (WaveDirection) EditorGUILayout.EnumPopup("Sine Wave Direction Z",
                        (WaveDirection) sineWaveDirection.z);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _sdfShape.SineWaveDirection = sineWaveDirection;
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(_sineWaveFreq);
                    EditorGUILayout.PropertyField(_sineWaveSpeed);
                    EditorGUILayout.PropertyField(_sineWaveAmplitude);


                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

#endif // UNITY_EDITOR
}