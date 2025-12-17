using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    public Transform[] cells;
    [HideInInspector] public int sizeX = 3;
    [HideInInspector] public int sizeY = 3;
    [HideInInspector] public bool[] blockShape;
    [SerializeField] private float notPickedSize = .7f;
    private bool _isPicked;
    private Camera _mainCam;
    private Vector3 _startPos;
    private Vector3 _mouseOffset;

    private void Awake() => SetSize(notPickedSize);

    private void Start() => _mainCam = Camera.main;
    
    #region Drag and drop
    
    private void OnMouseDown()
    {
        _isPicked = true;
        _startPos = transform.position;
        _mouseOffset = transform.position - _mainCam.ScreenToWorldPoint(Input.mousePosition);
        SetSize(Field.Instance.cellSize * 2);
    }

    private void OnMouseDrag()
    {
        if (_isPicked)
        {
            float minY = Field.Instance.minBlockDistanceFromCursorY;
            float maxY = Field.Instance.maxBlockDistanceFromCursorY;
            float minX = Field.Instance.minBlockDistanceFromCursorX;
            float maxX = Field.Instance.maxBlockDistanceFromCursorX;
            Vector3 mousePos = Input.mousePosition;
            float yOffset = Mathf.Clamp(mousePos.y / Screen.height * maxY, minY, maxY);
            float xOffset = Mathf.Clamp((mousePos.x / Screen.width - .5f) * maxX, minX, maxX);
            Vector3 offset = _mouseOffset +  new Vector3(xOffset, yOffset);
            Vector3 position = _mainCam.ScreenToWorldPoint(Input.mousePosition) + offset;
            position.z = 0;
            transform.position = position;
            Field.Instance.HideCellsPreview();
            if(Field.Instance.CheckIfBlockCanBePlaced(cells))
                Field.Instance.PreviewCells(cells);
        }
    }

    private void OnMouseUp()
    {
        _isPicked = false;
        if(Field.Instance.CheckIfBlockCanBePlaced(cells))
            Field.Instance.PlaceBlock(cells, cells[0].GetComponent<SpriteRenderer>().color, gameObject);
        else
        {
            SetSize(notPickedSize);
            Field.Instance.HideCellsPreview();
            StartCoroutine(PositionTranslateRoutine(transform.position, _startPos, .1f));
        }
    }
    
    #endregion

    #region Visuals
    
    public void SetColor(Color color)
    {
        foreach (var cell in cells)
            cell.GetComponent<SpriteRenderer>().color = color;
    }
    
    private void SetSize(float size) =>
        StartCoroutine(SizeChangeRoutine(transform.localScale, new Vector3(size, size, 1), .05f));
    
    #endregion

    #region Coroutines
    
    private IEnumerator PositionTranslateRoutine(Vector3 startPos, Vector3 endPos, float duration)
    {
        float estimatedTime = 0f;
        while (estimatedTime < duration)
        {
            estimatedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, estimatedTime / duration);
            yield return null;
        }
        transform.position = endPos;
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
    }
    
    #endregion
}
