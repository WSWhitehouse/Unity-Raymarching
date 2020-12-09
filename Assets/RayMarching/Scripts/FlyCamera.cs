using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace WSWhitehouse.RayMarching
{
    public class FlyCamera : MonoBehaviour
    {
        // File adapted from:
        // https://gist.github.com/McFunkypants/5a9dad582461cb8d9de3
        
        // Movement
        [SerializeField, Tooltip("Regular Movement Speed")]
        private float movementSpeed = 50.0f;

        [SerializeField, Tooltip("Multiplied by how long modifier key code is held")]
        private float modifySpeedAdd = 250.0f;

        [SerializeField, Tooltip("Maximum speed while holding modifier")]
        private float maxModifiedSpeed = 1000.0f;

        [SerializeField, Tooltip("How sensitive the mouse is at moving the camera")]
        private float mouseSensitivity = 0.25f;

        // Keycodes
        [SerializeField, Tooltip("KeyCode for moving forward")]
        private KeyCode moveForward = KeyCode.W;

        [SerializeField, Tooltip("KeyCode for moving backward")]
        private KeyCode moveBackward = KeyCode.S;

        [SerializeField, Tooltip("KeyCode for moving left")]
        private KeyCode moveLeft = KeyCode.A;

        [SerializeField, Tooltip("KeyCode for moving right")]
        private KeyCode moveRight = KeyCode.D;

        [SerializeField, Tooltip("KeyCode for moving up")]
        private KeyCode moveUp = KeyCode.E;

        [SerializeField, Tooltip("KeyCode for moving down")]
        private KeyCode moveDown = KeyCode.Q;

        [SerializeField, Tooltip("While this is held, camera is locked to the Y axis")]
        private KeyCode moveOnXZAxis = KeyCode.Space;

        [SerializeField, Tooltip("Increases movement speed while this key code is held")]
        private KeyCode fasterModifier = KeyCode.LeftShift;   
        
        [SerializeField, Tooltip("This button must be held to move")]
        private KeyCode buttonHeldToMove = KeyCode.Mouse1;

        private Vector3 _lastMouse = Vector3.zero;
        private float _totalRun = 1.0f;

        private void Awake()
        {
            _lastMouse = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
        }

        private void Update()
        {
            if (!Input.GetKey(buttonHeldToMove))
            {
                _lastMouse = Input.mousePosition;
                return;
            }

            _lastMouse = Input.mousePosition - _lastMouse;
            _lastMouse = new Vector3(-_lastMouse.y * mouseSensitivity, _lastMouse.x * mouseSensitivity, 0);
            _lastMouse = new Vector3(transform.eulerAngles.x + _lastMouse.x,
                transform.eulerAngles.y + _lastMouse.y, 0);
            transform.eulerAngles = _lastMouse;
            _lastMouse = Input.mousePosition;

            Vector3 input = GetBaseInput();

            if (Input.GetKey(fasterModifier))
            {
                _totalRun += Time.deltaTime;
                input *= _totalRun * modifySpeedAdd;
                input = ClampVector3(input, -maxModifiedSpeed, maxModifiedSpeed);
            }
            else
            {
                _totalRun = Mathf.Clamp(_totalRun * 0.5f, 1f, 1000f);
                input *= movementSpeed;
            }

            input *= Time.deltaTime;
            Vector3 newPosition = transform.position;
            if (Input.GetKey(moveOnXZAxis))
            {
                //If player wants to move on X and Z axis only
                transform.Translate(input);
                newPosition.x = transform.position.x;
                newPosition.z = transform.position.z;
                transform.position = newPosition;
            }
            else
            {
                transform.Translate(input);
            }
        }

        private Vector3 GetBaseInput()
        {
            Vector3 velocity = Vector3.zero;
            if (Input.GetKey(moveForward))
            {
                velocity += new Vector3(0, 0, 1);
            }

            if (Input.GetKey(moveBackward))
            {
                velocity += new Vector3(0, 0, -1);
            }

            if (Input.GetKey(moveLeft))
            {
                velocity += new Vector3(-1, 0, 0);
            }

            if (Input.GetKey(moveRight))
            {
                velocity += new Vector3(1, 0, 0);
            }

            if (Input.GetKey(moveDown))
            {
                velocity += new Vector3(0, -1, 0);
            }

            if (Input.GetKey(moveUp))
            {
                velocity += new Vector3(0, 1, 0);
            }

            return velocity;
        }

        private static Vector3 ClampVector3(Vector3 value, float min, float max)
        {
            Vector3 v;
            v.x = Mathf.Clamp(value.x, min, max);
            v.y = Mathf.Clamp(value.y, min, max);
            v.z = Mathf.Clamp(value.z, min, max);
            return v;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(FlyCamera))]
    public class FlyCameraEditor : Editor
    {
        // Target
        private FlyCamera _flyCamera;

        // Serialized Properties
        // Movement
        private SerializedProperty _movementSpeed;
        private SerializedProperty _modifySpeedAdd;
        private SerializedProperty _maxModifiedSpeed;
        private SerializedProperty _mouseSensitivity;

        // Keycodes
        private SerializedProperty _moveForward;
        private SerializedProperty _moveBackward;
        private SerializedProperty _moveLeft;
        private SerializedProperty _moveRight;
        private SerializedProperty _moveUp;
        private SerializedProperty _moveDown;
        private SerializedProperty _moveOnXZAxis;
        private SerializedProperty _fasterModifier;
        private SerializedProperty _buttonHeldToMove;

        // Private
        private bool _keycodeDropdown = false;

        private void OnEnable()
        {
            _flyCamera = (FlyCamera) target;

            // Movement
            _movementSpeed = serializedObject.FindProperty("movementSpeed");
            _modifySpeedAdd = serializedObject.FindProperty("modifySpeedAdd");
            _maxModifiedSpeed = serializedObject.FindProperty("maxModifiedSpeed");
            _mouseSensitivity = serializedObject.FindProperty("mouseSensitivity");

            // Keycodes
            _moveForward = serializedObject.FindProperty("moveForward");
            _moveBackward = serializedObject.FindProperty("moveBackward");
            _moveLeft = serializedObject.FindProperty("moveLeft");
            _moveRight = serializedObject.FindProperty("moveRight");
            _moveUp = serializedObject.FindProperty("moveUp");
            _moveDown = serializedObject.FindProperty("moveDown");
            _moveOnXZAxis = serializedObject.FindProperty("moveOnXZAxis");
            _fasterModifier = serializedObject.FindProperty("fasterModifier");
            _buttonHeldToMove = serializedObject.FindProperty("buttonHeldToMove");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Movement Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_movementSpeed);
            EditorGUILayout.PropertyField(_modifySpeedAdd);
            EditorGUILayout.PropertyField(_maxModifiedSpeed);
            EditorGUILayout.PropertyField(_mouseSensitivity);

            EditorGUILayout.Space();

            _keycodeDropdown = EditorGUILayout.BeginFoldoutHeaderGroup(_keycodeDropdown, "Movement KeyCodes");
            if (_keycodeDropdown)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_moveForward);
                EditorGUILayout.PropertyField(_moveBackward);
                EditorGUILayout.PropertyField(_moveLeft);
                EditorGUILayout.PropertyField(_moveRight);
                EditorGUILayout.PropertyField(_moveUp);
                EditorGUILayout.PropertyField(_moveDown);
                EditorGUILayout.PropertyField(_moveOnXZAxis);
                EditorGUILayout.PropertyField(_fasterModifier);
                EditorGUILayout.PropertyField(_buttonHeldToMove);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}