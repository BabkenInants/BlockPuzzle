using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Transform[] cells;
    [HideInInspector] public int sizeX = 3;
    [HideInInspector] public int sizeY = 3;
    [HideInInspector] public bool[] blockShape;
    public bool[,] blockShapeMatrix { get; private set; }
    private bool _isPicked = false;
    private Camera _mainCam;
    private Vector3 _startPos;
    private Vector3 _mouseOffset;
    private float notPickedSize = .7f;

    private void Awake()
    {
        SetSize(notPickedSize);
        GenerateBlockShapeMatrix();
    }

    private void Start()
    {
        _mainCam = Camera.main;
    }

    public void SetColor(Color color)
    {
        foreach (var cell in cells)
            cell.GetComponent<SpriteRenderer>().color = color;
    }

    private void GenerateBlockShapeMatrix()
    {
        blockShapeMatrix = new bool[sizeY, sizeX];
        for (int row = 0; row < sizeY; row++)
            for (int col = 0; col < sizeX; col++)
                blockShapeMatrix[row, col] = blockShape[row * sizeX + col];
    }
    
    private void SetSize(float size) =>
        StartCoroutine(SizeChangeRoutine(transform.localScale, new Vector3(size, size, 1), .05f));

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
            float minDistanceFromCursorY = Field.Instance.minBlockDistanceFromCursorY;
            float maxDistanceFromCursorY = Field.Instance.maxBlockDistanceFromCursorY;
            float minDistanceFromCursorX = Field.Instance.minBlockDistanceFromCursorX;
            float maxDistanceFromCursorX = Field.Instance.maxBlockDistanceFromCursorX;
            Vector3 mousePos = Input.mousePosition;
            float yOffset = Mathf.Clamp(mousePos.y / Screen.height * maxDistanceFromCursorY, minDistanceFromCursorY, maxDistanceFromCursorY);;
            float xOffset = Mathf.Clamp((mousePos.x / Screen.width - .5f) * maxDistanceFromCursorX, minDistanceFromCursorX, maxDistanceFromCursorX);;
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
}
