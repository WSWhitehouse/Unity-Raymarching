using System;
using UnityEngine;

namespace WSWhitehouse
{
    [CreateAssetMenu(fileName = "RaymarchingSettings", menuName = "Raymarhcing/Settings", order = 0)]
    public class RaymarchSettings : ScriptableObject
    {
        // Compute Shader
        public ComputeShader shader;
        [SerializeField] private string kernelName = "CSMain";

        [Space]

        // Raymarching
        public float renderDistance = 100.0f;
        public float hitResolution = 0.001f;
        public float relaxation = 1.0f;
        public int maxIterations = 164;

        [Space]

        // Lighting & Shadows
        public Color ambientColour = new(0.2117f, 0.2274f, 0.2588f, 1);

        public ShadowType shadowType = ShadowType.SoftShadows;
        public float shadowIntensity = 1.0f;
        public int shadowSteps = 10;
        public Vector2 shadowDistance = new(0.05f, 50.0f);
        public float shadowPenumbra;

        public enum ShadowType
        {
            NoShadows,
            HardShadows,
            SoftShadows
        }

        private int? _kernelIndex = null;
        public int KernelIndex
        {
            get
            {
                _kernelIndex ??= shader.FindKernel(kernelName);
                return _kernelIndex.Value;
            }
        }
    }
}