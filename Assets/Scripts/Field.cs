using UnityEngine;

public class Field : MonoBehaviour
{
    public bool isReady{get; private set;}
    public bool[,] cellIsFree { get; private set; }
    [SerializeField] private Settings settings;
    private Transform _firstCell;

    private void Awake()
    {
        isReady = true;
        cellIsFree = new bool[settings.rowsCount, settings.columnsCount];
        for (int i = 0; i < settings.rowsCount; i++) 
            for(int j = 0; j < settings.columnsCount; j++)
                cellIsFree[i, j] = true;
    }

    public void InitFirstCell(Transform firstCell) =>
        _firstCell = firstCell;

    #region Placement

    ///Implement only after checking if the cells are free
    public ChangesAfterMove PlaceBlock(GridPos[] cells, Color color)
    {
        ChangesAfterMove changesAfterMove = new ChangesAfterMove
        {
            BlockCellsPositions = cells,
            BlockColor = color
        };
        foreach (GridPos cell in cells)
            cellIsFree[cell.Row, cell.Column] = false;
        int rowsAndColumnsRemoved = CheckForRowOrColumnRemoval(ref changesAfterMove);
        return changesAfterMove;
    }
    
    ///Used only for drag and drop
    public bool CheckIfBlockCanBePlaced(Transform[] cells)
    {
        //Trying to preview and also checking if the block can be placed in its current position
        foreach(Transform cell in cells)
        {
            GridPos position = FieldUtils.GetCellCoordinatesOnField(cell.position, _firstCell.position, settings.cellSize);
            if (!position.IsValid(settings.rowsCount, settings.columnsCount))
                return false;
            if (!cellIsFree[position.Row, position.Column]) return false;
        }
        return true;
    }

    #endregion
    
    #region Removing full rows and columns
    
    private int CheckForRowOrColumnRemoval(ref ChangesAfterMove changesAfterMove)
    {
        var fullRows = new bool[settings.rowsCount];
        var fullCols = new bool[settings.columnsCount];
        //Checking rows
        for (int i = 0; i < settings.rowsCount; i++)
        {
            bool rowIsFull = true;
            for (int j = 0; j < settings.columnsCount; j++)
                if (cellIsFree[i, j])
                {
                    rowIsFull = false;
                    break;
                }
            fullRows[i] = rowIsFull;
        }
        //Checking columns
        for (int j = 0; j < settings.columnsCount; j++)
        {
            bool colIsFull = true;
            for (int i = 0; i < settings.rowsCount; i++)
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
        for(int j = 0; j < settings.columnsCount; j++)
            cellIsFree[row, j] = true;
    }
    
    private void RemoveColumn(int col, bool[] fullRows)
    {
        for (int i = 0; i < settings.rowsCount; i++)
        {
            if(fullRows[i]) continue;
            cellIsFree[i, col] = true;
        }
    } 

    #endregion
}
