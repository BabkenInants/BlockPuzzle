using UnityEngine;
using UnityEngine.UI; 

namespace Utilities
{
    [ExecuteAlways]
    public class ResponsiveCanvas : MonoBehaviour
    {
        private CanvasScaler _canvasScaler;

        private void Start() =>
            _canvasScaler = GetComponent<CanvasScaler>();

        private void Update()=>
            _canvasScaler.matchWidthOrHeight = Screen.width > Screen.height ? 1f : 0f;
    }
}