using UnityEngine;
using WSWhitehouse.RayMarching.Enums;
using WSWhitehouse.RayMarching.Util;

namespace WSWhitehouse.RayMarching
{
    [ExecuteAlways, DisallowMultipleComponent]
    public class Shape : MonoBehaviour
    {
        [SerializeField] private ShapeType shapeType = ShapeType.Cube;
        public ShapeType ShapeType => shapeType;

        [SerializeField] private Operation operation = Operation.None;
        public Operation Operation => operation;

        [SerializeField] private Color colour = Color.white;
        public Color Colour => colour;

        public Vector3 Position => transform.position;

        public Vector3 Rotation
        {
            get
            {
                Vector3 eulerAngles = transform.rotation.eulerAngles;
                float x = Util.Rotation.DegreesToRightAngles(eulerAngles.x);
                float y = eulerAngles.y;
                float z = eulerAngles.z;
                return Util.Rotation.EulerAnglesToRadians(new Vector3(x, y, z));
            }
        }


        public Vector3 Scale
        {
            get
            {
                if (transform.parent == null)
                {
                    return transform.localScale / 2f;
                }

                Vector3 parentScale = Vector3.one;
                Shape parentShape = transform.parent.GetComponent<Shape>();
                if (parentShape != null)
                {
                    parentScale = parentShape.Scale;
                }

                return Vector3.Scale(transform.localScale, parentScale);
            }
        }
    }
}