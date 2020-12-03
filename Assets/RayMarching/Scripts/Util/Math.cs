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
        public static Vector3 QuaternionToEulerAngles(Quaternion q)
        {
            Vector3 angles;

            // roll (x-axis rotation)
            float xSinCosp = 2 * (q.w * q.x + q.y * q.z);
            float xCosCosp = 1 - 2 * (q.x * q.x + q.y * q.y);

            // pitch (y-axis rotation)
            float ySin = 2 * (q.w * q.y - q.z * q.x);

            // yaw (z-axis rotation)
            float zSinCosp = 2 * (q.w * q.z + q.x * q.y);
            float zCosCosp = 1 - 2 * (q.y * q.y + q.z * q.z);

            angles.x = Mathf.Atan2(xSinCosp, xCosCosp);
            angles.y = Mathf.Abs(ySin) >= 1 ? CopySign(Mathf.PI / 2, ySin) : Mathf.Asin(ySin);
            angles.z = Mathf.Atan2(zSinCosp, zCosCosp);
            return angles;
        }

        #endregion // ROTATIONS
    }
}