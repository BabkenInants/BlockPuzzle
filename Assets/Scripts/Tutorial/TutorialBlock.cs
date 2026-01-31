using Core;
using UnityEngine;
using System.Collections;
using Themes;
using UnityEngine.SceneManagement;

namespace Tutorial
{
    public class TutorialBlock : MonoBehaviour, IThemeReceiver
    {
        public Transform[] cells;
        public Color color { get; private set; }
        private float _notPickedSize = .7f;
        [HideInInspector] public int sizeX = 3;
        [HideInInspector] public int sizeY = 3;
        ///true - busy, false - free
        [HideInInspector] public bool[] blockShape;
        private bool _isPicked;
        private Vector3 _startPos;
        private Settings _settings;
        private IEnumerator _sizeChangeCoroutine;

        public void ReceiveThemeOnGameStart(Theme theme)
        {
            color = theme.blockColors[Random.Range(0, theme.blockColors.Length)];
            foreach (Transform cell in cells)
                cell.GetComponent<SpriteRenderer>().color = color;
        }

        public void ReceiveTheme(Theme theme)
        {
            Color newColor = theme.blockColors[Random.Range(0, theme.blockColors.Length)];
            foreach (Transform cell in cells)
            {
                var sRenderer = cell.GetComponent<SpriteRenderer>();
                StartCoroutine(ThemeTools.SetSpriteRendererColor(sRenderer, color, newColor,
                    _settings.themeChangeDuration));
            }

            color = newColor;
        }

        public void InitSettings(Settings settings) => _settings = settings;

        public void InitNotPickedSize(float size)
        {
            _notPickedSize = size;
            SetSize(size);
        }
//
//         #region Drag and drop
//
//         public void OnPointerDown(Vector2 pos)
//         {
//             if (_settings == null) return;
//             _isPicked = true;
//             _startPos = transform.position;
//             Vector3 mp = pos;
//             mp.z = -_mainCam.transform.position.z;
//             Vector3 world = _mainCam.ScreenToWorldPoint(mp);
//             _mouseOffset = transform.position - world;
//             SetSize(_settings.cellSize * 2);
//             _collider.size = _colliderDefaultSize;
//             foreach (Transform cell in cells)
//                 cell.GetComponent<SpriteRenderer>().sortingOrder = _settings.blockCellsPickedSpriteLayer;
//         }
//
//         private void Update()
//         {
//             //New input system is not updating OnPointerMove every frame
//             //like OnMouseDrag, so I have to use Update instead
//             if (!_isPicked || !_mainCam || !_settings) return;
//             //moving the block to the mouse position + offset
//             float minY = _settings.minBlockDistanceFromCursorY;
//             float maxY = _settings.maxBlockDistanceFromCursorY;
//             float minX = _settings.minBlockDistanceFromCursorX;
//             float maxX = _settings.maxBlockDistanceFromCursorX;
// #if UNITY_EDITOR
//             if (Mouse.current == null) return;
//             Vector3 mousePos = Mouse.current.position.ReadValue();
// #else
//         if(Touchscreen.current == null) return;
//         Vector3 mousePos = Touchscreen.current.primaryTouch.position.ReadValue();
// #endif
//             float yOffset = Mathf.Clamp(mousePos.y / Screen.height * maxY, minY, maxY);
//             float xOffset = Mathf.Clamp((mousePos.x / Screen.width - .5f) * maxX, minX, maxX);
//             Vector3 offset = _mouseOffset + new Vector3(xOffset, yOffset);
//             mousePos.z = -_mainCam.transform.position.z;
//             Vector3 world = _mainCam.ScreenToWorldPoint(mousePos);
//             Vector3 position = world + offset;
//             position.z = 0;
//             transform.position = position;
//             GameEvents.RaiseOnBlockMoved();
//         }
//
//         public void OnPointerUp(PointerEventData eventData)
//         {
//             if (!_isPicked) return;
//             GameEvents.RaiseOnBlockUnpicked(this);
//             _isPicked = false;
//             foreach (Transform cell in cells)
//                 cell.GetComponent<SpriteRenderer>().sortingOrder = _settings.blockCellsDefaultSpriteLayer;
//         }
//
//         public void PutBlockBack(bool disableCanPick = false)
//         {
//             _canPick = false;
//             _isPicked = false;
//             _collider.size = _notPickedColliderSize;
//             SetSize(_notPickedSize);
//             StartCoroutine(PositionTranslateRoutine(transform.position, _startPos, .1f, disableCanPick));
//         }
//
//         #endregion

        #region Visuals

        public void SetColor(Color newColor)
        {
            color = newColor;
            foreach (var cell in cells)
                cell.GetComponent<SpriteRenderer>().color = newColor;
        }

        private void SetSize(float size)
        {
            if (_sizeChangeCoroutine != null) StopCoroutine(_sizeChangeCoroutine);
            _sizeChangeCoroutine = SizeChangeRoutine(transform.localScale, new Vector3(size, size, 1), .05f);
            StartCoroutine(_sizeChangeCoroutine);
        }

        #endregion

        #region Coroutines

        private IEnumerator PositionTranslateRoutine(Vector3 startPos, Vector3 endPos, float duration,
            bool disableCanPick)
        {
            var estimatedTime = 0f;
            while (estimatedTime < duration)
            {
                estimatedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, endPos, estimatedTime / duration);
                yield return null;
            }

            transform.position = endPos;
            //if (!disableCanPick) _canPick = true;
        }

        private IEnumerator SizeChangeRoutine(Vector3 startSize, Vector3 endSize, float duration)
        {
            var estimatedTime = 0f;
            while (estimatedTime < duration)
            {
                estimatedTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startSize, endSize, estimatedTime / duration);
                yield return null;
            }

            transform.localScale = endSize;
            _sizeChangeCoroutine = null;
        }

        #endregion
    }
}
