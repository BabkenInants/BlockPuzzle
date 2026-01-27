using UnityEngine;
using Core;

namespace Utilities
{
    [ExecuteAlways]
    public class SafeZonePreview : MonoBehaviour
    {
        [SerializeField] private Settings settings;
        void Update()
        {
#if UNITY_EDITOR
            if(!settings) return;
            Vector3 pos = settings.camCenter;
            pos.z = 0;
            transform.position = pos;
#endif
        }
    }
}
