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
        
        // Operation
        public float MarchingStepAmount;
        public int Operation;
        public float BlendStrength;
        public float Roundness;
        public int NumOfChildren;
        
        public static int GetSize()
        {
            return (sizeof(float) * 15) + (sizeof(int) * 3);
        }
    }
}