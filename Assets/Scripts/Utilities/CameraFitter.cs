using System.Collections;
using UnityEngine;
using Core;
using Themes;

namespace Utilities
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraFitter : MonoBehaviour, IThemeReceiver
    {
        [SerializeField] private Settings settings;
        private Camera _cam;
        private Vector2Int _lastResolution;

        private void Awake() => _cam = GetComponent<Camera>();

        private void Start() => Fit();

        private void Update()
        {
            var currentResolution = new Vector2Int(Screen.width, Screen.height);
            if (currentResolution == _lastResolution) return;
            _lastResolution = currentResolution;
            Fit();
        }

        private void Fit()
        {
            float aspectRatio = (float)Screen.width / Screen.height;
            float verticalSize = settings.screenHeight / 2;
            float horizontalSize = settings.screenWidth / aspectRatio / 2;
            _cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
            _cam.transform.position = settings.camCenter;
        }

        #region Themes
        
        public void ReceiveTheme(Theme theme) =>
            StartCoroutine(CamColorLerp(_cam.backgroundColor, theme.backgroundColor, 
                settings.themeChangeDuration));

        public void ReceiveThemeOnGameStart(Theme theme) =>
            _cam.backgroundColor = theme.backgroundColor;

        private IEnumerator CamColorLerp(Color start, Color end, float duration)
        {
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _cam.backgroundColor = Color.Lerp(start, end, elapsedTime / duration);
                yield return null;
            }
            _cam.backgroundColor = end;
        }

        #endregion
    }
}