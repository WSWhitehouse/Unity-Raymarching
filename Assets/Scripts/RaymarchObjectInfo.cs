using UnityEngine;

namespace WSWhitehouse
{
    [System.Serializable]
    public struct RaymarchObjectInfo
    {
        public int SdfShape;
        public float MarchingStepAmount;

        public Vector3 Position;
        public Vector4 Rotation;
        public Vector3 Scale;

        public Vector4 Colour;

        public int Operation;
        public int OperationSmooth;
        public float OperationMod;

        public float Roundness;
        public float WallThickness;

        public RaymarchObjectInfo(RaymarchObject _raymarchObject)
        {
            // Fill in Info Struct
            SdfShape = (int) _raymarchObject.SdfShape;
            MarchingStepAmount = _raymarchObject.MarchingStepAmount;
            Position = _raymarchObject.Position;
            Rotation = _raymarchObject.Rotation;
            Scale = _raymarchObject.Scale;
            Colour = _raymarchObject.Colour;
            Operation = (int) _raymarchObject.Operation;
            OperationSmooth = _raymarchObject.OperationSmooth ? 1 : 0;
            OperationMod = _raymarchObject.OperationMod;
            Roundness = _raymarchObject.Roundness;
            WallThickness = _raymarchObject.WallThickness;
        }

        public static int GetSize()
        {
            // https://stackoverflow.com/a/4956484 - size of struct in C#
            //return System.Runtime.InteropServices.Marshal.SizeOf(typeof(RaymarchObjectInfo));

            return (sizeof(float) * 18) + (sizeof(int) * 3);
            // return sizeof(RaymarchObjectInfo);
        }
    }
}