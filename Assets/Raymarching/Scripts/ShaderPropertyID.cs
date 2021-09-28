using UnityEngine;

namespace WSWhitehouse
{
    public static class ShaderPropertyID
    {
        public static readonly int Destination = Shader.PropertyToID("_Destination");

        public static readonly int CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");

        public static readonly int CamFrustumMain = Shader.PropertyToID("_CamFrustumMain");
        public static readonly int CamInverseProjectionMain = Shader.PropertyToID("_CamInverseProjectionMain");
        public static readonly int CamToWorldMain = Shader.PropertyToID("_CamToWorldMain");
        
        public static readonly int CamNearClipPlane = Shader.PropertyToID("_CamNearClipPlane");

        public static readonly int RenderDistance = Shader.PropertyToID("_RenderDistance");
        public static readonly int HitResolution = Shader.PropertyToID("_HitResolution");
        public static readonly int Relaxation = Shader.PropertyToID("_Relaxation");
        public static readonly int MaxIterations = Shader.PropertyToID("_MaxIterations");

        public static readonly int AmbientColour = Shader.PropertyToID("_AmbientColour");
        public static readonly string NoShadows = "NO_SHADOWS";
        public static readonly string HardShadows = "HARD_SHADOWS";
        public static readonly string SoftShadows = "SOFT_SHADOWS";
        public static readonly int ShadowIntensity = Shader.PropertyToID("_ShadowIntensity");
        public static readonly int ShadowSteps = Shader.PropertyToID("_ShadowSteps");
        public static readonly int ShadowDistance = Shader.PropertyToID("_ShadowDistance");
        public static readonly int ShadowPenumbra = Shader.PropertyToID("_ShadowPenumbra");

        public static readonly int ObjectInfo = Shader.PropertyToID("_ObjectInfo");
        public static readonly int ObjectInfoCount = Shader.PropertyToID("_ObjectInfoCount");

        public static readonly int LightInfo = Shader.PropertyToID("_LightInfo");
        public static readonly int LightInfoCount = Shader.PropertyToID("_LightInfoCount");
    }
}