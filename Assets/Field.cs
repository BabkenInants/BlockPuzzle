using System;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public static Field Instance;
    [SerializeField] private Transform firstCell;
    private Transform lastCell;
    public Transform[,] fieldCells = new Transform[8, 8];
    public bool[,] cellIsFree = new bool[8, 8];
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = .5f;
    [SerializeField] private int cellsCountX = 8;
    [SerializeField] private int cellsCountY = 8;
    public bool isAnyBlockPicked;
    private List<Vector2Int> lastPreviewedCells;
    [SerializeField] private Color defaultCellColor;
    [SerializeField] private Color cellPreviewColor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < cellsCountX; i++) 
            for(int j = 0; j < cellsCountY; j++)
                cellIsFree[i, j] = true;
        GenerateField();
    }

    private void CheckForRowOrColumnRemoval()
    {
        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();
        //Checking rows
        for (int i = 0; i < cellsCountX; i++)
        {
            bool rowIsFull = true;
            for (int j = 0; j < cellsCountY; j++)
                if (cellIsFree[i, j])
                {
                    rowIsFull = false;
                    break;
                }
            if (rowIsFull) fullRows.Add(i);
        }
        //Checking columns
        for (int j = 0; j < cellsCountY; j++)
        {
            bool colIsFull = true;
            for (int i = 0; i < cellsCountX; i++)
                if (cellIsFree[i, j])
                {
                    colIsFull = false;
                    break;
                }
            if (colIsFull) fullCols.Add(j);
        }

        //Removing full rows
        foreach (int row in fullRows)
        {
            for (int j = 0; j < cellsCountX; j++)
            {
                fieldCells[row, j].GetComponent<SpriteRenderer>().color = defaultCellColor;
                cellIsFree[row, j] = true;
            }
        }

        //Removing full columns
        foreach (int col in fullCols)
        {
            for (int i = 0; i < cellsCountY; i++)
            {
                fieldCells[i, col].GetComponent<SpriteRenderer>().color = defaultCellColor;
                cellIsFree[i, col] = true;
            }
        }
    }

    public bool CheckIfBlockCanBePlaced(Transform[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].position.x <= firstCell.position.x - .05f||
                cells[i].position.x >= lastCell.position.x + .05f)
                return false;
            if (cells[i].position.y >= firstCell.position.y + .05f ||
                cells[i].position.y <= lastCell.position.y - .05f)
                return false;
            Vector2Int position = GetCellCoordinatesOnField(cells[i].position);
            if (!cellIsFree[position.x, position.y]) return false;
        }
        return true;
    }

    //Implement only after checking if the cells are free
    public void PreviewCells(Transform[] cells)
    {
        lastPreviewedCells = new List<Vector2Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position =  GetCellCoordinatesOnField(cells[i].position);
            lastPreviewedCells.Add(position);
            fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().color = cellPreviewColor;
        }
    }

    public void HideCellsPreview()
    {
        List<Vector2Int> cells = lastPreviewedCells;
        if (cells == null) return;
        foreach (Vector2Int cell in cells)
            fieldCells[cell[0], cell[1]].GetComponent<SpriteRenderer>().color = defaultCellColor;
    }

    //Implement only after checking if the cells are free
    public void PlaceBlock(Transform[] cells, Color color, GameObject blockObj)
    {
        lastPreviewedCells = new List<Vector2Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position = GetCellCoordinatesOnField(cells[i].position);
            Debug.Log(position.x + ", " + position.y);
            cellIsFree[position.x, position.y] = false;
            fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().color = color;
        }
        Destroy(blockObj);
        CheckForRowOrColumnRemoval();
    }

    private Vector2Int GetCellCoordinatesOnField(Vector3 position)
    {
        float x = position.x - firstCell.position.x;
        float y = firstCell.position.y - position.y;
        x /= cellSize;
        y /= cellSize;
        int row = Convert.ToInt32(y);
        int col = Convert.ToInt32(x);
        row = Math.Clamp(row, 0, cellsCountY - 1);
        col = Math.Clamp(col, 0, cellsCountX - 1);
        return new Vector2Int(row, col);
    }

    private void GenerateField()
    {
        fieldCells[0, 0] = firstCell;
        for (int i = 0; i < cellsCountY; i++)
        {
            for (int j = 0; j < cellsCountX; j++)
            {
                if (i == 0 && j == 0) continue;
                Vector3 position = firstCell.position + new Vector3(j * cellSize, -i * cellSize, 0f);
                fieldCells[i, j] = Instantiate(cellPrefab, position, 
                    Quaternion.identity, transform).transform;
            }
        }
        lastCell = fieldCells[cellsCountY - 1, cellsCountX - 1];
    }
}
