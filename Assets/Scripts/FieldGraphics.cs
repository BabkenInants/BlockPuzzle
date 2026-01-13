using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGraphics : MonoBehaviour
{
    public bool isReady{get; private set;}
    [field:SerializeField] public Transform firstCell{get; private set;}
    [SerializeField] private Settings settings;
    private Transform[,] _fieldCells;
    private List<Vector2Int> _lastPreviewedCells;

    private void Awake() => _fieldCells = new Transform[settings.cellsCountY, settings.cellsCountX];

    private void Start() => GenerateField();

    private void OnEnable() => Subscribe();
    
    private void OnDisable() => Unsubscribe();
    
    private void Subscribe()
    {
        GameEvents.HideCellsPreview += HideCellsPreview;
        GameEvents.PreviewCells += PreviewCells;
    }

    private void Unsubscribe()
    {
        GameEvents.HideCellsPreview -= HideCellsPreview;
        GameEvents.PreviewCells -= PreviewCells;
    }

    #region Blocks Placement

    public void PlaceBlock(Vector2Int[] cells, Color color)
    {
        _lastPreviewedCells = null;
        foreach (Vector2Int cell in cells)
        {
            _fieldCells[cell.x, cell.y].GetComponent<SpriteRenderer>().sprite = settings.notEmptyCell;
            _fieldCells[cell.x, cell.y].GetComponent<SpriteRenderer>().color = color;
        }
    }

    public IEnumerator RemoveRow(int row)
    {
        for (int j = 0; j < settings.cellsCountX; j++)
        {
            _fieldCells[row, j].GetComponent<SpriteRenderer>().sprite = settings.emptyCell;
            _fieldCells[row, j].GetComponent<SpriteRenderer>().color = settings.defaultCellColor;
            yield return new WaitForSeconds(0.02f);
        }
    }
    
    public IEnumerator RemoveColumn(int col, bool[] fullRows)
    {
        for (int i = 0; i < settings.cellsCountY; i++)
        {
            if(fullRows[i]) continue;
            _fieldCells[i, col].GetComponent<SpriteRenderer>().sprite = settings.emptyCell;
            _fieldCells[i, col].GetComponent<SpriteRenderer>().color = settings.defaultCellColor;
            yield return new WaitForSeconds(0.02f);
        }
    } 

    #endregion
    
    #region Previewing
    
    private Vector2Int GetCellCoordinatesOnField(Vector3 position)
    {
        float x = position.x - firstCell.position.x;
        float y = firstCell.position.y - position.y;
        x /= settings.cellSize;
        y /= settings.cellSize;
        var row = Mathf.RoundToInt(y);
        var col = Mathf.RoundToInt(x);
        row = Math.Clamp(row, 0, settings.cellsCountY - 1);
        col = Math.Clamp(col, 0, settings.cellsCountX - 1);
        return new Vector2Int(row, col);
    }
    
    //Implement only after checking if the cells are free
    private void PreviewCells(Transform[] cells)
    {
        if (_lastPreviewedCells != null)
            HideCellsPreview();
        _lastPreviewedCells = new List<Vector2Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position = GetCellCoordinatesOnField(cells[i].position);
            _lastPreviewedCells.Add(position);
            _fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().color = settings.cellPreviewColor;
        }
    }

    private void HideCellsPreview()
    {
        List<Vector2Int> cells = _lastPreviewedCells;
        if (cells == null) return;
        foreach (Vector2Int cell in cells)
            _fieldCells[cell[0], cell[1]].GetComponent<SpriteRenderer>().color = settings.defaultCellColor;
        _lastPreviewedCells = null;
    }

    #endregion
    
    private void GenerateField()
    {
        _fieldCells[0, 0] = firstCell;
        for (int i = 0; i < settings.cellsCountY; i++)
        {
            for (int j = 0; j < settings.cellsCountX; j++)
            {
                if (i == 0 && j == 0) continue;
                Vector3 position = firstCell.position + new Vector3(j * settings.cellSize, -i * settings.cellSize, 0f);
                _fieldCells[i, j] = Instantiate(settings.cellPrefab, position, 
                    Quaternion.identity, transform).transform;
            }
        }
        isReady = true;
    }
}
