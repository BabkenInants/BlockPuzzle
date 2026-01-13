using UnityEngine;
using UnityEngine.SceneManagement;

public class Field : MonoBehaviour
{
    public bool isReady{get; private set;}
    public bool[,] cellIsFree { get; private set; }
    [SerializeField] private Settings settings;
    private Transform _firstCell;
    private int _score;

    private void Awake()
    {
        isReady = true;
        cellIsFree = new bool[settings.cellsCountY, settings.cellsCountX];
        for (int i = 0; i < settings.cellsCountY; i++) 
            for(int j = 0; j < settings.cellsCountX; j++)
                cellIsFree[i, j] = true;
    }

    public void InitFirstCell(Transform firstCell) =>
        _firstCell = firstCell;

    //TODO Transfer this function to ui manager
    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    #region Placement
    
    public Vector2Int GetCellCoordinatesOnField(Vector3 position)
    {
        float x = position.x - _firstCell.position.x;
        float y = _firstCell.position.y - position.y;
        x /= settings.cellSize;
        y /= settings.cellSize;
        var row = Mathf.RoundToInt(y);
        var col = Mathf.RoundToInt(x);
        return new Vector2Int(row, col);
    }

    //Implement only after checking if the cells are free
    public void PlaceBlock(Vector2Int[] cells, Color color)
    {
        ChangesAfterMove changesAfterMove = new ChangesAfterMove();
        changesAfterMove.BlockCellsPositions = cells;
        changesAfterMove.BlockColor = color;
        foreach (Vector2Int cell in cells)
            cellIsFree[cell.x, cell.y] = false;
        int rowsAndColumnsRemoved = CheckForRowOrColumnRemoval(ref changesAfterMove);
        GameEvents.RaiseChangesAfterMoveReport(changesAfterMove);
        GameEvents.RaiseRequestGameOverCheck();
    }
    
    //Used only for drag and drop
    public bool CheckIfBlockCanBePlaced(Transform[] cells)
    {
        //Trying to preview and also checking if the block can be placed in its current position
        foreach(Transform cell in cells)
        {
            Vector2Int position = GetCellCoordinatesOnField(cell.position);
            if (position.x < 0 || position.y < 0 || position.x >= settings.cellsCountY ||
                position.y >= settings.cellsCountX)
                return false;
            if (!cellIsFree[position.x, position.y]) return false;
        }
        return true;
    }

    //Don't use if the block is out of the field(use this function in loops, it's more efficient)
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

    #endregion
    
    #region Removing full rows and columns
    
    private int CheckForRowOrColumnRemoval(ref ChangesAfterMove changesAfterMove)
    {
        var fullRows = new bool[settings.cellsCountY];
        var fullCols = new bool[settings.cellsCountX];
        //Checking rows
        for (int i = 0; i < settings.cellsCountY; i++)
        {
            bool rowIsFull = true;
            for (int j = 0; j < settings.cellsCountX; j++)
                if (cellIsFree[i, j])
                {
                    rowIsFull = false;
                    break;
                }
            fullRows[i] = rowIsFull;
        }
        //Checking columns
        for (int j = 0; j < settings.cellsCountX; j++)
        {
            bool colIsFull = true;
            for (int i = 0; i < settings.cellsCountY; i++)
                if (cellIsFree[i, j])
                {
                    colIsFull = false;
                    break;
                }
            fullCols[j] = colIsFull;
        }
        
        var rowsNColsRemoved = 0;
        
        //Removing full rows
        for (var row = 0; row < fullRows.Length; row++)
            if (fullRows[row])
            {
                RemoveRow(row);
                rowsNColsRemoved++;
            }

        //Removing full columns
        for (var col = 0; col < fullCols.Length; col++)
            if (fullCols[col])
            {
                RemoveColumn(col, fullRows);
                rowsNColsRemoved++;
            }
        changesAfterMove.FullRows = (bool[]) fullRows.Clone();
        changesAfterMove.FullCols = (bool[]) fullCols.Clone();
        return rowsNColsRemoved;
    }

    private void RemoveRow(int row)
    {
        for(int j = 0; j < settings.cellsCountX; j++)
            cellIsFree[row, j] = true;
    }
    
    private void RemoveColumn(int col, bool[] fullRows)
    {
        for (int i = 0; i < settings.cellsCountY; i++)
        {
            if(fullRows[i]) continue;
            cellIsFree[i, col] = true;
        }
    } 

    #endregion
}

public class ChangesAfterMove
{
    //BlockPlacement
    public Vector2Int[] BlockCellsPositions;
    public Color BlockColor;
    //RowsAndColumnsRemoved
    public bool[] FullRows;
    public bool[] FullCols;
}
