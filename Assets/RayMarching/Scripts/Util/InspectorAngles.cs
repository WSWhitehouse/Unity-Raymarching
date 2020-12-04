using System.Collections.Generic;
using UnityEngine;

namespace WSWhitehouse.Util
{
    public class InspectorAngles
    {
        // ===========================================
        // EULER CALC
        // ===========================================
        private float _eulerX;
        private float _eulerY;
        private float _eulerZ;

        // Hold Euler list
        private List<float[]> _eulerList;

        // Hold last angle to calc angles over 360
        private float _lastEulerX;
        private float _lastEulerY;
        private float _lastEulerZ;

        // Hold 360 passes
        private float _eulerX360Pass;
        private float _eulerY360Pass;
        private float _eulerZ360Pass;

        public Vector3 Rotation => new Vector3(_eulerX, _eulerY, _eulerZ);

        public void Update(Transform transform)
        {
            // Quaternion localRotation = transform.localRotation;
            // float[,] matrix = QuaternionToRotationMatrix(localRotation);
            // Vector3 eulerAngles = RotationMatrixToEulerAngle(matrix, 2, 1, 0);

            Vector3 eulerAngles = Math.QuaternionToEulerAngles(transform.localRotation);
            
            float eulerXOrg = Mathf.Rad2Deg * eulerAngles.x;
            float eulerYOrg = Mathf.Rad2Deg * eulerAngles.y;
            float eulerZOrg = Mathf.Rad2Deg * eulerAngles.z;

            // Add 360 passes
            _eulerX = eulerXOrg + (360 * _eulerX360Pass);
            _eulerY = eulerYOrg + (360 * _eulerY360Pass);
            _eulerZ = eulerZOrg + (360 * _eulerZ360Pass);

            // Check if x passed 360
            float xChange = _lastEulerX - _eulerX;
            if (Mathf.Abs(xChange) > 180)
            {
                // Passed 360
                // Check direction we passed
                if (xChange > 0)
                {
                    // Add 360
                    _eulerX360Pass += 1;
                }
                else
                {
                    // Sub 360
                    _eulerX360Pass -= 1;
                }

                // Recalculate euler
                _eulerX = eulerXOrg + (360 * _eulerX360Pass);
            }

            // Check if y passed 360
            float yChange = _lastEulerY - _eulerY;
            if (Mathf.Abs(yChange) > 180)
            {
                // Passed 360
                // Check direction we passed
                if (yChange > 0)
                {
                    // Add 360
                    _eulerY360Pass += 1;
                }
                else
                {
                    // Sub 360
                    _eulerY360Pass -= 1;
                }

                // Recalculate euler
                _eulerY = eulerYOrg + (360 * _eulerY360Pass);
            }

            // Check if z passed 360
            float zChange = _lastEulerZ - _eulerZ;
            if (Mathf.Abs(zChange) > 180)
            {
                // Passed 360
                // Check direction we passed
                if (zChange > 0)
                {
                    // Add 360
                    _eulerZ360Pass += 1;
                }
                else
                {
                    // Sub 360
                    _eulerZ360Pass -= 1;
                }

                // Recalculate euler
                _eulerZ = eulerZOrg + (360 * _eulerZ360Pass);
            }

            // Save last
            _lastEulerX = _eulerX;
            _lastEulerY = _eulerY;
            _lastEulerZ = _eulerZ;
        }

        private static Vector4 SandwichProduct(Vector4 q1, Vector4 q2)
        {
            Vector4 q;
            q.w = -q1.x * q2.x - q1.y * q2.y - q1.z * q2.z + q1.w * q2.w;
            q.x = q1.x * q2.w + q1.y * q2.z - q1.z * q2.y + q1.w * q2.x;
            q.y = -q1.x * q2.z + q1.y * q2.w + q1.z * q2.x + q1.w * q2.y;
            q.z = q1.x * q2.y - q1.y * q2.x + q1.z * q2.w + q1.w * q2.z;

            return q;
        }

        private static Quaternion CreateVectorSandwich(Vector4 a, Vector4 b, Vector4 c)
        {
            Vector4 v = SandwichProduct(SandwichProduct(a, b), c);
            Quaternion q = new Quaternion(v.x, v.y, v.z, v.w);
            return q;
        }

        private static float[,] QuaternionToRotationMatrix(Quaternion q)
        {
            float x = q.z;
            float y = q.x;
            float z = q.y;
            float w = q.w;
            float[,] matrix = new float[3, 3];

            float m11 = 1.0f - 2.0f * (y * y + z * z);
            float m12 = 2.0f * (x * y - z * w);
            float m13 = 2.0f * (x * z + y * w);

            float m21 = 2.0f * (x * y + z * w);
            float m22 = 1.0f - 2.0f * (x * x + z * z);
            float m23 = 2.0f * (y * z - x * w);

            float m31 = 2.0f * (x * z - y * w);
            float m32 = 2.0f * (y * z + x * w);
            float m33 = 1.0f - 2.0f * (x * x + y * y);

            matrix[0, 0] = m11;
            matrix[0, 1] = m12;
            matrix[0, 2] = m13;
            matrix[1, 0] = m21;
            matrix[1, 1] = m22;
            matrix[1, 2] = m23;
            matrix[2, 0] = m31;
            matrix[2, 1] = m32;
            matrix[2, 2] = m33;

            return matrix;
        }

        private static Vector3 RotationMatrixToEulerAngle(float[,] matrix, int idx1, int idx2, int idx3)
        {
            float ftany = matrix[2, 1];
            float ftanx = matrix[2, 2];
            float first = Mathf.Atan2(ftany, ftanx);
            float stany = matrix[1, 0];
            float stanx = matrix[0, 0];
            float second = Mathf.Atan2(stany, stanx);
            float tsiny = matrix[2, 0];
            float third = Mathf.Asin(-1.0f * tsiny);
            return new Vector3(first,second,third);
        }
    }
}