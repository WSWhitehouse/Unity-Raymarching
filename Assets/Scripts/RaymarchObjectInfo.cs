using UnityEngine;

namespace WSWhitehouse
{
    [System.Serializable]
    public struct RaymarchObjectInfo
    {
        public Vector3 Position;
        public Vector4 Rotation;
        public Vector3 Scale;

        public Vector4 Colour;

        public int Operation;
        public float OperationMod;

        public RaymarchObjectInfo(RaymarchObject _raymarchObject)
        {
            // Fill in Info Struct
            Position = _raymarchObject.Position;
            Rotation = _raymarchObject.Rotation;
            Scale = _raymarchObject.Scale;
            Colour = _raymarchObject.Colour;
            Operation = (int)_raymarchObject.Operation;
            OperationMod = _raymarchObject.OperationMod;
        }

        public static int GetSize()
        {
            // https://stackoverflow.com/a/4956484 - size of struct in C#
            //return System.Runtime.InteropServices.Marshal.SizeOf(typeof(RaymarchObjectInfo));

            return (sizeof(float) * 15) + (sizeof(int) * 1);
            // return sizeof(RaymarchObjectInfo);
        }
    }
}