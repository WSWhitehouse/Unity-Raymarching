using UnityEngine;

namespace WSWhitehouse.Util
{
    public static class Math
    {
        #region MATH_FUNCTIONS

        /// <summary>
        /// Wraps value between 0 & a max value
        /// </summary>
        /// <param name="value">Value to wrap</param>
        /// <param name="max">Max value to wrap too</param>
        /// <returns>Wrapped value</returns>
        public static float WrapMax(float value, float max)
        {
            return (max + value % max) % max;
        }

        /// <summary>
        /// Wraps value between a min & max value
        /// </summary>
        /// <param name="value">Value to wrap</param>
        /// <param name="min">Min value to wrap too</param>
        /// <param name="max">Max value to wrap too</param>
        /// <returns>Wrapped value</returns>
        public static float WrapMinMax(float value, float min, float max)
        {
            return min + WrapMax(value - min, max - min);
        }

        /// <summary>
        /// Returns a value with the magnitude of x and the sign of y.
        /// </summary>
        /// <param name="x">A number whose magnitude is used in the result.</param>
        /// <param name="y">A number whose sign is the used in the result.</param>
        /// <returns>A value with the magnitude of x and the sign of y.</returns>
        public static float CopySign(float x, float y)
        {
            y = Mathf.Sign(y);
            return x * y;
        }

        public static float OppositeSign(float val)
        {
            float sign = Mathf.Sign(val);

            if (sign > 0)
            {
                return Mathf.Abs(val);
            }

            return -Mathf.Abs(val);
        }

        #endregion // MATH_FUNCTIONS

        #region ROTATIONS

        /// <summary>
        /// Converts euler angles to radians
        /// </summary>
        /// <param name="eulerAngles">euler angles to convert</param>
        /// <returns>Radians in Vector3</returns>
        public static Vector3 EulerAnglesToRadians(Vector3 eulerAngles)
        {
            return new Vector3(
                DegreesToRadian(eulerAngles.x),
                DegreesToRadian(eulerAngles.y),
                DegreesToRadian(eulerAngles.z));
        }

        /// <summary>
        /// Converts degrees to a radian
        /// </summary>
        /// <param name="degrees">Degree to convert</param>
        /// <returns>Radian</returns>
        public static float DegreesToRadian(float degrees)
        {
            return degrees * Mathf.PI / 180;
        }

        /// <summary>
        /// Converts Quaternions to euler angles
        /// </summary>
        /// <param name="q">Quaternion to convert</param>
        /// <returns>EulerAngles</returns>
        /// http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
        /// https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        public static Vector3 QuaternionToEulerAngles(Quaternion q)
        {
            q = q.normalized;
            float x; // Bank
            float y; // Heading
            float z; // Attitude

            float test = q.x * q.y + q.z * q.w;
            if (test > 0.499)
            {
                // singularity at north pole
                y = 2 * Mathf.Atan2(q.x, q.w);
                z = Mathf.PI / 2;
                x = 0;
                return new Vector3(x, y, z);
            }

            if (test < -0.499)
            {
                // singularity at south pole
                y = -2 * Mathf.Atan2(q.x, q.w);
                z = -Mathf.PI / 2;
                x = 0;
                return new Vector3(x, y, z);
            }

            float sqx = q.x * q.x;
            float sqy = q.y * q.y;
            float sqz = q.z * q.z;
            y = Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * sqy - 2 * sqz);
            z = Mathf.Asin(2 * test);
            x = Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * sqx - 2 * sqz);
            return new Vector3(x, y, z);
        }

        public static Vector3 QuaternionToOppositeEulerAngles(Quaternion q)
        {
            Vector3 eulerAngles = QuaternionToEulerAngles(q);
            eulerAngles.x = OppositeSign(eulerAngles.x);
            eulerAngles.y = OppositeSign(eulerAngles.y);
            eulerAngles.z = OppositeSign(eulerAngles.z);
            return eulerAngles;
        }

        #endregion // ROTATIONS
    }
}