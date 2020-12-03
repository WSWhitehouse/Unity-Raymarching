using UnityEngine;
using WSWhitehouse.RayMarching.Enums;
using WSWhitehouse.Util;

namespace WSWhitehouse.RayMarching
{
    [ExecuteAlways, DisallowMultipleComponent]
    public class SDFShape : MonoBehaviour
    {
        [SerializeField] private ShapeType shapeType = ShapeType.Cube;
        public ShapeType ShapeType => shapeType;

        [SerializeField] private Operation operation = Operation.None;
        public Operation Operation => operation;

        [SerializeField] private Color colour = Color.white;
        public Color Colour => colour;

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
    }
}