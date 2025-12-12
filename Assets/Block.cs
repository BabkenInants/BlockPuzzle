using UnityEngine;

public class Block : MonoBehaviour
{
    private bool _isPicked = false;
    private Camera _mainCam;
    public Transform[] cells;
    private Vector3 _startPos;
    private Vector3 _mouseOffset;

    private void Start()
    {
        _mainCam = Camera.main;
    }

    public void SetColor(Color color)
    {
        foreach (var cell in cells)
            cell.GetComponent<SpriteRenderer>().color = color;
    }
    
    private void OnMouseDown()
    {
        if (Field.Instance.isAnyBlockPicked) return;
        _isPicked = true;
        Field.Instance.isAnyBlockPicked = true;
        _startPos = transform.position;
        _mouseOffset = transform.position - _mainCam.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        if (_isPicked)
        {
            Vector3 position = _mainCam.ScreenToWorldPoint(Input.mousePosition) + _mouseOffset;
            position.z = 0;
            transform.position = position;
            Field.Instance.HideCellsPreview();
            if(Field.Instance.CheckIfBlockCanBePlaced(cells))
                Field.Instance.PreviewCells(cells);
        }
    }

    private void OnMouseUp()
    {
        Field.Instance.isAnyBlockPicked = false;
        _isPicked = false;
        if(Field.Instance.CheckIfBlockCanBePlaced(cells))
            Field.Instance.PlaceBlock(cells, cells[0].GetComponent<SpriteRenderer>().color, gameObject);
        else
        {
            Field.Instance.HideCellsPreview();
            transform.position = _startPos;
        }
    }
}
