using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace WSWhitehouse.RayMarching
{
    public struct ShapeData
    {
        // Translation
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        // Object Properties
        public Vector3 Colour;
        public int ShapeType;
        public Vector3 Modifier;

        // RayMarch
        public float MarchingStepAmount;
        public int Operation;
        public float BlendStrength;
        public float Roundness;
        public float WallThickness;
        
        // Sine Wave
        public int EnableSineWave;
        public Vector3 SineWaveDirection;
        public float SineWaveFreq;
        public float SineWaveSpeed;
        public float SineWaveAmp;
        
        // Num of Children
        public int NumOfChildren;

        public static int GetSize()
        {
            unsafe
            {
                return GetSizeUnsafe();
            }

            //return GetSizeSafe();
        }

        // private static int GetSizeSafe()
        // {
        //     return (sizeof(float) * 26) + (sizeof(int) * 3);
        // }
        
        private static int GetSizeUnsafe()
        {
            return UnsafeUtility.SizeOf<ShapeData>();
        }
    }
}