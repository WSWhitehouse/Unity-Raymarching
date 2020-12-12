using System;
using UnityEngine;
using WSWhitehouse.RayMarching.Enums;

namespace WSWhitehouse.RayMarching
{
    [Serializable]
    public class WaveDirectionVector
    {
        public WaveDirection x = WaveDirection.None;
        public WaveDirection y = WaveDirection.None;
        public WaveDirection z = WaveDirection.None;


        public Vector3 GetVector()
        {
            return new Vector3
            (
                (float)x,
                (float)y,
                (float)z
            );
        }

        public void SetVector(Vector3 vector3)
        {
            x = (WaveDirection)vector3.x;
            y = (WaveDirection)vector3.y;
            z = (WaveDirection)vector3.z;
        }
    }
}