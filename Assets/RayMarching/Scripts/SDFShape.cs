using UnityEngine;
using WSWhitehouse.RayMarching.Enums;
using Math = WSWhitehouse.Util.Math;

namespace WSWhitehouse.RayMarching
{
    [ExecuteAlways, DisallowMultipleComponent]
    public class SDFShape : MonoBehaviour
    {
        // Translation
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

        // Object Properties
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

        // Operation
        [SerializeField] private float marchingStepAmount = 1;
        public float MarchingStepAmount => marchingStepAmount;

        [SerializeField] private Operation operation = Operation.None;
        public Operation Operation => operation;

        [SerializeField, Range(0, 1)] private float blendStrength = 1;
        public float BlendStrength => blendStrength;

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


        public int NumOfChildren { get; set; }


    }
}