using System;
using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    public Transform[] cells;
    [field: SerializeField] public float notPickedSize { get; private set; } = .7f;
    [HideInInspector] public int sizeX = 3;
    [HideInInspector] public int sizeY = 3;
    [HideInInspector] public bool[] blockShape;
    private bool _isPicked;
    private Camera _mainCam;
    private Vector3 _startPos;
    private Vector3 _mouseOffset;
    private Settings _settings;
    private bool _canPick = true;
    private IEnumerator _sizeChangeCoroutine;

    private void Awake() => SetSize(notPickedSize);

    private void Start() => _mainCam = Camera.main;

    private void EndGame()
    {
        _canPick = false;
        if(_isPicked) PutBlockBack(true);
    }

    private void OnEnable()
    {
        GameEvents.OnGameOver += EndGame;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= EndGame;
    }

    public void InitSettings(Settings settings) => _settings = settings;
    
    #region Drag and drop
    
    private void OnMouseDown()
    {
        if (_settings == null || !_canPick || !_mainCam) return;
        GameEvents.RaiseOnBlockPicked(this);
        _isPicked = true;
        _startPos = transform.position;
        Vector3 mp = Input.mousePosition;
        mp.z = -_mainCam.transform.position.z;
        Vector3 world = _mainCam.ScreenToWorldPoint(mp);
        _mouseOffset = transform.position - world;
        SetSize(_settings.cellSize * 2);
    }

    private void OnMouseDrag()
    {
        if (!_isPicked || !_mainCam) return;
        //moving the block to the mouse position + offset
        if (_settings == null) return;
        float minY = _settings.minBlockDistanceFromCursorY;
        float maxY = _settings.maxBlockDistanceFromCursorY;
        float minX = _settings.minBlockDistanceFromCursorX;
        float maxX = _settings.maxBlockDistanceFromCursorX;
        Vector3 mousePos = Input.mousePosition;
        float yOffset = Mathf.Clamp(mousePos.y / Screen.height * maxY, minY, maxY);
        float xOffset = Mathf.Clamp((mousePos.x / Screen.width - .5f) * maxX, minX, maxX);
        Vector3 offset = _mouseOffset + new Vector3(xOffset, yOffset);
        Vector3 mp = Input.mousePosition;
        mp.z = -_mainCam.transform.position.z;
        Vector3 world = _mainCam.ScreenToWorldPoint(mp);
        Vector3 position = world + offset;
        position.z = 0;
        transform.position = position;
        GameEvents.RaiseOnBlockMoved();
    }

    private void OnMouseUp()
    {
        if(!_isPicked) return;
        GameEvents.RaiseOnBlockUnpicked(this);
        _isPicked = false;
    }
    
    public void PutBlockBack(bool disableCanPick = false)
    {
        _canPick = false;
        _isPicked = false;
        SetSize(notPickedSize);
        StartCoroutine(PositionTranslateRoutine(transform.position, _startPos, .1f, disableCanPick));
    }
    
    #endregion

    #region Visuals
    
    public void SetColor(Color color)
    {
        foreach (var cell in cells)
            cell.GetComponent<SpriteRenderer>().color = color;
    }

    private void SetSize(float size)
    {
        if(_sizeChangeCoroutine != null) StopCoroutine(_sizeChangeCoroutine);
        _sizeChangeCoroutine = SizeChangeRoutine(transform.localScale, new Vector3(size, size, 1), .05f);
        StartCoroutine(_sizeChangeCoroutine);
    }

    #endregion

    #region Coroutines
    
    private IEnumerator PositionTranslateRoutine(Vector3 startPos, Vector3 endPos, float duration, bool disableCanPick)
    {
        float estimatedTime = 0f;
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
        float estimatedTime = 0f;
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
