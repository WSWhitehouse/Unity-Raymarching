using System;
using UnityEngine;

namespace WSWhitehouse
{
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
            // Add to RaymarchCamera light list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();

            foreach (var cam in raymarchCams)
            {
                if (cam.RaymarchLights.Contains(this)) continue;
                cam.RaymarchLights.Add(this);
            }
        }

        private void OnDisable()
        {
            // Remove from RaymarchCamera light list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();

            foreach (var cam in raymarchCams)
            {
                if (!cam.RaymarchLights.Contains(this)) continue;
                cam.RaymarchLights.Remove(this);
            }
        }
    }
}