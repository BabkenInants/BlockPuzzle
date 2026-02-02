using System.Collections;
using Themes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Core
{
    public class Block : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IThemeReceiver
    {
        public Transform[] cells;
        public Color color { get; private set; }
        [HideInInspector] public int sizeX = 3;
        [HideInInspector] public int sizeY = 3;
        ///true - busy, false - free
        [HideInInspector] public bool[] blockShape;
        private SpriteRenderer[] _cellsSpriteRenderers;
        private float _notPickedSize  = .7f;
        private Vector2 _notPickedColliderSize;
        private Vector2 _colliderDefaultSize;
        private bool _isPicked;
        private Camera _mainCam;
        private Vector3 _startPos;
        private Vector3 _mouseOffset;
        private Settings _settings;
        private bool _canPick = true;
        private IEnumerator _sizeChangeCoroutine;
        private bool _otherBlockIsPicked;
        private BoxCollider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _cellsSpriteRenderers = new SpriteRenderer[cells.Length];
            for(var i = 0; i < cells.Length; i++)
                _cellsSpriteRenderers[i] = cells[i].GetComponent<SpriteRenderer>();
        }
        
        public void Init(Settings settings, float notPickedSize, Color blockColor)
        {
            _mainCam = Camera.main;
            _settings = settings;
            _notPickedSize = notPickedSize;
            SetBlockSize(notPickedSize);
            SetColor(blockColor);
            CalculateAndChangeColliderSize();
        }

        private void CalculateAndChangeColliderSize()
        {
            float blockSizePercentage = _notPickedSize * 100 / _settings.maxNotPickedBlockSize;
            _colliderDefaultSize = _collider.size;
            _notPickedColliderSize = _colliderDefaultSize * 100 / blockSizePercentage;
            _collider.size = _notPickedColliderSize;
        }

        #region Events
        
        private void HandleEndGame()
        {
            _canPick = false;
            if(_isPicked) PutBlockBack(true);
        }

        private void HandleOnBlockPicked(Block block)
        {
            if(block != this) _otherBlockIsPicked = true;
        }

        private void HandeOnBlockUnpicked(Block block)
        {
            if (block != this) _otherBlockIsPicked = false;
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += HandleEndGame;
            GameEvents.OnBlockPicked += HandleOnBlockPicked;
            GameEvents.OnBlockUnpicked +=  HandeOnBlockUnpicked;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= HandleEndGame;
            GameEvents.OnBlockPicked -= HandleOnBlockPicked;
            GameEvents.OnBlockUnpicked -= HandeOnBlockUnpicked;
        }

        #endregion

        #region Themes
        
        public void ReceiveThemeOnGameStart(Theme theme)
        {
            color = theme.blockColors[Random.Range(0, theme.blockColors.Length)];
            foreach (SpriteRenderer spriteRenderer in _cellsSpriteRenderers)
                spriteRenderer.color = color;
        }

        public void ReceiveTheme(Theme theme)
        {
            Color startColor = color;
            color = theme.blockColors[Random.Range(0, theme.blockColors.Length)];
            foreach (SpriteRenderer spriteRenderer in _cellsSpriteRenderers)
                StartCoroutine(ThemeTools.SetSpriteRendererColor(spriteRenderer, startColor, 
                    color, _settings.themeChangeDuration));
        }
        
        #endregion
    
        #region Drag and drop
    
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_settings || !_canPick || !_mainCam || _otherBlockIsPicked) return;
            GameEvents.RaiseOnBlockPicked(this);
            _isPicked = true;
            _startPos = transform.position;
#if UNITY_EDITOR
            Vector3 mp = Mouse.current.position.ReadValue();
#else
            if(Touchscreen.current == null) return;
            Vector3 mp = Touchscreen.current.primaryTouch.position.ReadValue();
#endif
            mp.z = -_mainCam.transform.position.z;   
            Vector3 mouseWorldPos = _mainCam.ScreenToWorldPoint(mp);
            _mouseOffset = transform.position - mouseWorldPos;
            SetBlockSize(_settings.cellSize * 2);
            _collider.size = _colliderDefaultSize;
            foreach (SpriteRenderer spriteRenderer in _cellsSpriteRenderers)
                spriteRenderer.sortingOrder = _settings.pickedBlockCellsSpriteLayer;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isPicked || !_mainCam || !_settings) return;
            float minY = _settings.minBlockDistanceFromCursorY;
            float maxY = _settings.maxBlockDistanceFromCursorY;
            float minX = _settings.minBlockDistanceFromCursorX;
            float maxX = _settings.maxBlockDistanceFromCursorX;
#if UNITY_EDITOR
            if (Mouse.current == null) return;
            Vector3 mousePos = Mouse.current.position.ReadValue();
#else
            if(Touchscreen.current == null) return;
            Vector3 mousePos = Touchscreen.current.primaryTouch.position.ReadValue();
#endif
            mousePos.z = -_mainCam.transform.position.z;
            Vector3 mouseWorldPos = _mainCam.ScreenToWorldPoint(mousePos);
            float yOffset = Mathf.Clamp(mousePos.y / Screen.height * maxY, minY, maxY);
            float xOffset = Mathf.Clamp((mousePos.x / Screen.width - .5f) * maxX, minX, maxX);
            Vector3 offset = _mouseOffset + new Vector3(xOffset, yOffset);
            //setting position (using vector2 so the z will be 0)
            Vector2 position = mouseWorldPos + offset;
            transform.position = position;
            GameEvents.RaiseOnBlockMoved();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(!_isPicked) return;
            GameEvents.RaiseOnBlockUnpicked(this);
            _isPicked = false;
            foreach (SpriteRenderer spriteRenderer in _cellsSpriteRenderers)
                spriteRenderer.sortingOrder = _settings.notPickedBlockCellsSpriteLayer;
        }
    
        public void PutBlockBack(bool disableCanPick = false)
        {
            _canPick = false;
            _isPicked = false;
            _collider.size = _notPickedColliderSize;
            SetBlockSize(_notPickedSize);
            StartCoroutine(PositionTranslateRoutine(transform.position, _startPos, .1f, disableCanPick));
        }
    
        #endregion

        #region Visuals
    
        public void SetColor(Color newColor)
        {
            color = newColor;
            foreach (SpriteRenderer spriteRenderer in _cellsSpriteRenderers)
                spriteRenderer.color = newColor;
        }

        private void SetBlockSize(float size)
        {
            if(_sizeChangeCoroutine != null) StopCoroutine(_sizeChangeCoroutine);
            _sizeChangeCoroutine = SizeChangeRoutine(transform.localScale, new Vector3(size, size, 1), .05f);
            StartCoroutine(_sizeChangeCoroutine);
        }

        #endregion

        #region Coroutines
    
        private IEnumerator PositionTranslateRoutine(Vector3 startPos, Vector3 endPos, float duration, bool disableCanPick)
        {
            var estimatedTime = 0f;
            while (estimatedTime < duration)
            {
                estimatedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, endPos, estimatedTime / duration);
                yield return null;
            }
            transform.position = endPos;
            if(!disableCanPick) _canPick = true;
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
