using UnityEngine;

namespace WSWhitehouse.RayMarching.Util
{
    public static class Rotation
    {
        public static Vector3 EulerAnglesToRadians(Vector3 eulerAngles)
        {
            return new Vector3(
                DegreesToRadian(eulerAngles.x),
                DegreesToRadian(eulerAngles.y),
                DegreesToRadian(eulerAngles.z));
        }

        public static float DegreesToRadian(float degrees)
        {
            return degrees * Mathf.PI / 180;
        }

        private const float RIGHT_ANGLE = 90.0f;

        public static Vector3 EulerAnglesToRightAngles(Vector3 eulerAngles)
        {
            return new Vector3(
                DegreesToRightAngles(eulerAngles.x),
                DegreesToRightAngles(eulerAngles.y),
                DegreesToRightAngles(eulerAngles.z)
            );
        }

        public static float DegreesToRightAngles(float degrees)
        {
            // if (degrees > 180)
            // {
            //     degrees -= 90;
            // }


            // degrees -= 180;
            // if (degrees < 180 && degrees > 270)
            // {
            //     degrees = Mathf.Abs(degrees);
            // }

            float minMax = WrapMinMax(degrees, 0, 90);


            // if (minMax < RIGHT_ANGLE /*&& minMax > -RIGHT_ANGLE*/)
            // {
            //     minMax = -(minMax);
            // }
            Debug.Log("Degrees: " + degrees + "   MinMax: " + minMax);
            return minMax;

            if (degrees > RIGHT_ANGLE)
            {
                Debug.Log("Greater than");
                return -(degrees - (degrees - RIGHT_ANGLE));
            }

            if (degrees < -RIGHT_ANGLE)
            {
                Debug.Log("Less than");
                float absDegrees = Mathf.Abs(degrees);
                return -Mathf.Abs(absDegrees - (absDegrees - RIGHT_ANGLE));
            }

            return -degrees;
        }

        private static float WrapMax(float value, float max)
        {
            /* integer math: `(max + x % max) % max` */
            return (max + value % max) % max;
        }

        private static float WrapMinMax(float value, float min, float max)
        {
            return min + WrapMax(value - min, max - min);
        }
    }
}