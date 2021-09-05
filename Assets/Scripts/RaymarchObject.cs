using UnityEngine;

namespace WSWhitehouse
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class RaymarchObject : MonoBehaviour
    {
        public Vector3 Position => transform.position;

        public Vector4 Rotation =>
            new Vector4(transform.rotation.x,
                transform.rotation.y,
                transform.rotation.z,
                transform.rotation.w);

        public Vector3 Scale => transform.lossyScale;

        private void OnEnable()
        {
            // Add to RaymarchCamera object list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();

            foreach (var cam in raymarchCams)
            {
                cam.RaymarchObjects.Add(this);
            }
        }

        private void OnDisable()
        {
            // Remove from RaymarchCamera object list
            RaymarchCamera[] raymarchCams = FindObjectsOfType<RaymarchCamera>();
            
            foreach (var cam in raymarchCams)
            {
                cam.RaymarchObjects.Remove(this);
            }
        }
    }
}