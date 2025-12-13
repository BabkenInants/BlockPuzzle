using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Field : MonoBehaviour
{
    public static Field Instance { get; private set; }
    
    [SerializeField] private Transform firstCell;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Color defaultCellColor;
    [SerializeField] private Color cellPreviewColor;
    [SerializeField] private GameObject gameOverMenu;
    [field: SerializeField] public float minBlockDistanceFromCursorY {get; private set; } = 0f;
    [field: SerializeField] public float maxBlockDistanceFromCursorY {get; private set; } = .5f;
    [field: SerializeField] public float minBlockDistanceFromCursorX {get; private set; } = 0f;
    [field: SerializeField] public float maxBlockDistanceFromCursorX {get; private set; } = .5f;
    [field: SerializeField] public float cellSize { get; private set; } = .5f;
    [field: SerializeField] public Sprite emptyCell { get; private set; }
    [field: SerializeField] public Sprite notEmptyCell { get; private set; }
    [field: SerializeField] public int cellsCountX { get; private set; } = 8;
    [field: SerializeField] public int cellsCountY { get; private set; } = 8;
    
    private Transform lastCell;
    private Transform[,] fieldCells = new Transform[8, 8];
    private bool[,] cellIsFree = new bool[8, 8];
    private List<Vector2Int> lastPreviewedCells;

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

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void CheckIfGameIsOver()
    {
        List<Block> currentBlocks = new List<Block>();
        foreach(var block in BlockSpawner.Instance.blocks)
        {
            if (block == null) continue;
            Block currentBlock = block.GetComponent<Block>();
            currentBlocks.Add(currentBlock);
        }
        if(currentBlocks.Count == 0) return;
        bool atLeastOneBlockCanBePlaced = false;
        foreach(var block in  currentBlocks)
        {
            for (int row = 0; row < cellsCountY; row++)
            {
                for (int col = 0; col < cellsCountX; col++)
                {
                    if (CheckIfBlockCanBePlacedAtCell(block, row, col))
                    {
                        atLeastOneBlockCanBePlaced = true;
                        break;
                    }
                }
                if (atLeastOneBlockCanBePlaced) break;
            }
            if (atLeastOneBlockCanBePlaced) break;
        }
        if (!atLeastOneBlockCanBePlaced)
        {
            //Game Over
            gameOverMenu.SetActive(true);
        }
    }
    
    #region Placement

    //Implement only after checking if the cells are free
    public void PlaceBlock(Transform[] cells, Color color, GameObject blockObj)
    {
        lastPreviewedCells = new List<Vector2Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position = GetCellCoordinatesOnField(cells[i].position);
            cellIsFree[position.x, position.y] = false;
            fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().sprite = notEmptyCell;
            fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().color = color;
            BlockSpawner.Instance.RemoveBlock(blockObj);
        }
        CheckForRowOrColumnRemoval();
    }
    
    public bool CheckIfBlockCanBePlaced(Transform[] cells)
    {
        //Trying to preview and also checking if the block can be placed in its current position
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].position.x <= firstCell.position.x - (cellSize * .5f - .05f)||
                cells[i].position.x >= lastCell.position.x + (cellSize * .5f - .05f))
                return false;
            if (cells[i].position.y >= firstCell.position.y + (cellSize * .5f - .05f) ||
                cells[i].position.y <= lastCell.position.y - (cellSize * .5f - .05f))
                return false;
            Vector2Int position = GetCellCoordinatesOnField(cells[i].position);
            if (!cellIsFree[position.x, position.y]) return false;
        }
        return true;
    }

    private bool CheckIfBlockCanBePlacedAtCell(Block block, int row, int col)
    {
        for (int y = 0; y < block.sizeY; y++)
            for (int x = 0; x < block.sizeX; x++)
            {
                if (!block.blockShapeMatrix[y, x]) 
                    continue;
                
                int fieldRow = row + y;
                int fieldCol = col + x;
                
                //checking if the block is outside the field
                if (fieldRow < 0 || fieldRow >= cellsCountY ||
                    fieldCol < 0 || fieldCol >= cellsCountX)
                    return false;
                
                //checking if the cell is not free
                if (!cellIsFree[fieldRow, fieldCol])
                    return false;
            }
        return true;
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
    
    #region Previewing
    
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

    #endregion

    #endregion
    
    #region Removing full rows and columns
    
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
            StartCoroutine(RemoveRow(row));
        
        //Removing full columns
        foreach (int col in fullCols)
            StartCoroutine(RemoveColumn(col));
        CheckIfGameIsOver();
    }

    private IEnumerator RemoveRow(int row)
    {
        for(int j = 0; j < cellsCountX; j++)
            cellIsFree[row, j] = true;
        for (int j = 0; j < cellsCountX; j++)
        {
            fieldCells[row, j].GetComponent<SpriteRenderer>().sprite = emptyCell;
            fieldCells[row, j].GetComponent<SpriteRenderer>().color = defaultCellColor;
            yield return new WaitForSeconds(0.02f);
        }
    }
    private IEnumerator RemoveColumn(int col)
    {
        for(int i = 0; i < cellsCountX; i++)
            cellIsFree[i, col] = true;
        for (int i = 0; i < cellsCountY; i++)
        {
            fieldCells[i, col].GetComponent<SpriteRenderer>().sprite = emptyCell;
            fieldCells[i, col].GetComponent<SpriteRenderer>().color = defaultCellColor;
            yield return new WaitForSeconds(0.02f);
        }
    } 

    #endregion
}
