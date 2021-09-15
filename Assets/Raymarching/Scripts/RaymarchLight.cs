using System;
using UnityEngine;

namespace WSWhitehouse
{
    public enum LightType
    {
        Directional = 0,
        Point = 1,
    }

    [DisallowMultipleComponent, ExecuteAlways, RequireComponent(typeof(Light))]
    public class RaymarchLight : MonoBehaviour
    {
        private Light _light;

        private Light Light
        {
            get
            {
                if (_light == null)
                {
                    _light = GetComponent<Light>();
                }

                return _light;
            }
        }

        public LightType LightType =>
            Light.type == UnityEngine.LightType.Directional ? LightType.Directional : LightType.Point;

        public Vector3 Position => transform.position;
        public Vector3 Direction => transform.forward;

        public Color Colour => Light.color;
        public float Range => Light.range;
        public float Intensity => Light.intensity;

        private void OnEnable()
        {
            if (!Raymarch.Lights.Contains(this))
            {
                Raymarch.Lights.Add(this);
            }
        }

        private void OnDisable()
        {
            if (Raymarch.Lights.Contains(this))
            {
                Raymarch.Lights.Remove(this);
            }
        }
    }
}