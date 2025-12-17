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
    [field: SerializeField] public float minBlockDistanceFromCursorY {get; private set; } = 1f;
    [field: SerializeField] public float maxBlockDistanceFromCursorY {get; private set; } = 5f;
    [field: SerializeField] public float minBlockDistanceFromCursorX {get; private set; } = -.5f;
    [field: SerializeField] public float maxBlockDistanceFromCursorX {get; private set; } = 1f;
    [field: SerializeField] public float cellSize { get; private set; } = .5f;
    [field: SerializeField] public Sprite emptyCell { get; private set; }
    [field: SerializeField] public Sprite notEmptyCell { get; private set; }
    [field: SerializeField] public int cellsCountX { get; private set; } = 8;
    [field: SerializeField] public int cellsCountY { get; private set; } = 8;
    
    private Transform _lastCell;
    private Transform[,] _fieldCells;
    public bool[,] cellIsFree { get; private set; }
    private List<Vector2Int> _lastPreviewedCells;
    private int score = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        _fieldCells = new Transform[cellsCountY, cellsCountX];
        cellIsFree = new bool[cellsCountY, cellsCountX];
        for (int i = 0; i < cellsCountY; i++) 
            for(int j = 0; j < cellsCountX; j++)
                cellIsFree[i, j] = true;
    }

    void Start()
    {
        GenerateField();
    }
    
    private void GenerateField()
    {
        _fieldCells[0, 0] = firstCell;
        for (int i = 0; i < cellsCountY; i++)
        {
            for (int j = 0; j < cellsCountX; j++)
            {
                if (i == 0 && j == 0) continue;
                Vector3 position = firstCell.position + new Vector3(j * cellSize, -i * cellSize, 0f);
                _fieldCells[i, j] = Instantiate(cellPrefab, position, 
                    Quaternion.identity, transform).transform;
            }
        }
        _lastCell = _fieldCells[cellsCountY - 1, cellsCountX - 1];
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void CheckIfGameIsOver()
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
            for (int row = 0; row <= cellsCountY - block.sizeY; row++)
            {
                for (int col = 0; col <= cellsCountX - block.sizeX; col++)
                {
                    if (CheckIfBlockCanBePlacedAtCell(cellIsFree, block, row, col))
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
            EndGame();
        }
    }

    public void EndGame()
    {
        gameOverMenu.SetActive(true);
    }

    #region Placement

    //Implement only after checking if the cells are free
    public int PlaceBlock(Transform[] cells, Color color, GameObject blockObj)
    {
        _lastPreviewedCells = new List<Vector2Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position = GetCellCoordinatesOnField(cells[i].position);
            cellIsFree[position.x, position.y] = false;
            _fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().sprite = notEmptyCell;
            _fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().color = color;
        }
        BlockSpawner.Instance.RemoveBlock(blockObj);
        int rowsAndColumnsRemoved = CheckForRowOrColumnRemoval();
        score += cells.Length;
        int rcScore = rowsAndColumnsRemoved * 1000 + rowsAndColumnsRemoved * 100; //rows and columns removed score
        score += rcScore;
        //Debug.Log(score);
        return score;
    }
    
    //Used only for drag and drop
    public bool CheckIfBlockCanBePlaced(Transform[] cells)
    {
        //Trying to preview and also checking if the block can be placed in its current position
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].position.x <= firstCell.position.x - (cellSize * .5f - .05f)||
                cells[i].position.x >= _lastCell.position.x + (cellSize * .5f - .05f))
                return false;
            if (cells[i].position.y >= firstCell.position.y + (cellSize * .5f - .05f) ||
                cells[i].position.y <= _lastCell.position.y - (cellSize * .5f - .05f))
                return false;
            Vector2Int position = GetCellCoordinatesOnField(cells[i].position);
            if (!cellIsFree[position.x, position.y]) return false;
        }
        return true;
    }

    //Don't use if the block is out of the field(check it in loop instead, it's more efficient)
    public bool CheckIfBlockCanBePlacedAtCell(bool[,] field, Block block, int row, int col)
    {
        for (int y = 0; y < block.sizeY; y++)
            for (int x = 0; x < block.sizeX; x++)
            {
                if (!block.blockShape[y * block.sizeX + x]) 
                    continue;
                //checking if the cell is not free
                if (field[row + y, col + x] == false)
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
        var row = Convert.ToInt32(y);
        var col = Convert.ToInt32(x);
        row = Math.Clamp(row, 0, cellsCountY - 1);
        col = Math.Clamp(col, 0, cellsCountX - 1);
        return new Vector2Int(row, col);
    }
    
    #region Previewing
    
    //Implement only after checking if the cells are free
    public void PreviewCells(Transform[] cells)
    {
        _lastPreviewedCells = new List<Vector2Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int position =  GetCellCoordinatesOnField(cells[i].position);
            _lastPreviewedCells.Add(position);
            _fieldCells[position.x, position.y].GetComponent<SpriteRenderer>().color = cellPreviewColor;
        }
    }

    public void HideCellsPreview()
    {
        List<Vector2Int> cells = _lastPreviewedCells;
        if (cells == null) return;
        foreach (Vector2Int cell in cells)
            _fieldCells[cell[0], cell[1]].GetComponent<SpriteRenderer>().color = defaultCellColor;
    }

    #endregion

    #endregion
    
    #region Removing full rows and columns
    
    private int CheckForRowOrColumnRemoval()
    {
        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();
        //Checking rows
        for (int i = 0; i < cellsCountY; i++)
        {
            bool rowIsFull = true;
            for (int j = 0; j < cellsCountX; j++)
                if (cellIsFree[i, j])
                {
                    rowIsFull = false;
                    break;
                }
            if (rowIsFull) fullRows.Add(i);
        }
        //Checking columns
        for (int j = 0; j < cellsCountX; j++)
        {
            bool colIsFull = true;
            for (int i = 0; i < cellsCountY; i++)
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
        return fullRows.Count + fullCols.Count;
    }

    private IEnumerator RemoveRow(int row)
    {
        for(int j = 0; j < cellsCountX; j++)
            cellIsFree[row, j] = true;
        for (int j = 0; j < cellsCountX; j++)
        {
            _fieldCells[row, j].GetComponent<SpriteRenderer>().sprite = emptyCell;
            _fieldCells[row, j].GetComponent<SpriteRenderer>().color = defaultCellColor;
            yield return new WaitForSeconds(0.02f);
        }
    }
    
    private IEnumerator RemoveColumn(int col)
    {
        for(int i = 0; i < cellsCountY; i++)
            cellIsFree[i, col] = true;
        
        for (int i = 0; i < cellsCountY; i++)
        {
            _fieldCells[i, col].GetComponent<SpriteRenderer>().sprite = emptyCell;
            _fieldCells[i, col].GetComponent<SpriteRenderer>().color = defaultCellColor;
            yield return new WaitForSeconds(0.02f);
        }
    } 

    #endregion
}
