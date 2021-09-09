using UnityEngine;

namespace WSWhitehouse
{
    [DisallowMultipleComponent, ExecuteAlways]
    public class RaymarchObject : MonoBehaviour
    {
        [SerializeField] private SdfShape sdfShape;
        public SdfShape SdfShape => sdfShape;
        
        [SerializeField] private float marchingStepAmount = 1;
        public float MarchingStepAmount => marchingStepAmount;

        public Vector3 Position => transform.position;

        public Vector4 Rotation =>
            new Vector4(transform.eulerAngles.x * Mathf.Deg2Rad,
                transform.eulerAngles.y * Mathf.Deg2Rad,
                transform.eulerAngles.z * Mathf.Deg2Rad,
                0);

        public Vector3 Scale => transform.lossyScale;

        [SerializeField] private Color colour = Color.white;
        public Color Colour => colour;

        [SerializeField] private _Operation operation = _Operation.None;
        public _Operation Operation => operation;

        public enum _Operation : int
        {
            None = 0,
            Blend = 1,
            Cut = 2,
            Mask = 3
        }

        [SerializeField] private bool operationSmooth = true;
        public bool OperationSmooth => operationSmooth;

        [SerializeField] private float operationMod = 1.0f;
        public float OperationMod => operationMod;

        [SerializeField] private int operationLayer = 0;
        public int OperationLayer => operationLayer;
        
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
                float maxThickness = minScale * 0.5f;
                return thickness * maxThickness;
            }
            set => wallThickness = Mathf.Clamp(value, 0f, 1f);
        }
        
        private void OnEnable()
        {
            // Add to RaymarchCamera object list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();

            foreach (var cam in raymarchCams)
            {
                if (cam.RaymarchObjects.Contains(this)) continue;
                cam.RaymarchObjects.Add(this);
            }
        }

        private void OnDisable()
        {
            // Remove from RaymarchCamera object list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();

            foreach (var cam in raymarchCams)
            {
                if (!cam.RaymarchObjects.Contains(this)) continue;
                cam.RaymarchObjects.Remove(this);
            }
        }
    }
}