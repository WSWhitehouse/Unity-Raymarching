using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace WSWhitehouse.RayMarching.Structs
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

        // Operation
        public float MarchingStepAmount;
        public int Operation;
        public float BlendStrength;
        public float Roundness;
        public int NumOfChildren;

        public static int GetSize()
        {
            unsafe
            {
                return GetSizeUnsafe();
            }

            return GetSizeSafe();
        }

        private static int GetSizeSafe()
        {
            return (sizeof(float) * 18) + (sizeof(int) * 3);
        }
        
        private static int GetSizeUnsafe()
        {
            return UnsafeUtility.SizeOf<ShapeData>();
        }
    }
}