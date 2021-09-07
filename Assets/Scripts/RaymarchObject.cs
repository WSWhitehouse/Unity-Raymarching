﻿using UnityEngine;

namespace WSWhitehouse
{
    [DisallowMultipleComponent, ExecuteAlways]
    public class RaymarchObject : MonoBehaviour
    {
        public Vector3 Position => transform.position;

        public Vector4 Rotation =>
            new Vector4(transform.rotation.x,
                transform.rotation.y,
                transform.rotation.z,
                transform.rotation.w);

        public Vector3 Scale => transform.lossyScale;

        [SerializeField] private Color colour = Color.white;
        public Color Colour => colour;

        [SerializeField] private _Operation operation = _Operation.NONE;
        public _Operation Operation => operation;

        public enum _Operation : int
        {
            NONE = 0,
            BLEND = 1,
            CUT = 2,
            MASK = 3
        }

        [SerializeField] private float operationMod = 1.0f;
        public float OperationMod => operationMod;

        private void OnEnable()
        {
            // Add to RaymarchCamera object list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();

            foreach (var cam in raymarchCams)
            {
                if (cam.RaymarchObjects.Contains(this)) continue;
                cam.RaymarchObjects.Add(this);
            }
        }

        private void OnDisable()
        {
            // Remove from RaymarchCamera object list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();

            foreach (var cam in raymarchCams)
            {
                if (!cam.RaymarchObjects.Contains(this)) continue;
                cam.RaymarchObjects.Remove(this);
            }
        }
    }
}