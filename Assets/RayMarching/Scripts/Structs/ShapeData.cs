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
        public int Operation;

        public static int GetSize()
        {
            return (sizeof(float) * 12) + (sizeof(int) * 2);
        }
    }
}